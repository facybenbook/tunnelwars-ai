/*
 * AIAgent.cs
 * 
 * This agent is responsible for the AI. It operates at 3 levels:
 * 
 * 	Level 1 - Short-term discretized adversarial search
 * 		State space: World's (without powerup spawning)
 * 		Action space: WorldActions (keypresses)
 * 		Transition model: Advancement of real gameplay
 * 		Action model: Valid key actions
 * 		Terminal state: Winning (1) or losing (-1)
 * 		Heuristic: Strategy-weighted World evaluation + Level 2 conformance
 * 
 * Level 2 - Bloxel-level classical A* search
 * 		State space: BlockWorld's
 * 		Action space: Simplified directions
 * 		Transition model: Move in direction, taking out block if necessary
 * 		Goal state: Strategy specific
 * 		Action model: Must have ammo to blow out blocks. Must have walls if climbing
 * 		Cost function: Step number, Strategy-weighted BlockWorld evaluation (dominate)
 * 		Heuristic: Distance (Euclidean if above ground)
 * 
 * Level 3 - Q-learned strategy selection
 * 		State space: SimplifiedWorld's
 * 		Action space: Strategy
 * 		Transition model: Set strategy
 * 		Action model: All, except only dig down once.
 * 		Reward function: Delta health, winning the game (dominating term)
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIAgent : PlayerAgentBase {

	// Agent parameters
	public const int Level1StepSize = 4;
	public const int Level3StepSize = 60 * 4;
	public Game ResourceScript { get; set; }

	public AIAgent(int player) : base(player) {
		playerNum = player;
		level1Searcher = new DiscreteAdversarialSearch(playerNum, utilHealthHeuristic,
		                                               getFillerAction, Level1StepSize, 4);
		decisionTimer = 0;
		level3Timer = Level3StepSize;
		fillerAction = WorldAction.NoAction;
	}

	// The center of the AI - get an action
	override public List<WorldAction> GetAction(World world) {

		// The immediate action comes from level 1
		WorldAction bestAction = WorldAction.NoAction;

		// Calculate new level 1 action if timer is up
		if (decisionTimer == 0) {

			ActionWithFiller decision = level1Searcher.ComputeBestAction(world, fillerAction);
			bestAction = decision.Action;
			fillerAction = decision.FillerAction;

			decisionTimer = Level1StepSize;
		
		// Otherwise do the filler action
		} else {
			bestAction = fillerAction;
			decisionTimer--;
		}

		if (level3Timer == 0) {

			BlockWorld blockWorld = new BlockWorld(playerNum, world);

			dangerZone = new DangerZone(2, world, blockWorld);

			dangerZone.Render(ResourceScript);
			dangerZone.RenderPlayerBeliefs(ResourceScript);
			level3Timer = Level3StepSize;
		} else {
			level3Timer--;
		}

		// Return a single-valued list with the best action
		return new List<WorldAction>() {bestAction};
	}



	// Number of the player
	int playerNum;

	// Our adversarial searcher for level 1
	DiscreteAdversarialSearch level1Searcher;

	// Level 1 - Time until next decision must be made, and filler action for meantime
	int decisionTimer;
	WorldAction fillerAction;

	// Level 2 - The danger zone of the opponent
	DangerZone dangerZone;

	// Time until action must end TODO take out
	int level3Timer;

	// Defines the association between level 1 actions and filler actions
	WorldAction getFillerAction(WorldAction action, WorldAction prevFillerAction) {
		
		// Decide filler action - should never be jumping or firing
		if (action == leftAction || action == rightAction || action == WorldAction.NoAction) {
			return action;
		} else {
			return prevFillerAction;
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
	
	// The most robust heuristic, drawing upon the distance heuristic
	float utilHealthHeuristic(World state) {
		
		World.Player currentPlayer = playerNum == 1 ? state.Player1 : state.Player2;
		World.Player opponentPlayer = playerNum == 1 ? state.Player2 : state.Player1;

		float util = currentPlayer.Health / 200.0f - opponentPlayer.Health / 200.0f + 
			utilDistanceHeuristic(state) * 0.05f;
		if (!currentPlayer.IsMaster && !opponentPlayer.IsMaster) util += (currentPlayer.Ammo - opponentPlayer.Ammo) / 3.0f * 0.1f;
		return util;
	}
}
