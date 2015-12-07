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
	
	public const int Level2MaxNodesInPrioQueue = 10000;
	public const int Level2MaxExpansions = 200;
	public const int DangerZoneRecalcDistance = DangerZone.DistributionSteps;
	public const float PathDeviationRecalcDistance = World.BlocksHeight * 6;

	public const int Level3StepSize = 20;
	public Game ResourceScript { get; set; }
	public QLearning qLearner;
	public bool IsLearning;

	public AIAgent(int player) : base(player) {

		playerNum = player;
		opponentNum = playerNum == 1 ? 2 : 1;

		// Get qLearner
		float alpha = 0.5f;
		float epsilon = 0.3f;
		float discount = 0.95f;
		qLearner = new QLearning (alpha, epsilon, discount);
		IsLearning = false;

		// Set strategy
		strategy = Strategy.StrategyWithType(playerNum, StrategyType.RunAway);

		level1Searcher = new DiscreteAdversarialSearch(playerNum,
		                                               strategy.Level1Heuristic,
		                                               getFillerAction,
		                                               getNewPathIndex,
		                                               Level1StepSize,
		                                               4);
		decisionTimer = 0;
		level2Searcher = new AStar(Level2MaxNodesInPrioQueue, Level2MaxExpansions, strategy.Level2CostFunction,
		                           strategy.Level2GoalFunction, strategy.Level2HeuristicFunction);
		fillerAction = WorldAction.NoAction;
		isFirstTime = true;
	}

	// The center of the AI - get an action
	override public List<WorldAction> GetAction(World world) {

		// The immediate action comes from level 1
		WorldAction bestAction = WorldAction.NoAction;

		// Update level 1 heuristic parameters
		World.Player player = playerNum == 1 ? world.Player1 : world.Player2;


		// Calculate new level 1 action if timer is up
		if (decisionTimer <= 0) {

			ActionWithFiller decision = level1Searcher.ComputeBestAction(world, fillerAction, strategy.NextPathIndex);
			bestAction = decision.Action;
			fillerAction = decision.FillerAction;

			decisionTimer = Level1StepSize;
		
		// Otherwise do the filler action
		} else {
			bestAction = fillerAction;

			// Update level three in a fast frame

			// Create block world and danger zone
			blockWorld = new BlockWorld(playerNum, world);
			dangerZone = new DangerZone(opponentNum, world, blockWorld);

			// Get currentState
			State currentState = new State(world,playerNum);

			// Check if this is the first time GetAction has been called or if one of the cases for changing strategies is hit
			if (isFirstTime || (!previousState.IsEquivalent(currentState))) {

				// Get reward and update QValues if learning
				if (IsLearning) {
					float reward = State.Reward(previousState,strategy.Type,currentState);
					qLearner.UpdateQValue(previousState,strategy.Type,currentState,reward);
				}

				// Get a new strategy
				StrategyType newStrategy = qLearner.GetStrategy(currentState);
				strategy = Strategy.StrategyWithType(playerNum, newStrategy);

				// Reset previous state
				previousState = currentState;
			}
		
			// Must be set before using the level 2 reward, cost, and goal functions
			strategy.Level2DangerZone = dangerZone;

			// Calculate player path
			Path path = level2Searcher.ComputeBestPath(blockWorld);
		
			// Must be set before using the level 1 heuristic
			strategy.SearchPath = path;
			strategy.NextPathIndex = 0;

			//dangerZone.Render(ResourceScript);
			//dangerZone.RenderPlayerBeliefs(ResourceScript);

		}
	
		decisionTimer--;

		if (strategy.SearchPath != null) strategy.SearchPath.Render(ResourceScript, strategy.NextPathIndex);
		/*if (strategy.SearchPath != null) {
			int len = strategy.SearchPath.States.Count;
			if (true) {

				int targetI = strategy.SearchPath.States[len - 1].Player.I;
				int targetJ = strategy.SearchPath.States[len - 1].Player.J;
				float targetX = World.IToXMin(targetI); //+ World.BlockSize / 2.0f;
				float targetY = World.JToYMin(targetJ); //+ World.BlockSize / 2.0f;
				GameObject obj = Object.Instantiate(ResourceScript.Protopath);
				obj.transform.position = new Vector3(targetX, targetY);
			}
		}*/
		
		// Update
		strategy.NextPathIndex = getNewPathIndex(player, strategy.NextPathIndex);

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
	Strategy strategy;
	State previousState;
	bool isFirstTime;

	// Defines the association between level 1 actions and filler actions
	WorldAction getFillerAction(WorldAction action, WorldAction prevFillerAction) {
		
		// Decide filler action - should never be jumping or firing
		if (action == leftAction || action == rightAction || action == WorldAction.NoAction) {
			return action;
		} else {
			return prevFillerAction;
		}
	}

	// Returns the path index - the index of the next path node to target given the current player
	int getNewPathIndex(World.Player player, int currentIndex) {

		if (strategy.SearchPath == null) return currentIndex;
		int pathLength = strategy.SearchPath.States.Count;

		// Get target
		for (int i = 0; i + currentIndex < pathLength; i++) {

			BlockWorld targetWorld = strategy.SearchPath.States[currentIndex + i];
			int targetI = targetWorld.Player.I;
			int targetJ = targetWorld.Player.J;
			
			int playerI = World.XToI(player.X);
			int playerJ = World.YToJ(player.Y);
			
			// If the player is touching the next path coord, then return the index of the new path coord
			if (playerI == targetI && playerJ == targetJ) {
				return currentIndex + i + 1;
			}

		}

		return currentIndex;
	}

	// Checks if the enemy player has changed position
	bool dangerZoneShifted(World world) {

		World.Player opponent = playerNum == 1 ? world.Player2 : world.Player1;
		int opponentI = World.XToI(opponent.X);
		int opponentJ = World.YToJ(opponent.Y);
		return Util.ManhattanDistance(opponentI, opponentJ,
		                              dangerZone.SourceI, dangerZone.SourceJ) > DangerZoneRecalcDistance;
	}

	// Checks if player left path
	bool playerLeftPath(World world, Path path) {

		if (path != null) {
			World.Player player = playerNum == 1 ? world.Player1 : world.Player2;
			float cutOffSquared = PathDeviationRecalcDistance * PathDeviationRecalcDistance;

			foreach (BlockWorld blockWorld in path.States) {
				int pathI = blockWorld.Player.I;
				int pathJ = blockWorld.Player.J;
				float pathX = World.IToXMin(pathI) + World.BlockSize / 2.0f;
				float pathY = World.JToYMin(pathJ) + World.BlockSize / 2.0f;
				if (Util.SquareDistance(player.X, player.Y, pathX, pathY) > cutOffSquared) {
					return true;
				}
			}
		}
		return false;
	}

}
