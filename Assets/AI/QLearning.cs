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
using System.IO

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

	// Returns the optimal action at a state
	public Strategy ComputeStrategyFromQValues (State state) {

		Strategy currentStrategy = Strategy.Attack;
		float currentQValue = 0.0;

		// Iterate through all possible strategies
		foreach (Strategy strategy in allStrategies) {

			Dictionary<State,Strategy> key = new Dictionary<State, Strategy>(){{state, strategy}};

			// If the utility for this strategy is higher than the current best
			if (utilities[key] > currentQValue) {
				currentQValue = utilities[key];
				currentStrategy = strategy;
			}
		}

		return currentStrategy;
	}

	// Updates the q value of a state-action tuple
	public void UpdateQValue (State state, Strategy strategy, State nextState, float reward) {

		// Get the key to utility dictionary
		Dictionary<State,Strategy> key = new Dictionary<State, Strategy>(){{state, strategy}};

		// Get the current QValue
		float qValue = utilities [key];

		// Update the QValue
		utilities [key] = qValue + Alpha * (reward + Discount * computeValueFromQValues (nextState) - qValue);

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

	void Serialize(Dictionary<string, int> dictionary, Stream stream)
	{
		BinaryWriter writer = new BinaryWriter(stream);
		writer.Write(dictionary.Count);
		foreach (var kvp in dictionary)
		{
			writer.Write(kvp.Key);
			writer.Write(kvp.Value);
		}
		writer.Flush();
	}
	
	Dictionary<string, int> Deserialize(Stream stream)
	{
		BinaryReader reader = new BinaryReader(stream);
		int count = reader.ReadInt32();
		var dictionary = new Dictionary<string,int>(count);
		for (int n = 0; n < count; n++)
		{
			var key = reader.ReadString();
			var value = reader.ReadInt32();
			dictionary.Add(key, value);
		}
		return dictionary;                
	}

	// Map of state-action dictionary to estimated utilities
	Dictionary <Dictionary <State,Strategy>, float> utilities;
}


