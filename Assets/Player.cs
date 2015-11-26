/*
 * Player.cs
 * 
 * The Player class manages the game logic player movement and firing. This
 * class is defined as an inner class to the World class
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

partial class World : IAdvancing {

	public class Player : IAdvancing {

		// Transformation
		public float X { get; set; }
		public float Y { get; set; }
		public float XScale { get; set; }

		// Abilities
		public float Speed { get; set; }
		public float Gravity { get; set; }
		
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
		public WeaponType Weapon {
			get {
				return weapon;
			}
			set {
				// Set ammo as side effect of setting the weapon
				weapon = value;
				if (IsMaster) return;
				if (weapon == WeaponType.Bombs ||
				    weapon == WeaponType.Rockets ||
				    weapon == WeaponType.Minions) {

					Ammo = 3;
				} else if (weapon == WeaponType.Lightning) {
					Ammo = 1;
				}
			}
		
		}
		public virtual bool IsMaster {
			get {
				return isMaster;
			}
			set {
				isMaster = value;
				if (isMaster) {
					Ammo = -1;
				} else {
					Ammo = 0;
				}
			}
		}

		// Constructors
		public Player() {}
		public Player(World parent, bool isMaster, int actionSet) {
			init(parent, isMaster, actionSet);
		}

		// Takes an input list of world actions and updates the state
		virtual public void Advance(List<WorldAction> actions) {

			// Update timers
			if (fireWait > 0) fireWait -= 1;
			if (wallStick > 0) wallStick -= 1;
			if (speedTimer >= 0) speedTimer -= 1;
			if (gravityTimer >= 0) gravityTimer -= 1;

			// Fire off timer events
			if (speedTimer == 0) Speed = defaultSpeed;

			// Handle action input if alive
			if (Alive) {

				bool left = false;
				bool right = false;
				bool firing = false;
				bool jumping = false;

				// Interpret action list
				foreach (WorldAction action in actions) {
					if (action == leftAction) {
						left = true;
					} else if (action == rightAction) {
						right = true;
					} else if (action == jumpAction) {
						jumping = true;
					} else if (action == fireAction) {
						firing = true;
					}
				}

				if (firing) fire();
				if (jumping) jump();
				if (left && !right) {
					moveHorizontally(-Speed);
				} else if (right) {
					moveHorizontally(Speed);
				}
			}

			// Gravity always affects motion
			fall();

			// Collect powerups
			collectPowerups();
		}



		World world;
		bool isMaster;
		int playerNum;
		WeaponType weapon;

		// Constants
		const float defaultSpeed = 7.0f;

		// Kinematics
		float vSpeed;
		bool noFall;

		// Timers
		int wallStick;
		int fireWait;
		int speedTimer = 0;
		int gravityTimer = 0;

		WorldAction leftAction;
		WorldAction rightAction;
		WorldAction jumpAction;
		WorldAction fireAction;

		// Init
		protected void init(World parent, bool isMaster, int actionSet) {
			
			X = 0.0f;
			Y = 0.0f;
			XScale = 1.0f;
			Health = 100.0f;
			Ammo = 0;
			Weapon = WeaponType.None;
			IsMaster = isMaster;
			Speed = defaultSpeed;
			Gravity = 1.0f;

			vSpeed = 0.0f;
			noFall = true;
			world = parent;
			playerNum = actionSet;

			wallStick = 0;
			fireWait = 0;
			speedTimer = 0;
			gravityTimer = 0;
			
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

		// Attempt to jump
		void jump() {
			if (noFall || wallStick > 0) {
				vSpeed = -14.0f;
			}
		}

		// Attempt to move horizontally
		void moveHorizontally(float dx) {

			XScale = dx > 0.0f ? -1.0f : 1.0f;

			if (world.checkGround(X - (XScale * 18.0f) + dx, Y - 25.0f) ||
			    world.checkGround(X - (XScale * 18.0f) + dx, Y + 24.0f)) {

				vSpeed = Mathf.Min(vSpeed, 0.0f);
				wallStick = 3;

			} else {
				X += dx;
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
				noFall = false;

				// Fall but limit fall speed
				vSpeed += Gravity;
				vSpeed = Mathf.Min(45.0f, vSpeed);
				vSpeed = Mathf.Min(45.0f, vSpeed);
				
				// Check for head-hitting
				if (vSpeed < 0.0f) {
					if (world.checkGround(X - 18.0f, Y - 25.0f + vSpeed) ||
					    world.checkGround(X + 18.0f, Y - 25.0f + vSpeed)) {

						// Has hit head
						vSpeed = 0.0f;
						Y = Mathf.Ceil((Y - floorLevel) / blockSize) * blockSize + floorLevel - 39.0f;
					}
				}

				// Don't let player get blasted through the ceiling
				float minY = World.blockSize / 2;
				if (Y < minY)
				{
					Y = minY;
					vSpeed = Mathf.Max(0.0f, vSpeed);
				}
			}
		}

		// Fire weapon
		void fire() {

			if (Ammo > 0 || IsMaster) {

				Projectile  projectile = new Projectile(X, Y, XScale > 0.0f, Weapon, playerNum);
				world.projectiles.Add(projectile);
			}
		}

		// Collide with powerups
		void collectPowerups() {
		
			int len = world.powerups.Count;
			for (int i = len - 1; i >= 0; i--) {
			
				Powerup powerup = world.powerups[i];
				float x = powerup.X;
				float y = powerup.Y;
				bool isSpeedGrav = powerup.Weapon == WeaponType.None;

				// Collide with player
				if (!isSpeedGrav && Util.checkRectIntersect(x + 10.0f, y + 7.0f, x + 54.0f, y + 57.0f,
				                                       X - 32.0f, Y - 32.0f, X + 32.0f, Y + 32.0f)) {

					// Collided with player - delete powerup
					world.powerups.Remove(powerup);
					
					// Switch mastermode?
					if (Random.Range(0, 30) == 0) { // 1 in 30 odds

						if (IsMaster) {
							IsMaster = false;
						} else {
							IsMaster = true;
						}

						world.startedAsMaster = 0;
					}
					
					// Switch weapon
					if (powerup.Weapon != WeaponType.None) {

						powerup.Weapon = WeaponType.None; // Make sure setter is invoked
						Weapon = powerup.Weapon;
					}

				} else if (isSpeedGrav && checkPlayerRectIntersect(x - 8.0f, y - 8.0f, x + 8.0f, y + 8.0f)) {
				
					// Collided with player - delete powerup
					world.powerups.Remove(powerup);

					if (powerup.Type == PowerupType.Speed) {
						if (Speed < 28.0f) Speed *= 2.0f;
						speedTimer = 600;

					} else if (powerup.Type == PowerupType.Gravity) {
						if (Gravity > 0.25f) Gravity /= 2.0f;
						gravityTimer = 600;
					}
				}
			}
		}

		bool checkPlayerPointIntersect(float x, float y) {
			return ((x > X - 18.0f) && (x < X + 18.0f) && (y > Y - 25.0f) && (y < Y + 25.0f));
		}

		bool checkPlayerRectIntersect(float x1, float y1, float x2, float y2) {
			return (checkPlayerPointIntersect(x1, y1) || checkPlayerPointIntersect(x2, y2) ||
			        checkPlayerPointIntersect(x1, y2) || checkPlayerPointIntersect(x2, y1));
		}
	}
}
