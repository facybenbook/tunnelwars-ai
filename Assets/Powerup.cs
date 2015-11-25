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

		public Powerup() {}
		public Powerup(World parent, float x, float y, PowerupType type) {

			Init(parent, x, y, type);
		}

		public void Init(World parent, float x, float y, PowerupType type) {

			this.x = x;
			this.y = y;
			vSpeed = 0.0f;
			world = parent;
			this.type = type;
		}

		// Spawns a random powerup
		public static void SpawnRandom() {
		
			
		}

		public void Advance(List<WorldAction> actions) {


		}



		World world;

		// Position/kinematics
		float x;
		float y;
		float vSpeed;
		PowerupType type;
	}
}