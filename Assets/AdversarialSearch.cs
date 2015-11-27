/*
 * AdversarialSearch.cs
 * 
 * An agent that does simple adversarial search.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AdversarialSearch : PlayerAgentBase {

	// Constructor
	public AdversarialSearch(int player) : base(player) {
	}

	// The center of the AI: get an action for a state
	override public List<WorldAction> GetAction(World world) {

		WorldAction bestAction;

		// Determine which actions are possible
		List<WorldAction> possibleActions = new List<WorldAction>();
		WorldAction[] allPlayerActions = {
			leftAction,
			rightAction,
			fireAction,
			jumpAction
		};
		foreach (WorldAction action in allPlayerActions) {
			if (world.CheckActionApplicable(action)) possibleActions.Add(action);
		}

		// Choose an action at random
		int randIndex = Random.Range(0, possibleActions.Count);
		bestAction = possibleActions[randIndex];
		
		// Return a single-element list with the best action
		return new List<WorldAction>() {
			bestAction
		};
	}
}
