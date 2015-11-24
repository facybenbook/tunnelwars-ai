/*
 * World.cs
 * 
 * Defines a World object. These objects are states that encapsulate ALL data
 * from a state that is needed to exactly produce a new state. 
 *
 */

using UnityEngine;
using System.Collections;

public interface IWorld {
	
}

class World : IWorld {

	// Width and height of worlds in blocks
	public const int blocksWidth = 46;
	public const int blocksHeight = 16;

	// The dimensions of one square block
	public const float blockSize = 64.0f;

	// The y coordinate of the floor
	public const float floorLevel = blockSize * 14.0f;

	// Get the initial world state
	public World() {
		Debug.Log ("!WORLD CONSTRUCTOR");
		Setup();
	}
	public World(bool empty) {
		if (!empty) {
			Setup();
		}
	}


	// An array of bools determining whether ground is filled in
	protected bool[,] ground = new bool[blocksWidth, blocksHeight];

	// Sets up a new world
	protected void Setup() {

		// Fill in regular ground with holes
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
	}

	// Sets the ground at indices i, j
	virtual protected void setGroundByIndex(int i, int j, bool value) {
		ground[i, j] = value;
	}
}
