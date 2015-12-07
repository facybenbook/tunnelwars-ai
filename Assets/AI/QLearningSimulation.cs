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

public class QLearningSimulation: MonoBehaviour {

	// World
	World currentWorld;

	// QLearning Object
	QLearning qLearner;

	// A list of all agents that are used for the game
	List<IAgent> agentList;

	// Number of games to be played
	int numberOfGames;

	// Specifies which iteration of games we are on
	int gameIteration;

	public void Awake () {

		// Setup framerate and vsynccount
		Application.targetFrameRate = 300;
		QualitySettings.vSyncCount = 1;

		// Set up the wor8ld with the initial state
		currentWorld = new World();
		
		// Create 2 ai agents
		agentList = new List<IAgent>();
		AIAgent ai1 = new AIAgent(1);
		AIAgent ai2 = new AIAgent (2);
		agentList.Add(ai1);
		agentList.Add (ai2);
		
		// Create QLearning obj
		float alpha = 0.5f;
		float gamma = 0.5f;
		float discount = 0.5f;
		qLearner = new QLearning (alpha, gamma, discount);

		// Specify number of games
		numberOfGames = 3;

		// Specify which iteration of games we are on
		gameIteration = 1;

		Debug.Log ("Beginning learning.  Simulating " + numberOfGames.ToString() + " games.");

	}
	
	// Called every frame
	void Update () {

		// Learning is over
		if (currentWorld.IsTerminal () && gameIteration == numberOfGames) {

			Debug.Log ("Learning finished.  Saving QValues...");

			qLearner.SaveData();

			Debug.Log ("Finished saving QValues.  Closing Application.");

			Application.Quit();
		}
		// Game is over but learning continues
		else if (currentWorld.IsTerminal() && gameIteration < numberOfGames) {

			Debug.Log("Finished game " + gameIteration.ToString() + " of " + numberOfGames.ToString() + ".");

			numberOfGames += 1;

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
		}
	}
	
	// Restarts the game
	void RestartGame () {
		currentWorld = new World ();
		agentList = new List<IAgent>();
		AIAgent ai1 = new AIAgent(1);
		AIAgent ai2 = new AIAgent (2);
		agentList.Add(ai1);
		agentList.Add (ai2);
	}
}