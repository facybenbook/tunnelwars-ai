/* 
 * QLearning.cs
 * 
 * The QLearning class is responsible for learning and storing the Q values
 * that associate a generalized state with a strategy. Actually selection and
 * implementation of strategy is carried out by the AIAgent
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum Strategy {
	Attack,
	RunAway,
	GetAmmo,
	DigDown
}

// Class used in QLearning
public class QLearning {

	// The learning rate
	public float Alpha { get; set; }

	// Near vs far sighted factor
	public float Gamma { get; set; }

	// Constructor
	public QLearning () {
		initializeQValuesToZero ();
	}

	// Saves the Q function to the disk
	public void SaveData (string fileName) {
		
	}

	// Returns the optimal action at a state
	public Strategy ComputeStrategyFromQValues (State state) {
		return Strategy.Attack;
	}

	// Updates the q value of a state-action tuple
	public void Learn (State state, Strategy action, State nextState, float reward) {
		
	}




	// Sets all Q values to 0
	void initializeQValuesToZero () {

		// Iterate through every strategy and state
		foreach (State state in State.AllPossible()) {
			foreach (Strategy strategy in allStrategies) {

				// Get the key for the dictionary and enter in 0.0
				Dictionary<State,Strategy> key = new Dictionary<State, Strategy>(){{state, strategy}};
				utilities.Add(key,0.0f);

			}
		}
	}
	
	// Returns the q value of a state-action tuple
	float getQValue (State state, Strategy strategy) {
		Dictionary<State,Strategy> key = new Dictionary<State, Strategy>(){{state, strategy}};
		return utilities [key];
	}

	// Returns the max q value at a state
	float computeValueFromQValues (State state) {
		return 0.0f;
	}

	Strategy[] allStrategies = new Strategy[] {
		Strategy.Attack,
		Strategy.RunAway,
		Strategy.GetAmmo,
		Strategy.DigDown
	};

	// Map of state-action dictionary to estimated utilities
	Dictionary <Dictionary <State,Strategy>, float> utilities;
}


