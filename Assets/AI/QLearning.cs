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
using System.IO;

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

	public float Discount { get; set; }

	// Constructor
	public QLearning (float alpha, float gamma, float discount) {
		Alpha = alpha;
		Gamma = gamma;
		Discount = discount;
		initializeQValuesToZero ();
	}

	// Saves the Q function to the disk
	public void SaveData (string fileName) {



	}

	// Returns the q value of a state-action tuple
	public float getQValue (State state, Strategy strategy) {
		Dictionary<State,Strategy> key = new Dictionary<State, Strategy>(){{state, strategy}};
		return utilities [key];
	}

	// Returns the utility value of a state
	public float ComputeValueFromQValues (State state) {

		float currentQValue = 0.0f;
		
		// Iterate through all possible strategies
		foreach (Strategy strategy in allStrategies) {
			
			float qValue = getQValue (state, strategy);
			
			// If the utility for this strategy is higher than the current best
			if (qValue > currentQValue) {
				currentQValue = qValue;
			}
		}
		
		return currentQValue;
	}

	// Returns the optimal action at a state
	public Strategy ComputeStrategyFromQValues (State state) {

		Strategy currentStrategy = Strategy.Attack;
		float currentQValue = 0.0f;

		// Iterate through all possible strategies
		foreach (Strategy strategy in allStrategies) {

			float qValue = getQValue (state, strategy);

			// If the utility for this strategy is higher than the current best
			if (qValue > currentQValue) {
				currentQValue = qValue;
				currentStrategy = strategy;
			}
		}

		return currentStrategy;
	}

	// Updates the q value of a state-action tuple
	public void UpdateQValue (State state, Strategy strategy, State nextState, float reward) {

		// Get the current QValue
		float qValue = getQValue (state, strategy);

		// Update the QValue
		Dictionary<State,Strategy> key = new Dictionary<State, Strategy>(){{state, strategy}};
		utilities [key] = qValue + Alpha * (reward + Discount * ComputeValueFromQValues (nextState) - qValue);
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

	Strategy[] allStrategies = new Strategy[] {
		Strategy.Attack,
		Strategy.RunAway,
		Strategy.GetAmmo,
		Strategy.DigDown
	};

	// Map of state-action dictionary to estimated utilities
	Dictionary <Dictionary <State,Strategy>, float> utilities;
}


