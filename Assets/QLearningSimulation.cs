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

public class QLearningSimulation : MonoBehaviour {
	
	bool firstUpdate;
	QLearning obj;
	
	// The current world
	World currentWorld = null;
	
	void  Awake () {
		Application.targetFrameRate = 60;
		QualitySettings.vSyncCount = 1;
	}
	
	// First-time setup
	void Start () {
		
		// Set up the world with the initial state
		currentWorld = new World(this);
		
		// Create a keyboard control agent for both players
		agentList = new List<IAgent>();
		agentList.Add(new WASDFAgent(2));
		AIAgent ai = new AIAgent(1);
		agentList.Add(ai);
		//agentList.Add(new WASDFAgent(1));


		float alpha = 0.5f;
		float gamma = 0.5f;
		float discount = 0.5f;
		obj = new QLearning (alpha, gamma, discount);
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