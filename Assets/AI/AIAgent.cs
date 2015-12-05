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
 * 		Heuristic: StrategyType-weighted World evaluation + Level 2 conformance
 * 
 * Level 2 - Bloxel-level classical A* search
 * 		State space: BlockWorld's
 * 		Action space: Simplified directions
 * 		Transition model: Move in direction, taking out block if necessary
 * 		Goal state: StrategyType specific
 * 		Action model: Must have ammo to blow out blocks. Must have walls if climbing
 * 		Cost function: Step number, StrategyType-weighted BlockWorld evaluation (dominate)
 * 		Heuristic: Distance (Euclidean if above ground)
 * 
 * Level 3 - Q-learned strategy selection
 * 		State space: SimplifiedWorld's
 * 		Action space: StrategyType
 * 		Transition model: Set strategy
 * 		Action model: All, except only dig down once.
 * 		Reward function: Delta health, winning the game (dominating term)
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Program {
	public static void Main (string[] args) {

	}
}

public class AIAgent : PlayerAgentBase {

	// Agent parameters
	public const int Level1StepSize = 4;

	public const float Level2DangerDistanceRatio = 100.0f;
	public const int Level2MaxNodesInPrioQueue = 10000;
	public const int Level2MaxExpansions = 200;

	public const int Level3StepSize = 20;
	public Game ResourceScript { get; set; }

	public AIAgent(int player) : base(player) {
		playerNum = player;
		opponentNum = playerNum == 1 ? 2 : 1;
		level1Searcher = new DiscreteAdversarialSearch(playerNum, strategy.Level1Heuristic,
		                                               getFillerAction, Level1StepSize, 4);
		decisionTimer = 0;
		level2Searcher = new AStar(Level2MaxNodesInPrioQueue, Level2MaxExpansions, strategy.Level2CostFunction,
		                           strategy.Level2GoalFunction, strategy.Level2HeuristicFunction);
		level3Timer = Level3StepSize;
		fillerAction = WorldAction.NoAction;

		// Set strategy

	}

	// The center of the AI - get an action
	override public List<WorldAction> GetAction(World world) {

		// The immediate action comes from level 1
		WorldAction bestAction = WorldAction.NoAction;

		// Calculate new level 1 action if timer is up
		if (decisionTimer <= 0) {

			ActionWithFiller decision = level1Searcher.ComputeBestAction(world, fillerAction);
			bestAction = decision.Action;
			fillerAction = decision.FillerAction;

			decisionTimer = Level1StepSize;
		
		// Otherwise do the filler action
		} else {
			bestAction = fillerAction;

			// Update level three in a fast frame
			if (level3Timer <= 0) {

				// Create block world and danger zone
				blockWorld = new BlockWorld(playerNum, world);
				dangerZone = new DangerZone(opponentNum, world, blockWorld);

				//Debug.Log (blockWorld.ApplicableActions().Count);
				//Debug.Log(blockWorld.CheckActionApplicable(BlockWorldAction.Right));

				// Calculate player path
				//Debug.Log (level2HeuristicFunction(blockWorld));
				Path path = level2Searcher.ComputeBestPath(blockWorld);
				if (path != null) path.Render(ResourceScript);

				dangerZone.Render(ResourceScript);
				dangerZone.RenderPlayerBeliefs(ResourceScript);
				level3Timer = Level3StepSize;
			}
		}
	
		decisionTimer--;
		level3Timer--;

		// Return a single-valued list with the best action
		return new List<WorldAction>() {bestAction};
	}


	// Number of the player
	int playerNum;
	int opponentNum;

	// Level 1
	DiscreteAdversarialSearch level1Searcher;
	int decisionTimer;
	WorldAction fillerAction;

	// Level 2
	DangerZone dangerZone;
	BlockWorld blockWorld;
	AStar level2Searcher;

	// Level 3
	int level3Timer;
	Strategy strategy;

	// Defines the association between level 1 actions and filler actions
	WorldAction getFillerAction(WorldAction action, WorldAction prevFillerAction) {
		
		// Decide filler action - should never be jumping or firing
		if (action == leftAction || action == rightAction || action == WorldAction.NoAction) {
			return action;
		} else {
			return prevFillerAction;
		}
	}
}
