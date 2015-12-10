/*
 * Game.cs
 * 
 * Runs the game. Connects Unity references with the game logic, and directs
 * the behavior of all other scripts.
 * 
 * Right now Game is configured to set up a Q-learning AI against a human player
 * playing with WASDF
 * 
 * Should not be enabled in the Unity Editor at the same time as QLearningSimulation
 *
 */

// Disable for human versus AI
//#define AIVersusAI

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

	// First-time setup
	void Start () {
	
		// Set up the world with the initial state
		currentWorld = new RenderedWorld(this);
		currentWorld.Display();
		Gui.SetMode(0);

		restartTimer = -1;
		winner = 0;
		agentList = new List<IAgent>();

		// Create q-learning object for the AI, and pick up learning where we left off
		qLearner = new QLearner (alpha: 0.3f, epsilon: 0.25f, discount: 0.66f);
		qLearner.OpenSavedData();

#if !AIVersusAI
		// Create the human agent
		agentList.Add(new WASDFAgent(1));
#else
		// Create AI for player 1
		AIAgent player1Ai = new AIAgent(1);
		player1Ai.ResourceScript = this; // For debug rendering
		player1Ai.QLearner = qLearner;
		player1Ai.IsLearning = true;
		agentList.Add(player1Ai);
#endif

		// Create and add the AI agent
		AIAgent player2Ai = new AIAgent(2);
		player2Ai.ResourceScript = this; // For debug rendering
		player2Ai.QLearner = qLearner;
		player2Ai.IsLearning = true;
		agentList.Add(player2Ai);
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

			float termUtil = currentWorld.TerminalUtility();
			if (termUtil > 0.0f) winner = 1;
			if (termUtil < 0.0f) winner = -1;

			if (restartTimer == -1) {
				restartTimer = 60 * 3;
			} else if (restartTimer == 60) {
				if (winner == 1) {
					Gui.SetMode(1);
				} else if (winner == 2) {
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

	// The current world
	RenderedWorld currentWorld = null;
	
	// QLearner Object
	QLearner qLearner;
	
	// The restart timer
	int restartTimer;

	// The winner
	int winner = 0;


}