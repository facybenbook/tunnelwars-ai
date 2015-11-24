/*
 * Player.cs
 * 
 * The player class manages the game logic player movement and firing. This
 * class is defined as an inner class to the world class
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Types of weapons
public enum WeaponType {
	None,
	Bombs,
	Rockets,
	Minions,
	Lightning
}

partial class World : IWorld {
	interface IPlayer {

		// Transformation
		float X { get; set; }
		float Y { get; set; }
		float XScale { get; set; }

		// Health
		float Health { get; set; }
		bool Alive { get; }

		// Weapons
		int Ammo { get; set; }
		WeaponType Weapon { get; set; }
		bool IsMaster { get; set; }

		// Takes an input list of world actions and updates the state
		void Advance(List<WorldAction> actions);
	}

	public class Player : IPlayer {

		// Transformation
		public float X { get; set; }
		public float Y { get; set; }
		public float XScale { get; set; }
		
		// Health
		float health;
		public float Health {
			get { return health; }
			set {
				health = value;
				if (health <= 0.0f) {
					// Kill player
				}
			}
		}
		public bool Alive {
			get { return health > 0.0f; }
		}

		// Weapons
		public int Ammo { get; set; }
		public WeaponType Weapon { get; set; }
		public bool IsMaster { get; set; }

		// Constructor
		public Player(World parent, bool isMaster, int actionSet) {

			X = 0.0f;
			Y = 0.0f;
			XScale = 1.0f;
			Health = 100.0f;
			Ammo = 0;
			Weapon = WeaponType.None;
			IsMaster = isMaster;

			vSpeed = 0.0f;
			gravity = 1.0f;
			noFall = true;
			wallStick = 0;
			world = parent;

			// Assign actions to respond to
			if (actionSet == 1) {
				leftAction = WorldAction.P1Left;
				rightAction = WorldAction.P1Right;
				jumpAction = WorldAction.P1Jump;
				fireAction = WorldAction.P1Fire;

			} else if (actionSet == 2) {
				leftAction = WorldAction.P2Left;
				rightAction = WorldAction.P2Right;
				jumpAction = WorldAction.P2Jump;
				fireAction = WorldAction.P2Fire;
			}
		}

		// Takes an input list of world actions and updates the state
		public void Advance(List<WorldAction> actions) {

			// Handle action input if alive
			if (Alive) {
				foreach (WorldAction action in actions) {

					if (action == jumpAction) {

						jump();
						break;
					}
				}
			}

			// Gravity always affects motion
			fall();
		}



		World world;

		// Kinematics
		float vSpeed;
		float gravity;
		bool noFall;
		int wallStick; //TODO

		WorldAction leftAction;
		WorldAction rightAction;
		WorldAction jumpAction;
		WorldAction fireAction;

		// Attempt to jump
		void jump() {
			if (noFall || wallStick > 0) {
				vSpeed = -14.0f;
			}
		}

		// Fall
		void fall() {
			if (world.checkGround(X, Y + 33.0f + vSpeed) ||
			    world.checkGround(X - 18.0f, Y + 33.0f + vSpeed) ||
			    world.checkGround(X + 18.0f, Y + 33.0f + vSpeed)) {

				if (vSpeed > 0.0f) {
					Y = Mathf.Floor((Y - floorLevel) / blockSize) * blockSize + floorLevel + 39.0f;
				}
				vSpeed = Mathf.Min(vSpeed, 0.0f);
				noFall = true;

			} else {

				Y += vSpeed;
				vSpeed += gravity;
				noFall = false;
				
				// Check for head-hitting
				if (vSpeed < 0.0f) {
					if (world.checkGround(X - 18.0f, Y - 25.0f + vSpeed) ||
					    world.checkGround(X + 18.0f, Y - 25.0f + vSpeed)) {

						// Has hit head
						vSpeed = 0.0f;
						Y = Mathf.Ceil((Y - floorLevel) / blockSize) * blockSize + floorLevel - 39.0f;
					}
				}
			}
		}

		private bool checkPointIntersect(float x, float y) {
			return ((x > X - 18.0f) && (x < X + 18.0f) && (y > Y - 25.0f) && (y < Y + 25.0f));
		}

		private bool  checkRectIntersect(float x1, float y1, float x2, float y2) {
			return (checkPointIntersect(x1, y1) || checkPointIntersect(x2, y2) ||
			        checkPointIntersect(x1, y2) || checkPointIntersect(x2, y1));
		}
	}
}
