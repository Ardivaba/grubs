﻿using Grubs.Player;
using Grubs.Utils;
using Grubs.Weapons.Projectiles;

namespace Grubs.Weapons;

public class Grenade : GrubsWeapon
{
	public override string WeaponName => "Grenade";
	public override string ModelPath => "models/weapons/grenade/grenade.vmdl";
	public override string ProjectileModelPath => "models/weapons/grenade/grenade.vmdl";
	public override FiringType FiringType => FiringType.Charged;
	public override HoldPose HoldPose => HoldPose.Throwable;

	protected override void OnFire()
	{
		base.OnFire();

		var segments = new ArcTrace( Parent, Parent.EyePosition )
			.RunTowardsWithBounces( Parent.EyeRotation.Forward.Normal, 0.4f * Charge, 0 );

		new Projectile()
			.WithWorm( Parent as Worm )
			.WithModel( ProjectileModelPath )
			.SetPosition( Position )
			.MoveAlongTrace( segments )
			.WithSpeed( 1000 )
			.WithExplosionRadius( 100 )
			.WithCollisionExplosionDelay( 5f );
	}
}
