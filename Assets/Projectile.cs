﻿/*
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

		// Clone - can specify target player references
		public Projectile Clone(World cloneWorld, Player player1, Player player2) {
			Projectile p = new Projectile();
			p.X = X;
			p.Y = Y;
			p.FacingRight = FacingRight;
			p.Type = Type;
			p.TargetPlayer = targetPlayerNum == 1 ? player1 : player2;
			p.targetPlayerNum = targetPlayerNum;
			p.world = cloneWorld;
			p.hSpeed = hSpeed;
			p.vSpeed = vSpeed;
			p.timer = timer;

			// Any other fields or properties you add go here...

			return p;
		}

		// Takes an input list of world actions and updates the state
		virtual public void Advance(List<WorldAction> actions) {
			Advance(actions, false);
		}
		virtual public void Advance(List<WorldAction> actions, bool enlarge) {

			float normalized = FacingRight ? 1.0f : -1.0f;

			float eFactor = enlarge ? EnlargementFactor : 1.0f;

			switch(Type) {
				
			// Bombs
			case WeaponType.Bombs: {

				Y += vSpeed;
				vSpeed += 1.0f;

				bool destroy = false;
				
				// Player collide
				if (TargetPlayer.CheckPlayerRectIntersect(X - 12.0f * eFactor, Y - 12.0f * eFactor,
				                                          X + 12.0f * eFactor, Y + 12.0f * eFactor)) {
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
				if (TargetPlayer.CheckPlayerRectIntersect(X - 16.0f * eFactor, Y - 12.0f * eFactor,
				                                          X + 16.0f * eFactor, Y + 12.0f * eFactor)) {
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
					Y = Mathf.Ceil((Y - FloorLevel) / BlockSize) * BlockSize + FloorLevel - 15.0f;
				}
				
				if (!pushingIntoWall) {
					X += hSpeed;
					timer = 12;

				// Pushing into wall case
				} else {
					X = Mathf.Round(X / BlockSize) * BlockSize - normalized * 14.0f;

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
							bool blowRight = world.CheckGround(X + BlockSize * normalized, Y);
							bool blowBelow = world.CheckGround(X, Y + BlockSize);
							if (blowRight && blowBelow) {

								// Can't blow up both at once. Randomly select which
								int r = Random.Range(0, 2);
								if (r == 0) blowRight = false;
								if (r == 1) blowBelow = false;
							}
							if (blowRight) {
								world.setGround(X + BlockSize * normalized, Y, false);
							}
							if (blowBelow) {
								world.setGround(X, Y + BlockSize, false);
							}
						}
					}
				}
				
				// Check player collision
				if (TargetPlayer.CheckPlayerRectIntersect(X - 18.0f * eFactor, Y - 18.0f * eFactor,
				                                          X + 18.0f * eFactor, Y + 18.0f * eFactor)) {
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
					for (int j = 0; j < BlocksWidth; j++) {

						for (int k = 0; k < BlocksHeight; k++) {

							float xx = j * BlockSize;
							float yy = k * BlockSize + FloorLevel;
							if (yy < Y && xx + BlockSize > X - 16.0f * eFactor && xx < X + 16.0f * eFactor) {
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
				world.setGround(X, Y + BlockSize, false);

				// Harm nearby players
				world.explode(X, Y, 100.0f, 48.0f, TargetPlayer);
				world.explode(X, Y, 100.0f, 48.0f, other.TargetPlayer);
				
				return true;
			}
			return false;
		}



		World world;
		int targetPlayerNum;

		// Kinematics
		float hSpeed;
		float vSpeed;

		// Minions, lightning only
		int timer;

		// Init
		void init(World parent, float x, float y, bool facingRight, WeaponType type, int playerNum) {

			world = parent;
			timer = type == WeaponType.Lightning ? 21 : 12;
			targetPlayerNum = playerNum == 1 ? 2 : 1;

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
