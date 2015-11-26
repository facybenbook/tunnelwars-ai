/*
 * Projectile.cs
 * 
 * The projectile class is a nested class in the World class and controls
 * movement of projectiles
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

partial class World : IAdvancing {

	public class Projectile : IAdvancing {

		// Transformation
		public float X { get; set; }
		public float Y { get; set; }
		public bool FacingRight { get; set; }
		
		// Constructor
		public Projectile() {}
		public Projectile(float x, float y, bool facingRight, WeaponType type, int playerNum) {

			init(x, y, facingRight, type, playerNum);
		}

		// Takes an input list of world actions and updates the state
		virtual public void Advance(List<WorldAction> actions) {


		}



		// The type of the projectile
		WeaponType type;

		// The parent player num
		int sourcePlayer;

		// Init
		void init(float x, float y, bool facingRight, WeaponType type, int playerNum) {

			this.type = type;
			sourcePlayer = playerNum;
			X = x;
			Y = y;
			FacingRight = facingRight;
		}
	}
}
