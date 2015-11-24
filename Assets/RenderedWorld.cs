/*
 * RenderedWorld.cs
 * 
 * Same as the World class exept that it becomes visible upon Display.
 * Subsequent updates to rendered worlds affect the Unity scene.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IRenderedWorld {

	// Sets up Unity objects
	void Display();
}

class RenderedWorld : World, IWorld {

	public RenderedWorld() {}

	// Sets up Unity instances to display the world specified. The game controller script
	// is also passed in so that resources can be accessed. Should be called once.
	public RenderedWorld(Game resourceSourceScript) : base(true) {

		// Retain the game script reference for later use
		resourceScript = resourceSourceScript;

		player1Transform = player2Transform = playerDeadTransform = null;
		isSetup = false;

		Setup();
	}

	// Sets up Unity objects
	public void Display() {

		isSetup = true;

		// Find player transforms
		player1Transform = GameObject.Find("Player 1").transform;
		player2Transform = GameObject.Find("Player 2").transform;

		// Add grounds
		for (int i = 0; i < blocksWidth; i++) {

			for (int j = 0; j < blocksHeight; j++) {
				if (ground[i, j]) setGroundByIndex(i, j, true);
			}
		}
	}
	


	// Whether Unity objects have been set up
	bool isSetup;

	// A reference to the script with all resources
	Game resourceScript = null;

	// The ground transforms
	Transform[,] groundTransforms = new Transform[blocksWidth, blocksHeight];

	// The player transforms
	Transform player1Transform;
	Transform player2Transform;
	Transform playerDeadTransform;

	override protected void setGroundByIndex(int i, int j, bool value) {
		ground[i, j] = value;

		// Do nothing if not setup to render
		if (!isSetup) return;

		if (value) {

			// Add Unity object for ground
			Transform clone = Object.Instantiate(resourceScript.Protoground);
			float size = World.blockSize;
			clone.position = new Vector3(i * size, j * size + floorLevel);
			groundTransforms[i, j] = clone;

		} else {

			// Destroy Unity object for ground
			Transform groundTransform = groundTransforms[i, j];
			Transform transform = groundTransforms[i, j];
			UnityEngine.Object.Destroy(transform.gameObject);
		}
	}

	override protected void postUpdate() {

		// Do nothing if not setup to render
		if (!isSetup) return;

		// Update the transforms
		player1Transform.position = new Vector3(player1.X, player1.Y);
		player1Transform.localScale = new Vector3(player1.XScale, 1.0f);
		player2Transform.position = new Vector3(player2.X, player2.Y);
		player2Transform.localScale = new Vector3(player2.XScale, 1.0f);
	}
}
