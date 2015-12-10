/*
 * DiscreteAdversarialSearch.cs
 * 
 * Runs alpha-beta adversarial search on World's this class is responsible for searching
 * through World's, advancing at discrete intervals, and generating the best action
 * possible. Actions are combined with filler actions to fill the space of the discrete
 * intervals.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Define a struct for a action-filler tuple
public struct ActionWithFiller {
	public readonly WorldAction Action;
	public readonly WorldAction FillerAction;
	public ActionWithFiller(WorldAction action, WorldAction filler) {
		Action = action; FillerAction = filler;
	} 
}

// Define a delegate for a World-evaluating heuristic function
public delegate float WorldHeuristic(World world, int pathIndex);

// Define delegate to determine filler action from action
public delegate WorldAction FillerActionDeterminationFunction(WorldAction action, WorldAction prevFillerAction);

// Define delegate to determine the new path index
public delegate int NewPathIndexFunction(World.Player player, int currentPathIndex);

class DiscreteAdversarialSearch {

	// The number of frames for each action
	public int StepSize { get; set; }

	// The number of actions into the future to search
	public int SearchDepth { get; set; }

	// The heuristic function
	public WorldHeuristic Heuristic { get; set; }

	// The filler action function
	public FillerActionDeterminationFunction FillerActionFunction { get; set; }

	// The path index function
	public NewPathIndexFunction PathIndexFunction { get; set; }

	// Constructor
	public DiscreteAdversarialSearch(int playerNum, WorldHeuristic heuristic, FillerActionDeterminationFunction fillerActionFunction,
	                                 NewPathIndexFunction pathIndexFunction, int stepSize=4, int searchDepth=4) {
		init(playerNum, stepSize, searchDepth, heuristic, fillerActionFunction, pathIndexFunction);
	}

	// Searches the world for an action
	public ActionWithFiller ComputeBestAction(World world, WorldAction currentFillerAction,
	                                          int currentPathIndex) {

		WorldAction bestAction = WorldAction.NoAction;
		WorldAction bestFillerAction = WorldAction.NoAction;
		float bestActionUtility = float.MinValue;

		// Determine which actions are possible
		World.Player currentPlayer = playerNum == 1 ? world.Player1 : world.Player2;
		List<WorldAction> possibleActions = currentPlayer.GetPossibleActions();
		
		// Choose maximum-utility action out of all possibilies
		foreach (WorldAction action in possibleActions) {
			
			// Make a new clone of the world to run a simulated step
			World newState = world.Clone();
			World.Player newCurrentPlayer = playerNum == 1 ? newState.Player1 : newState.Player2;

			newCurrentPlayer.Advance(new List<WorldAction>(){action});
			newState.Advance(emptyList, false, false);
			//currentPathIndex = PathIndexFunction(newCurrentPlayer, currentPathIndex);
			
			// Decide filler action and do it repeatedly
			WorldAction potentialFillerAction = FillerActionFunction(action, currentFillerAction);
			List<WorldAction> fillerActionList = new List<WorldAction>(){potentialFillerAction};
			for (int i = 0; i < StepSize - 1; i++) {
				newCurrentPlayer.Advance(fillerActionList);
				newState.Advance(emptyList, false, false);
				//currentPathIndex = PathIndexFunction(newCurrentPlayer, currentPathIndex);
			}

			// Calculate utility and update maximum
			//float utility = 0.0f;
			float utility = calculateUtility(newState, 0, true, float.MinValue, float.MaxValue, potentialFillerAction,
			                                 currentPathIndex);
			
			if (utility > bestActionUtility) {
				bestAction = action;
				bestFillerAction = potentialFillerAction;
				bestActionUtility = utility;
			}
		}

		return new ActionWithFiller(bestAction, bestFillerAction);
	}

	

	List<WorldAction> emptyList;

	// The number of the player to search on behalf of
	int playerNum;

	protected void init(int playerNum, int stepSize, int searchDepth, WorldHeuristic heuristic,
	                    FillerActionDeterminationFunction fillerActionFunction,
	                    NewPathIndexFunction pathIndexFunction) {
		
		this.playerNum = playerNum;
		StepSize = stepSize;
		SearchDepth = searchDepth;
		Heuristic = heuristic;
		FillerActionFunction = fillerActionFunction;
		PathIndexFunction = pathIndexFunction;
		emptyList = new List<WorldAction>();
	}

	// Calculates the utility of a state
	protected float calculateUtility(World state, int depth, bool isOpponentsTurn, float alpha, float beta,
	                                 WorldAction prevFillerAction, int currentPathIndex) {
		
		// Check if terminal and return terminal utility
		if (state.IsTerminal()) {
			float p1sTermUtil = state.TerminalUtility();
			return (playerNum == 1 ? p1sTermUtil : -p1sTermUtil);
		}
		
		// Use heuristic for over max depth
		if (depth > SearchDepth) {
			// Uncomment below to check heuristic bounds between -1 and 1
			//float h = Heuristic(state, currentPathIndex);
			//if (h > 1.0f || h < -1.0f) Debug.LogWarning("Heuristic has magnitude greater than 1!");
			//return h;
			return Heuristic(state, currentPathIndex);
		}
		
		if (isOpponentsTurn) {
			
			// Determine which actions are possible
			World.Player currentPlayer = playerNum == 1 ? state.Player2 : state.Player1;
			List<WorldAction> possibleActions = currentPlayer.GetPossibleActions();
			
			// Minimize utility
			float minUtil = float.MaxValue;
			
			// Find utility of possible actions
			foreach (WorldAction action in possibleActions) {
				
				// Make a new clone of the world to run a simulated step of *only the opponent*
				World newState = state.Clone();
				World.Player newCurrentPlayer = playerNum == 1 ? newState.Player2 : newState.Player1;
				newCurrentPlayer.Advance(new List<WorldAction>(){action});
				
				// Do filler action
				WorldAction potentialFillerAction = FillerActionFunction(action, prevFillerAction);
				List<WorldAction> fillerActionList = new List<WorldAction>(){potentialFillerAction};
				for (int i = 0; i < StepSize - 1; i++) {
					newCurrentPlayer.Advance(fillerActionList);
				}

				// Calculate utility and update minimum
				float utility = calculateUtility(newState, depth + 1, false, alpha, beta, potentialFillerAction,
				                                 currentPathIndex);
				if (utility < minUtil) {
					minUtil = utility;
					
					// Alpha check
					if (minUtil <= alpha) return minUtil;
					
					// Beta update
					if (minUtil < beta) beta = minUtil;
				}
			}
			
			return minUtil;
			
		} else {
			
			// Determine which actions are possible
			World.Player currentPlayer = playerNum == 1 ? state.Player1 : state.Player2;
			List<WorldAction> possibleActions = currentPlayer.GetPossibleActions();
			
			// Maximize utility
			float maxUtil = float.MinValue;
			
			// Find utility of possible actions
			foreach (WorldAction action in possibleActions) {
				
				// Make a new clone of the world to run a simulated step with *player and projectiles*
				World newState = state.Clone();
				World.Player newCurrentPlayer = playerNum == 1 ? newState.Player1 : newState.Player2;
				newCurrentPlayer.Advance(new List<WorldAction>(){action});
				newState.Advance(emptyList, false, false);
				//currentPathIndex = PathIndexFunction(newCurrentPlayer, currentPathIndex);
				
				// Do filler action
				WorldAction potentialFillerAction = FillerActionFunction(action, prevFillerAction);
				List<WorldAction> fillerActionList = new List<WorldAction>(){potentialFillerAction};
				for (int i = 0; i < StepSize - 1; i++) {
					newCurrentPlayer.Advance(fillerActionList);
					newState.Advance(emptyList, false, false);
					//currentPathIndex = PathIndexFunction(newCurrentPlayer, currentPathIndex);
				}

				// Calculate utility and update maximum
				float utility = calculateUtility(newState, depth + 1, true, alpha, beta, potentialFillerAction,
				                                 currentPathIndex);
				if (utility > maxUtil) {
					
					maxUtil = utility;
					
					// Beta check
					if (maxUtil >= beta) return maxUtil;
					
					// Alpha update
					if (maxUtil > alpha) alpha = maxUtil;
				}
			}
			
			return maxUtil;
		}
	}
}