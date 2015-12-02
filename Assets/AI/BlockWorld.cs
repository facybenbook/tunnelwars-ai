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
			if (world.Ground[i1, j] || world.Ground[i2, j]) {
				break;
			}

			y += World.BlockSize;
		}
	}
}

// The main class
public class BlockWorld {

	// The World's ground arrangement
	public bool[,] Ground{ get; set; }

	// The AI player
	public BlockPlayer Player { get; set; }

	// The powerups
	List<BlockPowerup> Powerups { get; set; }
	
	// A reference to the danger zone
	DangerZone enemyDangerZone;

	// Checks ground at an i, j coordinate
	public bool CheckGround(int i, int j) {

		// TODO: This can be 10 times faster
		float x = i * World.BlockSize + 0.01f;
		float y = j * World.BlockSize + World.FloorLevel + 0.01f;
		if (x <= 0.0f || x >= 2944.0f) return true;
		if (x >= 1408.0f && x <= 1536.0f && y <= 1152.0f) return true;
		if (y < World.FloorLevel) return false;
		if (y > 1920.0f) return true;
		float relX = x / World.BlockSize;
		float relY = (y - World.FloorLevel) / World.BlockSize;
		int xIndex = Mathf.FloorToInt(relX);
		int yIndex = Mathf.FloorToInt(relY);
		if (xIndex < 0 || xIndex >= World.BlocksWidth) return true;
		if (yIndex >= World.BlocksHeight) return true;
		return Ground[xIndex, yIndex];
	}

	// A method checking whether a position is supported - meaning it can serve
	// as a jumping-off point
	public bool CheckPositionSupported(int i, int j) {

		// Ground below means good to jump
		if (CheckGround(i, j + 1)) return true;
		if (CheckGround(i + 1, j + 1) || CheckGround(i - 1, j + 1)) return true;

		// Ground to the sides means good to jump
		if (CheckGround(i + 1, j) || CheckGround(i - 1, j)) return true;

		return false;
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