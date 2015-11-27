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

class RenderedWorld : World {

	public RenderedWorld() {}

	// Sets up Unity instances to display the world specified. The game controller script
	// is also passed in so that resources can be accessed. Should be called once.
	public RenderedWorld(Game resourceSourceScript) : base(true) {

		// Retain the game script reference for later use
		resourceScript = resourceSourceScript;
		isSetup = false;
		//ToDestroy = new List<Transform>();

		init();
	}

	// Sets up Unity objects
	public void Display() {

		isSetup = true;

		// Add grounds
		for (int i = 0; i < blocksWidth; i++) {

			for (int j = 0; j < blocksHeight; j++) {
				if (ground[i, j]) setGroundByIndex(i, j, true);
			}
		}

		// Players are always added

		// Add immutable ground
		// Left edge
		for (int i = 0; i < 31; i++)
		{
			Transform clone = Object.Instantiate(resourceScript.ProtogroundImmutable.transform);
			clone.position = new Vector3(-64.0f, i * 64.0f);
		};
		// Right edge
		for (int i = 0; i < 31; i++)
		{
			Transform clone = Object.Instantiate(resourceScript.ProtogroundImmutable.transform);
			clone.position = new Vector3(2944.0f, i * 64.0f);
		};
		// Top edge
		for (int i = -1; i < 47; i++)
		{
			Transform clone = Object.Instantiate(resourceScript.ProtogroundImmutable.transform);
			clone.position = new Vector3(i * 64.0f, -64.0f);
		};
		// Bottom edge
		for (int i = -1; i < 47; i++)
		{
			Transform clone = Object.Instantiate(resourceScript.ProtogroundImmutable.transform);
			clone.position = new Vector3(i * 64.0f, 1920.0f);
		};
		// Middle wall
		for (int i = 0; i < 18; i++)
		{
			Transform clone = Object.Instantiate(resourceScript.ProtogroundImmutable.transform);
			clone.position = new Vector3(1408.0f, 64.0f * i);
			clone = Object.Instantiate(resourceScript.ProtogroundImmutable.transform);
			clone.position = new Vector3(1472.0f, 64.0f * i);
		};
	}



	// A reference to the script with all resources
	public Game resourceScript;

	// A list of Unity objects to be destroyed
	//public List<Transform> ToDestroy;

	// Whether Unity objects have been set up
	bool isSetup;

	// The ground transforms
	Transform[,] groundTransforms = new Transform[blocksWidth, blocksHeight];

	override public void Advance(List<WorldAction> actions) {
		base.Advance(actions);

		// Delete objects in list
		/*foreach (Transform obj in ToDestroy) {
			Debug.Log ("DESTROY");
			Object.Destroy(obj.gameObject);
		}*/
	}

	override protected void setGroundByIndex(int i, int j, bool value) {

		// Do functionality
		base.setGroundByIndex(i, j, value);

		// Do nothing if not setup to render
		if (!isSetup) return;

		if (value) {

			// Add Unity object for ground
			Transform clone = Object.Instantiate(resourceScript.Protoground.transform);
			float size = World.blockSize;
			clone.position = new Vector3(i * size, j * size + floorLevel);
			groundTransforms[i, j] = clone;

		} else {

			// Destroy Unity object for ground
			Transform transform = groundTransforms[i, j];
			UnityEngine.Object.Destroy(transform.gameObject);
		}
	}

	// Create renderable subclass of Player and use it when creating players
	public class RenderedPlayer : Player {

		public RenderedPlayer(World parent, bool isMaster, int actionSet) {

			getTransform(actionSet);
			base.init(parent, isMaster, actionSet);
		}

		// Override the set master method so that Unity scene is updated
		public override bool IsMaster {
			get {
				return base.IsMaster;
			}
			set {
				base.IsMaster = value; // Call derived assignment
				playerTransform.gameObject.SendMessage("SetMaster", value);
			}
		}

		public override void Advance(List<WorldAction> actions) {

			base.Advance(actions);
			playerTransform.position = new Vector3(X, Y);
			playerTransform.localScale = new Vector3(XScale, 1.0f);
		}

		// The player's unity transform
		Transform playerTransform;

		// Get transform variable
		void getTransform(int playerNum) {

			Transform t = null;
			if (playerNum == 1) {
				t = GameObject.Find("Player 1").transform;
			} else if (playerNum == 2) {
				t = GameObject.Find("Player 2").transform;
			}
			playerTransform = t;
		}

	}
	override protected Player createPlayer(bool isMaster, int actionSet) {
		return new RenderedPlayer(this, isMaster, actionSet);
	}
	
	// Override powerup class to render
	public class RenderedPowerup : Powerup {

		public Transform ObjectTransform { get; set; }

		public RenderedPowerup(RenderedWorld parent, float x, float y, PowerupType type, Game resourceScript) : base(parent, x, y, type) {

			// Create the unity object
			GameObject cloneSource = null;
			switch (type) {

			case PowerupType.Bombs:
				cloneSource = resourceScript.Protobombs;
				break;
			case PowerupType.Gravity:
				cloneSource = resourceScript.Protogravity;
				break;
			case PowerupType.Lightning:
				cloneSource = resourceScript.Protolightnings;
				break;
			case PowerupType.Minions:
				cloneSource = resourceScript.Protominions;
				break;
			case PowerupType.Rockets:
				cloneSource = resourceScript.Protorockets;
				break;
			case PowerupType.Speed:
				cloneSource = resourceScript.Protospeed;
				break;

			}
			ObjectTransform = Object.Instantiate(cloneSource.transform);
			ObjectTransform.position = new Vector3(x, y);
		}

		public override void Advance(List<WorldAction> actions) {

			base.Advance(actions);
			ObjectTransform.position = new Vector3(X, Y);
		}
	}
	override protected Powerup createPowerup(float x, float y, PowerupType type) {
		RenderedPowerup rendered = new RenderedPowerup(this, x, y, type, resourceScript);
		powerups.Add(rendered as Powerup);
		return rendered;
	}
	override protected void destroyPowerup(Powerup powerup) {

		RenderedPowerup rendered = powerup as RenderedPowerup;

		// Remove Unity object
		Object.Destroy(rendered.ObjectTransform.gameObject);
		base.destroyPowerup(powerup);
	}

	override protected void postUpdate() {

		// Do nothing if not setup to render
		if (!isSetup) return;
	}
}
