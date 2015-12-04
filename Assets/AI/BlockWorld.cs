/*
 * BlockWorld.cs
 * 
 * The BlockWorld class is a World representation that includes only the ground, powerups,
 * and a single simplified Player. BlockWorld's are used in the level 2 A* pathfinding search.
 * An important addition is the static calculation of the "danger zone" of the opponent
 * player, on construction, which factors into the search cost function. This danger zone
 * functionally replaces an opponent position.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

// Actions that advance the block world
public enum BlockWorldAction {
	Up,
	Right,
	Down,
	Left
}

// I-J coordinates struct
public struct IJCoords {
	
	public int I { get { return i; } }
	public int J { get { return j; } }
	
	public IJCoords(int i, int j) {
		this.i = i;
		this.j = j;
	}
	
	int i;
	int j;
}

public class BlockWorld: WorldWithGround {

	// Make a class for the simplified player
	public class BlockPlayer {
		
		// Position is now stored in i, j coordinates
		public int I { get; set; }
		public int J { get; set; }
		
		// Used for tunneling simulation
		public int Ammo { get; set; }
		public WeaponType Weapon { get; set; }
		
		// Constructor takes a real player to simplify
		public BlockPlayer(World.Player realPlayer) {
			UpdateWithPlayer(realPlayer);
		}
		
		// Updates with a real player
		public void UpdateWithPlayer(World.Player realPlayer) {
			I = World.XToI(realPlayer.X);
			J = World.YToJ(realPlayer.Y);
			Ammo = realPlayer.Ammo;
			Weapon = realPlayer.Weapon;
		}
		
		// Clone
		public BlockPlayer Clone() {
			return (BlockPlayer)this.MemberwiseClone();
		}
		
		// Compare
		public bool PropertiesEqual(BlockPlayer player) {
			
			if (I != player.I) return false;
			if (J != player.J) return false;
			if (Ammo != player.Ammo) return false;
			if (Weapon != player.Weapon) return false;
			return true;
		}
	}
	
	// Make a class for a simplified powerup. Coordinates stored as both types
	public class BlockPowerup {
		
		public float X { get; set; }
		public float Y { get; set; }
		public WeaponType Weapon { get; set; }
		public PowerupType Type { get; set; }
		public bool Falling { get { return isFalling; } }
		
		public float I { get { return World.XToI(X); } }
		public float J { get { return World.YToJ(Y); } }
		
		// Constructor from real powerup
		public BlockPowerup (World.Powerup powerup) {
			UpdateWithPowerup(powerup);
		}
		
		// Update with a real powerup
		public void UpdateWithPowerup(World.Powerup powerup) {
			X = powerup.X;
			Y = powerup.Y;
			Weapon = powerup.Weapon;
			Type = powerup.Type;
			isFalling = powerup.VSpeed > 0.0f;
		}
		
		// Clone
		public BlockPowerup Clone() {
			return (BlockPowerup)this.MemberwiseClone();
		}
		
		// Compare
		public bool PropertiesEqual(BlockPowerup powerup) {
			
			if (X != powerup.X) return false;
			if (Y != powerup.Y) return false;
			if (Weapon != powerup.Weapon) return false;
			if (Type != powerup.Type) return false;
			if (isFalling != powerup.isFalling) return false;
			return true;
		}
		
		// Projects the powerup position downwards to rest on the ground
		public void ProjectDownwards(BlockWorld world) {
			
			// Don't project downwards if not falling anymore!
			if (!isFalling) return;
			
			// Speed and gravity fall
			if (Type != PowerupType.Speed && Type != PowerupType.Gravity) return;
			
			// Get nearest two horizontal indices
			int i1 = World.XToI(X - 64.0f); // HACK: Powerup size better not change
			int i2 = i1 + 1;
			
			// Increase y until hit ground
			float y = World.FloorLevel;
			for (int j = 0; j < World.BlocksHeight; j++) {
				if (world.CheckGroundByIndex(i1, j) || world.CheckGroundByIndex(i2, j)) {
					break;
				}
				
				y += World.BlockSize;
			}
		}
		
		bool isFalling;
	}

	// Whether to exclude speed/gravity powerups from the block world
	const bool excludeSpeedGrav = true;

	// The AI player
	public BlockPlayer Player { get; set; }

	// The powerups
	public List<BlockPowerup> Powerups { get; set; }

	// Whether ammo was just collected
	public bool JustCollectedAmmo { get { return justCollectedAmmo; } }
	
	// Constructors
	public BlockWorld() {}
	public BlockWorld(int playerNum, World world) {

		ground = new bool[World.BlocksWidth, World.BlocksHeight];
		for (int i = 0; i < World.BlocksWidth; i++) {
			for (int j = 0; j < World.BlocksHeight; j++) {
				ground[i, j] = world.CheckGroundByIndex(i, j);
			}
		}

		justCollectedAmmo = false;

		// Turn player to BlockPlayer
		Player = new BlockPlayer(playerNum == 1 ? world.Player1 : world.Player2);

		// Turn powerups to BlockPowerups
		Powerups = new List<BlockPowerup>();
		for (int i = 0; i < world.NumPowerups; i++) {

			World.Powerup powerup = world.GetPowerup(i);

			// Conditionally exclude speed and gravity
			if (excludeSpeedGrav && powerup.Type == PowerupType.Gravity || 
			    powerup.Type == PowerupType.Speed) continue;

			BlockPowerup blockPowerup = new BlockPowerup(powerup);
			blockPowerup.ProjectDownwards(this);
			Powerups.Add(blockPowerup);
		}

	}

	// Clone
	public BlockWorld Clone() {

		BlockWorld world = new BlockWorld();

		world.Player = Player.Clone();
		world.Powerups = new List<BlockPowerup>();
		foreach (BlockPowerup powerup in Powerups) {
			world.Powerups.Add(powerup.Clone());
		}
		world.ground = (bool[,])ground.Clone();
		world.justCollectedAmmo = justCollectedAmmo;

		// Any other members should go here...

		return world;
	}

	// Compares equality of two block worlds
	public bool PropertiesEqual(BlockWorld blockWorld) {

		// Compare ground
		for (int i = 0; i < World.BlocksWidth; i++) {
			for (int j = 0; j < World.BlocksHeight; j++) {
				if (ground[i, j] != blockWorld.ground[i, j]) return false;
			}
		}

		if (justCollectedAmmo != blockWorld.justCollectedAmmo) return false;

		// Compare player
		if (!Player.PropertiesEqual(blockWorld.Player)) return false;

		// Compare powerups
		for (int i = 0; i < Powerups.Count; i++) {
			if (!Powerups[i].PropertiesEqual(blockWorld.Powerups[i])) return false;
		}

		return true;
	}

	// A method checking whether a position is supported - meaning it can serve
	// as a jumping-off point
	public bool CheckPositionSupported(int i, int j) {

		// Ground below means good to jump
		if (CheckGroundByIndex(i, j + 1)) return true;
		if (CheckGroundByIndex(i + 1, j + 1) || CheckGroundByIndex(i - 1, j + 1)) return true;

		// Ground to the sides means good to jump
		if (CheckGroundByIndex(i + 1, j) || CheckGroundByIndex(i - 1, j)) return true;

		return false;
	}

	// Allow mutability of ground
	public void SetGroundByIndex(int i, int j, bool val) {
		setGroundByIndex(i, j, val);
	}

	// Checks for ammo at a certain position and returns its type. Returns type None otherwise
	// NOTE: Fails to recognize multiple ammos in same area
	public WeaponType CheckAmmo(int i, int j) {

		foreach (BlockPowerup powerup in Powerups) {
			if (powerup.I == i && powerup.J == j && powerup.Type != PowerupType.Gravity
			    && powerup.Type != PowerupType.Speed) {

				return powerup.Weapon;
			}
		}

		return WeaponType.None;
	}

	// Action model - check if action is applicable
	public bool CheckActionApplicable(BlockWorldAction action) {

		int i = ActionToI(action);
		int j = ActionToJ(action);

		bool ground = CheckGroundByIndex(i, j);
		bool immutable = CheckGroundImmutableByIndex(i, j);
		bool currentSupported = CheckPositionSupported(Player.I, Player.J);

		if (immutable) return false;

		// Only certain times can the player move through ground
		if (ground) {
			if (Player.Ammo == 0) return false;

			// Lightning is only way to move upwards through ground
			if (action == BlockWorldAction.Up) {
				return currentSupported && Player.Weapon == WeaponType.Lightning;
			}

			// Rockets go to the sides
			if (action == BlockWorldAction.Left || action == BlockWorldAction.Right) {
				return currentSupported && Player.Weapon == WeaponType.Rockets;
			}

			// Bombs go down
			if (action == BlockWorldAction.Down) {
				return Player.Weapon == WeaponType.Bombs;
			}

			return false;

			// TODO: Minions cannot be used to determine a definite path. How to handle this?
		} else {

			// Ground support is the only influence
			if (action != BlockWorldAction.Down && !currentSupported) return false;
			return true;
			
		}
	}

	// Return all applicable actions
	public List<BlockWorldAction> ApplicableActions() {

		List<BlockWorldAction> list = Enum.GetValues(typeof(BlockWorldAction)).Cast<BlockWorldAction>().ToList();

		for (int i = list.Count - 1; i >= 0; i--) {
			if (!CheckActionApplicable(list[i])) list.RemoveAt(i);
		}

		return list;
	}

	// Advance with a single action
	public void Advance(BlockWorldAction action, bool collectAmmo=true) {

		// Checks action applicability
		if (!CheckActionApplicable(action)) return;

		// Move to new location
		Player.I = ActionToI(action);
		Player.J = ActionToJ(action);

		// Collect ammo
		if (collectAmmo) {
			WeaponType weapon = CheckAmmo(Player.I, Player.J);
			if (weapon != WeaponType.None) {
				if (Player.Ammo != -1) Player.Ammo = 3;
				Player.Weapon = weapon;
				justCollectedAmmo = true;
			}
		}

		// Blow out ground if needed and reduce ammo
		if (CheckGroundByIndex(Player.I, Player.J)) {
			SetGroundByIndex(Player.I, Player.J, false);
			Player.Ammo--;
			if (Player.Ammo == 0) Player.Weapon = WeaponType.None;
		}
	}

	// Gets I, J coordinates of new player position for an action
	public int ActionToI(BlockWorldAction action) {
		if (action == BlockWorldAction.Left) return Player.I - 1;
		else if (action == BlockWorldAction.Right) return Player.I + 1;
		return Player.I;
	}
	public int ActionToJ(BlockWorldAction action) {
		if (action == BlockWorldAction.Up) return Player.J - 1;
		else if (action == BlockWorldAction.Down) return Player.J + 1;
		return Player.I;
	}

	bool justCollectedAmmo;
}