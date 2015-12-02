/*
 * DangerZone.cs
 * 
 * The DangerZone class accompanies a BlockWorld, but is immutable. It is an array
 * of floats describing how dangerous each position of the world is.
 * 
 * The DangerZone algorithm starts by calculating an initial distribution cloud
 * around the player, which represents possible weapon-firing sources.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DangerZone {

	// A number of heuristic estimations must be made in computing a danger zone:

	// Number of block steps to simulate in initial source distribution calculation
	public const int DistributionSteps = 3;

	// A player's danger zone must be calculated from a World and a blockWorld
	public DangerZone(int playerNum, World world, BlockWorld blockWorld) {

		sourceBeliefs = new Dictionary<IJCoords, ProjectileSourceBelief>();

		// Uses exact filtering to compute a belief distribution for player position
		// and weapon type/ammo
		computeSourceBeliefs(playerNum, world, blockWorld);


	}

	

	// The probability distribution of initial weapon sources / player configurations
	Dictionary<IJCoords, ProjectileSourceBelief> sourceBeliefs;

	// Adds a belief to a dictionary given an old belief, derived chance, and block world
	void addBeliefUsingWorld(Dictionary<IJCoords, ProjectileSourceBelief> dict, int newI, int newJ,
	                         ProjectileSourceBelief oldBelief, float derivedChance, ref float c, BlockWorld blockWorld) {
		int newAmmo = oldBelief.Ammo;
		WeaponType newAmmoType = WeaponType.None;
		
		// Check for ammo in new position
		WeaponType ammoTypeCheckResult = blockWorld.CheckAmmo(newI, newJ);
		if (ammoTypeCheckResult != WeaponType.None) {
			newAmmo = 3; // Lightning doesn't matter
			newAmmoType = ammoTypeCheckResult;
		}
		ProjectileSourceBelief derivedBelief = new ProjectileSourceBelief(derivedChance,
		                                                                  newAmmo, newAmmoType);
		addBelief(dict, newI, newJ, derivedBelief, ref c);
	}

	// Adds a belief to a dictionary modifying in place, also adding prob. to counter c
	void addBelief(Dictionary<IJCoords, ProjectileSourceBelief> dict, int i, int j,
	               ProjectileSourceBelief belief, ref float c) {
		IJCoords key = new IJCoords(i, j);
		if (dict.ContainsKey(key)) {

			ProjectileSourceBelief oldBelief = dict[key];
			oldBelief.Probability += belief.Probability;
			oldBelief.PossibleWeapons.UnionWith(belief.PossibleWeapons);
			oldBelief.Ammo = belief.Ammo > oldBelief.Ammo ? belief.Ammo : oldBelief.Ammo;
		} else {
			dict[key] = belief;
		}

		// Add to counter
		c += belief.Probability;
	}

	// Computes a belief distribution for the projectile sources
	void computeSourceBeliefs(int playerNum, World world, BlockWorld blockWorld) {

		// Find player
		World.Player player = playerNum == 1 ? world.Player1 : world.Player2;
		int playerI = World.XToI(player.X);
		int playerJ = World.YToJ(player.Y);
		
		// The initial source is at the player
		ProjectileSourceBelief initial = new ProjectileSourceBelief(1.0f, player.Ammo, player.Weapon);
		sourceBeliefs.Add(new IJCoords(playerI, playerJ), initial);
		
		// Compute distribution of projectile sources
		for (int iter = 0; iter < DistributionSteps; iter++) {
			
			Dictionary<IJCoords, ProjectileSourceBelief> newBeliefs = new Dictionary<IJCoords, ProjectileSourceBelief>();
			float newBeliefsTotal = 0.0f;
			
			// Advance our belief state, except keep the old one around for efficiency
			foreach (KeyValuePair<IJCoords, ProjectileSourceBelief> entry in sourceBeliefs) {
				
				ProjectileSourceBelief belief = entry.Value;
				
				int i = entry.Key.I;
				int j = entry.Key.J;
				
				float prior = belief.Probability;
				
				// Determine possible directions
				bool upPossible = false;
				bool leftPossible = false;
				bool rightPossible = false;
				bool downPossible = false;
				
				int numPossibleDirections = 1; // Staying still
				bool supported = blockWorld.CheckPositionSupported(i, j);
				if (!supported) {
					
					// Can only move down from unsupported positions
					numPossibleDirections++;
					downPossible = true;
					
				} else {
					
					if (!blockWorld.CheckGround(i, j + 1)) {
						downPossible = true;
						numPossibleDirections++;
					}
					if (!blockWorld.CheckGround(i + 1, j)) {
						leftPossible = true;
						numPossibleDirections++;
					}
					if (!blockWorld.CheckGround(i - 1, j)) {
						rightPossible = true;
						numPossibleDirections++;
					}
					if (!blockWorld.CheckGround(i, j - 1)) {
						upPossible = true;
						numPossibleDirections++;
					}
				}
				
				// Compute chance of each direction - uniform
				float chance = 1.0f / numPossibleDirections;
				float derivedChance = chance * prior;
				
				// Update beliefs based on possible directions
				
				// Staying in place is always an option
				if (true) {
					ProjectileSourceBelief derivedBelief = new ProjectileSourceBelief(derivedChance,
					                                                                  belief.Ammo,
					                                                                  WeaponType.None);
					addBelief(newBeliefs, i, j, derivedBelief, ref newBeliefsTotal);
				}
				
				if (upPossible) {
					addBeliefUsingWorld(newBeliefs, i, j - 1, belief, derivedChance, ref newBeliefsTotal, blockWorld);
				}
				if (downPossible) {
						addBeliefUsingWorld(newBeliefs, i, j + 1, belief, derivedChance, ref newBeliefsTotal, blockWorld);
				}
				if (leftPossible) {
						addBeliefUsingWorld(newBeliefs, i + 1, j, belief, derivedChance, ref newBeliefsTotal, blockWorld);
				}
				if (rightPossible) {
						addBeliefUsingWorld(newBeliefs, i - 1, j, belief, derivedChance, ref newBeliefsTotal, blockWorld);
				}
			}
			
			// Normalize our new belief state
			foreach (KeyValuePair<IJCoords, ProjectileSourceBelief> entry in newBeliefs) {
				entry.Value.Probability /= newBeliefsTotal;
			}
			
			// Update
			sourceBeliefs = newBeliefs;
		}
	}

	// Internal structure for IJ coordinates
	struct IJCoords {
		
		public int I { get { return i; } }
		public int J { get { return j; } }
		
		public IJCoords(int i, int j) {
			this.i = i;
			this.j = j;
		}
		
		int i;
		int j;
	}
	
	// Internal structure used for a potential weapon source. Is possible player configuration
	// in the above distribution
	class ProjectileSourceBelief {	
		public float Probability { get; set; }
		public int Ammo { get; set; } // Note: Ammo max is taken - approximation
		public HashSet<WeaponType> PossibleWeapons { get; set; }
		
		// Constructor
		public ProjectileSourceBelief(float probability, int ammo, WeaponType initialWeapon) {
			Ammo = ammo;
			Probability = probability;
			PossibleWeapons.Add(initialWeapon);
		}
		
		// Clone
		public ProjectileSourceBelief Clone() {
			ProjectileSourceBelief source = new ProjectileSourceBelief(Probability, Ammo, WeaponType.None);
			HashSet<WeaponType> newPossibleWeapons = new HashSet<WeaponType>(PossibleWeapons);
			source.PossibleWeapons = newPossibleWeapons;
			return source;
		}
	}

}