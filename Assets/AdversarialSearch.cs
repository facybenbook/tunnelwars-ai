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
	}

	// The center of the AI: get an action for a state
	override public List<WorldAction> GetAction(World world) {

		WorldAction bestAction = WorldAction.NoAction;

		decisionTimer -= 1;

		if (decisionTimer == 0 || true) {

			// Determine which actions are possible
			World.Player currentPlayer = playerNum == 1 ? world.Player1 : world.Player2;
			List<WorldAction> possibleActions = currentPlayer.GetPossibleActions();

			// Choose maximum-utility action
			float bestActionUtility = -100000.0f;
			foreach (WorldAction action in possibleActions) {

				// Make a new clone of the world to run a simulated step
				World newState = world.Clone();
				World.Player newCurrentPlayer = playerNum == 1 ? newState.Player1 : newState.Player2;
				World.Player newOpponentPlayer = playerNum == 1 ? newState.Player2 : newState.Player1;

				int reps = moveSteps;
				for (int i = 0; i < reps; i++) {
					newCurrentPlayer.Advance(new List<WorldAction>(){action});
				}

				float utility = calculateUtility(newState, 0, true, -100000.0f, 100000.0f);
				//float utility = utilDistanceHeuristic(newState);
				//float utility = Util.Distance(newCurrentPlayer.X, newCurrentPlayer.Y, newOpponentPlayer.X, newOpponentPlayer.Y);
				if (utility > bestActionUtility) {
					bestAction = action;
					bestActionUtility = utility;
				}

			}

			if (bestAction == leftAction || bestAction == rightAction) previousDecision = bestAction;
			// Otherwise keeping doing what you're doing
			decisionTimer = moveSteps;

		} else {
			
			bestAction = previousDecision;
		}

		// Return a single-element list with the best action
		return new List<WorldAction>() {bestAction};
	}



	// The number of the player
	int playerNum;

	int decisionTimer;
	WorldAction previousDecision;

	// The maximum search depth
	const int maxDepth = 6;

	// The number of steps to repeat for moves
	const int moveSteps = 5;

	// Determines the utility of a given state
	float calculateUtility(World state, int depth, bool isOpponentsTurn, float alpha, float beta) {

		// Check if terminal and return terminal utility
		if (state.IsTerminal()) {
			float p1sTermUtil = state.TerminalUtility();
			return (playerNum == 1 ? p1sTermUtil : -p1sTermUtil);
		}

		// Heuristic for over max depth
		if (depth > maxDepth) {
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

				for (int i = 0; i < moveSteps; i++) {
					newCurrentPlayer.Advance(new List<WorldAction>(){action});

					// Conform with World.Advance execution order
					if (playerNum == 1) newState.Advance(new List<WorldAction>(), false);
				}
				
				float utility = calculateUtility(newState, depth + 1, false, alpha, beta);
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

				for (int i = 0; i < moveSteps; i++) {
					newCurrentPlayer.Advance(new List<WorldAction>(){action});

					// Conform with World.Advance execution order
					if (playerNum == 2) newState.Advance(new List<WorldAction>(), false);
				}
				
				float utility = calculateUtility(newState, depth + 1, true, alpha, beta);

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

		return d * distanceScalar / 3000.0f;
	}

	float utilHealthHeuristic(World state) {

		World.Player currentPlayer = playerNum == 1 ? state.Player1 : state.Player2;
		World.Player opponentPlayer = playerNum == 1 ? state.Player2 : state.Player1;

		float util = currentPlayer.Health / 200.0f - opponentPlayer.Health / 200.0f + 
			utilDistanceHeuristic(state) * 0.1f;
		if (!currentPlayer.IsMaster && !opponentPlayer.IsMaster) util += (currentPlayer.Ammo - opponentPlayer.Ammo) / 3.0f * 0.2f;
		return util;
	}
}
