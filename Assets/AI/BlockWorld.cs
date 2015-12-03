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

// Make a class for the simplified player
public class BlockPlayer {

	// Position is now stored in i, j coordinates
	public int I { get; set; }
	public int J { get; set; }

	// Used for tunneling simulation
	public int Ammo { get; set; }
	public WeaponType Weapon { get; set; }
	public bool IsMaster { get { return IsMaster; } }

	// Constructor takes a real player to simplify
	public BlockPlayer(World.Player realPlayer) {
		I = World.XToI(realPlayer.X);
		J = World.YToJ(realPlayer.Y);
		Ammo = realPlayer.Ammo;
		Weapon = realPlayer.Weapon;
		this.isMaster = realPlayer.IsMaster;
	}

	// Clone
	public BlockPlayer Clone() {
		return (BlockPlayer)this.MemberwiseClone();
	}

	bool isMaster;
}

// Make a class for a simplified powerup. Coordinates stored as both types
public class BlockPowerup {
	
	public float X { get; set; }
	public float Y { get; set; }
	public WeaponType Weapon { get; set; }
	public PowerupType Type { get; set; }

	public float I { get { return World.XToI(X); } }
	public float J { get { return World.YToJ(Y); } }

	// Constructor from real powerup
	public BlockPowerup (World.Powerup powerup) {
		X = powerup.X;
		Y = powerup.Y;
		Weapon = powerup.Weapon;
		Type = powerup.Type;
	}

	// Clone
	public BlockPowerup Clone() {
		return (BlockPowerup)this.MemberwiseClone();
	}

	// Projects the powerup position downwards to rest on the ground
	public void ProjectDownwards(BlockWorld world) {

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
}

// The main class
public class BlockWorld: WorldWithGround {

	// The AI player
	public BlockPlayer Player { get; set; }

	// The powerups
	List<BlockPowerup> Powerups { get; set; }
	
	// Constructors
	public BlockWorld() {}
	public BlockWorld(int playerNum, World world) {

		ground = new bool[World.BlocksWidth, World.BlocksHeight];
		for (int i = 0; i < World.BlocksWidth; i++) {
			for (int j = 0; j < World.BlocksHeight; j++) {
				ground[i, j] = world.CheckGroundByIndex(i, j);
			}
		}

		// Turn player to BlockPlayer
		Player = new BlockPlayer(playerNum == 1 ? world.Player1 : world.Player2);

		// Turn powerups to BlockPowerups
		Powerups = new List<BlockPowerup>();
		for (int i = 0; i < world.NumPowerups; i++) {
			Powerups.Add(new BlockPowerup(world.GetPowerup(i)));
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

		// Any other members should go here...

		return world;
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
}