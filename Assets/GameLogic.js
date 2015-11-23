#pragma strict

var protoground : Transform;
var protogroundImmutable : Transform;
var protobombs : Transform;
var protorockets : Transform;
var protominions : Transform;
var protolightnings : Transform;
var protobomb : Transform;
var protorocket : Transform;
var protominion : Transform;
var protomasterminion : Transform;
var protolightning : Transform;
var protogravity : Transform;
var protospeed : Transform;
var protoexplosion : Transform;
var dead : Transform;
var gui : GameObject;

var clickSound : AudioClip;
var hurtSound : AudioClip;
var shiftSound : AudioClip;
var ammoSound : AudioClip;
var lightningSound : AudioClip;

// STATIC
private static var started_as_master : int = 0;

// Input variables
private var left : boolean = false;
private var right : boolean = false;
private var up : boolean = false;
private var ctrloption : boolean = false;
private var w : boolean = false;
private var a : boolean = false;
private var d : boolean = false;
private var f : boolean = false;
private var w_prev : boolean = false;
private var f_prev : boolean = false;
private var up_prev : boolean = false;
private var ctrloption_prev : boolean = false;

// Global variables
private var player1 : Transform;
private var player2 : Transform;
private var player1_speed : float = 7;
private var player2_speed : float = 7;
private var player1_vspeed : float = 3;
private var player2_vspeed : float = 3;
private var player1_grav : float = 1;
private var player2_grav : float = 1;
private var player1_nofall : boolean = true;
private var player2_nofall : boolean = true;
private var player1_wallstick : int = 0;
private var player2_wallstick : int = 0;
private var player1_firewait : int = 0;
private var player2_firewait : int = 0;

private var player1_mode : int = 0;
private var player2_mode : int = 0;
private var player1_ammo : int = 0;
private var player2_ammo : int = 0;
private var player1_hh : float = 100;
private var player2_hh : float = 100;
private var player1_speed_timer : int = -1;
private var player1_grav_timer : int = -1;
private var player1_camo_timer : int = -1;
private var player2_speed_timer : int = -1;
private var player2_grav_timer : int = -1;
private var player2_camo_timer : int = -1;

private var spawnTimer : int = 7;
private var restartTimer : int = -1;

private var floor_level : float = 64 * 14;
private var ground = new Array ();
private var ground_sprite = new Array();
private var ground_reveal = new Array();

private var ammo = new Array();
private var ammo_type = new Array();
private var ammo_vspeed = new Array();

private var projectile = new Array();
private var projectile_type = new Array();
private var projectile_speed = new Array();
private var projectile_parent = new Array();
private var projectile_speed2 = new Array();
private var projectile_aux = new Array();

private var explosions = new Array();

private var clone : Transform;
private var startTimer : int = 1;

function Awake()
{
    Application.targetFrameRate = 10;
    QualitySettings.vSyncCount = 1;
}

function LateStart()
{   
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

    player1.position.x = 1344;
    player1.position.y = 864;
    player2.position.x = 1600;
    player2.position.y = 864;

    // Make regular ground
    for (var i = 0; i < 46; i++)
    {
        for (var j = 0; j < 16; j++)
        {
            var chance : float = 0.02; // Default chance

            if(j != 0)
            {
                if (!ground[i * 46 + j - 1]
                && !(i == 22 && j == 4)
                && !(i == 23 && j == 4)) chance += 0.40; // Up chance
            }
            else
            {
                chance += 0.15; // Surface level chance
            }
            if(i != 0)
            {
                if (!ground[(i - 1) * 46 + j] 
                && !(i == 24 && j < 4)) chance += 0.40; // Left chance
            }
            var val : float = 0.0;
            val = Random.Range(0.0, 1.0);
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
                
                clone.position.x = i * 64;
                clone.position.y = j * 64 + floor_level;

                ground_sprite[i * 46 + j] = clone;
            }
        };
    };

    // Create immutable ground
    // Left edge
    for (i = 0; i < 31; i++)
    {
        clone = Instantiate(protogroundImmutable);
        clone.position.x = -64;
        clone.position.y = i * 64;
    };
    // Right edge
    for (i = 0; i < 31; i++)
    {
        clone = Instantiate(protogroundImmutable);
        clone.position.x = 2944;
        clone.position.y = i * 64;
    };
    // Top edge
    for (i = -1; i < 47; i++)
    {
        clone = Instantiate(protogroundImmutable);
        clone.position.x = i * 64;
        clone.position.y = -64;
    };
    // Bottom edge
    for (i = -1; i < 47; i++)
    {
        clone = Instantiate(protogroundImmutable);
        clone.position.x = i * 64 ;
        clone.position.y = 1920;
    };
    // Middle wall
    for (i = 0; i < 18; i++)
    {
        clone = Instantiate(protogroundImmutable);
        clone.position.x = 1408 ;
        clone.position.y = 64 * i;
        clone = Instantiate(protogroundImmutable);
        clone.position.x = 1472;
        clone.position.y = 64 * i;
    };

    // Create reveal ground
    for (i = 0; i < 4; i++)
    {
        clone = Instantiate(protoground);
        clone.position.x = 1408;
        clone.position.y = 64 * i + 896;
        // Colorize here
        ground_reveal[i * 2] = clone;

        clone = Instantiate(protoground);
        clone.position.x = 1472;
        clone.position.y = 64 * i + 896;
        // Colorize here
        ground_reveal[i * 2 + 1] = clone;
    };

    // Create bombs that are there at the start
    for (i = 0; i < 4; i++)
    {
        clone = Instantiate(protobombs);
        if (i == 0) clone.position.x = 1792;
        else if (i == 1) clone.position.x = 1952;
        else if (i == 2) clone.position.x = 1088;
        else clone.position.x = 928;
        clone.position.y = floor_level - 64;
        ammo.Push(clone);
        ammo_type.Push(1);
        ammo_vspeed.Push(0);
    };

    gui.SendMessage("SetMode", 0);
}

function LateUpdate()
{
    f_prev = f;
    ctrloption_prev = ctrloption;
    up_prev = up;
    w_prev = w;

    ctrloption = Input.GetAxis("Player 1 Fire") > 0.25;
    f = Input.GetAxis("Player 2 Fire") > 0.25;

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

function Update()
{
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
    right = Input.GetAxis("Player 1 Horizontal") < -0.25;
    left = Input.GetAxis("Player 1 Horizontal") > 0.25;
    d = Input.GetAxis("Player 2 Horizontal") < -0.25;
    a = Input.GetAxis("Player 2 Horizontal") > 0.25;
    up = Input.GetAxis("Player 1 Jump") > 0.25;
    w = Input.GetAxis("Player 2 Jump") > 0.25;

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
        player1.localScale.x = 1;
        if (checkGround(player1.position.x - 18 - player1_speed, player1.position.y - 25) ||
            checkGround(player1.position.x - 18 - player1_speed, player1.position.y + 24))
        {
            player1_vspeed = Mathf.Min(player1_vspeed, 0);
            player1_wallstick = 3;
        }
        else
        {
            player1.position.x -= player1_speed;
        }
    }
    if (a && player2_hh > 0 && !d)
    {
        player2.localScale.x = 1;
        if (checkGround(player2.position.x - 18 - player2_speed, player2.position.y - 25) ||
            checkGround(player2.position.x - 18 - player2_speed, player2.position.y + 24))
        {
            player2_vspeed = Mathf.Min(player2_vspeed, 0);
            player2_wallstick = 3;
        }
        else
        {
            player2.position.x -= player2_speed;
        }
    }
    if (right && player1_hh > 0 && !left)
    {
        player1.localScale.x = -1;
        if (checkGround(player1.position.x + 18 + player1_speed, player1.position.y - 25) ||
            checkGround(player1.position.x + 18 + player1_speed, player1.position.y + 24))
        {
            player1_vspeed = Mathf.Min(player1_vspeed, 0);
            player1_wallstick = 3;
        }
        else
        {
            player1.position.x += player1_speed;
        }
    }
    if (d && player2_hh > 0 && !a)
    {
        player2.localScale.x = -1;
        if (checkGround(player2.position.x + 18 + player2_speed, player2.position.y - 25) ||
            checkGround(player2.position.x + 18 + player2_speed, player2.position.y + 24))
        {
            player2_vspeed = Mathf.Min(player2_vspeed, 0);
            player2_wallstick = 3;
        }
        else
        {
            player2.position.x += player2_speed;
        }
    }

    // Falling
    if (checkGround(player1.position.x, player1.position.y + 33 + player1_vspeed) ||
        checkGround(player1.position.x - 18, player1.position.y + 33 + player1_vspeed) ||
        checkGround(player1.position.x + 18, player1.position.y + 33 + player1_vspeed))
    {
        if (player1_vspeed > 0)
        {
            player1.position.y = Mathf.Floor((player1.position.y - floor_level) / 64) * 64 + floor_level + 39;
        }
        player1_vspeed = Mathf.Min(player1_vspeed, 0);
        player1_nofall = true;
    }
    else
    {
        player1.position.y += player1_vspeed;
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
                player1.position.y = Mathf.Ceil((player1.position.y - floor_level) / 64) * 64 + floor_level - 39;
            }
        }
    }
    if (checkGround(player2.position.x, player2.position.y + 33 + player2_vspeed) ||
        checkGround(player2.position.x - 18, player2.position.y + 33 + player2_vspeed) ||
        checkGround(player2.position.x + 18, player2.position.y + 33 + player2_vspeed))
    {
        if (player2_vspeed > 0)
        {
            player2.position.y = Mathf.Floor((player2.position.y - floor_level) / 64) * 64 + floor_level + 39;
        }
        player2_vspeed = Mathf.Min(player2_vspeed, 0);
        player2_nofall = true;
    }
    else
    {
        player2.position.y += player2_vspeed;
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
                player2.position.y = Mathf.Ceil((player2.position.y - floor_level) / 64) * 64 + floor_level - 39;
            }
        }
    }

    player1_vspeed = Mathf.Min(45, player1_vspeed);
    player2_vspeed = Mathf.Min(45, player2_vspeed);

    if (player1.position.y < 32)
    {
        player1.position.y = 32;
        player1_vspeed = Mathf.Max(0, player1_vspeed);
    }
    if (player2.position.y < 32)
    {
        player2.position.y = 32;
        player2_vspeed = Mathf.Max(0, player2_vspeed);
    }

    // Spawn stuff
    spawnTimer -= 1;
    if (spawnTimer == 0 && ammo_type.length < 64)
    {
        var spawnX : float;
        var spawnY : float;
        do {
            spawnX = Random.Range(0.0, 2880.0);
            spawnY = Random.Range(0.0, 640.0);
        }
        while (spawnX > 1344 && spawnX < 1536);

        var type : int = (UnityEngine.Random.Range(0, 5) + 1);
        var temp : int = UnityEngine.Random.Range(0, 40);
        var vspeed : float = 0;
        if (temp == 0) type = 6;
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
        clone.position.x = spawnX;
        clone.position.y = spawnY;
        ammo.Push(clone);
        ammo_type.Push(type);
        ammo_vspeed.Push(vspeed);

        spawnTimer = 60;
        redo_health();
    }

    // Make ammo fall and collide
    var deleteMe : int = -1;
    for (var i = 0; i < ammo.length; i++)
    {
        var object : Transform;
        var x : float;
        var y : float;
        // var vspeed : float;
        // var type : int;
        object = ammo[i];
        x = object.position.x;
        y = object.position.y;
        vspeed = ammo_vspeed[i];
        type = ammo_type[i];

        // Grav and fast don't fall or collide
        if (type != 4 && type != 5)
        {
            if (ammo_vspeed[i] != -1)
            {
                if (!(checkGround(x, y + 65 + vspeed) || checkGround(x + 64, y + 65 + vspeed)))
                {
                    object.position.y += vspeed;
                    ammo_vspeed[i] = Mathf.Min(vspeed + 1, 45);
                }
                else
                {
                    ammo_vspeed[i] = -1;
                    object.position.y = Mathf.Ceil(y / 64) * 64;
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

                    AudioSource.PlayClipAtPoint(ammoSound, Camera.main.transform.position, 0.5);

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
                    
                    AudioSource.PlayClipAtPoint(ammoSound, Camera.main.transform.position, 0.5);

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
                if (player1_grav > 0.25) player1_grav /= 2;
                player1_grav_timer = 600;
                deleteMe = i;
                
                AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
            }
            if (checkPlayerIntersectRect(x - 8, y - 8, x + 8, y + 8, 2))
            {
                if (player2_grav > 0.25) player2_grav /= 2;
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
                if (player2_grav > 0.25) player2_grav /= 2;
                player2_camo_timer = 600;
                deleteMe = i;
                
                AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
            }
        }
    };
    if (deleteMe != -1)
    {
        object = ammo[deleteMe];
        ammo.Splice(deleteMe, 1);
        ammo_vspeed.Splice(deleteMe, 1);
        ammo_type.Splice(deleteMe, 1);

        // Destroy objects
        UnityEngine.Object.Destroy(object.gameObject);
    }

    // Make projectiles move and collide
    for (i = 0; i < projectile.length; i++)
    {
        if(projectile[i] == null) break;
        var speed : float;
        var speed2 : float;
        var aux : float;
        var parent : int;
        x = (projectile[i] as Transform).position.x;
        y = (projectile[i] as Transform).position.y;
        type = projectile_type[i];
        speed = projectile_speed[i];
        speed2 = projectile_speed2[i];
        aux = projectile_aux[i];
        parent = projectile_parent[i];

        switch(type)
        {

        // Bombs
        case 1:
            (projectile[i] as Transform).position.y += speed;
            y += speed;
            var temp_int_unity : int = projectile_speed[i];
            projectile_speed[i] = temp_int_unity + 1;

            var destroy : boolean = false;

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
                var other : Transform = getGroundSprite(x - 6, y);
                if (other != null)
                {
                    UnityEngine.Object.Destroy(other.gameObject);
                    setGround(x - 6, y, false);
                }
                destroy = true;

                AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
            }
            else if (checkGround(x + 6, y))
            {
                // Destroy ground
                other = getGroundSprite(x + 6, y);
                UnityEngine.Object.Destroy(other.gameObject);
                setGround(x + 6, y, false);
                destroy = true;
                
                AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
            }

            if (destroy)
            {
                // Go boom

                if (parent == 1) explode(x, y + vspeed, 100, 48, 2);
                else explode(x, y + vspeed, 100, 48, 1);

                // Destroy self
                UnityEngine.Object.Destroy((projectile[i] as Transform).gameObject);
                projectile.Splice(i, 1);
                projectile_speed.Splice(i, 1);
                projectile_type.Splice(i, 1);
                projectile_parent.Splice(i, 1);
                projectile_speed2.Splice(i, 1);
                projectile_aux.Splice(i, 1);
                i -= 1;
            }
            break;

        // Rockets
        case 2:
            (projectile[i] as Transform).position.x += (projectile[i] as Transform).localScale.x * 14;
            x += (projectile[i] as Transform).localScale.x * 14;

            destroy = false;

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
                other = getGroundSprite(x + (projectile[i] as Transform).localScale.x * 16, y - 8);
                if (other != null)
                {
                    UnityEngine.Object.Destroy(other.gameObject);
                    setGround(x + (projectile[i] as Transform).localScale.x * 16, y - 8, false);

                    AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);

                }
                destroy = true;
            }
            else if (checkGround(x + (projectile[i] as Transform).localScale.x * 16, y + 8))
            {
                // Destroy ground
                other = getGroundSprite(x + (projectile[i] as Transform).localScale.x * 16, y + 8);
                if (other != null)
                {
                    UnityEngine.Object.Destroy(other.gameObject);
                    setGround(x + (projectile[i] as Transform).localScale.x * 16, y + 8, false);

                    AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
                }
                destroy = true;
            }

            if (destroy)
            {
                // Destroy self
                UnityEngine.Object.Destroy((projectile[i] as Transform).gameObject);
                projectile.Splice(i, 1);
                projectile_speed.Splice(i, 1);
                projectile_type.Splice(i, 1);
                projectile_parent.Splice(i, 1);
                projectile_speed2.Splice(i, 1);
                projectile_aux.Splice(i, 1);
                i -= 1;
            }
            break;

        // Minions
        case 3:
            // Check ground collision
            var falling : boolean = !(checkGround(x - 9, y + 16 + speed) || checkGround(x + 9, y + 16 + speed));
            var pushing_into_wall : boolean = (checkGround(x + speed2 + (projectile[i] as Transform).localScale.x * 18, y + 8) ||
                checkGround(x + speed2 + (projectile[i] as Transform).localScale.x * 18, y - 15));

            destroy = false;

            if (falling)
            {
                (projectile[i] as Transform).position.y += speed;
                y += speed;
                temp_int_unity = projectile_speed[i]; 
temp_int_unity = projectile_speed[i]; projectile_speed[i] = temp_int_unity +  1;
                projectile_aux[i] = 12;
            }
            else
            {
                projectile_speed[i] = 0;
                (projectile[i] as Transform).position.y = Mathf.Ceil((y - floor_level) / 64) * 64 + floor_level - 15;
            }

            if (!pushing_into_wall)
            {
                (projectile[i] as Transform).position.x += speed2;
                x += speed2;
                projectile_aux[i] = 12;
            }
            else
            {
                (projectile[i] as Transform).position.x = x = Mathf.Round(x / 64) * 64 - (projectile[i] as Transform).localScale.x * 28;
                if (falling)
                {
                    // Speed doubles?
                    projectile_speed2[i] = 20 * (projectile[i] as Transform).localScale.x;
                }
                else
                {
                    var blow_ground = false;
                    // Pushing into a wall and also not falling. Wait then die. 
temp_int_unity = projectile_aux[i]; projectile_aux[i] = temp_int_unity - 1;
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
                        var sprite_right : Transform = getGroundSprite(x + 128 * (projectile[i] as Transform).localScale.x, y);
                        var sprite_below : Transform = getGroundSprite(x, y + 64);
                        var blow_right : boolean = (null != sprite_right && checkGround(x + 128 * (projectile[i] as Transform).localScale.x, y));
                        var blow_below : boolean = (null != sprite_below && checkGround(x, y + 64));
                        if (blow_right && blow_below)
                        {
                            // Can't blow up both at once. Randomly select which
                            var r : int = Random.Range(0,2);
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
            if (i != projectile_type.length - 1)
            {
                // If not the last check for minions ahead
                for (var j : int = i + 1; j < projectile_type.length; j++)
                {
                    if (projectile_type[j] == 3 && projectile_parent[j] != parent)
                    {
                        var other_x : float = (projectile[j] as Transform).position.x;
                        var other_y : float = (projectile[j] as Transform).position.y;

                        // See if collided
                        if ((x - other_x)*(x - other_x)+(y - other_y)*(y - other_y) < 1024)
                        {
                            // Collided. Blow below
                            if (checkGround(x, y + 64))
                            {
                                sprite_below = getGroundSprite(x, y + 64);
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
                            projectile.Splice(j, 1);
                            projectile_speed.Splice(j, 1);
                            projectile_type.Splice(j, 1);
                            projectile_parent.Splice(j, 1);
                            projectile_speed2.Splice(j, 1);
                            projectile_aux.Splice(j, 1);
                            destroy = true;
                        }
                    }
                };
            }

            if (destroy)
            {
                // Destroy self
                UnityEngine.Object.Destroy((projectile[i] as Transform).gameObject);
                projectile.Splice(i, 1);
                projectile_speed.Splice(i, 1);
                projectile_type.Splice(i, 1);
                projectile_parent.Splice(i, 1);
                projectile_speed2.Splice(i, 1);
                projectile_aux.Splice(i, 1);
                i -= 1;
            }
            break;

        // Lightning
        case 6:
            projectile_aux[i] = aux - 1;
            if (aux == 20)
            {
                // Play sound
                AudioSource.PlayClipAtPoint(lightningSound, Camera.main.transform.position);

                // Break blocks above by cycling through
                for (j = 0; j < 46; j++)
                {
                    for (var k : int = 0; k < 16; k++)
                    {
                        var xx : float = j * 64;
                        var yy : float = k * 64 + floor_level;
                        if (yy < y && xx + 64 > x - 16 && xx < x + 16)
                        {
                            // Get rid of ground
                            var temp_transform : Transform = (ground_sprite[j * 46 + k] as Transform);
                            if (temp_transform != null) UnityEngine.Object.Destroy(temp_transform.gameObject);
                            ground[j * 46 + k] = 0;
                        }
                    };
                };
            }
            else if (aux == 0)
            {
                // Destroy self
                UnityEngine.Object.Destroy((projectile[i] as Transform).gameObject);
                projectile.Splice(i, 1);
                projectile_speed.Splice(i, 1);
                projectile_type.Splice(i, 1);
                projectile_parent.Splice(i, 1);
                projectile_speed2.Splice(i, 1);
                projectile_aux.Splice(i, 1);
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
    };

    // Kill players
    if (player1_hh <= 0)
    {
        if (!dead.gameObject.GetComponent(DeadControl).visible)
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
            player1.gameObject.GetComponent.<Renderer>().enabled = false;
        }
        dead.position.x = player1.position.x;
        dead.position.y = player1.position.y;
    }
    if (player2_hh <= 0)
    {
        if (!dead.gameObject.GetComponent(DeadControl).visible)
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
            player2.gameObject.GetComponent.<Renderer>().enabled = false;
        }
        dead.position.x = player2.position.x;
        dead.position.y = player2.position.y;
    }
}

private function createProjectile(type : int, parent : int)
{
    var creator : Transform;
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
        clone.position.x = creator.position.x;
        clone.position.y = creator.position.y;
        
        projectile.Push(clone);
        projectile_type.Push(1);
        projectile_speed.Push(0);
        projectile_parent.Push(parent);
        projectile_speed2.Push(0);
        projectile_aux.Push(0);
    }
    if (type == 2)
    {
        // Rockets
        clone = Instantiate(protorocket);
        clone.position.x = creator.position.x;
        clone.position.y = creator.position.y;
        clone.localScale.x = -creator.localScale.x;
        
        
        
        projectile.Push(clone);
        
        projectile_type.Push(2);
        projectile_speed.Push(0);
        projectile_parent.Push(parent);
        projectile_speed2.Push(0);
        projectile_aux.Push(0);
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
        clone.position.x = creator.position.x;
        clone.position.y = creator.position.y;
        clone.localScale.x = -creator.localScale.x / 2;
        clone.localScale.y = creator.localScale.y / 2;
        
        projectile.Push(clone);
        
        projectile_type.Push(3);
        var temp_float_unity = Random.Range(-6, 0);
        projectile_speed.Push(temp_float_unity);
        projectile_parent.Push(parent);
        projectile_speed2.Push(-creator.localScale.x * 7.5);
        projectile_aux.Push(12);
    }
    if (type == 6)
    {
        // Lightning
        clone = Instantiate(protolightning);
        clone.position.x = creator.position.x;
        clone.position.y = creator.position.y - 32;
        clone.localScale.y = (creator.position.y + 32) / 320;
        
        
        
        projectile.Push(clone);
        
        projectile_type.Push(6);
        projectile_speed.Push(0);
        projectile_parent.Push(parent);
        projectile_speed2.Push(0);
        projectile_aux.Push(20);
    }
    redo_health();
}

private function checkGround(x : float, y : float)
{
    if (x <= 0 || x >= 2944) return true;
    if (x >= 1408 && x <= 1536 && y <= 1152) return true;
    if (y < floor_level) return false;
    if (y > 1920) return true;
    var index : int = Mathf.Floor(x / 64) * 46 + Mathf.Floor((y - floor_level) / 64);
    if (index < 0 || index > ground.length) return true;
    return ground[index];
}


private function checkRectIntersect(x1 : float, y1 : float, x2 : float, y2 : float, x3 : float, y3 : float, x4 : float, y4 : float)
{
    // Note: this cheats
    if ((x1 >= x3 && x1 < x4) || (x2 >= x3 && x2 < x4))
    {
        if ((y1 >= y3 && y1 <= y4) || (y2 >= y3 && y2 <= y4)) return true;
    }
    return false;
}

private function checkPlayerIntersect(x : float, y : float, player : int)
{
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

private function getGroundSprite(x : float, y : float) : Transform
{
    if (x < 0 || x > 2944) return null;
    if (x >= 1408 && x < 1536 && y < 1152) return null;
    if (y < floor_level) return null;
    if (y > 1920) return null;

    return ground_sprite[(Mathf.Floor(x / 64)) * 46 + Mathf.Floor((y - floor_level) / 64)];
}

private function checkPlayerIntersectRect(x1 : float, y1 : float, x2 : float, y2 : float, player : int)
{
    return (checkPlayerIntersect(x1, y1, player) || checkPlayerIntersect(x2, y2, player) || checkPlayerIntersect(x1, y2, player) || checkPlayerIntersect(x2, y1, player));
}

private function setGround(x : float, y : float, state : boolean)
{
    var index : int = (Mathf.Floor(x / 64)) * 46 + Mathf.Floor((y - floor_level) / 64);
    if(index < 0 || index > ground.length) return;
    ground[index] = state;
    ground_sprite[index] = null;
}

private function explode(x : float, y : float, radius : float, max_strength : float, who_to_hurt : int)
{
    var d : float;
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

private function play_ground()
{
    for (var groundInst : GameObject in GameObject.FindGameObjectsWithTag("Ground"))
    {
        groundInst.SendMessage("Play");
    }
}

private function dist(x1 : float, y1 : float, x2 : float, y2 : float)
{
    return Mathf.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
}

private function createExplosionSprite(x : float, y : float)
{
    Instantiate(protoexplosion, Vector3(x, y, 0.0), Quaternion.identity);
}

private function checkRevealGround(x : float, y : float)
{
    if (x < 1408 || x > 1536 || y < 896 || y > 1152) return 0;
    var i2 : int;
    var j2 : int;
    if (x < 1472) j2 = 0;
    else j2 = 1;
    i2 = Mathf.Floor((y - 896)/64);
    return ground_reveal[i2 * 2 + j2] != null;
}

private function removeRevealGround(x : float, y : float)
{
    var i : int;
    var j : int;
    if (x < 1472) j = 0;
    else j = 1;
    i = Mathf.Floor((y - 896)/64);
    var victim : Transform;
    victim = (ground_reveal[i * 2 + j] as Transform);
    UnityEngine.Object.Destroy(victim.gameObject);
    ground_reveal[i * 2 + j] = null;
}

private function redo_health()
{
    gui.SendMessage("SupplyHealth1", player1_hh);
    gui.SendMessage("SupplyHealth2", player2_hh);
}