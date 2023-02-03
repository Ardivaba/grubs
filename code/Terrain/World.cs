﻿using Sandbox.Csg;

namespace Grubs;

public partial class World : Entity
{
	[Net]
	public CsgSolid CsgWorld { get; set; }

	[Net]
	public CsgSolid CsgBackground { get; set; }

	public CsgBrush CubeBrush { get; } = ResourceLibrary.Get<CsgBrush>( "brushes/cube.csg" );
	public CsgBrush CoolBrush { get; } = ResourceLibrary.Get<CsgBrush>( "brushes/cool.csg" );
	public CsgMaterial DefaultMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/default.csgmat" );
	public CsgMaterial SandMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/sand.csgmat" );
	public CsgMaterial LavaMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/lava.csgmat" );

	public readonly float WorldLength = GrubsConfig.TerrainLength;
	public readonly float WorldHeight = GrubsConfig.TerrainHeight;
	public const float WorldWidth = 64f;

	private readonly float _zoom = GrubsConfig.TerrainNoiseZoom;
	private const float GridSize = 1024f;

	public override void Spawn()
	{
		Assert.True( Game.IsServer );

		Reset();
	}

	public void Reset()
	{
		CsgWorld?.Delete();

		CsgWorld = new CsgSolid( GridSize );
		CsgWorld.Add( CubeBrush, SandMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 0, -WorldHeight / 2 ) );

		CsgBackground = new CsgSolid( GridSize );
		CsgBackground.Add( CubeBrush, DefaultMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 72, -WorldHeight / 2 ) );

		GenerateRandomWorld();
	}

	public void SubtractDefault( Vector3 min, Vector3 max )
	{
		CsgWorld.Subtract( CoolBrush, (min + max) * 0.5f, max - min );
		CsgWorld.Paint( CoolBrush, DefaultMaterial, (min + max) * 0.5f, (max - min) * 1.1f );
	}

	private float[,] _terrainGrid;
	private readonly int _resolution = 16;

	public void GenerateRandomWorld()
	{
		var pointsX = (WorldLength / _resolution).CeilToInt();
		var pointsZ = (WorldHeight / _resolution).CeilToInt();

		_terrainGrid = new float[pointsX, pointsZ];

		var r = Game.Random.Int( 99999 );

		// Initialize Simplex array of noise.
		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var z = 0; z < pointsZ; z++ )
			{
				var n = Noise.Perlin( (x + r) * _zoom, r, (z + r) * _zoom );
				n = Math.Abs( (n * 2) - 1 );
				_terrainGrid[x, z] = n;

				// Subtract from the solid where the noise is under a certain threshold.
				if ( _terrainGrid[x, z] < 0.1f )
				{
					// Pad the subtraction so the subtraction is more clean.
					var paddedRes = _resolution + (_resolution * 0.75f);

					var min = new Vector3( (x * _resolution) - paddedRes, -32, (z * _resolution) - paddedRes );
					var max = new Vector3( (x * _resolution) + paddedRes, 32, (z * _resolution) + paddedRes );

					// Offset by position.
					min -= new Vector3( WorldLength / 2, 0, WorldHeight );
					max -= new Vector3( WorldLength / 2, 0, WorldHeight );
					SubtractDefault( min, max );
				}
			}
		}
	}

	[ConCmd.Admin( "gr_regen" )]
	public static void RegenWorld()
	{
		var game = GrubsGame.Instance;
		var world = game.World;

		world.Reset();
	}
}
