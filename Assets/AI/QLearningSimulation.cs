/*
 * Game.cs
 * 
 * Runs the game. Connects Unity references with the game logic, and directs
 * the behavior of all other scripts.
 *
 */

using System.Collections;
using System.Collections.Generic;

public class QLearningSimulation {

	// World
	World currentWorld;

	// QLearning Object
	QLearning qLearner;

	// A list of all agents that are used for the game
	List<IAgent> agentList;

	public void RunQLearning () {

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
		int numberOfGames = 3;

		// Play the number of games specified
		for (int i = 0; i < numberOfGames; i++) {

			// Play game until terminal state
			while (! currentWorld.IsTerminal ()) {
				Update();
			}

			// Restart game
			RestartGame ();
		}

		// Save the QLearning object
		qLearner.SaveData ();
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
		currentWorld = new World ();
		agentList = new List<IAgent>();
		AIAgent ai1 = new AIAgent(1);
		AIAgent ai2 = new AIAgent (2);
		agentList.Add(ai1);
		agentList.Add (ai2);
	}
}