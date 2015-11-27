﻿/*
 * World.cs
 * 
 * Defines a World object. These objects are states that encapsulate ALL data
 * from a state that is needed to exactly produce a new state. 
 *
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// An enumeration of all possible actions between world states
public enum WorldAction {
	P1Left,
	P1Right,
	P1Jump,
	P1Fire,
	P2Left,
	P2Right,
	P2Jump,
	P2Fire
}

// An interface for all objects that can "advance" in the same sense that a world does
// from a list of actions.
public interface IAdvancing {

	// Takes an input list of world actions and updates the state
	void Advance(List<WorldAction> actions);
}

public partial class World : IAdvancing {

	// Get the initial world state
	public World() {
		init();
	}
	public World(bool empty) {
		if (!empty) {
			init();
		}
	}

	// Takes an input list of world actions and updates the state
	virtual public void Advance(List<WorldAction> actions) {

		// Advance players
		player1.Advance(actions);
		player2.Advance(actions);

		// Advance spawn timer and spawn powerups
		spawnTimer -= 1;
		if (spawnTimer <= 0 && powerups.Count < 64) {
			Powerup.SpawnRandom(this);
			spawnTimer = spawnTimerMax;
		}

		// Advance powerups
		foreach (Powerup powerup in powerups) {
			powerup.Advance(null);
		}

		// Advance projectiles
		foreach (Projectile projectile in projectiles) {
			projectile.Advance(null);
		}

		postUpdate();
	}



	// Width and height of worlds in blocks
	protected const int blocksWidth = 46;
	protected const int blocksHeight = 16;
	
	// The dimensions of one square block
	protected const float blockSize = 64.0f;
	
	// The y coordinate of the floor
	protected const float floorLevel = blockSize * 14.0f;


	// Players
	protected Player player1;
	protected Player player2;

	// The number of the player that started as master
	int startedAsMaster;

	// List of powerups/projectiles
	protected List<Powerup> powerups;
	protected List<Projectile> projectiles;

	// The spawning timer
	int spawnTimer;
	const int spawnTimerMax = 60;

	// An array of bools determining whether ground is filled in
	protected bool[,] ground = new bool[blocksWidth, blocksHeight];

	// Sets up a new world
	protected void init() {
		initWithMasterPlayer(0);
	}
	protected void initWithMasterPlayer(int masterPlayer) {

		spawnTimer = spawnTimerMax;
		startedAsMaster = masterPlayer;

		// Initialize lists
		powerups = new List<Powerup>();
		projectiles = new List<Projectile>();

		// Add players
		player1 = createPlayer(masterPlayer == 1, actionSet: 1);
		player2 = createPlayer(masterPlayer == 2, actionSet: 2);
		player1.X = 1344.0f;
		player1.Y = 864.0f;
		player2.X = 1600.0f;
		player2.Y = 864.0f;

		// Create bombs that are there at the start
		for (int i = 0; i < 4; i++)
		{
			float x = 0.0f;
			float y = 0.0f;
			if (i == 0) x = 1792.0f;
			else if (i == 1) x = 1952.0f;
			else if (i == 2) x = 1088.0f;
			else x = 928.0f;
			y = floorLevel - 64.0f;
			createPowerup(x, y, PowerupType.Bombs);
		};

		// Fill in regular ground with caves
		for (int i = 0; i < blocksWidth; i++) {
			for (int j = 0; j < blocksHeight; j++) {
				
				// Default chance
				float chance = 0.02f;
				
				if (j != 0) {
					// Up chance
					if (!ground[i, j - 1]
					    && !(i == blocksWidth / 2 - 1 && j == 4)
					    && !(i == blocksWidth / 2 && j == 4)) chance += 0.4f;
				} else {
					chance += 0.15f; // Surface level chance
				}
				
				if (i != 0) {
					if (!ground[i - 1, j] 
					    && !(i == blocksWidth / 2 + 1 && j < 4)) chance += 0.4f; // Left chance
				}
				
				float val = Random.Range(0.0f, 1.0f);
				if ((i >= blocksWidth / 2 - 1 && i < blocksWidth / 2 + 1 && j <= 3) || (val <= chance)) {
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

	// Checks the ground at a point
	protected bool checkGround(float x, float y) {
		if (x <= 0.0f || x >= 2944.0f) return true;
		if (x >= 1408.0f && x <= 1536.0f && y <= 1152.0f) return true;
		if (y < floorLevel) return false;
		if (y > 1920.0f) return true;
		float relX = x / blockSize;
		float relY = (y - floorLevel) / blockSize;
		int xIndex = Mathf.FloorToInt(relX);
		int yIndex = Mathf.FloorToInt(relY);
		if (xIndex < 0 || xIndex > blocksWidth) return true;
		if (yIndex > blocksHeight) return true;
		return ground[xIndex, yIndex];
	}

	// Intersects two rectangles. Almost
	protected bool checkRectIntersect(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4) {
		// Note: this cheats
		if ((x1 >= x3 && x1 < x4) || (x2 >= x3 && x2 < x4)) {
			if ((y1 >= y3 && y1 <= y4) || (y2 >= y3 && y2 <= y4)) return true;
		}
		return false;
	}

	// Sets the ground at indices i, j
	virtual protected void setGroundByIndex(int i, int j, bool value) {

		if (i < 0 || i >= blocksWidth || j < 0 || j >= blocksHeight) return;
		ground[i, j] = value;
	}

	// Set the ground at a position
	protected void setGround(float x, float y, bool state) {
		int i = Mathf.FloorToInt(x / blockSize);
		int j = (int) (Mathf.FloorToInt(y - floorLevel) / blockSize);
		setGroundByIndex(i, j, state);
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