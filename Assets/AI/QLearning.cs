/* 
 * QLearner.cs
 * 
 * The QLearner class is responsible for learning and storing the Q values
 * that associate a generalized state with a strategy. Actually selection and
 * implementation of strategy is carried out by the AIAgent
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

// Class used in QLearner
public class QLearner {
	
	// Filename
	public string PathFromDesktop = "/QLearning/QValues";

	// The learning rate
	public float Alpha { get; set; }

	// Exploration rate
	public float Epsilon { get; set; }

	// Gamma factor
	public float Discount { get; set; }

	// Constructor
	public QLearner (float alpha=0.25f, float epsilon=0.05f, float discount=0.75f) {
		Alpha = alpha;
		Epsilon = epsilon;
		Discount = discount;

		// Set file name
		string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		fileName = path + PathFromDesktop;

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

		string timeSpecific = fileName + DateTime.Now.ToString("yyyyMMdd-HH,mm,ss");
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
		File.Copy(timeSpecific, fileName, true);
	}

	// Open the Q function thats been saved to the disk
	public void OpenSavedData () {

		FileStream file = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite);
		var reader = new StreamReader(file);
		string line;

		Dictionary <String, List<float>> EmptyAmmoDict = new Dictionary <String, List<float>> ();

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
			}

			string keyString = key.ToString();

			// TEMPORARILY CHANGE THE KEY ACCORDINGLY
			Key tempKey = Key.FromString(keyString);

			if ((tempKey.state.AmmoAmount == 0 && tempKey.state.Weapon != WeaponType.None) ||
			    tempKey.state.EnemyAmmoAmount == 0 && tempKey.state.EnemyWeapon != WeaponType.None) {

				if (tempKey.state.AmmoAmount == 0) {

					tempKey.state.Weapon = WeaponType.None;

				} else if (tempKey.state.EnemyAmmoAmount == 0) {

					tempKey.state.EnemyWeapon = WeaponType.None;

				}

				if (EmptyAmmoDict.ContainsKey(tempKey.ToString())) {

					EmptyAmmoDict[tempKey.ToString()].Add(value);

				} else {

					List<float> newList = new List<float>();
					
					newList.Add(value);
					EmptyAmmoDict.Add(tempKey.ToString(),newList);

				}


				EmptyAmmoDict[tempKey.ToString()].Add(value);

			} else {
				utilities[keyString] = value;
			}
		}

		foreach (KeyValuePair<string, List<float>> entry in EmptyAmmoDict) {
			float avgQValue = average(entry.Value);
			utilities[entry.Key] = avgQValue;
		}

		file.Close ();
	}

	// Returns the q value of a state-action tuple
	public float getQValue (SimplifiedWorld state, StrategyType strategy) {

		string key = (new Key (state, strategy)).ToString();

		if (utilities.ContainsKey (key)) {
			return utilities [key];
		} else {
			StrategyType recommended = strategyRecommendationFromState(state);
			return recommended == strategy ? 1.0f : 0.0f;
		}
	}

	// Returns the utility value of a state
	public float ComputeValueFromQValues (SimplifiedWorld state) {

		float currentQValue = 0.0f;
		
		// Iterate through all possible strategies
		foreach (StrategyType strategy in allStrategies) {
			
			float qValue = getQValue (state, strategy);
			
			// If the utility for this strategy is higher than the current best, update
			if (qValue >= currentQValue) {
				currentQValue = qValue;
			}
		}
		
		return currentQValue;
	}

	// Returns the optimal action at a state
	public StrategyType ComputeStrategyFromQValues(SimplifiedWorld state) {

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
	public void UpdateQValue (SimplifiedWorld state, StrategyType strategy, SimplifiedWorld nextState, float reward) {

		// Get the key
		float qValue = getQValue(state, strategy);
		string key = (new Key(state, strategy)).ToString();

		// Update the QValue
		utilities[key] = qValue + Alpha * (reward + Discount * ComputeValueFromQValues(nextState) - qValue);
	}

	public StrategyType GetStrategy (SimplifiedWorld state) {

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



	string fileName;

	StrategyType[] allStrategies = new StrategyType[] {
		StrategyType.Attack,
		StrategyType.RunAway,
		StrategyType.GetAmmo,
		StrategyType.DigDown
	};

	// Map of state-action dictionary to estimated utilities
	Dictionary <String, float> utilities = new Dictionary <String, float>();

	// List of possible strategyies
	List<StrategyType> allPossibleStrategies = new List<StrategyType> ();

	// The hard-coded recommendation for new situations that aren't in the dictionary
	static StrategyType strategyRecommendationFromState(SimplifiedWorld state) {
		
		// Enemy has no ammo
		if (state.EnemyAmmoAmount == 0 || state.EnemyWeapon == WeaponType.None) {
			
			// Both the enemy and the player have no ammo
			if (state.AmmoAmount == 0 || state.Weapon == WeaponType.None) {
				
				return StrategyType.GetAmmo;
			}
			
			// Player has ammo but enemy doesn't
			else {
				
				return StrategyType.Attack;
			}
		}
		
		// Enemy has ammo
		else {
			
			// Enemy has ammo and the player doesn't
			if (state.AmmoAmount == 0 || state.Weapon == WeaponType.None) {
				
				// Player is vulnerable to enemies weapons. Positive Y closeness is player below
				if ((state.EnemyWeapon == WeaponType.Rockets && 
				     (state.YDistanceToEnemy == YCloseness.PosNear || state.YDistanceToEnemy == YCloseness.NegNear)) ||
				    (state.EnemyWeapon == WeaponType.Bombs && state.XDistanceToEnemy == XCloseness.Near) ||
				    (state.EnemyWeapon == WeaponType.Lightning && state.XDistanceToEnemy == XCloseness.Near)) {

					return StrategyType.RunAway;
				}
				
				
				// Player is probably safe from enemy attack
				else {
					return StrategyType.GetAmmo;
				}
			}
			
			// Both the player and the enemy have ammo
			else {
				
				// Player is vulnerable to enemies weapons
				if ((state.EnemyWeapon == WeaponType.Rockets && 
				     (state.YDistanceToEnemy == YCloseness.PosNear || state.YDistanceToEnemy == YCloseness.NegNear)) ||
				    (state.EnemyWeapon == WeaponType.Bombs && state.XDistanceToEnemy == XCloseness.Near) ||
				    (state.EnemyWeapon == WeaponType.Lightning && state.XDistanceToEnemy == XCloseness.Near)) {
					
					return StrategyType.RunAway;
				}
				// Player is probably safe from enemy attack
				else {
					return StrategyType.Attack;
				}    
			}
		}
	}

	float average (List<float> list) {
		
		float currAvg = 0;
		float numEls = 0;
		
		foreach (float num in list) {
			currAvg += num;
			numEls += 1;
		}
		
		return (currAvg / numEls);
		
	}
}

// Key class that stores a state and a strategy
class Key {
	
	public SimplifiedWorld state { get; set; }
	public StrategyType strategy { get; set; }
	
	// Constructors
	public Key (SimplifiedWorld state, StrategyType strategy) {
		this.state = state;
		this.strategy = strategy;
	}
	
	public Key () {
		state = new SimplifiedWorld();
		strategy = StrategyType.Attack;
	}
	
	public override string ToString ()  {
		
		string keyString = "";
		
		// SimplifiedWorld
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
				key.state = SimplifiedWorld.FromString(stateString);
				key.strategy = (StrategyType) Enum.Parse(typeof(StrategyType),propertyArray[i]);
			}
		}
		
		return key;
	}
}