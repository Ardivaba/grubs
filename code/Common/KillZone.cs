﻿using Grubs.Helpers;
using Grubs.Pawn;

namespace Grubs.Common;

[Title( "Grubs - Kill Zone" ), Category( "Grubs" )]
public class KillZone : Component, Component.ITriggerListener
{
	[Property] public ParticleSystem KillParticles { get; set; }
	[Property] public SoundEvent KillSound { get; set; }

	public void OnTriggerEnter( Collider other )
	{
		// kidd: Workaround for ArcProjectile being destroyed immediately for non-owner clients,
		// despite the Transform appearing to be fine. Probably an interp bug, but Transform.ClearInterpolation()
		// in ArcProjectile.OnStart() didn't do SHIT.
		// kidd 7/2/24: Added Transform.ClearInterpolation to ArcProjectile.OnStart() because this same bug started popping.
		// It fixed it, but removing this code caused it to start happening on other clients again...
		var otherOwner = other.GameObject.Root.Network.OwnerConnection;
		if ( otherOwner != null && otherOwner != Connection.Local )
			return;

		if ( other.Transform.Position == 0f )
			return;

		CollisionEffects( other.Transform.World );

		if ( other.GameObject.Components.TryGet( out Grub grub, FindMode.EverythingInSelfAndAncestors ) )
		{
			grub.Health.TakeDamage( GrubsDamageInfo.FromKillZone(), true );
		}
		if ( other.GameObject.Components.TryGet( out Mountable mountable, FindMode.EverythingInSelfAndAncestors ) )
		{
			var mountableGrub = mountable.Grub;
			mountable.Dismount();
			mountableGrub.Health.TakeDamage( GrubsDamageInfo.FromKillZone(), true );
		}
		else
		{
			DestroyObjectWithTags( other.GameObject, "projectile", "drop" );
		}
	}

	private void DestroyObjectWithTags( GameObject go, params string[] args )
	{
		foreach ( var tag in args )
		{
			if ( go.Tags.Has( "ignore_killzone" ) )
				continue;

			if ( go.Tags.Has( tag ) && go.Transform.Position != 0f )
			{
				CollisionEffects( go.Transform.World );
				go.Destroy();
			}
		}
	}

	public void OnTriggerExit( Collider other )
	{
	}

	[Broadcast]
	public void CollisionEffects( Transform transform )
	{
		if ( KillSound is not null )
			Sound.Play( KillSound, transform.Position );

		if ( KillParticles is not null )
			ParticleHelper.Instance.PlayInstantaneous( KillParticles, transform );
	}
}
