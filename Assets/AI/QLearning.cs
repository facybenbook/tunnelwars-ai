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

// Class used in QLearning
public class QLearning {
	
	// Filename
	public const string FileName = "QValues";

	// The learning rate
	public float Alpha { get; set; }

	// Exploration rate
	public float Epsilon { get; set; }

	// Gamma factor
	public float Discount { get; set; }

	// Constructor
	public QLearning (float alpha, float epsilon, float discount) {
		Alpha = alpha;
		Epsilon = epsilon;
		Discount = discount;

		// Construct allPossibleStrategies
		foreach (StrategyType strategy in Enum.GetValues(typeof(StrategyType))) {
			allPossibleStrategies.Add(strategy);
		}
	}

	public void PrintUtilities () {

		int i = 0;
		foreach (KeyValuePair<string, float> entry in utilities) {

			string key = entry.Key;
			float value = entry.Value;

			Debug.Log (key + ", " + value.ToString());

			if (i > 50) return;
			i++;
			Console.Write(key.ToString() + value.ToString());
		}

		Debug.Log (i.ToString());
	}

	// Saves the Q function to the disk
	public void SaveData () {

		string timeSpecific = FileName + DateTime.Now.ToString("yyyyMMdd-HH,mm,ss");
		FileStream file = File.Open(timeSpecific, FileMode.CreateNew, FileAccess.ReadWrite);
		var writer = new StreamWriter(file);

		int i = 0;
		foreach (KeyValuePair<string, float> entry in utilities) {

			string key = entry.Key.ToString();
			string value = entry.Value.ToString();

			i++;
			writer.WriteLine(key + " " + value);
		}
		Debug.Log (i);
		writer.Close();

		// Copy
		File.Copy(timeSpecific, FileName, true);
	}

	// Open the Q function thats been saved to the disk
	public void OpenSavedData () {

		FileStream file = File.Open(FileName, FileMode.Open, FileAccess.ReadWrite);
		var reader = new StreamReader(file);
		string line;

		while ((line = reader.ReadLine()) != null) {

			string[] keyValueArray = line.Split(' ');
			Key key = new Key();
			float value = 0.0f;


			for (int i = 0; i < keyValueArray.Length; i++) {

				if (i == 0) {
					key = Key.FromString(keyValueArray[0] + " " + keyValueArray[1] + " " + keyValueArray[2] + " " + keyValueArray[3] + " " + keyValueArray[4] + " " + keyValueArray[5] + " " + keyValueArray[6]);
				} else if (i == 7) {
					value = float.Parse(keyValueArray[i]);
				}

				string keyString = key.ToString();

				utilities[keyString] = value;
			}
		}



		file.Close ();
	}

	// Returns the q value of a state-action tuple
	public float getQValue (State state, StrategyType strategy) {

		string key = (new Key (state, strategy)).ToString();

		if (utilities.ContainsKey (key)) {
			return utilities [key];
		} else {
			return 0.0f;
		}
	}

	// Returns the utility value of a state
	public float ComputeValueFromQValues (State state) {

		float currentQValue = 0.0f;
		
		// Iterate through all possible strategies
		foreach (StrategyType strategy in allStrategies) {
			
			float qValue = getQValue (state, strategy);
			
			// If the utility for this strategy is higher than the current best
			if (qValue > currentQValue) {
				currentQValue = qValue;
			}
		}
		
		return currentQValue;
	}

	// Returns the optimal action at a state
	public StrategyType ComputeStrategyFromQValues(State state) {

		StrategyType currentStrategy = StrategyType.Attack;
		float maxQValue = float.MinValue;

		// Iterate through all possible strategies
		foreach (StrategyType strategy in allStrategies) {

			float qValue = getQValue (state, strategy);

			// If the utility for this strategy is higher than the current best
			if (qValue > maxQValue) {

				maxQValue = qValue;
				currentStrategy = strategy;
			}
		}

		return currentStrategy;
	}

	// Updates the q value of a state-action tuple
	public void UpdateQValue (State state, StrategyType strategy, State nextState, float reward) {



		// Get the key
		float qValue;
		string key = (new Key(state,strategy)).ToString();

		// If the key is already in the dictionary then get the current QValue otherwise set the current QValue to 0
		if (utilities.ContainsKey(key)) {
			qValue = getQValue(state, strategy);
		} else {
			qValue = 0.0f;
		}

		// Update the QValue
		utilities [key] = qValue + Alpha * (reward + Discount * ComputeValueFromQValues(nextState) - qValue);
	}

	public StrategyType GetStrategy (State state) {

		float random = UnityEngine.Random.Range (0.0f, 1.0f);

		// Take random strategy
		if (random <= Epsilon) {
			int numberOfStrategies = Enum.GetNames(typeof(StrategyType)).Length;
			int randomIndex = UnityEngine.Random.Range(0, numberOfStrategies);
			return allPossibleStrategies[randomIndex];
		}
		// Otherwise take best possible strategy
		else {
			return ComputeStrategyFromQValues(state);
		}
	}




	// Sets all Q values to 0
	void initializeQValuesToZero () {

		// Iterate through every strategy and state
		foreach (State state in State.AllPossible()) {
			foreach (StrategyType strategy in allStrategies) {

				// Get the key for the dictionary and enter in 0.0
				string key = (new Key(state,strategy)).ToString();
				utilities.Add(key,0.0f);

			}
		}
	}

	StrategyType[] allStrategies = new StrategyType[] {
		StrategyType.RunAway,
		StrategyType.GetAmmo,
		StrategyType.Attack,
		StrategyType.DigDown
	};

	// Map of state-action dictionary to estimated utilities
	Dictionary <String, float> utilities = new Dictionary <String, float>();

	// List of possible strategyies
	List<StrategyType> allPossibleStrategies = new List<StrategyType> ();
}

// Key class that stores a state and a strategy
class Key {
	
	public State state { get; set; }
	public StrategyType strategy { get; set; }
	
	// Constructors
	public Key (State state1, StrategyType strategy1) {
		state = state1;
		strategy = strategy1;
	}
	
	public Key () {
		state = new State();
		strategy = StrategyType.Attack;
	}
	
	public override string ToString ()  {
		
		string keyString = "";
		
		// State
		keyString = keyString + state.ToString ();
		keyString = keyString + " ";
		
		// StrategyType
		keyString = keyString + strategy.ToString ();
		
		return keyString;
	}

	static public Key FromString (string keyString) {
		
		Key key = new Key ();
		
		string[] propertyArray = keyString.Split (' ');

		int l = propertyArray.Length;
		string stateString = "";

		for (int i = 0; i < l; i++) {
			if (i < l - 1) {
				stateString += propertyArray[i] + " ";
			} else {
				key.state = State.FromString(stateString);
				key.strategy = (StrategyType) Enum.Parse(typeof(StrategyType),propertyArray[i]);
			}
		}
		
		return key;
	}
}
