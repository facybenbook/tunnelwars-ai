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

	// Number of games to be played
	int numberOfGames;

	// Specifies which iteration of games we are on
	int gameIteration;

	public void Awake () {

		// Setup framerate and vsynccount
		Application.targetFrameRate = 30000;
		QualitySettings.vSyncCount = 1;

		// Set up the wor8ld with the initial state
		currentWorld = new World();

		// Create QLearning obj
		float alpha = 0.65f;
		float epsilon = 0.05f;
		float discount = 0.95f;
		qLearner = new QLearning (alpha, epsilon, discount);
		
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

		FileStream file = File.Open("Test" + DateTime.Now.ToString("yyyyMMdd-HH:mm:ss"), FileMode.CreateNew, FileAccess.ReadWrite);
		var writer = new StreamWriter(file);
		
		int i = 0;
		var list = new List<int>(){3, 5, 10, 11};
		writer.Write(" START ");
		foreach (int a in list) {

			string value = a.ToString();

			writer.WriteLine(value + " HELLO \n");
		}
		
		writer.Close();

		Debug.Log ("Beginning learning.  Simulating " + numberOfGames.ToString() + " games.");

	}
	
	// Called every frame
	void Update () {

		if (currentWorld == null) return;

		// Learning is over
		if (currentWorld.IsTerminal () && gameIteration == numberOfGames) {

			float termUtil = currentWorld.TerminalUtility();
			int winnerNum = termUtil == 1 ? 1 : 2;
			Debug.Log ("Player " + winnerNum.ToString () + " WINS!");

			Debug.Log ("Learning finished.  Saving QValues...");

			qLearner.SaveData();

			Debug.Log ("Finished saving QValues.  Closing Application.");

			currentWorld = null;
			Application.Quit();
		}
		// Game is over but learning continues
		else if (currentWorld.IsTerminal() && gameIteration < numberOfGames) {

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
	}
}