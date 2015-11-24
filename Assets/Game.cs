/*
 * Game.cs
 * 
 * Runs the game. Connects Unity references with the game logic, and directs
 * the behavior of all other scripts.
 *
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

	// Unity resource references
	public Transform Protoground;
	public Transform ProtogroundImmutable;
	public Transform Protobombs;
	public Transform Protorockets;
	public Transform Protominions;
	public Transform Protolightnings;
	public Transform Protobomb;
	public Transform Protorocket;
	public Transform Protominion;
	public Transform Protomasterminion;
	public Transform Protolightning;
	public Transform Protogravity;
	public Transform Protospeed;
	public Transform Protoexplosion;
	public Transform Dead;
	public GameObject Gui;
	
	public AudioClip ClickSound;
	public AudioClip HurtSound;
	public AudioClip ShiftSound;
	public AudioClip AmmoSound;
	public AudioClip LightningSound;

	// The current world
	World currentWorld = null;

	// First-time setup
	void Start () {

		// Set up the world with the initial state
		currentWorld = new RenderedWorld(this);

		// Create a keyboard control agent for player 1
		player1Agent = new WASDFAgent(1);
	}

	// Called every frame
	void Update () {

		// Advance world using our agents
		List<WorldAction> actions = player1Agent.getAction(currentWorld);
		currentWorld.Advance(actions);
	}

	// Restarts the game
	void RestartGame () {
		
	}



	WASDFAgent player1Agent;

}