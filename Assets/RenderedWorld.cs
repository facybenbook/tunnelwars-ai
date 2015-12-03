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
		for (int i = 0; i < BlocksWidth; i++) {

			for (int j = 0; j < BlocksHeight; j++) {
				if (ground[i, j]) setGroundByIndex(i, j, true);
			}
		}

		// Players are always added

		// Add immutable ground
		// Left edge
		for (int i = 0; i < 31; i++) {
			Transform clone = Object.Instantiate(resourceScript.ProtogroundImmutable.transform);
			clone.position = new Vector3(-64.0f, i * 64.0f);
		};
		// Right edge
		for (int i = 0; i < 31; i++) {
			Transform clone = Object.Instantiate(resourceScript.ProtogroundImmutable.transform);
			clone.position = new Vector3(2944.0f, i * 64.0f);
		};
		// Top edge
		for (int i = -1; i < 47; i++) {
			Transform clone = Object.Instantiate(resourceScript.ProtogroundImmutable.transform);
			clone.position = new Vector3(i * 64.0f, -64.0f);
		};
		// Bottom edge
		for (int i = -1; i < 47; i++) {
			Transform clone = Object.Instantiate(resourceScript.ProtogroundImmutable.transform);
			clone.position = new Vector3(i * 64.0f, 1920.0f);
		};
		// Middle wall
		for (int i = 0; i < 18; i++) {
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
	Transform[,] groundTransforms = new Transform[BlocksWidth, BlocksHeight];

	override public void Advance(List<WorldAction> actions) {
		base.Advance(actions);
	}

	override protected void setGroundByIndex(int i, int j, bool value) {

		// Re-check if out of bounds
		if (i < 0 || i >= World.BlocksWidth || j < 0 || j >= World.BlocksHeight) return;

		// Do functionality
		base.setGroundByIndex(i, j, value);

		// Do nothing if not setup to render
		if (!isSetup) return;

		if (value) {

			// Add Unity object for ground
			Transform clone = Object.Instantiate(resourceScript.Protoground.transform);
			float size = World.BlockSize;
			clone.position = new Vector3(i * size, j * size + FloorLevel);
			groundTransforms[i, j] = clone;

		} else {

			// Destroy Unity object for ground
			Transform transform = groundTransforms[i, j];
			if (transform) {
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
	}

	// Create renderable subclass of Player and use it when creating players
	public class RenderedPlayer : Player {

		public RenderedPlayer(World parent, bool isMaster, int actionSet, Game resourceScript) {

			this.resourceScript = resourceScript;
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
			playerTransform.localScale = new Vector3(-XScale, 1.0f);

			// Update health bars
			if (playerNum == 1) {
				resourceScript.Gui.SupplyHealth1(Health);
			} else {
				resourceScript.Gui.SupplyHealth2(Health);
			}

			// Check if dead
			if (Health <= 0.0f)
			{
				if (!deadTransform.gameObject.GetComponent<BodyControl>().visible) {

					// TODO: Playing the ground
					deadTransform.gameObject.SendMessage("SetMaster", IsMaster);
					playerTransform.gameObject.GetComponent<Renderer>().enabled = false;
				}
				
				deadTransform.position = playerTransform.position;
			}
		}



		// The player's unity transform
		Transform playerTransform;

		// The dead object
		Transform deadTransform;

		Game resourceScript;

		// Get transform variable
		void getTransform(int playerNum) {

			Transform t = null;
			if (playerNum == 1) {
				t = GameObject.Find("Player 1").transform;
			} else if (playerNum == 2) {
				t = GameObject.Find("Player 2").transform;
			}
			playerTransform = t;
			deadTransform = GameObject.Find("Dead").transform;
		}

	}
	override protected Player createPlayer(bool isMaster, int actionSet) {
		return new RenderedPlayer(this, isMaster, actionSet, resourceScript);
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

	// Override projectile class to render
	public class RenderedProjectile : Projectile {
		
		public Transform ObjectTransform { get; set; }
		
		public RenderedProjectile(World parent, float x, float y, bool facingRight, WeaponType type, int playerNum, Game resourceScript)
			: base(parent, x, y, facingRight, type, playerNum) {

			// Create the unity object
			GameObject cloneSource = null;
			switch (type) {
				
			case WeaponType.Bombs:
				cloneSource = resourceScript.Protobomb;
				break;
			case WeaponType.Lightning:
				cloneSource = resourceScript.Protolightning;
				break;
			case WeaponType.Minions:

				// Determine if player is master so that their minions are darker
				Player sourcePlayer = playerNum == 1 ? parent.Player1 : parent.Player2;
				bool master = sourcePlayer.IsMaster;
				cloneSource = master ? resourceScript.Protomasterminion : resourceScript.Protominion;
				break;

			case WeaponType.Rockets:
				cloneSource = resourceScript.Protorocket;
				break;
			}

			// Instantiate object
			ObjectTransform = Object.Instantiate(cloneSource.transform);
			ObjectTransform.position = new Vector3(x, y);

			// Set object position
			float xScale = facingRight ? 1.0f : -1.0f;
			if (type == WeaponType.Minions) {
				ObjectTransform.localScale = new Vector3(xScale / 2.0f, 0.5f);
			} else if (type == WeaponType.Lightning) {
				ObjectTransform.localScale = new Vector3(xScale, (Y + 32.0f) / 320.0f);
			} else {
				ObjectTransform.localScale = new Vector3(xScale, 1.0f);
			}
		}
		
		public override void Advance(List<WorldAction> actions) {
			base.Advance(actions);
			ObjectTransform.position = new Vector3(X, Y);
		}
		public override void Advance(List<WorldAction> actions, bool enlarge) {
			base.Advance(actions, enlarge);
			ObjectTransform.position = new Vector3(X, Y);
		}
	}
	override protected Projectile createProjectile(float x, float y, bool facingRight, WeaponType type, int playerNum) {
		RenderedProjectile rendered = new RenderedProjectile(this, x, y, facingRight, type, playerNum, resourceScript);
		projectiles.Add(rendered as Projectile);
		return rendered;
	}
	override protected void destroyProjectile(Projectile projectile) {
		
		RenderedProjectile rendered = projectile as RenderedProjectile;
		
		// Remove Unity object
		Object.Destroy(rendered.ObjectTransform.gameObject);
		base.destroyProjectile(projectile);
	}

	// Make a graphic explosion
	override protected void explode(float x, float y, float radius, float maxStrength, Player target) {
		base.explode(x, y, radius, maxStrength, target);
		GameObject explosion = Object.Instantiate(resourceScript.Protoexplosion);
		explosion.transform.position = new Vector3(x, y, 5.0f);
	}

	override protected void postUpdate() {

		// Do nothing if not setup to render
		if (!isSetup) return;
	}
}
