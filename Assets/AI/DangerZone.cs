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
	public const float LightningDangerWeight = 1.5f;
	public const float RocketsDangerWeight = 1.0f;
	public const float BombsDangerWeight = 1.0f;
	public const float MinionsDangerWeight = 1.0f;

	// Number of block steps to simulate in initial source distribution calculation
	public const int DistributionSteps = 5;

	// A player's danger zone must be calculated from a World and a blockWorld
	public DangerZone(int playerNum, World world, BlockWorld blockWorld) {

		// Init danger array
		dangerZoneArray = new float[World.BlocksWidth, dangerZoneHeight];

		// Init to zero
		for (int i = 0; i < World.BlocksWidth; i++) {
			for (int j = 0; j < dangerZoneHeight; j++) {
				dangerZoneArray[i, j] = 0.0f;
			}
		}

		// Store block world as a copy for modification
		this.blockWorld = blockWorld.Clone();

		// Uses exact filtering to compute a belief distribution for player position
		// and weapon type/ammo
		sourceBeliefs = new Dictionary<IJCoords, ProjectileSourceBelief>();
		computeSourceBeliefs(playerNum, world);

		// For each source belief computed by exact filtering, run the trajectory
		runTrajectories();
	}

	// Gets danger zone readings for a point at i,j coordinates
	public float CheckDanger(int i, int j) {
		return dangerZoneArray[i, j + World.FloorLevelJ];
	}

	

	// The probability distribution of initial weapon sources / player configurations
	Dictionary<IJCoords, ProjectileSourceBelief> sourceBeliefs;

	// The block world
	BlockWorld blockWorld;

	// The danger zone array itself
	const int dangerZoneHeight = World.FloorLevelJ + World.BlocksHeight;
	float[,] dangerZoneArray;

	// Sets the danger array using I, J coords
	void setDanger(int i, int j, float val) {
		dangerZoneArray[i, j + World.FloorLevelJ] = val;
	}
	void addDanger(int i, int j, float delta) {
		dangerZoneArray[i, j + World.FloorLevelJ] += delta;
	}

	// Adds a belief to a dictionary given an old belief, derived chance, and block world
	void addBeliefUsingWorld(Dictionary<IJCoords, ProjectileSourceBelief> dict, int newI, int newJ,
	                         ProjectileSourceBelief oldBelief, float derivedChance, ref float c) {

		ProjectileSourceBelief derivedBelief = oldBelief.Clone();
		derivedBelief.Probability = derivedChance;

		// Check for ammo in new position
		WeaponType ammoTypeCheckResult = blockWorld.CheckAmmo(newI, newJ);
		if (ammoTypeCheckResult != WeaponType.None) {
			derivedBelief.Ammo = 3; // Lightning doesn't matter
			derivedBelief.PossibleWeapons.Add(ammoTypeCheckResult);
		}

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
	void computeSourceBeliefs(int playerNum, World world) {

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
				bool stayPossible = false;
				
				int numPossibleDirections = 0;
				bool supported = blockWorld.CheckPositionSupported(i, j);
				if (!supported) {
					
					// Can only move down from unsupported positions
					numPossibleDirections++;
					downPossible = true;
					
				} else {

					stayPossible = true;
					numPossibleDirections++;

					if (!blockWorld.CheckGroundByIndex(i, j + 1)) {
						downPossible = true;
						numPossibleDirections++;
					}
					if (!blockWorld.CheckGroundByIndex(i + 1, j)) {
						leftPossible = true;
						numPossibleDirections++;
					}
					if (!blockWorld.CheckGroundByIndex(i - 1, j)) {
						rightPossible = true;
						numPossibleDirections++;
					}
					if (!blockWorld.CheckGroundByIndex(i, j - 1)) {
						upPossible = true;
						numPossibleDirections++;
					}
				}
				
				// Compute chance of each direction - uniform
				float chance = 1.0f / numPossibleDirections;
				float derivedChance = chance * prior;
				
				// Update beliefs based on possible directions
				
				// Staying in place is always an option
				if (stayPossible) {
					addBeliefUsingWorld(newBeliefs, i, j, belief, derivedChance, ref newBeliefsTotal);
				}
				
				if (upPossible) {
					addBeliefUsingWorld(newBeliefs, i, j - 1, belief, derivedChance, ref newBeliefsTotal);
				}
				if (downPossible) {
						addBeliefUsingWorld(newBeliefs, i, j + 1, belief, derivedChance, ref newBeliefsTotal);
				}
				if (leftPossible) {
						addBeliefUsingWorld(newBeliefs, i + 1, j, belief, derivedChance, ref newBeliefsTotal);
				}
				if (rightPossible) {
						addBeliefUsingWorld(newBeliefs, i - 1, j, belief, derivedChance, ref newBeliefsTotal);
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
			PossibleWeapons = new HashSet<WeaponType>();
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

	// Runs trajectories of projectile source beliefs
	void runTrajectories() {

		// Iterate through each possibility
		foreach (KeyValuePair<IJCoords, ProjectileSourceBelief> entry in sourceBeliefs) {

			ProjectileSourceBelief sourceBelief = entry.Value;
			int sourceI = entry.Key.I;
			int sourceJ = entry.Key.J;

			int sourceAmmo = sourceBelief.Ammo;
			float sourceProbability = sourceBelief.Probability;

			// Iterate through each weapon type for each possibility
			foreach (WeaponType weaponType in sourceBelief.PossibleWeapons) {

				if (weaponType == WeaponType.Lightning) {

					// Add danger to all above for lightning - easy
					for (int j = -World.FloorLevelJ; j < World.BlocksHeight; j++) {

						if (j < sourceJ) {
							addDanger(sourceI, j, sourceProbability * LightningDangerWeight);
						}
					}

				} else {
					// Recursively add values to the danger zone 2D array

					// Use same blockworld for both directions since paths won't cross
					BlockWorld newBlockWorld = blockWorld.Clone();
					addDangerToBlockAndNeighbors(sourceI, sourceJ, weaponType, sourceProbability, sourceAmmo, true, newBlockWorld);
					addDangerToBlockAndNeighbors(sourceI, sourceJ, weaponType, sourceProbability, sourceAmmo, false, newBlockWorld);
				}
			}
		}
	}

	// Probabilities below this are ignored
	const float epsilon = 0.01f;

	// Reduce probability with more grounds for performance
	const float groundBlowoutFactor = 1.0f;

	// Recursively add danger to the 2D array 
	void addDangerToBlockAndNeighbors(int i, int j, WeaponType type, float probability, int ammo,
	                                  bool facingRight, BlockWorld blockWorld, bool isFalling=false) {

		// Base cases
		if (probability < epsilon) return;
		if (ammo == 0) return; // Master mode has negative ammo - do not return

		// Blow out ground if there is any at the current position. If there is 
		if (blockWorld.CheckGroundByIndex(i, j)) {

			// Another base case for immutable ground
			bool immutable = blockWorld.CheckGroundImmutableByIndex(i, j);
			if (immutable) return; // End of the line

			// Blow out ground otherwise
			blockWorld.SetGroundByIndex(i, j, false);
			addDangerToBlockAndNeighbors(i, j, type, probability * groundBlowoutFactor, ammo - 1,
			                             facingRight, blockWorld);
			return;

		} else {

			int normalized = facingRight ? -1 : 1;

			switch (type) {
			case WeaponType.Rockets: {

				addDanger(i, j, probability * RocketsDangerWeight);
				addDangerToBlockAndNeighbors(i + normalized, j, type, probability, ammo,
				                             facingRight, blockWorld);
				break;
			}
			
			case WeaponType.Bombs: {
				addDanger(i, j, probability * BombsDangerWeight);
				addDangerToBlockAndNeighbors(i, j + 1, type, probability, ammo, facingRight, blockWorld);
				break;
			}

			case WeaponType.Minions: {
				addDanger(i, j, probability * MinionsDangerWeight);

				bool groundDown = blockWorld.CheckGroundByIndex(i, j + 1);
				bool groundRight = blockWorld.CheckGroundByIndex(i + normalized, j);
				bool goRight = false;
				bool goDown = false;

				// Do natural motion - right first unless hasn't fallen
				if (!groundRight && !groundDown) {

					if (isFalling && blockWorld.CheckGroundByIndex(i + normalized, j + 1)) {
						goRight = true;
					} else {
						goDown = true;
					}

				} else if (!groundRight && groundDown) {
					goRight = true;
				} else if (groundRight && !groundDown) {
					goDown = true;
				} else {

					// Block worlds diverge, so make a new one
					BlockWorld newBlockWorld = blockWorld.Clone();

					// Go both directions
					addDangerToBlockAndNeighbors(i + normalized, j, type, probability / 2.0f,
					                             ammo, facingRight, blockWorld, false);
					addDangerToBlockAndNeighbors(i, j + 1, type, probability / 2.0f,
					                             ammo, facingRight, newBlockWorld, true);
				}
					
				// Do sole motions
				if (goRight) {
					addDangerToBlockAndNeighbors(i + normalized, j, type, probability,
					                             ammo, facingRight, blockWorld, false);
					return;
				}
				if (goDown) {
					addDangerToBlockAndNeighbors(i, j + 1, type, probability,
					                             ammo, facingRight, blockWorld, true);
					return;
				}

				break;
			}

			}
		}
	}

	// Renders the danger zone
	public void Render(Game resourceScript) {

		for (int i = 0; i < World.BlocksWidth; i++) {

			for (int j = 0; j < dangerZoneHeight; j++) {

				if (dangerZoneArray[i, j] == 0) continue;
				GameObject obj = Object.Instantiate(resourceScript.Protodanger);
				obj.transform.position = new Vector3(i * World.BlockSize, j * World.BlockSize);
				SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
				renderer.color = new Color(1.0f, 1.0f, 1.0f, dangerZoneArray[i, j] / 2.0f);
			}
		}
	}

	// Renders the player beliefs
	public void RenderPlayerBeliefs(Game resourceScript) {

		foreach (KeyValuePair<IJCoords, ProjectileSourceBelief> entry in sourceBeliefs) {

			//if (!entry.Value.PossibleWeapons.Contains(WeaponType.Rockets) || entry.Value.Ammo != 3) continue;

			GameObject obj = Object.Instantiate(resourceScript.Protobelief);
			obj.transform.position = new Vector3(entry.Key.I * World.BlockSize, entry.Key.J * World.BlockSize + World.FloorLevel);
			SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
			renderer.color = new Color(1.0f, 1.0f, 1.0f, entry.Value.Probability);
		}
	}

}