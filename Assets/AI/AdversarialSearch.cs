/*
 * AdversarialSearch.cs
 * 
 * An agent that does simple adversarial search with alpha-beta pruning, except that if
 * the AI chooses to move, it is committed to moving for a number of steps (moveSteps)
 * before a new decision can be made.
 * 
 * Cons:
 * 1. If the player is set to move for 20 frames straight then decide again, the game
 * will lag every 20 frames, and go full speed the rest of the frames. The computing
 * time is therefore very uneven.
 * 
 * 2. This algorithm dodges very poorly when the two players are close together.
 * Committing to an action for 20 whole frames is a lot when there is much less than
 * 20 frames of moving separating the players
 * 
 * 3. Might be a waste of computing power to predict what the player is doing - much more
 * random in actuality. Expectimax would be better, but then there's no pruning.
 * 
 * 
 * Pros:
 * 1. Pulls off some sick dodges when it has time to see the projectiles coming
 * 
 * 2. Relatively good at attacking you. A good heuristic involving projectile trajectories
 * would help this even more.
 * 
 * 3. Having a fast heuristic function helps the speed *A TON*
 * 
 * Overall:
 * This approach would only be useful for really short-term dodging. The step size feature
 * may be completely useless. Maybe increase the step size the deeper into the search it goes?
 * 
 * Maybe we need a World method that simulates a large number of the same consecutive action
 * to a lesser degree of accuracy, but faster?
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AdversarialSearch : PlayerAgentBase {

	// Constructor
	public AdversarialSearch(int player) : base(player) {
		playerNum = player;
		decisionTimer = 1;
		fillerAction = WorldAction.NoAction;
		emptyList = new List<WorldAction>();
	}

	// The center of the AI: get an action for a state
	override public List<WorldAction> GetAction(World world) {

		WorldAction bestAction = WorldAction.NoAction;

		decisionTimer -= 1;

		if (decisionTimer == 0) {

			// Determine which actions are possible
			World.Player currentPlayer = playerNum == 1 ? world.Player1 : world.Player2;
			List<WorldAction> possibleActions = currentPlayer.GetPossibleActions();

			// Choose maximum-utility action
			float bestActionUtility = -100000.0f;
			foreach (WorldAction action in possibleActions) {

				// Make a new clone of the world to run a simulated step
				World newState = world.Clone();
				World.Player newCurrentPlayer = playerNum == 1 ? newState.Player1 : newState.Player2;
				//World.Player newOpponentPlayer = playerNum == 1 ? newState.Player2 : newState.Player1;

				newCurrentPlayer.Advance(new List<WorldAction>(){action});
				newState.Advance(emptyList, false, false);

				// Decide filler action and do it - should never be jumping or firing
				WorldAction potentialFillerAction = getFillerAction(action, fillerAction);
				List<WorldAction> FillerActionList = new List<WorldAction>(){potentialFillerAction};
				for (int i = 0; i < moveSteps - 1; i++) {
					newCurrentPlayer.Advance(FillerActionList);
					newState.Advance(emptyList, false, false);
				}

				float utility = calculateUtility(newState, 0, true, -100000.0f, 100000.0f, potentialFillerAction);
				//float utility = utilDistanceHeuristic(newState);
				//float utility = Util.Distance(newCurrentPlayer.X, newCurrentPlayer.Y, newOpponentPlayer.X, newOpponentPlayer.Y);

				if (utility > bestActionUtility) {
					bestAction = action;
					bestActionUtility = utility;
					fillerAction = potentialFillerAction;
				}

			}

			// Set timer for new decision
			decisionTimer = moveSteps;

		} else {
			bestAction = fillerAction;
		}

		// Return a single-element list with the best action
		return new List<WorldAction>() {bestAction};
	}



	List<WorldAction> emptyList;

	// The number of the player
	int playerNum;

	// Number of steps before a new decision must be made
	int decisionTimer;

	// The action to do in the meantime
	WorldAction fillerAction;

	// The maximum search depth
	protected int maxDepth = 5;

	// The number of steps to repeat for moves
	protected int moveSteps = 4; // Example 15 is make decision 4 times a second

	// Determines the utility of a given state
	virtual protected float calculateUtility(World state, int depth, bool isOpponentsTurn, float alpha, float beta, WorldAction prevFillerAction) {

		// Check if terminal and return terminal utility
		if (state.IsTerminal()) {
			float p1sTermUtil = state.TerminalUtility();
			return (playerNum == 1 ? p1sTermUtil : -p1sTermUtil); // Tested. This works
		}

		// Heuristic for over max depth
		if (depth > maxDepth) {
			// Uncomment below to check heuristic bounds between -1 and 1
			//float h = utilHealthHeuristic(state);
			//if (h > 1.0f || h < -1.0f) Debug.LogWarning("Heuristic has magnitude greater than 1!");
			//return h;
			return utilHealthHeuristic(state);
		}

		if (isOpponentsTurn) {

			// Determine which actions are possible
			World.Player currentPlayer = playerNum == 1 ? state.Player2 : state.Player1;
			List<WorldAction> possibleActions = currentPlayer.GetPossibleActions();
			//List<WorldAction> possibleActions = new List<WorldAction>(){WorldAction.NoAction};

			// Minimize utility
			float minUtil = 100000.0f;

			// Find utility of possible actions
			foreach (WorldAction action in possibleActions) {

				// Make a new clone of the world to run a simulated step
				World newState = state.Clone();
				World.Player newCurrentPlayer = playerNum == 1 ? newState.Player2 : newState.Player1;

				newCurrentPlayer.Advance(new List<WorldAction>(){action});

				// Decide filler action and do it - should never be jumping or firing
				WorldAction potentialFillerAction = getFillerAction(action, prevFillerAction);
				List<WorldAction> fillerActionList = new List<WorldAction>(){potentialFillerAction};
				for (int i = 0; i < moveSteps - 1; i++) {
					newCurrentPlayer.Advance(fillerActionList);
				}
				
				float utility = calculateUtility(newState, depth + 1, false, alpha, beta, potentialFillerAction);
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
			float maxUtil = -100000.0f;

			// Find utility of possible actions
			foreach (WorldAction action in possibleActions) {
				
				// Make a new clone of the world to run a simulated step
				World newState = state.Clone();
				World.Player newCurrentPlayer = playerNum == 1 ? newState.Player1 : newState.Player2;

				newCurrentPlayer.Advance(new List<WorldAction>(){action});
				newState.Advance(emptyList, false, false);

				// Decide filler action and do it - should never be jumping or firing
				WorldAction potentialFillerAction = getFillerAction(action, prevFillerAction);
				List<WorldAction> fillerActionList = new List<WorldAction>(){potentialFillerAction};
				for (int i = 0; i < moveSteps; i++) {
					newCurrentPlayer.Advance(fillerActionList);
					newState.Advance(emptyList, false, false);
				}
				
				float utility = calculateUtility(newState, depth + 1, true, alpha, beta, potentialFillerAction);

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

	// A heuristic estimating the utility of a state based on distance between the players,
	// ammunition, and master mode
	float utilDistanceHeuristic(World state) {

		World.Player currentPlayer = playerNum == 1 ? state.Player1 : state.Player2;
		World.Player opponentPlayer = playerNum == 1 ? state.Player2 : state.Player1;

		float d = Util.ManhattanDistance(currentPlayer.X, currentPlayer.Y, opponentPlayer.X, opponentPlayer.Y);

		// Determine whether to charge the player
		float distanceScalar = 0.0f;
		if (opponentPlayer.IsMaster && !currentPlayer.IsMaster) {
			distanceScalar = 1.0f;
		} else if (currentPlayer.IsMaster && !opponentPlayer.IsMaster) {
			distanceScalar = -1.0f;
		} else if (currentPlayer.Ammo < opponentPlayer.Ammo) {
			distanceScalar = 1.0f;
		} else if (currentPlayer.Ammo >= opponentPlayer.Ammo) {
			distanceScalar = -1.0f;
		}


		//Debug.Log(d * distanceScalar / 3000.0f);

		return d * distanceScalar / 3000.0f;
	}

	// Runs away
	float utilRunAwayHeuristic(World state) {

		World.Player currentPlayer = playerNum == 1 ? state.Player1 : state.Player2;
		World.Player opponentPlayer = playerNum == 1 ? state.Player2 : state.Player1;

		return Util.ManhattanDistance(currentPlayer.X, currentPlayer.Y, opponentPlayer.X, opponentPlayer.Y);
	}

	// The most robust heuristic, drawing upon the distance heuristic
	float utilHealthHeuristic(World state) {

		World.Player currentPlayer = playerNum == 1 ? state.Player1 : state.Player2;
		World.Player opponentPlayer = playerNum == 1 ? state.Player2 : state.Player1;

		float util = currentPlayer.Health / 200.0f - opponentPlayer.Health / 200.0f + 
			utilDistanceHeuristic(state) * 0.05f;
		if (!currentPlayer.IsMaster && !opponentPlayer.IsMaster) util += (currentPlayer.Ammo - opponentPlayer.Ammo) / 3.0f * 0.1f;
		return util;
	}

	// Returns the filler action coupled with an initial action
	WorldAction getFillerAction(WorldAction action, WorldAction prevFillerAction) {

		// Decide filler action - should never be jumping or firing
		if (action == leftAction || action == rightAction || action == WorldAction.NoAction) {
			return action;
		} else {
			return prevFillerAction;
		}
	}
}
