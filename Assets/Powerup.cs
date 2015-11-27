/*
 * Powerup.cs
 * 
 * The Powerup class manages the game logic powerup movement. This
 * class is defined as an inner class to the World class
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum PowerupType {
	Bombs,
	Rockets,
	Minions,
	Lightning,
	Speed,
	Gravity
}

partial class World : IAdvancing {
	
	public class Powerup : IAdvancing {

		public float X { set; get; }
		public float Y { set; get; }
		public PowerupType Type { set; get; }

		// The weapon that the player gets by collecting the powerup
		public WeaponType Weapon { set; get; }

		public Powerup() {}
		public Powerup(World parent, float x, float y, PowerupType type) {

			init(parent, x, y, type);
		}

		// Spawns a random powerup
		public static void SpawnRandom(World parent) {

			float spawnX, spawnY;
			do {
				spawnX = Random.Range(0.0f, 2880.0f);
				spawnY = Random.Range(0.0f, 640.0f);
			}
			while (spawnX > 1344.0f && spawnX < 1536.0f);

			// Determine powerup type
			List<PowerupType> types = PowerupType.GetValues(typeof(PowerupType)).Cast<PowerupType>().ToList();
			types.Remove(PowerupType.Lightning);
			int randIndex = Random.Range(0, types.Count);
			PowerupType type = types[randIndex];

			// Lightning should be more rare
			int lightningChanceSample = Random.Range(0, 40);
			if (lightningChanceSample == 0) type = PowerupType.Lightning;

			// Create instances
			parent.createPowerup(spawnX, spawnY, type);
		}

		virtual public void Advance(List<WorldAction> actions) {

			// Grav and fast don't fall or collide
			if (Type != PowerupType.Speed && Type != PowerupType.Gravity) {

				if (vSpeed != -1.0f) {

					if (!(world.checkGround(X, Y + 65.0f + vSpeed) ||
					      world.checkGround(X + 64.0f, Y + 65.0f + vSpeed))) {

						Y += vSpeed;
						vSpeed = Mathf.Min(vSpeed + 1.0f, 45.0f);

					} else {
						vSpeed = -1.0f;
						Y = Mathf.Ceil(Y / World.blockSize) * World.blockSize;
					}
				}
			}
		}



		World world;
		float vSpeed;

		// Init
		void init(World parent, float x, float y, PowerupType type) {
			
			X = x;
			Y = y;
			vSpeed = 0.0f;
			world = parent;
			Type = type;

			// Assign weapon type
			Weapon = WeaponType.None;
			switch (type) {
			
			case PowerupType.Bombs:
				Weapon = WeaponType.Bombs;
				break;
			case PowerupType.Rockets:
				Weapon = WeaponType.Rockets;
				break;
			case PowerupType.Minions:
				Weapon = WeaponType.Minions;
				break;
			case PowerupType.Lightning:
				Weapon = WeaponType.Lightning;
				break;
			}
		}

	}
}