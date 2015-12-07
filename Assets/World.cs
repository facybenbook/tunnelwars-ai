/*
 * World.cs
 * 
 * Defines a World object. These objects are states that encapsulate ALL data
 * from a state that is needed to exactly produce a new state.
 * 
 * Because multiple types of world need to implement the ground checking/setting
 * features, World inherits from the WorldWithGround class
 *
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Priority_Queue;

// An enumeration of all possible actions between world states
public enum WorldAction {
	NoAction = 0,
	P1Jump,
	P1Fire,
	P1Left,
	P1Right,
	P2Jump,
	P2Fire,
	P2Left,
	P2Right,
}

// An interface for all objects that can "advance" in the same sense that a world does
// from a list of actions.
public interface IAdvancing {

	// Takes an input list of world actions and updates the state
	void Advance(List<WorldAction> actions);
}

public partial class World : WorldWithGround, IAdvancing {

	// Players
	public Player Player1 { get; set; }
	public Player Player2 { get; set; }

	// The enlargement factor for projectile collisions without players
	public const float EnlargementFactor = 1.1f; // This shouldn't be too big or else
												 // players can slip through the middle
												 // of the projectiles
												

	// Get the initial world state
	public World() {
		init();
	}
	public World(bool empty) {
		if (!empty) {
			init();
		}
	}

	// Clone the world with a deep copy
	public World Clone() {

		// Clone self
		World w = new World(empty: true);

		// Clone players
		Player p1 = w.Player1 = this.Player1.Clone(w);
		Player p2 = w.Player2 = this.Player2.Clone(w);

		// Clone powerups
		w.powerups = new List<Powerup>();
		foreach (Powerup powerup in powerups) {
			w.powerups.Add(powerup.Clone(w));
		}

		// Clone projectiles
		w.projectiles = new List<Projectile>();
		foreach (Projectile projectile in projectiles) {
			w.projectiles.Add(projectile.Clone(w, p1, p2));
		}

		// Clone other properties
		w.startedAsMaster = startedAsMaster;
		w.spawnTimer = spawnTimer;
		w.ground = (bool[,]) ground.Clone();

		// Any other properties you add to the world go here...

		return w;
	}

	// Takes an input list of world actions and updates the state
	virtual public void Advance(List<WorldAction> actions) {
		Advance(actions, true ,true);
	}

	// A world can be advanced without the players being advanced too. If this is the case
	// then projectile collision boundaries are enlarged automatically to account for
	// lack of knowledge about where players are
	virtual public void Advance(List<WorldAction> actions, bool advancePlayers, bool spawnPowerups) {

		// Advance players
		if (advancePlayers) {
			Player1.Advance(actions);
			Player2.Advance(actions);
		}

		// Advance spawn timer and spawn powerups
		if (spawnPowerups) {
			spawnTimer -= 1;
			if (spawnTimer <= 0 && powerups.Count < 64) {
				Powerup.SpawnRandom(this);
				spawnTimer = spawnTimerMax;
			}
		}

		// Advance powerups
		foreach (Powerup powerup in powerups) {
			powerup.Advance(null);
		}

		// Advance projectiles
		int len = projectiles.Count;

		for (int i = len - 1; i >= 0; i--) {
			Projectile projectile = projectiles[i];

			if (projectile.Type == WeaponType.Minions) {

				// Destroy if marked for deletion with a HACK
				if (projectile.Type == WeaponType.None) {
					destroyProjectile(projectile);
				
				// Collide with all other projectiles (that are minions)
				} else if (i != 0) {

					for (int j = i - 1; j >= 0; j--) {
						Projectile other = projectiles[j];
						bool didCollide = projectile.CollideWith(other);
						if (didCollide) {

							// Destroy both parties involved
							destroyProjectile(projectile);
							other.Type = WeaponType.None; // Mark other for deletion
						}
					}
				}
			}

			// In an AI situation, enlarge players
			bool enlarge = advancePlayers == false;
			projectile.Advance(null, enlarge);
		}

		postUpdate();
	}
	
	// Whether or not the world is terminal
	public bool IsTerminal() {

		if (!Player1.IsAlive || !Player2.IsAlive) return true;
		return false;
	}

	// The winner of a terminal world, returned as a float.
	// Player 2 is -1, player 1 is 1. 0 if not yet finished
	public float TerminalUtility() {

		if (Player1.IsAlive && !Player2.IsAlive) {
			return 1.0f;
		} else if (Player2.IsAlive && !Player1.IsAlive) {
			return -1.0f;
		}
		return 0.0f;
	}

	// Determine whether an action is applicable
	public bool CheckActionApplicable(WorldAction action) {
		return Player1.CheckActionApplicable(action) && Player2.CheckActionApplicable(action);
	}

	// Gets the ith powerup
	public int NumPowerups {
		get {
			return powerups.Count;
		}
	}
	public Powerup GetPowerup(int index) {
		return powerups[index];
	}



	// The number of the player that started as master
	int startedAsMaster;

	// List of powerups/projectiles
	protected List<Powerup> powerups;
	protected List<Projectile> projectiles;

	// The spawning timer
	int spawnTimer;
	const int spawnTimerMax = 60;

	// Sets up a new world
	protected void init() {
		initWithMasterPlayer(2);
	}
	protected void initWithMasterPlayer(int masterPlayer) {

		// Create ground array
		initGroundArray();

		spawnTimer = spawnTimerMax;
		startedAsMaster = masterPlayer;

		// Initialize lists/arrays
		powerups = new List<Powerup>();
		projectiles = new List<Projectile>();

		// Add players
		Player1 = createPlayer(masterPlayer == 1, actionSet: 1);
		Player2 = createPlayer(masterPlayer == 2, actionSet: 2);
		Player1.X = 1344.0f;
		Player1.Y = 864.0f;
		Player2.X = 400.0f;//1600.0f;
		Player2.Y = 864.0f;

		// Create bombs that are there at the start
		for (int i = 0; i < 4; i++)
		{
			float x = 0.0f;
			float y = 0.0f;
			if (i == 0) x = 1792.0f;
			else if (i == 1) x = 1952.0f;
			else if (i == 2) x = 1088.0f;
			else x = 928.0f;
			y = FloorLevel - 64.0f;
			createPowerup(x, y, PowerupType.Minions);
		};

		// Fill in regular ground with caves
		for (int i = 0; i < BlocksWidth; i++) {
			for (int j = 0; j < BlocksHeight; j++) {
				
				// Default chance
				float chance = 0.02f;
				
				if (j != 0) {
					// Up chance
					if (!ground[i, j - 1]
					    && !(i == BlocksWidth / 2 - 1 && j == 4)
					    && !(i == BlocksWidth / 2 && j == 4)) chance += 0.4f;
				} else {
					chance += 0.15f; // Surface level chance
				}
				
				if (i != 0) {
					if (!ground[i - 1, j] 
					    && !(i == BlocksWidth / 2 + 1 && j < 4)) chance += 0.4f; // Left chance
				}
				
				float val = Random.Range(0.0f, 1.0f);
				if ((i >= BlocksWidth / 2 - 1 && i < BlocksWidth / 2 + 1 && j <= 3) || (val <= chance)) {
					setGroundByIndex(i, j, false);
					continue;
				}
				else {
					setGroundByIndex(i, j, true);
				}
			};
		};

		postUpdate();
	}

	// Explodes at a point and hurts a player
	virtual protected void explode(float x, float y, float radius, float maxStrength, Player target) {

		float d;
		if (target.IsAlive && maxStrength > 0.0f) {

			d = Util.Distance(x, y, target.X, target.Y);
			if (d < radius) {
				target.Health -= (radius - d) / radius * maxStrength;
			}
		}
	}

	// Player creation
	virtual protected Player createPlayer(bool isMaster, int actionSet) {
		return new Player(this, isMaster, actionSet);
	}

	// Powerup creation/deletion
	virtual protected Powerup createPowerup(float x, float y, PowerupType type) {

		Powerup powerup = new Powerup(this, x, y, type);
		powerups.Add(powerup);
		return powerup;
	}
	virtual protected void destroyPowerup(Powerup powerup) {
		powerups.Remove(powerup);
	}

	// Projectile
	virtual protected Projectile createProjectile(float x, float y, bool facingRight, WeaponType type, int playerNum) {

		Projectile projectile = new Projectile(this, x, y, facingRight, type, playerNum);
		projectiles.Add(projectile);
		return projectile;
	}
	virtual protected void destroyProjectile(Projectile projectile) {
		projectiles.Remove(projectile);
	}

	// Called at the end of each world creation/advance
	virtual protected void postUpdate() {}
}

// World with ground
public class WorldWithGround {

	// Width and height of worlds in blocks
	public const int BlocksWidth = 46;
	public const int BlocksHeight = 16;
	
	// The dimensions of one square block
	public const float BlockSize = 64.0f;
	
	// The Y coordinate of the floor
	public const int FloorLevelJ = 14;
	public const float FloorLevel = BlockSize * FloorLevelJ;

	// The extension into the ground of the wall
	public const int WallDepthJ = 4;
	public const float WallDepth = BlockSize * WallDepthJ;

	// Constructor does nothing. Call init to use
	public WorldWithGround() {}

	// Coordinate conversion
	public static int XToI(float x) {
		return Mathf.FloorToInt(x / BlockSize);
	}
	public static int YToJ(float y) {
		return Mathf.FloorToInt((y - FloorLevel) / BlockSize);
	}
	public static float IToXMin(int i) {
		return i * BlockSize;
	}
	public static float JToYMin(int j) {
		return j * BlockSize + FloorLevel;
	}

	// Checks the ground at a point
	public bool CheckGround(float x, float y) {
		int i = XToI(x);
		int j = YToJ(y);
		return CheckGroundByIndex(i, j);
	}

	// Checks the ground at index (including immutable)
	public bool CheckGroundByIndex(int i, int j) {

		if (CheckGroundImmutableByIndex(i, j)) return true;

		// Check within boundaries (most are covered by immutable check)
		if (j < 0) return false;

		// Check ground
		return ground[i, j];
	}

	// Checks for ground immutable
	public bool CheckGroundImmutable(float x, float y) {
		int i = XToI(x);
		int j = YToJ(y);
		return CheckGroundImmutableByIndex(i, j);
	}

	// Checks for ground immutabile. Negative indices are accepted
	public bool CheckGroundImmutableByIndex(int i, int j) {

		// Check normal boundaries
		if (i < 0 || i >= BlocksWidth) return true; // Left, right
		if (j >= BlocksHeight) return true; // Bottom
		if (j + FloorLevelJ < 0) return true; // Top

		// Middle wall
		if ((i == 22 || i == 23) && j < WallDepthJ) return true;

		return false;
	}



	// An array of bools determining whether ground is filled in
	protected bool[,] ground;

	protected void initGroundArray() {
		ground = new bool[World.BlocksWidth, World.BlocksHeight];
	}

	// Set the ground at a position
	protected void setGround(float x, float y, bool state) {
		int i = XToI(x);
		int j = YToJ(y);
		setGroundByIndex(i, j, state);
	}

	// Sets the ground at indices i, j
	virtual protected void setGroundByIndex(int i, int j, bool value) {
		
		if (i < 0 || i >= BlocksWidth || j < 0 || j >= BlocksHeight) return;
		ground[i, j] = value;
	}
}
