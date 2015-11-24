/*
 * RenderedWorld.cs
 * 
 * Same as the World class exept that it becomes visible upon instantiation.
 * Subsequent updates to rendered worlds affect the Unity scene.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IRenderedWorld : IWorld {
	
}

class RenderedWorld : World, IRenderedWorld {

	public RenderedWorld() {}

	// Sets up Unity instances to display the world specified. The game controller script
	// is also passed in so that resources can be accessed. Should be called once.
	public RenderedWorld(Game resourceSourceScript) : base(true) {

		// Retain the game script reference for later use
		resourceScript = resourceSourceScript;

		Setup();
	}




	// A reference to the script with all resources
	Game resourceScript = null;

	// The ground transforms
	Transform[,] groundTransforms = new Transform[World.blocksWidth, World.blocksHeight];

	override protected void setGroundByIndex(int i, int j, bool value) {
		ground[i, j] = value;

		if (value) {

			// Add Unity object for ground
			Transform clone = Object.Instantiate(resourceScript.Protoground);
			float size = World.blockSize;
			clone.position = new Vector3(i * size, j * size + World.floorLevel);
			groundTransforms[i, j] = clone;

		} else {

			// Destroy Unity object for ground
			Transform groundTransform = groundTransforms[i, j];
			if (groundTransform) {
				Transform transform = groundTransforms[i, j];
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
	}
}
