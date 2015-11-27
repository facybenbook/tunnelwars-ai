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
		public WeaponType Type { get; set; }

		// The player the projectile is intended to hit
		public Player TargetPlayer { get; set; }
		
		// Constructor
		public Projectile() {}
		public Projectile(World parent, float x, float y, bool facingRight, WeaponType type, int playerNum) {

			init(parent, x, y, facingRight, type, playerNum);
		}

		// Takes an input list of world actions and updates the state
		virtual public void Advance(List<WorldAction> actions) {

			float normalized = FacingRight ? 1.0f : -1.0f;

			switch(Type) {
				
			// Bombs
			case WeaponType.Bombs: {

				Y += vSpeed;
				vSpeed += 1.0f;

				bool destroy = false;
				
				// Player collide
				if (TargetPlayer.CheckPlayerRectIntersect(X - 12.0f, Y - 12.0f, X + 12.0f, Y + 12.0f)) {
					destroy = true;
				}
				
				// Ground collide
				if (world.CheckGround(X - 6.0f, Y)) {

					// Destroy ground
					world.setGround(X - 6.0f, Y, false);
					destroy = true;
				
				} else if (world.CheckGround(X + 6.0f, Y)) {

					world.setGround(X + 6.0f, Y, false);
					destroy = true;
				}
				
				if (destroy) {

					// Go boom
					world.explode(X, Y, 100.0f, 48.0f, TargetPlayer);
					world.destroyProjectile(this);
				}
				break;
			}
				
			// Rockets
			case WeaponType.Rockets: {

				X += normalized * 14.0f;
				bool destroy = false;
				
				// Player collide
				if (TargetPlayer.CheckPlayerRectIntersect(X - 16.0f, Y - 12.0f, X + 16.0f, Y + 12.0f)) {
					// Go boom
					TargetPlayer.Health -= 42.0f;
					destroy = true;
				}
				
				// TODO: Remove ground collide
				/*if (checkRevealGround(x + (projectile[i] as Transform).localScale.x * 16, y))
				{
					removeRevealGround(x + (projectile[i] as Transform).localScale.x * 16, y);
					AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
				}*/
				
				// Ground collide
				if (world.CheckGround(X + normalized * 16.0f, Y - 8.0f)) {

					// Destroy ground
					world.setGround(X + normalized * 16.0f, Y - 8.0f, false);
					destroy = true;

				} else if (world.CheckGround(X + normalized * 16.0f, Y + 8.0f)) {

					// Destroy ground
					world.setGround(X + normalized * 16.0f, Y + 8.0f, false);
					destroy = true;
				}
				
				if (destroy) {
					world.explode(X, Y, 10.0f, 0.0f, TargetPlayer);
					world.destroyProjectile(this);
				}

				break;
			}
				
			// Minions
			case WeaponType.Minions: {

				// Check ground collision
				bool falling = !(world.CheckGround(X - 9.0f, Y + 36.0f + vSpeed) ||
				                 world.CheckGround(X + 9.0f, Y + 36.0f + vSpeed));
				bool pushingIntoWall = (world.CheckGround(X + hSpeed + normalized * 9.0f, Y + 8.0f) ||
				                        world.CheckGround(X + hSpeed + normalized * 9.0f, Y - 15.0f));
				bool destroy = false;

				if (falling) {

					Y += vSpeed;
					vSpeed += 1.0f;
					timer = 12;

				} else {
					vSpeed = 0.0f;
					Y = Mathf.Ceil((Y - floorLevel) / blockSize) * blockSize + floorLevel - 15.0f;
				}
				
				if (!pushingIntoWall) {
					X += hSpeed;
					timer = 12;

				// Pushing into wall case
				} else {
					X = Mathf.Round(X / blockSize) * blockSize - normalized * 14.0f;

					// Falling and pushing into wall makes the minions feistier
					if (falling) {
						hSpeed = 10.0f * normalized;

					// Stationary minions blow up
					} else {
						bool blowGround = false;

						timer -= 1;
						if (timer <= 0) {
							destroy = true;
							world.explode(X, Y, 100.0f, 48.0f, TargetPlayer);
							blowGround = true;
						}

						// Blow away ground
						if (blowGround) {

							// Check if either ground is non-blowupable
							// TODO: Reveal ground should be blowupable too
							bool blowRight = world.CheckGround(X + blockSize * normalized, Y);
							bool blowBelow = world.CheckGround(X, Y + blockSize);
							if (blowRight && blowBelow) {

								// Can't blow up both at once. Randomly select which
								int r = Random.Range(0,2);
								if (r == 0) blowRight = false;
								if (r == 1) blowBelow = false;
							}
							if (blowRight) {
								world.setGround(X + blockSize * normalized, Y, false);
							}
							if (blowBelow) {
								world.setGround(X, Y + blockSize, false);
							}
						}
					}
				}
				
				// Check player collision
				if (TargetPlayer.CheckPlayerRectIntersect(X - 18.0f, Y - 18.0f, X + 18.0f, Y + 18.0f)) {
					TargetPlayer.Health -= 42.0f;
					world.explode(X, Y, 10.0f, 0.0f, TargetPlayer);
					destroy = true;
				}
				
				if (destroy) {
					world.destroyProjectile(this);
				}
				
				break;
			}

			// Lightning
			case WeaponType.Lightning: {

				// At the outset
				timer -= 1;
				if (timer == 20) {
					
					// Break blocks above by cycling through
					for (int j = 0; j < blocksWidth; j++) {

						for (int k = 0; k < blocksHeight; k++) {

							float xx = j * blockSize;
							float yy = k * blockSize + floorLevel;
							if (yy < Y && xx + blockSize > X - 16.0f && xx < X + 16.0f) {
								// Get rid of ground
								world.setGroundByIndex(j, k, false);
							}
						}
					}
				
				// Go away after a certain amount of time
				} else if (timer == 0) {

					// Destroy self
					world.destroyProjectile(this);
				}

				// Do some damage to players
				if (TargetPlayer.X > X - 34.0f && TargetPlayer.X < X + 34.0f
				    && TargetPlayer.Y < Y) {

					// Zap!
					TargetPlayer.Health = 0.0f;
					TargetPlayer.Zap(); // Shoots the player up in the air
				}
				break;
			}
			}
		}

		// Simulates collisions between this powerup and another powerup. Returns whether collided.
		// IMPORTANT: the caller is responsible for destroying both minions involved
		public bool CollideWith(Projectile other) {

			// Only minions have inter-projectile collisions
			if (Type != WeaponType.Minions || other.Type != WeaponType.Minions) return false;

			// Only minions from opposing players collide
			if (TargetPlayer == other.TargetPlayer) return false;
						
			// See if collided
			float dx = (X - other.X);
			float dy = (Y - other.Y);
			if (dx * dx + dy * dy < 1024.0f) {

				// Collided. Blow below
				world.setGround(X, Y + blockSize, false);

				// Harm nearby players
				world.explode(X, Y, 100.0f, 48.0f, TargetPlayer);
				world.explode(X, Y, 100.0f, 48.0f, other.TargetPlayer);
				
				return true;
			}
			return false;
		}



		World world;

		// Kinematics
		float hSpeed;
		float vSpeed;

		// Minions, lightning only
		int timer;

		// Init
		void init(World parent, float x, float y, bool facingRight, WeaponType type, int playerNum) {

			world = parent;
			timer = type == WeaponType.Lightning ? 20 : 12;

			hSpeed = 0.0f;
			vSpeed = 0.0f;

			if (type == WeaponType.Minions) {
			
				vSpeed = Random.Range(-6.0f, 0.0f);
				hSpeed = facingRight ? 7.5f : -7.5f;
			}

			X = x;
			Y = y;
			FacingRight = facingRight;
			Type = type;
			TargetPlayer = playerNum == 1 ? world.Player2 : world.Player1;
		}
	}
}
