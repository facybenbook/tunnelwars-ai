/*
 * Game.cs
 * 
 * Runs the game. Connects Unity references with the game logic, and directs
 * the behavior of all other scripts.
 * 
 * Right now Game is configured to set up a Q-learning AI against a human player
 * playing with WASDF
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
	public GameObject Protodanger;
	public GameObject Protobelief;
	public GameObject Protopath;
	public GUIControl Gui;
	
	public AudioClip ClickSound;
	public AudioClip HurtSound;
	public AudioClip ShiftSound;
	public AudioClip AmmoSound;
	public AudioClip LightningSound;

	// The current world
	RenderedWorld currentWorld = null;

	// QLearner Object
	QLearner qLearner;

	// The restart timer
	int restartTimer;

	// First-time setup
	void Start () {
	
		// Set up the world with the initial state
		currentWorld = new RenderedWorld(this);
		currentWorld.Display();
		Gui.SetMode(0);

		restartTimer = -1;
		agentList = new List<IAgent>();
		
		// Create the human agent
		agentList.Add(new WASDFAgent(1));

		// Create q-learning object for the AI, and pick up learning where we left off
		qLearner = new QLearner (alpha: 0.65f, epsilon: 0.05f, discount: 0.95f);
		qLearner.OpenSavedData();

		// Create and add the AI agent
		AIAgent ai = new AIAgent(2);
		ai.ResourceScript = this; // For debug rendering
		ai.QLearner = qLearner;
		ai.IsLearning = true;
		agentList.Add(ai);
	}

	// Called every frame
	void Update () {

		// Advance world using our agents
		List<WorldAction> actions = new List<WorldAction>();
		foreach (IAgent agent in agentList) {
			actions.AddRange(agent.GetAction(currentWorld));
		}

		currentWorld.Advance(actions);

		// Timing of things after game is over
		if (currentWorld.IsTerminal()) {
			if (restartTimer == -1) {
				restartTimer = 60 * 3;
			} else if (restartTimer == 60) {
				float termUtil = currentWorld.TerminalUtility();
				if (termUtil > 0.0f) {
					Gui.SetMode(1);
				} else if (termUtil < 0.0f) {
					Gui.SetMode(2);
				}
			} else if (restartTimer == 0) {
				restartGame();
			}
			restartTimer--;
		}
	}

	// Restarts the game
	void restartGame () {

		// Save our Q function
		qLearner.SaveData();

		Application.LoadLevel(0);
	}

	// A list of all agents that are used for the game
	List<IAgent> agentList;

}