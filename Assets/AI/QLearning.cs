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

	// Constructors
	public QLearning () {
	}

	// Saves the Q function to the disk
	public void SaveData (string fileName) {
		
	}

	// Returns the optimal action at a state
	public Strategy ComputeStrategyFromQValues (State state) {
		
	}

	// Updates the q value of a state-action tuple
	public void Learn (State state, Strategy action, State nextState, float reward) {
		
	}




	// Sets all Q values to 0
	void initializeQValuesToZero () {

		// Iterate through every strategy and state
		foreach (Strategy strategy in allStrategies) {
			foreach (State state in State.allPossible()) {

			}
		}
	}
	
	// Returns the q value of a state-action tuple
	float getQValue (State state, Strategy action) {

	}

	// Returns the max q value at a state
	float computeValueFromQValues (State state) {
		
	}

	const Strategy[] allStrategies = new Strategy[] {
		Strategy.Attack,
		Strategy.RunAway,
		Strategy.GetAmmo,
		Strategy.DigDown
	};

	// Map of state-action tuples to estimated utilities
	Dictionary <Dictionary <State,float>, Strategy> utilities;
}


