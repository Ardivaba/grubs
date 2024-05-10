﻿using Grubs.Extensions;
using Grubs.Gamemodes;
using Grubs.UI;

namespace Grubs.Pawn;

[Title( "Grubs - Player" ), Category( "Grubs" )]
public sealed class Player : Component
{
	public bool IsActive => Gamemode.FFA?.ActivePlayerId == Id;
	public bool ShouldHaveTurn => GameObject.IsValid() && !IsDead();
	public Grub ActiveGrub { get; set; }

	public bool HasFiredThisTurn { get; set; }

	[Sync] public string SelectedColor { get; set; } = "";

	[Sync] public Guid ActiveGrubId { get; set; }

	[Sync] public NetList<Guid> Grubs { get; set; } = new();

	[Property] public required GameObject GrubPrefab { get; set; }

	[Property] public required PlayerInventory Inventory { get; set; }

	protected override void OnStart()
	{
		SelectedColor = Color.Random.Hex;
	}

	public void EndTurn()
	{
		Inventory.Holster( Inventory.ActiveSlot );
		HasFiredThisTurn = false;
	}

	public IEnumerable<Grub> GetOwnedGrubs()
	{
		return Scene.GetAllComponents<Grub>()
			.Where( g => g.IsValid && g.Network.OwnerConnection == Network.OwnerConnection );
	}

	public bool IsDead()
	{
		return GetOwnedGrubs().All( g => g.IsDead );
	}
}
