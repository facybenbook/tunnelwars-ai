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
using System;
using System.IO;

public class QLearningSimulation: MonoBehaviour {

	// World
	World currentWorld;

	// QLearning Object
	QLearning qLearner;

	// A list of all agents that are used for the game
	List<IAgent> agentList;

	const long FramesCutoff = 4800;

	// Number of games to be played
	int numberOfGames;

	// Specifies which iteration of games we are on
	int gameIteration;

	long gameFrames; // Number of frames in a game
	
	public void Start() {
		
		// Set up the world with the initial state
		currentWorld = new World();

		// Create QLearning obj
		float alpha = 0.65f;
		float epsilon = 0.05f;
		float discount = 0.95f;
		qLearner = new QLearning (alpha, epsilon, discount);
		qLearner.OpenSavedData();
		
		// Create 2 ai agents
		agentList = new List<IAgent>();
		AIAgent ai1 = new AIAgent(1);
		ai1.IsLearning = true;
		ai1.QLearner = qLearner;
		AIAgent ai2 = new AIAgent (2);
		ai2.IsLearning = true;
		ai2.QLearner = qLearner;
		agentList.Add(ai1);
		agentList.Add (ai2);

		// Specify number of games
		numberOfGames = 1;

		// Specify which iteration of games we are on
		gameIteration = 1;
		gameFrames = 0;

		Debug.Log ("Beginning learning.  Simulating " + numberOfGames.ToString() + " games before writing.");

	}
	
	// Called every frame
	void Update () {

		if (currentWorld == null) return;

		// Learning is over
		if ((currentWorld.IsTerminal () || gameFrames >= FramesCutoff)
		    && gameIteration == numberOfGames) {

			if (gameFrames < FramesCutoff) {
				float termUtil = currentWorld.TerminalUtility();
				int winnerNum = termUtil == 1 ? 1 : 2;
				Debug.Log ("Player " + winnerNum.ToString () + " WINS!");
			}

			Debug.Log ("Learning finished.  Saving QValues...");


			qLearner.SaveData();

			Debug.Log ("Finished saving QValues.  Restarting...");

			// Restart don't quit
			RestartGame();
		}
		// Game is over but learning continues
		else if ((currentWorld.IsTerminal() || gameFrames >= FramesCutoff)
		         && gameIteration < numberOfGames) {

			float termUtil = currentWorld.TerminalUtility();
			int winnerNum = termUtil == 1 ? 1 : 2;
			Debug.Log ("Player " + winnerNum.ToString () + " WINS!");
			Debug.Log("Finished game " + gameIteration.ToString() + " of " + numberOfGames.ToString() + ".");

			gameIteration += 1;

			RestartGame();
		}
		// Game is not over
		else {

			// Advance world using our agents
			List<WorldAction> actions = new List<WorldAction>();

			foreach (IAgent agent in agentList) {

				actions.AddRange(agent.GetAction(currentWorld));
			}
			
			currentWorld.Advance(actions);

			gameFrames++;
			if (gameFrames == 60 * 60) {
				Debug.Log ("A minute goes by...");
			}
		}
	}
	
	// Restarts the game
	void RestartGame () {
		currentWorld = new World ();
		agentList = new List<IAgent>();
		AIAgent ai1 = new AIAgent(1);
		ai1.IsLearning = true;
		ai1.QLearner = qLearner;
		AIAgent ai2 = new AIAgent (2);
		ai2.IsLearning = true;
		ai2.QLearner = qLearner;
		agentList.Add(ai1);
		agentList.Add (ai2);
		gameFrames = 0;
	}
}