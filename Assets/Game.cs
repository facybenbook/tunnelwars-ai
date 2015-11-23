using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

	// Unity transforms of prototypes of objects to clone
	public Transform protoground;
	public Transform protogroundImmutable;
	public Transform protobombs;
	public Transform protorockets;
	public Transform protominions;
	public Transform protolightnings;
	public Transform protobomb;
	public Transform protorocket;
	public Transform protominion;
	public Transform protomasterminion;
	public Transform protolightning;
	public Transform protogravity;
	public Transform protospeed;
	public Transform protoexplosion;
	public Transform dead;
	public GameObject gui;

	// Our sounds
	public AudioClip clickSound;
	public AudioClip hurtSound;
	public AudioClip shiftSound;
	public AudioClip ammoSound;
	public AudioClip lightningSound;
	
	// Must be accessible during game restart?
	private static int started_as_master = 0;
	
	// Input variables
	private bool  left = false;
	private bool  right = false;
	private bool  up = false;
	private bool  ctrloption = false;
	private bool  w = false;
	private bool  a = false;
	private bool  d = false;
	private bool  f = false;
	private bool  w_prev = false;
	private bool  f_prev = false;
	private bool  up_prev = false;
	private bool  ctrloption_prev = false;
	
	// Global variables
	private Transform player1;
	private Transform player2;
	private float player1_speed = 7;
	private float player2_speed = 7;
	private float player1_vspeed = 3;
	private float player2_vspeed = 3;
	private float player1_grav = 1;
	private float player2_grav = 1;
	private bool  player1_nofall = true;
	private bool  player2_nofall = true;
	private int player1_wallstick = 0;
	private int player2_wallstick = 0;
	private int player1_firewait = 0;
	private int player2_firewait = 0;
	
	private int player1_mode = 0;
	private int player2_mode = 0;
	private int player1_ammo = 0;
	private int player2_ammo = 0;
	private float player1_hh = 100;
	private float player2_hh = 100;
	private int player1_speed_timer = -1;
	private int player1_grav_timer = -1;
	private int player1_camo_timer = -1;
	private int player2_speed_timer = -1;
	private int player2_grav_timer = -1;
	private int player2_camo_timer = -1;
	
	private int spawnTimer = 7;
	private int restartTimer = -1;
	
	private float floor_level = 64.0f * 14.0f;
	private List<bool> ground= new List<bool>();
	private List<Transform> ground_sprite= new List<Transform>();
	private List<Transform> ground_reveal= new List<Transform>();
	
	private List<Transform> ammo= new List<Transform>();
	private List<int> ammo_type= new List<int>();
	private List<float> ammo_vspeed= new List<float>();
	
	private List<Transform> projectile= new List<Transform>();
	private List<int> projectile_type= new List<int>();
	private List<float> projectile_speed= new List<float>();
	private List<int> projectile_parent= new List<int>();
	private List<float> projectile_speed2= new List<float>();
	private List<int> projectile_aux= new List<int>();
	
	private List<Transform> explosions= new List<Transform>();
	
	private Transform clone;
	private int startTimer = 1;

	// Intermediary vector used for updating
	private Vector3 temp = new Vector3();
	
	void  Awake (){
		Application.targetFrameRate = 10;
		QualitySettings.vSyncCount = 1;
	}
	
	void  LateStart (){

		// Find GameObjects
		player1 = GameObject.Find("Player 1").transform;
		player2 = GameObject.Find("Player 2").transform;
		
		// Set master if a player lost last round
		if (started_as_master == 1)
		{
			player1_ammo = -1;
			player1.gameObject.SendMessage("SetMaster", true);
		}
		else if (started_as_master == 2)
		{
			player2_ammo = -1;
			player2.gameObject.SendMessage("SetMaster", true);
		}

		temp.x = 1344;
		temp.y = 864;
		player1.position = temp;

		temp.x = 1600;
		temp.y = 864;
		player2.position = temp;

		// Add space
		for (int i = 0; i < 7000; i++) {
			ground.Add (true);
			ground_sprite.Add(null);
		}

		// Make regular ground
		for (int i= 0; i < 46; i++)
		{
			for (int j= 0; j < 16; j++)
			{

				float chance = 0.02f; // Default chance
				
				if(j != 0)
				{
					if (!ground[i * 46 + j - 1]
					    && !(i == 22 && j == 4)
					    && !(i == 23 && j == 4)) chance += 0.40f; // Up chance
				}
				else
				{
					chance += 0.15f; // Surface level chance
				}
				if(i != 0)
				{
					if (!ground[(i - 1) * 46 + j] 
					    && !(i == 24 && j < 4)) chance += 0.40f; // Left chance
				}
				float val = 0.0f;
				val = Random.Range(0.0f, 1.0f);
				if ((i >= 22 && i < 24 && j <= 3) || (val <= chance))
				{
					ground[i * 46 + j] = false;
					ground_sprite[i * 46 + j] = null;
					continue;
				}
				else
				{
					ground[i * 46 + j] = true;
					clone = Instantiate(protoground);

					temp.x = i * 64;
					temp.y = j * 64 + floor_level;
					clone.position = temp;
					
					ground_sprite[i * 46 + j] = clone;
				}
			};
		};
		
		// Create immutable ground
		// Left edge
		for (int i = 0; i < 31; i++)
		{
			clone = Instantiate(protogroundImmutable);
			temp.x = -64;
			temp.y = i * 64;
			clone.position = temp;
		};
		// Right edge
		for (int i = 0; i < 31; i++)
		{
			clone = Instantiate(protogroundImmutable);
			temp.x = 2944;
			temp.y = i * 64;
			clone.position = temp;
		};
		// Top edge
		for (int i = -1; i < 47; i++)
		{
			clone = Instantiate(protogroundImmutable);
			temp.x = i * 64;
			temp.y = -64;
			clone.position = temp;
		};
		// Bottom edge
		for (int i = -1; i < 47; i++)
		{
			clone = Instantiate(protogroundImmutable);
			temp.x = i * 64;
			temp.y = 1920;
			clone.position = temp;
		};
		// Middle wall
		for (int i = 0; i < 18; i++)
		{
			clone = Instantiate(protogroundImmutable);
			temp.x = 1408 ;
			temp.y = 64 * i;
			clone.position = temp;
			clone = Instantiate(protogroundImmutable);
			temp.x = 1472;
			temp.y = 64 * i;
			clone.position = temp;
		};

		// Create reveal ground
		for (int i = 0; i < 10; i++) {
			ground_reveal.Add(null);
		}
		for (int i = 0; i < 4; i++)
		{
			clone = Instantiate(protoground);
			temp.x = 1408;
			temp.y = 64 * i + 896;
			clone.position = temp;
			// Colorize here
			ground_reveal[i * 2] = clone;
			
			clone = Instantiate(protoground);
			temp.x = 1472;
			temp.y = 64 * i + 896;
			clone.position = temp;
			// Colorize here
			ground_reveal[i * 2 + 1] = clone;
		};
		
		// Create bombs that are there at the start
		for (int i = 0; i < 4; i++)
		{
			clone = Instantiate(protobombs);
			if (i == 0) temp.x = 1792;
			else if (i == 1) temp.x = 1952;
			else if (i == 2) temp.x = 1088;
			else temp.x = 928;
			temp.y = floor_level - 64;
			clone.position = temp;
			ammo.Add(clone);
			ammo_type.Add(1);
			ammo_vspeed.Add(0);
		};
		
		gui.GetComponent<GUIControl>().SetMode(0);
	}
	
	void  LateUpdate (){
		f_prev = f;
		ctrloption_prev = ctrloption;
		up_prev = up;
		w_prev = w;
		
		ctrloption = Input.GetAxis("Player 1 Fire") > 0.25f;
		f = Input.GetAxis("Player 2 Fire") > 0.25f;
		
		// Firing
		if (ctrloption && ctrloption_prev == false)
		{
			if (player1_mode == 1 && player1_ammo != 0 && player1_hh > 0 && player1_firewait == 0)
			{
				createProjectile(1, 1);
			}
			if (player1_mode == 2 && player1_ammo != 0 && player1_hh > 0 && player1_firewait == 0)
			{
				createProjectile(2, 1);
			}
			if (player1_mode == 3 && player1_ammo != 0 && player1_hh > 0 && player1_firewait == 0)
			{
				createProjectile(3, 1);
			}
			if (player1_mode == 6 && player1_ammo != 0 && player1_hh > 0 && player1_firewait == 0)
			{
				createProjectile(6, 1);
			}
		}
		if (f && f_prev == false)
		{
			if (player2_mode == 1 && player2_ammo != 0 && player2_hh > 0 && player2_firewait == 0)
			{
				createProjectile(1, 2);
			}
			if (player2_mode == 2 && player2_ammo != 0 && player2_hh > 0 && player2_firewait == 0)
			{
				createProjectile(2, 2);
			}
			if (player2_mode == 3 && player2_ammo != 0 && player2_hh > 0 && player2_firewait == 0)
			{
				createProjectile(3, 2);
			}
			if (player2_mode == 6 && player2_ammo != 0 && player2_hh > 0 && player2_firewait == 0)
			{
				createProjectile(6, 2);
			}
		}
	}
	
	void  Update (){
		if (startTimer == 1)
		{
			LateStart();
			startTimer = 0;
		}
		
		// Update timers
		if (player1_firewait > 0) player1_firewait -= 1;
		if (player2_firewait > 0) player2_firewait -= 1;
		if (player1_wallstick > 0) player1_wallstick -= 1;
		if (player2_wallstick > 0) player2_wallstick -= 1;
		if (player1_speed_timer >= 0) player1_speed_timer -= 1;
		if (player1_speed_timer == 0) player1_speed = 7;
		if (player2_speed_timer >= 0) player2_speed_timer -= 1;
		if (player2_speed_timer == 0) player2_speed = 7;
		if (player1_grav_timer >= 0) player1_grav_timer -= 1;
		if (player1_grav_timer == 0) player1_grav = 1;
		if (player2_grav_timer >= 0) player2_grav_timer -= 1;
		if (player2_grav_timer == 0) player2_grav = 1;
		if (player1_camo_timer >= 0) player1_grav_timer -= 1;
		if (player2_camo_timer >= 0) player2_grav_timer -= 1;
		if (restartTimer >= 0) restartTimer -= 1;
		if (restartTimer == 30)
		{
			if (player1_hh <= 0)
			{
				if(started_as_master == 1) gui.SendMessage("SetMode", 4);
				else gui.SendMessage("SetMode", 2);
			}
			if (player2_hh <= 0)
			{
				if(started_as_master == 2) gui.SendMessage("SetMode", 3);
				else gui.SendMessage("SetMode", 1);
			}
		}
		if (restartTimer == 0)
		{
			// Set new master
			if (player1_hh <= 0) started_as_master = 1;
			else started_as_master = 2;
			
			// Much easier to restart in Unity
			Application.LoadLevel(0);
			redo_health();
		}
		
		// Input
		right = Input.GetAxis("Player 1 Horizontal") < -0.25f;
		left = Input.GetAxis("Player 1 Horizontal") > 0.25f;
		d = Input.GetAxis("Player 2 Horizontal") < -0.25f;
		a = Input.GetAxis("Player 2 Horizontal") > 0.25f;
		up = Input.GetAxis("Player 1 Jump") > 0.25f;
		w = Input.GetAxis("Player 2 Jump") > 0.25f;
		
		// Jumping
		if (up && up_prev == false)
		{
			if ((player1_nofall || player1_wallstick > 0) && player1_hh > 0)
			{
				player1_vspeed = -14;
			}
		}
		if (w && w_prev == false)
		{
			if ((player2_nofall || player2_wallstick > 0) && player2_hh > 0)
			{
				player2_vspeed = -14;
			}
		}
		
		// Arrow key movement
		if (left && player1_hh > 0 && !right)
		{
			temp = player1.localScale;
			temp.x = 1;
			player1.localScale = temp;
			if (checkGround(player1.position.x - 18 - player1_speed, player1.position.y - 25) ||
			    checkGround(player1.position.x - 18 - player1_speed, player1.position.y + 24))
			{
				player1_vspeed = Mathf.Min(player1_vspeed, 0);
				player1_wallstick = 3;
			}
			else
			{
				temp = player1.position;
				temp.x -= player1_speed;
				player1.position = temp;
			}
		}
		if (a && player2_hh > 0 && !d)
		{
			temp = player2.localScale;
			temp.x = 1;
			player2.localScale = temp;
			if (checkGround(player2.position.x - 18 - player2_speed, player2.position.y - 25) ||
			    checkGround(player2.position.x - 18 - player2_speed, player2.position.y + 24))
			{
				player2_vspeed = Mathf.Min(player2_vspeed, 0);
				player2_wallstick = 3;
			}
			else
			{
				temp = player2.position;
				temp.x -= player2_speed;
				player2.position = temp;
			}
		}
		if (right && player1_hh > 0 && !left)
		{
			temp = player1.localScale;
			temp.x = -1;
			player1.localScale = temp;
			if (checkGround(player1.position.x + 18 + player1_speed, player1.position.y - 25) ||
			    checkGround(player1.position.x + 18 + player1_speed, player1.position.y + 24))
			{
				player1_vspeed = Mathf.Min(player1_vspeed, 0);
				player1_wallstick = 3;
			}
			else
			{
				temp = player1.position;
				temp.x += player1_speed;
				player1.position = temp;
			}
		}
		if (d && player2_hh > 0 && !a)
		{
			temp = player2.localScale;
			temp.x = -1;
			player2.localScale = temp;
			if (checkGround(player2.position.x + 18 + player2_speed, player2.position.y - 25) ||
			    checkGround(player2.position.x + 18 + player2_speed, player2.position.y + 24))
			{
				player2_vspeed = Mathf.Min(player2_vspeed, 0);
				player2_wallstick = 3;
			}
			else
			{
				temp = player2.position;
				temp.x += player2_speed;
				player2.position = temp;
			}
		}
		
		// Falling
		if (checkGround(player1.position.x, player1.position.y + 33 + player1_vspeed) ||
		    checkGround(player1.position.x - 18, player1.position.y + 33 + player1_vspeed) ||
		    checkGround(player1.position.x + 18, player1.position.y + 33 + player1_vspeed))
		{
			if (player1_vspeed > 0)
			{
				temp = player1.position;
				temp.y = Mathf.Floor((player1.position.y - floor_level) / 64) * 64 + floor_level + 39;
				player1.position = temp;
			}
			player1_vspeed = Mathf.Min(player1_vspeed, 0);
			player1_nofall = true;
		}
		else
		{
			temp = player1.position;
			temp.y += player1_vspeed;
			player1.position = temp;
			player1_vspeed += player1_grav;
			player1_nofall = false;
			
			// Check for head-hitting
			if (player1_vspeed < 0)
			{
				if (checkGround(player1.position.x - 18, player1.position.y - 25 + player1_vspeed) ||
				    checkGround(player1.position.x + 18, player1.position.y - 25 + player1_vspeed))
				{
					// Has hit head
					player1_vspeed = 0;
					temp = player1.position;
					temp.y = Mathf.Ceil((player1.position.y - floor_level) / 64) * 64 + floor_level - 39;
					player1.position = temp;
				}
			}
		}
		if (checkGround(player2.position.x, player2.position.y + 33 + player2_vspeed) ||
		    checkGround(player2.position.x - 18, player2.position.y + 33 + player2_vspeed) ||
		    checkGround(player2.position.x + 18, player2.position.y + 33 + player2_vspeed))
		{
			if (player2_vspeed > 0)
			{
				temp = player2.position;
				temp.y = Mathf.Floor((player2.position.y - floor_level) / 64) * 64 + floor_level + 39;
				player2.position = temp;
			}
			player2_vspeed = Mathf.Min(player2_vspeed, 0);
			player2_nofall = true;
		}
		else
		{
			temp = player2.position;
			temp.y += player2_vspeed;
			player2.position = temp;
			player2_vspeed += player2_grav;
			player2_nofall = false;
			
			// Check for head-hitting
			if (player2_vspeed < 0)
			{
				if (checkGround(player2.position.x - 18, player2.position.y - 25 + player2_vspeed) ||
				    checkGround(player2.position.x + 18, player2.position.y - 25 + player2_vspeed))
				{
					// Has hit head
					player2_vspeed = 0;
					temp = player2.position;
					temp.y = Mathf.Ceil((player2.position.y - floor_level) / 64) * 64 + floor_level - 39;
					player2.position = temp;
				}
			}
		}
		
		player1_vspeed = Mathf.Min(45, player1_vspeed);
		player2_vspeed = Mathf.Min(45, player2_vspeed);
		
		if (player1.position.y < 32)
		{
			temp = player1.position;
			temp.y = 32;
			player1.position = temp;
			player1_vspeed = Mathf.Max(0, player1_vspeed);
		}
		if (player2.position.y < 32)
		{
			temp = player2.position;
			temp.y = 32;
			player2.position = temp;
			player2_vspeed = Mathf.Max(0, player2_vspeed);
		}
		
		// Spawn stuff
		spawnTimer -= 1;
		if (spawnTimer == 0 && ammo_type.Count < 64)
		{
			float spawnX;
			float spawnY;
			do {
				spawnX = Random.Range(0.0f, 2880.0f);
				spawnY = Random.Range(0.0f, 640.0f);
			}
			while (spawnX > 1344 && spawnX < 1536);
			
			int type = (UnityEngine.Random.Range(0, 5) + 1);
			int tempNum = UnityEngine.Random.Range(0, 40);
			float vspeed = 0;
			if (tempNum == 0) type = 6;
			if (type == 1)
			{
				clone = Instantiate(protobombs);
			}
			if (type == 2)
			{
				clone = Instantiate(protorockets);
			}
			if (type == 3)
			{
				clone = Instantiate(protominions);
			}
			if (type == 4)
			{
				clone = Instantiate(protospeed);
			}
			if (type == 5)
			{
				clone = Instantiate(protogravity);
			}
			if (type == 6)
			{
				clone = Instantiate(protolightnings);
			}
			temp.x = spawnX;
			temp.y = spawnY;
			clone.position = temp;
			ammo.Add(clone);
			ammo_type.Add(type);
			ammo_vspeed.Add(vspeed);
			
			spawnTimer = 60;
			redo_health();
		}
		
		// Make ammo fall and collide
		int deleteMe = -1;
		for (int i= 0; i < ammo.Count; i++)
		{
			Transform obj;
			float x;
			float y;
			// float vspeed;
			// int type;
			obj = ammo[i];
			x = obj.position.x;
			y = obj.position.y;
			float vspeed = ammo_vspeed[i];
			int type = ammo_type[i];
			
			// Grav and fast don't fall or collide
			if (type != 4 && type != 5)
			{
				if (ammo_vspeed[i] != -1)
				{
					if (!(checkGround(x, y + 65 + vspeed) || checkGround(x + 64, y + 65 + vspeed)))
					{
						temp = obj.position;
						temp.y += vspeed;
						obj.position = temp;
						ammo_vspeed[i] = Mathf.Min(vspeed + 1, 45);
					}
					else
					{
						ammo_vspeed[i] = -1;
						temp = obj.position;
						temp.y = Mathf.Ceil(y / 64) * 64;
						obj.position = temp;
					}
				}
				
				// Ammo collisions
				if (deleteMe == -1)
				{
					if (checkRectIntersect(x + 10, y + 7, x + 54, y + 57, player1.position.x - 32, player1.position.y - 32,
					                       player1.position.x + 32, player1.position.y + 32))
					{
						// Collided with player 1. Delete.
						deleteMe = i;
						
						AudioSource.PlayClipAtPoint(ammoSound, Camera.main.transform.position, 0.5f);
						
						// Switch mastermode?
						if (UnityEngine.Random.Range(0,30) == 0) // 1 in 30 odds
						{
							if (player1_ammo >= 0)
							{
								player1_ammo = -1;
								player1.gameObject.SendMessage("SetMaster", true);
							}
							else
							{
								player1_ammo = 0;
								player1.gameObject.SendMessage("SetMaster", false);
							}
							
							AudioSource.PlayClipAtPoint(shiftSound, Camera.main.transform.position);
							
							started_as_master = 0;
						}
						
						// Load up ammo
						player1_mode = ammo_type[i];
						if ((ammo_type[i] == 1 || ammo_type[i] == 2 || ammo_type[i] == 3) && player1_ammo >= 0)
						{
							player1_ammo = 3;
						}
						else if (ammo_type[i] == 6 && player1_ammo >= 0)
						{
							player1_ammo = 1;
						}
					}
					if (checkRectIntersect(x + 10, y + 7, x + 54, y + 57, player2.position.x - 32, player2.position.y - 32, player2.position.x + 32, player2.position.y + 32))
					{
						// Collided with player 2. Delete.
						deleteMe = i;
						
						AudioSource.PlayClipAtPoint(ammoSound, Camera.main.transform.position, 0.5f);
						
						// Check if it is time to randomly switch mastermode
						if (UnityEngine.Random.Range(0,30) == 0) // 1 in 30 odds
						{
							if (player2_ammo >= 0)
							{
								player2_ammo = -1;
								player2.gameObject.SendMessage("SetMaster", true);
							}
							else
							{
								player2_ammo = 0;
								player2.gameObject.SendMessage("SetMaster", false);
							}
							
							AudioSource.PlayClipAtPoint(shiftSound, Camera.main.transform.position);
							
							started_as_master = 0;
						}
						
						// Load up ammo
						player2_mode = ammo_type[i];
						if ((ammo_type[i] == 1 || ammo_type[i] == 2 || ammo_type[i] == 3) && player2_ammo >= 0)
						{
							player2_ammo = 3;
						}
						else if (ammo_type[i] == 6 && player2_ammo >= 0)
						{
							player2_ammo = 1;
						}
					}
				}
			}
			
			// Fast powerup
			else if (type == 4)
			{
				if (checkPlayerIntersectRect(x - 8, y - 8, x + 8, y + 8, 1))
				{
					if (player1_speed < 28) player1_speed *= 2;
					player1_speed_timer = 600;
					deleteMe = i;
					
					AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
					
				}
				if (checkPlayerIntersectRect(x - 8, y - 8, x + 8, y + 8, 2))
				{
					if (player2_speed < 28) player2_speed *= 2;
					player2_speed_timer = 600;
					deleteMe = i;
					
					AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
				}
			}
			
			// Grav powerup
			else if (type == 5)
			{
				if (checkPlayerIntersectRect(x - 8, y - 8, x + 8, y + 8, 1))
				{
					if (player1_grav > 0.25f) player1_grav /= 2;
					player1_grav_timer = 600;
					deleteMe = i;
					
					AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
				}
				if (checkPlayerIntersectRect(x - 8, y - 8, x + 8, y + 8, 2))
				{
					if (player2_grav > 0.25f) player2_grav /= 2;
					player2_grav_timer = 600;
					deleteMe = i;
					
					AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
				}
			}
			// Camo powerup
			else
			{
				if (checkPlayerIntersectRect(x - 8, y - 8, x + 8, y + 8, 1))
				{
					player1_camo_timer = 600;
					deleteMe = i;
					
					AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
				}
				if (checkPlayerIntersectRect(x - 8, y - 8, x + 8, y + 8, 2))
				{
					if (player2_grav > 0.25f) player2_grav /= 2;
					player2_camo_timer = 600;
					deleteMe = i;
					
					AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
				}
			}
		};
		if (deleteMe != -1)
		{
			Transform obj = ammo[deleteMe];
			ammo.RemoveAt(deleteMe);
			ammo_vspeed.RemoveAt(deleteMe);
			ammo_type.RemoveAt(deleteMe);
			
			// Destroy objects
			UnityEngine.Object.Destroy(obj.gameObject);
		}
		
		// Make projectiles move and collide
		for (int i = 0; i < projectile.Count; i++)
		{
			if(projectile[i] == null) break;
			float speed;
			float speed2;
			int aux;
			int parent;
			float x = (projectile[i] as Transform).position.x;
			float y = (projectile[i] as Transform).position.y;
			int type = projectile_type[i];
			speed = projectile_speed[i];
			speed2 = projectile_speed2[i];
			aux = projectile_aux[i];
			parent = projectile_parent[i];
			Transform projTransform = (projectile[i] as Transform);

			switch(type)
			{
				
			// Bombs
			case 1: {

				temp = projTransform.position;
				temp.y += speed;
				projTransform.position = temp;
				y += speed;
				int temp_int_unity = (int)projectile_speed[i];
				projectile_speed[i] = temp_int_unity + 1;
				
				bool  destroy = false;
				
				// Player collide
				if (parent == 2)
				{
					if (checkPlayerIntersectRect(x - 12, y - 12, x + 12, y + 12, 1))
					{
						AudioSource.PlayClipAtPoint(hurtSound, Camera.main.transform.position);
						destroy = true;
					}
				}
				else
				{
					if (checkPlayerIntersectRect(x - 12, y - 12, x + 12, y + 12, 2))
					{
						AudioSource.PlayClipAtPoint(hurtSound, Camera.main.transform.position);
						destroy = true;
					}
				}
				
				// Ground collide
				if (checkGround(x - 6, y))
				{
					// Destroy ground
					Transform other2 = getGroundSprite(x - 6, y);
					if (other2 != null)
					{
						UnityEngine.Object.Destroy(other2.gameObject);
						setGround(x - 6, y, false);
					}
					destroy = true;
					
					AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
				}
				else if (checkGround(x + 6, y))
				{
					// Destroy ground
					Transform other2 = getGroundSprite(x + 6, y);
					UnityEngine.Object.Destroy(other2.gameObject);
					setGround(x + 6, y, false);
					destroy = true;
					
					AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
				}
				
				if (destroy)
				{
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
			}
		};
		
		// Kill players
		if (player1_hh <= 0)
		{
			if (!dead.gameObject.GetComponent<BodyControl>().visible)
			{
				restartTimer = 138;
				if (started_as_master == 1) play_ground();
				if (player1_ammo < 0)
				{
					dead.gameObject.SendMessage("SetMaster", true);
				}
				else
				{
					dead.gameObject.SendMessage("SetMaster", false);
				}
				player1_vspeed -= 14;
				player1.gameObject.GetComponent<Renderer>().enabled = false;
			}

			dead.position = player1.position;
		}
		if (player2_hh <= 0)
		{
			if (!dead.gameObject.GetComponent<BodyControl>().visible)
			{
				restartTimer = 138;
				if (started_as_master == 2) play_ground();
				if (player2_ammo < 0)
				{
					dead.gameObject.SendMessage("SetMaster", true);
				}
				else
				{
					dead.gameObject.SendMessage("SetMaster", false);
				}
				player2_vspeed -= 14;
				player2.gameObject.GetComponent<Renderer>().enabled = false;
			}
			dead.position = player2.position;
		}
	}
	
	private void  createProjectile ( int type ,   int parent  ){
		Transform creator = player1;
		if (parent == 1)
		{
			creator = player1;
			player1_ammo -= 1;
			if (type == 6) player1_firewait = 30;
			else if (type == 3) player1_firewait = 3;
			else if (type == 2) player1_firewait = 2;
		}
		if (parent == 2)
		{
			creator = player2;
			player2_ammo -= 1;
			if (type == 6) player2_firewait = 30;
			else if (type == 3) player2_firewait = 3;
			else if (type == 2) player2_firewait = 2;
		}
		if (type == 1)
		{
			// Bombs
			clone = Instantiate(protobomb);
			temp.x = creator.position.x;
			temp.y = creator.position.y;
			clone.position = temp;
			
			projectile.Add(clone);
			projectile_type.Add(1);
			projectile_speed.Add(0);
			projectile_parent.Add(parent);
			projectile_speed2.Add(0);
			projectile_aux.Add(0);
		}
		if (type == 2)
		{
			// Rockets
			clone = Instantiate(protorocket);
			temp.x = creator.position.x;
			temp.y = creator.position.y;
			clone.position = temp;

			temp = clone.localScale;
			temp.x = -creator.localScale.x;
			clone.localScale = temp;
			
			
			
			projectile.Add(clone);
			
			projectile_type.Add(2);
			projectile_speed.Add(0);
			projectile_parent.Add(parent);
			projectile_speed2.Add(0);
			projectile_aux.Add(0);
		}
		if (type == 3)
		{
			// Minions
			if ((parent == 1 && player1_ammo < 0)
			    || (parent == 2 && player2_ammo < 0))
			{
				clone = Instantiate(protomasterminion);
			}
			else clone = Instantiate(protominion);
			temp.x = creator.position.x;
			temp.y = creator.position.y;
			clone.position = temp;

			temp = clone.localScale;
			temp.x = -creator.localScale.x / 2;
			temp.y = creator.localScale.y / 2;
			clone.localScale = temp;
			
			projectile.Add(clone);
			
			projectile_type.Add(3);
			float temp_float_unity= Random.Range(-6, 0);
			projectile_speed.Add(temp_float_unity);
			projectile_parent.Add(parent);
			projectile_speed2.Add(-creator.localScale.x * 7.5f);
			projectile_aux.Add(12);
		}
		if (type == 6)
		{
			// Lightning
			clone = Instantiate(protolightning);
			temp = creator.position;
			temp.y = creator.position.y - 32;
			clone.position = temp;

			temp = clone.localScale;
			temp.y = (creator.position.y + 32) / 320;
			clone.localScale = temp;
			
			
			
			projectile.Add(clone);
			
			projectile_type.Add(6);
			projectile_speed.Add(0);
			projectile_parent.Add(parent);
			projectile_speed2.Add(0);
			projectile_aux.Add(20);
		}
		redo_health();
	}

	private bool checkGround ( float x ,   float y  ){
		if (x <= 0.0f || x >= 2944.0f) return true;
		if (x >= 1408.0f && x <= 1536.0f && y <= 1152.0f) return true;
		if (y < floor_level) return false;
		if (y > 1920) return true;
		float x64th = x / 64.0f;
		float relY64th = (y - floor_level) / 64.0f;
		int index = (int) (Mathf.Floor(x64th) * 46.0f +
			Mathf.Floor(relY64th));
		if (index < 0 || index > ground.Count) return true;
		return ground[index];
	}
	
	
	private bool  checkRectIntersect ( float x1 ,   float y1 ,   float x2 ,   float y2 ,   float x3 ,   float y3 ,   float x4 ,   float y4  ){
		// Note: this cheats
		if ((x1 >= x3 && x1 < x4) || (x2 >= x3 && x2 < x4))
		{
			if ((y1 >= y3 && y1 <= y4) || (y2 >= y3 && y2 <= y4)) return true;
		}
		return false;
	}
	
	private bool  checkPlayerIntersect ( float x ,   float y ,   int player  ){
		if (player == 1)
		{
			return ((x > player1.position.x - 18) && (x < player1.position.x + 18) && (y > player1.position.y - 25) && (y < player1.position.y + 25));
		}
		else if (player == 2)
		{
			return ((x > player2.position.x - 18) && (x < player2.position.x + 18) && (y > player2.position.y - 25) && (y < player2.position.y + 25));
		}
		
		return false;
	}
	
	private Transform getGroundSprite ( float x ,   float y  ){
		if (x < 0 || x > 2944) return null;
		if (x >= 1408 && x < 1536 && y < 1152) return null;
		if (y < floor_level) return null;
		if (y > 1920) return null;

		int ind = (int) ((Mathf.Floor(x / 64.0f)) * 46.0f + Mathf.Floor((y - floor_level) / 64.0f));
		return ground_sprite[ind];
	}
	
	private bool  checkPlayerIntersectRect ( float x1 ,   float y1 ,   float x2 ,   float y2 ,   int player  ){
		return (checkPlayerIntersect(x1, y1, player) || checkPlayerIntersect(x2, y2, player) || checkPlayerIntersect(x1, y2, player) || checkPlayerIntersect(x2, y1, player));
	}
	
	private void  setGround ( float x ,   float y ,   bool state  ){
		int index = (int)((Mathf.Floor(x / 64.0f)) * 46.0f + Mathf.Floor((y - floor_level) / 64.0f));
		if(index < 0 || index > ground.Count) return;
		ground[index] = state;
		ground_sprite[index] = null;
	}
	
	private void  explode ( float x ,   float y ,   float radius ,   float max_strength ,   int who_to_hurt  ){
		float d;
		createExplosionSprite(x, y);
		if (who_to_hurt == 1 && player2_hh > 0)
		{
			d = dist(x, y, player1.position.x, player1.position.y);
			if (d < radius)
			{
				player1_hh -= (radius - d) / radius * max_strength;
				
				//AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
				
				redo_health();
			}
		}
		if (who_to_hurt == 2 && player1_hh > 0)
		{
			d = dist(x, y, player2.position.x, player2.position.y);
			if (d < radius)
			{
				player2_hh -= (radius - d) / radius * max_strength;
				
				//AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
				
				redo_health();
			}
		}
	}
	
	private void  play_ground (){
		foreach (GameObject groundInst in GameObject.FindGameObjectsWithTag("Ground"))
		{
			groundInst.SendMessage("Play");
		}
	}
	
	private float  dist ( float x1 ,   float y1 ,   float x2 ,   float y2  ){
		return Mathf.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
	}
	
	private void  createExplosionSprite ( float x ,   float y  ){
		Instantiate(protoexplosion, new Vector3(x, y, 0.0f), Quaternion.identity);
	}
	
	private bool  checkRevealGround ( float x ,   float y  ){
		if (x < 1408 || x > 1536 || y < 896 || y > 1152) return false;
		int i2;
		int j2;
		if (x < 1472) j2 = 0;
		else j2 = 1;
		i2 = (int)Mathf.Floor((y - 896.0f)/64.0f);
		return ground_reveal[i2 * 2 + j2] != null;
	}
	
	private void  removeRevealGround ( float x ,   float y  ){
		int i;
		int j;
		if (x < 1472) j = 0;
		else j = 1;
		i = (int)Mathf.Floor((y - 896.0f)/64.0f);
		Transform victim;
		victim = (ground_reveal[i * 2 + j] as Transform);
		UnityEngine.Object.Destroy(victim.gameObject);
		ground_reveal[i * 2 + j] = null;
	}
	
	private void  redo_health (){
		gui.SendMessage("SupplyHealth1", player1_hh);
		gui.SendMessage("SupplyHealth2", player2_hh);
	}
}