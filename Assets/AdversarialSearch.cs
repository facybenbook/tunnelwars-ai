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

		WorldAction bestAction = WorldAction.NoAction;

		// Clone the world
		World state = world.Clone();

		// Determine which actions are possible
		List<WorldAction> possibleActions = new List<WorldAction>();
		WorldAction[] allPlayerActions = {
			leftAction,
			rightAction,
			fireAction,
			jumpAction/*,
			WorldAction.NoAction*/
		};
		foreach (WorldAction action in allPlayerActions) {
			if (state.CheckActionApplicable(action)) possibleActions.Add(action);
		}

		// Choose maximum-utility action
		float bestActionUtility = -100000.0f;
		foreach (WorldAction action in possibleActions) {

			float utility = calculateUtility(action, state);
			if (utility > bestActionUtility) {
				bestAction = action;
				bestActionUtility = utility;
			}
		}
		
		// Return a single-element list with the best action
		return new List<WorldAction>() {
			bestAction
		};
	}



	// Determines the utility of an action given the state
	float calculateUtility(WorldAction action, World state) {

		state.Advance(new List<WorldAction>() {WorldAction.NoAction});
		return 0.0f;
	}
}
