/*
 * SimplifiedWorld.cs
 * 
 * The SimplifiedWorld class is a simplified world used for Q learning. It ideally should encapsulate
 * all of the most important information required to choose a strategy.
 *
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// Specifies how positively or negatively close an enemy is to the player
public enum XCloseness {
	Near,
	Medium,
	Far,
	WallBetween
}

// Specifies closeness with direction for vertical axis
public enum YCloseness {
	PosNear,
	PosMedium,
	PosFar,
	NegNear,
	NegMedium,
	NegFar,
	WallBetween
}

// Class which takes in a world and creates a simplified state
public class SimplifiedWorld {

	// An added reward of winning or penalty of losing (otherwise reward is basically delta health)
	const float WinningReward = 50.0f;

	// Player's weapon type
	public WeaponType Weapon { get; set; }

	// Player's ammo
	public int AmmoAmount { get; set; }

	// Enemy weapon type
	public WeaponType EnemyWeapon { get; set; }

	// Enemy ammo
	public int EnemyAmmoAmount { get; set; }

	// X closeness to enemy
	public XCloseness XDistanceToEnemy { get; set; }

	// Y closeness to enemy
	public YCloseness YDistanceToEnemy { get; set; }

	// Constructor method that simplifies a world into a state
	public SimplifiedWorld (World world, int playerNum) {

		World.Player player;
		World.Player enemy;

		// Set the player and enemy
		if (playerNum == 1) {
			player = world.Player1;
			enemy = world.Player2;
		} else {
			player = world.Player2;
			enemy = world.Player1;
		}

		// Set the player and enemy weapons and ammo amounts
		if (player.Ammo == 0) {
			Weapon = WeaponType.None;
		} else {
			Weapon = player.Weapon;
		}

		if (enemy.Ammo == 0) {
			EnemyWeapon = WeaponType.None;
		} else {
			EnemyWeapon = enemy.Weapon;
		}

		AmmoAmount = player.Ammo;
		EnemyAmmoAmount = enemy.Ammo;

		// Convert these distances into closeness types
		XDistanceToEnemy = HowXClose(world);
		YDistanceToEnemy = HowYClose(world, playerNum);


		// Set the player and enemy health
		health = player.Health;
		enemyHealth = enemy.Health;
	}
	public SimplifiedWorld() {}

	// Returns all possible states
	static public List<SimplifiedWorld> AllPossible () {

		// Create empty list
		List<SimplifiedWorld> stateList = new List<SimplifiedWorld>();

		// Iterate through each property
		WeaponType[] weaponArray = new WeaponType[] {
			WeaponType.None,
			WeaponType.Bombs,
			WeaponType.Rockets,
			WeaponType.Minions,
			WeaponType.Lightning
		};

		int[] ammoArray = new int[]{-1,0,1,2,3};

		XCloseness[] xClosenessArray = new XCloseness[] { 
			XCloseness.Near,
			XCloseness.Medium,
			XCloseness.Far,
			XCloseness.WallBetween
		};

		YCloseness[] yClosenessArray = new YCloseness[] { 
			YCloseness.PosNear,
			YCloseness.PosMedium,
			YCloseness.PosFar,
			YCloseness.NegNear,
			YCloseness.NegMedium,
			YCloseness.NegFar,
			YCloseness.WallBetween
		};

		// weapon
		foreach (WeaponType tempWeapon in weaponArray) {

			// ammoAmount
			foreach (int tempAmmoAmount in ammoArray) {

				// enemyWeapon
				foreach (WeaponType tempEnemyWeapon in weaponArray) {

					// enemyAmmoAmount
					foreach (int tempEnemyAmmoAmount in ammoArray) {

						// xDistanceToEnemy
						foreach (XCloseness tempXDistanceToEnemy in xClosenessArray) {

							// yDistanceToEnemy
							foreach (YCloseness tempYDistanceToEnemy in yClosenessArray) {

								// Create new state class with the above properties
								SimplifiedWorld newState = new SimplifiedWorld ();
								newState.Weapon = tempWeapon;
								newState.AmmoAmount = tempAmmoAmount;
								newState.EnemyWeapon = tempEnemyWeapon;
								newState.EnemyAmmoAmount = tempEnemyAmmoAmount;
								newState.XDistanceToEnemy = tempXDistanceToEnemy;
								newState.YDistanceToEnemy = tempYDistanceToEnemy;

								// Add newState to stateList
								stateList.Add(newState);
							}
						}
					}
				}
			}
		}

		return stateList;
	}

	// Returns horizontal closeness between the players
	public XCloseness HowXClose (World world) {

		// Calculate the x distance to the enemy
		float xDist = Mathf.Abs(world.Player1.X - world.Player2.X);

		// Check if there is ground between the players
		if (isGroundBetween(world.Player1.X, world.Player1.Y, world.Player2.X, world.Player2.Y, world)) {
			return XCloseness.WallBetween;
		}

		// Categorize the horizontal distance
		float blockUnit = World.BlockSize;
		if (xDist < blockUnit * 3.0f) {
			return XCloseness.Near;
		}
		if (xDist < blockUnit * 7.0f) {
			return XCloseness.Medium;
		}
		return XCloseness.Far;
	}
	
	// Method which takes in a world and playerNum and returns the XCloseness of the player and the enemy
	public YCloseness HowYClose (World world, int playerNum) {

		World.Player player = playerNum == 1 ? world.Player1 : world.Player2;
		World.Player other = playerNum == 1 ? world.Player2 : world.Player1;

		float dY = player.Y - other.Y;

		if (isGroundBetween(player.X, player.Y, other.X, other.Y, world)) {
			return YCloseness.WallBetween;
		}

		float blockUnit = World.BlockSize;

		if (dY < -10.0f * blockUnit) {
			return YCloseness.NegFar;
		}
		if (dY < -1.0f * blockUnit) {
			return YCloseness.NegMedium;
		}
		if (dY < 0.0f * blockUnit) {
			return YCloseness.NegNear;
		}
		if (dY < 1.0f * blockUnit) {
			return YCloseness.PosNear;
		}
		if (dY < 10.0f * blockUnit) {
			return YCloseness.PosMedium;
		}
		return YCloseness.PosFar;
	}

	public override string ToString () {

		string stateString = "";

		// Players Weapon Type
		stateString = stateString + Weapon.ToString ();
		stateString = stateString + " ";
		
		// Players Ammo
		stateString = stateString + AmmoAmount.ToString ();
		stateString = stateString + " ";
		
		// Enemy Weapon Type
		stateString = stateString + EnemyWeapon.ToString ();
		stateString = stateString + " ";
		
		// Enemy Ammo
		stateString = stateString + EnemyAmmoAmount.ToString ();
		stateString = stateString + " ";

		// XCloseness to Enemy
		stateString = stateString + XDistanceToEnemy.ToString ();
		stateString = stateString + " ";
		
		// YCloseness to Enemy
		stateString = stateString + YDistanceToEnemy.ToString ();

		return stateString;
	}

	static public SimplifiedWorld FromString (string stateString) {

		SimplifiedWorld state = new SimplifiedWorld ();

		string[] propertyArray = stateString.Split (' ');

		for (int i = 0; i < propertyArray.Count(); i++) {
			if (i == 0) {
				state.Weapon = (WeaponType) Enum.Parse (typeof(WeaponType), propertyArray [i]);
			} else if (i == 1) {
				state.AmmoAmount = int.Parse (propertyArray [i]);
			} else if (i == 2) {
				state.EnemyWeapon = (WeaponType) Enum.Parse (typeof(WeaponType), propertyArray [i]);
			} else if (i == 3) {
				state.EnemyAmmoAmount = int.Parse (propertyArray [i]);
			} else if (i == 4) {
				state.XDistanceToEnemy = (XCloseness) Enum.Parse (typeof(XCloseness), propertyArray [i]);
			} else if (i == 5) {
				state.YDistanceToEnemy = (YCloseness) Enum.Parse (typeof(YCloseness), propertyArray [i]);
			}
		}

		return state;
	}

	// Returns a bool on whether or not input state is the same as this state
	public bool IsEquivalent (SimplifiedWorld comparisonState) {

		return comparisonState.AmmoAmount == this.AmmoAmount
			&& comparisonState.EnemyAmmoAmount == this.EnemyAmmoAmount
				&& comparisonState.EnemyWeapon == this.EnemyWeapon
				&& comparisonState.Weapon == this.Weapon
				&& comparisonState.XDistanceToEnemy == this.XDistanceToEnemy
				&& comparisonState.YDistanceToEnemy == this.YDistanceToEnemy;
	}

	// The reward function between States
	public static float Reward (SimplifiedWorld initState, StrategyType strategy, SimplifiedWorld resultState) {

		// If you've won, then you get huge reward
		float winningBonus = 0.0f;
		if (resultState.health > 0.0f && resultState.enemyHealth <= 0.0f) winningBonus = WinningReward;
		if (resultState.health > 0.0f && resultState.enemyHealth <= 0.0f) winningBonus = -WinningReward;

		// Hold on to your health
		return resultState.health - resultState.enemyHealth + winningBonus;
	}



	float health;
	float enemyHealth;

	// Checks if there is ground on the line between the two players
	bool isGroundBetween(float p1x, float p1y, float p2x, float p2y, World world) {

		float slope = (p2y - p1y) / (p2x - p1x);

		float y = p1y;
		for (float x = p1x; x <= p2x; x += World.BlockSize) {
			y += slope * World.BlockSize;
			if (world.CheckGround(x, y)) return true;
		}
		return false;
	}
}