﻿using Grubs.Terrain;
using Sandbox.Sdf;

namespace Grubs.Terrain;

public partial class GrubsTerrain
{
	void SetupWorldFromTexture()
	{
		DoTextureLoad();
	}

	async void DoTextureLoad()
	{
		var mapSdfTexture = await Texture.LoadAsync( FileSystem.Mounted, "textures/texturelevels/" + GrubsConfig.WorldTerrainTexture.ToString() + ".png" );
		WorldTextureHeight = mapSdfTexture.Height * 2;
		WorldTextureLength = mapSdfTexture.Width * 2;

		GrubsConfig.TerrainLength = mapSdfTexture.Width * 2;
		GrubsConfig.TerrainHeight = mapSdfTexture.Height * 2;

		var mapSdf = new TextureSdf( mapSdfTexture, 10, mapSdfTexture.Width * 2f, pivot: 0f );
		var transformedSdf = mapSdf.Transform( new Vector2( -GrubsConfig.TerrainLength / 2f, 0 ) );

		var cfg = new MaterialsConfig( true, true );
		var materials = GetActiveMaterials( cfg );

		SdfWorld.Add( transformedSdf, materials.ElementAt( 0 ).Key );

		mapSdfTexture = await Texture.LoadAsync( FileSystem.Mounted, "textures/texturelevels/" + GrubsConfig.WorldTerrainTexture.ToString() + "_back.png" );
		mapSdf = new TextureSdf( mapSdfTexture, 10, mapSdfTexture.Width * 2f, pivot: 0f );
		transformedSdf = mapSdf.Transform( new Vector2( -GrubsConfig.TerrainLength / 2f, 0 ) );

		SdfWorld.Add( transformedSdf, materials.ElementAt( 1 ).Key );

	}
}
