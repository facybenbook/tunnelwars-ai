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
	public const int Level1StepSize = 1;

	public const float Level2DangerDistanceRatio = 100.0f;
	public const int Level2MaxNodesInPrioQueue = 10000;
	public const int Level2MaxExpansions = 200;

	public const int Level3StepSize = 20;
	public Game ResourceScript { get; set; }

	public AIAgent(int player) : base(player) {
		playerNum = player;
		opponentNum = playerNum == 1 ? 2 : 1;
		level1Searcher = new DiscreteAdversarialSearch(playerNum, utilHealthHeuristic,
		                                               getFillerAction, Level1StepSize, 4);
		decisionTimer = 0;
		level2Searcher = new AStar(Level2MaxNodesInPrioQueue, Level2MaxExpansions, level2CostFunction,
		                           level2GoalFunction, level2HeuristicFunction);
		level3Timer = Level3StepSize;
		fillerAction = WorldAction.NoAction;
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
				RenderPath(path);

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

	// Render the level 2 path
	void RenderPath(Path path) {

		if (path == null) return;

		foreach (BlockWorld world in path.States) {
			BlockWorld.BlockPlayer player = world.Player;
			GameObject obj = Object.Instantiate(ResourceScript.Protopath);
			obj.transform.position = new Vector3(player.I * World.BlockSize, player.J * World.BlockSize + World.FloorLevel);
			SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
			renderer.color = new Color(1.0f, 1.0f, 1.0f, 0.25f);
		}
	}



	// Number of the player
	int playerNum;
	int opponentNum;

	// Our adversarial searcher for level 1
	DiscreteAdversarialSearch level1Searcher;
	int decisionTimer;
	WorldAction fillerAction;

	// Level 2 - The danger zone of the opponent
	DangerZone dangerZone;
	BlockWorld blockWorld;
	AStar level2Searcher;

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

	// Level 2 functions
	float level2CostFunction(BlockWorld blockWorld) {
		//return 1.0f;
		return 1.0f + Level2DangerDistanceRatio * dangerZone.CheckDanger(blockWorld.Player.I, blockWorld.Player.J);
	}
	bool level2GoalFunction(BlockWorld blockWorld) {
		//return blockWorld.Player.I == 0;
		return blockWorld.JustCollectedAmmo;
	}
	float level2HeuristicFunction(BlockWorld blockWorld) {

		//return 0.0f;
		//return blockWorld.Player.I;

		BlockWorld.BlockPlayer player = blockWorld.Player;

		// Return distance to nearest ammo
		float minDistance = float.MaxValue;
		foreach (BlockWorld.BlockPowerup powerup in blockWorld.Powerups) {

			float d = Util.ManhattanDistance(powerup.I, powerup.J, player.I, player.J);
			if (d < minDistance) {
				minDistance = d;
			}
		}

		return minDistance;
	}
}
