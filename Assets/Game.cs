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
	public GameObject Protoground;
	public GameObject ProtogroundImmutable;
	public GameObject Protobombs;
	public GameObject Protorockets;
	public GameObject Protominions;
	public GameObject Protolightnings;
	public GameObject Protobomb;
	public GameObject Protorocket;
	public GameObject Protominion;
	public GameObject Protomasterminion;
	public GameObject Protolightning;
	public GameObject Protogravity;
	public GameObject Protospeed;
	public GameObject Protoexplosion;
	public Transform Dead;
	public GUIControl Gui;
	
	public AudioClip ClickSound;
	public AudioClip HurtSound;
	public AudioClip ShiftSound;
	public AudioClip AmmoSound;
	public AudioClip LightningSound;

	// The current world
	RenderedWorld currentWorld = null;

	void  Awake () {
		Application.targetFrameRate = 60;
		QualitySettings.vSyncCount = 1;
	}

	// First-time setup
	void Start () {

		// Set up the world with the initial state
		currentWorld = new RenderedWorld(this);
		currentWorld.Display();
		Gui.GetComponent<GUIControl>().SetMode(0);

		// Create a keyboard control agent for both players
		agentList = new List<IAgent>();
		agentList.Add(new WASDFAgent(2));
		agentList.Add(new AdversarialSearch(1));
	}

	// Called every frame
	void Update () {

		// Advance world using our agents
		List<WorldAction> actions = new List<WorldAction>();
		foreach (IAgent agent in agentList) {
			actions.AddRange(agent.GetAction(currentWorld));
		}

		currentWorld.Advance(actions);
	}

	// Restarts the game
	void RestartGame () {
		
	}

	// A list of all agents that are used for the game
	List<IAgent> agentList;

}