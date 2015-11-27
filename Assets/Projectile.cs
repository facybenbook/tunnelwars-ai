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
		public Projectile(World parent, float x, float y, bool facingRight, WeaponType type, int playerNum) {

			init(parent, x, y, facingRight, type, playerNum);
		}

		// Takes an input list of world actions and updates the state
		virtual public void Advance(List<WorldAction> actions) {

			/*switch(type) {
				
			// Bombs
			case WeaponType.Bombs: {

				Y += vSpeed;
				vSpeed += 1.0f;

				bool destroy = false;
				
				// Player collide
				if (targetPlayer.CheckPlayerRectIntersect(X - 12, Y - 12, X + 12, Y + 12)) {
					destroy = true;
				}
				
				// Ground collide
				if (checkGround(X - 6.0f, Y)) {

					// Destroy ground
					world.setGround(X - 6.0f, Y, false);
					destroy = true;
				
				} else if (checkGround(X + 6.0f, Y) {

					world.setGround(X + 6.0f, Y, false);
					destroy = true;
				}
				
				if (destroy) {

					// Go boom
					if (parent == 1) explode(x, y, 100, 48, 2);
					else explode(x, y, 100, 48, 1);
					
					// Destroy self
					UnityEngine.Object.Destroy((projectile[i] as Transform).gameObject);
					projectile.RemoveAt(i);
					projectile_speed.RemoveAt(i);
					projectile_type.RemoveAt(i);
					projectile_parent.RemoveAt(i);
					projectile_speed2.RemoveAt(i);
					projectile_aux.RemoveAt(i);
					i -= 1;
				}
				break;
			}
				
				// Rockets
			case 2:{
				
				temp = projTransform.position;
				temp.x += projTransform.localScale.x * 14;
				projTransform.position = temp;
				x += projTransform.localScale.x * 14;
				
				bool destroy = false;
				
				// Player collide
				if (parent == 2)
				{
					if (checkPlayerIntersectRect(x - 16, y - 12, x + 16, y + 12, 1) && player2_hh > 0)
					{
						player1_hh -= 42;
						destroy = true;
						createExplosionSprite(x + speed2, y);
						
						AudioSource.PlayClipAtPoint(hurtSound, Camera.main.transform.position);
						
						redo_health();
					}
				}
				else
				{
					if (checkPlayerIntersectRect(x - 16, y - 12, x + 16, y + 12, 2) && player1_hh > 0)
					{
						player2_hh -= 42;
						destroy = true;
						createExplosionSprite(x + speed2, y);
						
						AudioSource.PlayClipAtPoint(hurtSound, Camera.main.transform.position);
						
						redo_health();
					}
				}
				
				// Remove ground collide
				if (checkRevealGround(x + (projectile[i] as Transform).localScale.x * 16, y))
				{
					removeRevealGround(x + (projectile[i] as Transform).localScale.x * 16, y);
					AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
				}
				
				// Ground collide
				if (checkGround(x + (projectile[i] as Transform).localScale.x * 16, y - 8))
				{
					// Destroy ground
					Transform other2 = getGroundSprite(x + (projectile[i] as Transform).localScale.x * 16, y - 8);
					if (other2 != null)
					{
						UnityEngine.Object.Destroy(other2.gameObject);
						setGround(x + (projectile[i] as Transform).localScale.x * 16, y - 8, false);
						
						AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
						
					}
					destroy = true;
				}
				else if (checkGround(x + (projectile[i] as Transform).localScale.x * 16, y + 8))
				{
					// Destroy ground
					Transform other2 = getGroundSprite(x + (projectile[i] as Transform).localScale.x * 16, y + 8);
					if (other2 != null)
					{
						UnityEngine.Object.Destroy(other2.gameObject);
						setGround(x + (projectile[i] as Transform).localScale.x * 16, y + 8, false);
						
						AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
					}
					destroy = true;
				}
				
				if (destroy)
				{
					// Destroy self
					UnityEngine.Object.Destroy((projectile[i] as Transform).gameObject);
					projectile.RemoveAt(i);
					projectile_speed.RemoveAt(i);
					projectile_type.RemoveAt(i);
					projectile_parent.RemoveAt(i);
					projectile_speed2.RemoveAt(i);
					projectile_aux.RemoveAt(i);
					i -= 1;
				}
				break;
			}
				
				// Minions
			case 3: {
				// Check ground collision
				bool  falling = !(checkGround(x - 9.0f, y + 36.0f + speed) || checkGround(x + 9.0f, y + 36.0f + speed));
				bool  pushing_into_wall = (checkGround(x + speed2 + (projectile[i] as Transform).localScale.x * 18, y + 8) ||
				                           checkGround(x + speed2 + (projectile[i] as Transform).localScale.x * 18, y - 15));
				
				bool destroy = false;
				
				temp = projTransform.position;
				
				if (falling)
				{
					temp.y += speed;
					y += speed;
					
					projectile_speed[i] += 1.0f;
					projectile_aux[i] = 12;
				}
				else
				{
					projectile_speed[i] = 0;
					y = temp.y = Mathf.Ceil((y - floor_level) / 64.0f) * 64.0f + floor_level - 15.0f;
				}
				
				if (!pushing_into_wall)
				{
					temp.x += speed2;
					x += speed2;
					projectile_aux[i] = 12;
				}
				else
				{
					temp.x = Mathf.Round(x / 64) * 64 - (projectile[i] as Transform).localScale.x * 28;
					temp.y = y;//projTransform.position.y;
					if (falling)
					{
						// Speed doubles?
						projectile_speed2[i] = 20 * (projectile[i] as Transform).localScale.x;
					}
					else
					{
						bool blow_ground= false;
						// Pushing into a wall and also not falling. Wait then die.
						int temp_int_unity = projectile_aux[i];
						projectile_aux[i] = temp_int_unity - 1;
						//print(projectile_aux[i]);
						if (temp_int_unity - 1 <= 0)
						{
							destroy = true;
							if (parent == 1) explode(x, y, 100, 48, 2);
							else explode(x, y, 100, 48, 1);
							blow_ground = true;
						}
						if (blow_ground)
						{
							// Check if either ground is non-blowupable
							Transform sprite_right = getGroundSprite(x + 128 * (projectile[i] as Transform).localScale.x, y);
							Transform sprite_below = getGroundSprite(x, y + 64);
							bool  blow_right = (null != sprite_right && checkGround(x + 128 * (projectile[i] as Transform).localScale.x, y));
							bool  blow_below = (null != sprite_below && checkGround(x, y + 64));
							if (blow_right && blow_below)
							{
								// Can't blow up both at once. Randomly select which
								int r = Random.Range(0,2);
								if (r == 0) blow_below = false;
								if (r == 1) blow_right = false;
							}
							if (blow_right)
							{
								UnityEngine.Object.Destroy(sprite_right.gameObject);
								setGround(x + 128 * (projectile[i] as Transform).localScale.x, y, false);
								
								AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
							}
							if (blow_below)
							{
								UnityEngine.Object.Destroy(sprite_below.gameObject);
								setGround(x, y + 64, false);
								
								AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
							}
						}
					}
				}
				
				// Check player collision
				if (parent == 2)
				{
					if (checkPlayerIntersectRect(x - 18, y - 18, x + 18, y + 18, 1) && player2_hh > 0)
					{
						player1_hh -= 42;
						destroy = true;
						createExplosionSprite(x, y);
						
						AudioSource.PlayClipAtPoint(hurtSound, Camera.main.transform.position);
						
						redo_health();
					}
				}
				else
				{
					if (checkPlayerIntersectRect(x - 18, y - 18, x + 18, y + 18, 2) && player1_hh > 0)
					{
						player2_hh -= 42;
						destroy = true;
						createExplosionSprite(x, y);
						
						AudioSource.PlayClipAtPoint(hurtSound, Camera.main.transform.position);
						
						redo_health();
					}
				}
				
				// Check collision with other minions
				if (i != projectile_type.Count - 1)
				{
					// If not the last check for minions ahead
					for (int j = i + 1; j < projectile_type.Count; j++)
					{
						if (projectile_type[j] == 3 && projectile_parent[j] != parent)
						{
							float other_x = (projectile[j] as Transform).position.x;
							float other_y = (projectile[j] as Transform).position.y;
							
							// See if collided
							if ((x - other_x)*(x - other_x)+(y - other_y)*(y - other_y) < 1024)
							{
								// Collided. Blow below
								if (checkGround(x, y + 64))
								{
									Transform sprite_below = getGroundSprite(x, y + 64);
									if (sprite_below != null)
									{
										UnityEngine.Object.Destroy(sprite_below.gameObject);
										setGround(x, y + 64, false);
									}
								}
								
								createExplosionSprite(x,y);
								
								AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
								
								// Destroy other than self
								UnityEngine.Object.Destroy((projectile[j] as Transform).gameObject);
								projectile.RemoveAt(j);
								projectile_speed.RemoveAt(j);
								projectile_type.RemoveAt(j);
								projectile_parent.RemoveAt(j);
								projectile_speed2.RemoveAt(j);
								projectile_aux.RemoveAt(j);
								destroy = true;
							}
						}
					};
				}
				
				// Update position
				projTransform.position = temp;
				
				if (destroy)
				{
					// Destroy self
					UnityEngine.Object.Destroy((projectile[i] as Transform).gameObject);
					projectile.RemoveAt(i);
					projectile_speed.RemoveAt(i);
					projectile_type.RemoveAt(i);
					projectile_parent.RemoveAt(i);
					projectile_speed2.RemoveAt(i);
					projectile_aux.RemoveAt(i);
					i -= 1;
				}
				
				break;
			}
				
			// Lightning
			case 6: {
				projectile_aux[i] = aux - 1;
				if (aux == 20)
				{
					// Play sound
					AudioSource.PlayClipAtPoint(lightningSound, Camera.main.transform.position);
					
					// Break blocks above by cycling through
					for (int j = 0; j < 46; j++)
					{
						for (int k = 0; k < 16; k++)
						{
							float xx = j * 64;
							float yy = k * 64 + floor_level;
							if (yy < y && xx + 64 > x - 16 && xx < x + 16)
							{
								// Get rid of ground
								Transform temp_transform = (ground_sprite[j * 46 + k] as Transform);
								if (temp_transform != null) UnityEngine.Object.Destroy(temp_transform.gameObject);
								ground[j * 46 + k] = false;
							}
						};
					};
				}
				else if (aux == 0)
				{
					// Destroy self
					UnityEngine.Object.Destroy((projectile[i] as Transform).gameObject);
					projectile.RemoveAt(i);
					projectile_speed.RemoveAt(i);
					projectile_type.RemoveAt(i);
					projectile_parent.RemoveAt(i);
					projectile_speed2.RemoveAt(i);
					projectile_aux.RemoveAt(i);
					i -= 1;
				}
				// Do some damage to players
				if (parent == 1 && player2.position.x > x - 34 && player2.position.x < x + 34 && player2.position.y < y)
				{
					// Zap!
					player2_hh = 0;
					player2_vspeed -= 10;
				}
				else if (parent == 2 && player1.position.x > x - 34 && player1.position.x < x + 34 && player1.position.y < y)
				{
					// Zap!
					player1_hh = 0;
					player1_vspeed -= 10;
				}
				break;
			}
			}*/
		}



		WeaponType type;
		World world;
		Player targetPlayer; // The player the projectile is intended to hit

		// Kinematics
		float hSpeed;
		float vSpeed;

		// Init
		void init(World parent, float x, float y, bool facingRight, WeaponType type, int playerNum) {

			this.type = type;
			world = parent;
			targetPlayer = playerNum == 1 ? world.player2 : world.player1;

			hSpeed = 0.0f;
			vSpeed = 0.0f;

			X = x;
			Y = y;
			FacingRight = facingRight;
		}
	}
}
