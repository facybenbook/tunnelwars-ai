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

	// Filename
	public const string FileName = "QValues.txt";

	// The learning rate
	public float Alpha { get; set; }

	// Near vs far sighted factor
	public float Gamma { get; set; }

	// Gamma factor
	public float Discount { get; set; }

	// Constructor
	public QLearning (float alpha, float gamma, float discount) {
		Alpha = alpha;
		Gamma = gamma;
		Discount = discount;
	}

	public void PrintUtilities () {

		int i = 0;

		foreach (KeyValuePair<Util.Key, float> entry in utilities) {

			Util.Key key = entry.Key;
			float value = entry.Value;

			i++;
		}

		Debug.Log (i.ToString ());
	}

	// Saves the Q function to the disk
	public void SaveData () {

		var file = File.Open(FileName, FileMode.CreateNew, FileAccess.ReadWrite);
		var writer = new StreamWriter(file);

		foreach (KeyValuePair<Util.Key, float> entry in utilities) {
			string key = entry.Key.ToString();
			string value = entry.Value.ToString();

			writer.WriteLine(key + " " + value);
		}
	}

	// Open the Q function thats been saved to the disk
	public void OpenSavedData () {

		var file = File.Open(FileName, FileMode.Open, FileAccess.ReadWrite);
		var reader = new StreamReader(file);
		string line;

		while ((line = reader.ReadLine()) != null) {

			string[] keyValueArray = line.Split(' ');
			Util.Key key = new Util.Key();
			float value = 0.0f;
			
			for (int i = 0; i < keyValueArray.Length; i++) {

				if (i == 0) {
					key = Util.Key.FromString(keyValueArray[0] + " " + keyValueArray[1] + " " + keyValueArray[2] + " " + keyValueArray[3] + " " + keyValueArray[4] + " " + keyValueArray[5] + " " + keyValueArray[6]);
				} else if (i == 7) {
					value = float.Parse(keyValueArray[i]);
				}

				utilities[key] = value;
			}
		}
	}

	// Returns the q value of a state-action tuple
	public float getQValue (State state, Strategy strategy) {
		Util.Key key = new Util.Key (state, strategy);
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

		// Get the key
		float qValue;
		Util.Key key = new Util.Key(state,strategy);

		// If the key is already in the dictionary then get the current QValue otherwise set the current QValue to 0
		if (utilities.ContainsKey (key)) {
			qValue = getQValue (state, strategy);
		} else {
			qValue = 0.0f;
		}

		// Update the QValue
		utilities [key] = qValue + Alpha * (reward + Discount * ComputeValueFromQValues (nextState) - qValue);
	}




	// Sets all Q values to 0
	void initializeQValuesToZero () {

		// Iterate through every strategy and state
		foreach (State state in State.AllPossible()) {
			foreach (Strategy strategy in allStrategies) {

				// Get the key for the dictionary and enter in 0.0
				Util.Key key = new Util.Key(state,strategy);
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
	Dictionary <Util.Key, float> utilities = new Dictionary <Util.Key, float>();
}


