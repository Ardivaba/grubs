﻿using Grubs.Player;

namespace Grubs.Equipment;

[Title( "Grubs - Equipment" ), Category( "Equipment" )]
public class EquipmentComponent : Component
{
	[Property] public required SkinnedModelRenderer Model { get; set; }
	[Property] public HoldPose HoldPose { get; set; } = HoldPose.None;

	[Property, Sync] public int SlotIndex { get; set; }

	/// <summary>
	/// This bool exists to break out of the component lifecycle, as it is behaving weirdly.
	/// </summary>
	public bool ShouldShow { get; set; }

	public Grub? Grub { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		Holster();
	}

	protected override void OnUpdate()
	{
		UpdateVisibility();
	}

	private void UpdateVisibility()
	{
		if ( Grub is null )
			return;

		var show = Grub.PlayerController.ShouldShowWeapon() && ShouldShow;
		Model.Enabled = show;
	}

	public void Deploy( Grub grub )
	{
		Log.Info( $"Deploying {GameObject.Name} for {grub.Name}" );

		Grub = grub;

		var target = grub.GameObject.GetAllObjects( true ).First( c => c.Name == "hold_L" );
		GameObject.SetParent( target, false );
		Model.Enabled = true;
		ShouldShow = true;
	}

	public void Holster()
	{
		Log.Info( $"Holstering {GameObject.Name} for {Grub?.Name}" );

		Model.BoneMergeTarget = null;
		Model.Enabled = false;
		ShouldShow = false;
	}
}
