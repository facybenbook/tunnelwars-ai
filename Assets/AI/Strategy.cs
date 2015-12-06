﻿/*
 * StrategyType.cs
 * 
 * The Strategy class encapsulates all strategy-specific logic. A strategy influences
 * both Level 1 in the form of a heuristic, but also Level 2 in the form of the AStar
 * heuristic, goal, and distance-danger factor. All logic in Strategy is completely
 * dependent on the AIAgent's 3-level approach.
 * 
 * All approximations of the AI are defined in this file and in SimplifiedState.
 * 
 */

using UnityEngine;
using System.Collections;

public enum StrategyType {
	Attack,
	RunAway,
	GetAmmo,
	DigDown
}

// Abstract class for strategies
public abstract class Strategy {

	// The max number of path points that have influence on level 1
	public const int MaxPathPointsWithInfluence = 5;

	// Level two - increase to give priority to danger over distance of path
	public const float Level2DangerDistanceRatio = 100.0f;

	// The distance away that is adequate for the "run away" strategy
	public const float RunAwayBlockDistance = 30;


	// The enumerated identity of the strategy
	public StrategyType Type { get { return type; } }

	// Must be set before using the level 1 heuristic
	public Path Path { get; set; }
	public int NextPathIndex { get; set; }

	// Must be set before using the level 2 reward, cost, and goal functions
	public DangerZone Level2DangerZone { get; set; }

	// The main way to construct strategies
	public static Strategy StrategyWithType(int playerNum, StrategyType type) {
		switch (type) {
		case StrategyType.Attack:
			return new AttackStrategy(playerNum);
		case StrategyType.DigDown:
			return new DigDownStrategy(playerNum);
		case StrategyType.GetAmmo:
			return new GetAmmoStrategy(playerNum);
		case StrategyType.RunAway:
			return new RunAwayStrategy(playerNum);
		}

		return null;
	}

	// Constructor
	public Strategy(int playerNum) {
		this.playerNum = playerNum;
	}

	// The level 1 adversarial search heuristic
	public float Level1Heuristic(World world) {

		World.Player currentPlayer = playerNum == 1 ? world.Player1 : world.Player2;
		World.Player opponentPlayer = playerNum == 1 ? world.Player2 : world.Player1;

		// Compute normalized factors
		float dHealth = (currentPlayer.Health - opponentPlayer.Health) / 200.0f;
		float dAmmo = currentPlayer.Ammo - opponentPlayer.Ammo;
		if (currentPlayer.IsMaster || opponentPlayer.IsMaster) dAmmo = 0.0f;

		return -Util.ManhattanDistance(currentPlayer.X, currentPlayer.Y,
		                               opponentPlayer.X, opponentPlayer.Y);
		float normalizedInverseDist = Util.BoundedInverseManhattanDistance(currentPlayer.X, currentPlayer.Y,
		                                                     			   opponentPlayer.X, opponentPlayer.Y);

		// Normalized level 2 conformance
		float normalizedConformance = 0.0f;
		int pathLength = Path.States.Count;
		for (int i = NextPathIndex, k = 0; i < pathLength && k < MaxPathPointsWithInfluence; i++, k++) {


			BlockWorld.BlockPlayer target = Path.States[i].Player;
			int playerI = World.XToI(currentPlayer.X);
			int playerJ = World.YToJ(currentPlayer.Y);

			if (target.I == playerI && target.J == playerJ) {
				normalizedConformance = 1.0f;
				break;
				//normalizedConformance = 1.0f / (MaxPathPointsWithInfluence - k);
			}
		}

		// Return weighted sum of influences
		return level1HealthWeight * dHealth
			+ level1AmmoWeight * dAmmo
			+ level1ConfrontationWeight * normalizedInverseDist
			+ level1SuperlevelWeight * normalizedConformance;
	}

	// Override points
	abstract public float Level2CostFunction(BlockWorld blockWorld);
	abstract public bool Level2GoalFunction(BlockWorld blockWorld);
	abstract public float Level2HeuristicFunction(BlockWorld blockWorld);


	

	protected StrategyType type;
	protected int playerNum;

	const float oneOverXSquaredNormalizationFactor = 0.6079f;

	// Level 1 weights
	protected float level1HealthWeight;
	protected float level1AmmoWeight;
	protected float level1ConfrontationWeight;
	protected float level1SuperlevelWeight; // How much to conform with the directions of level 2
	// What about speed and grav?

	// The interpolation function between level 2 path points. Converges to pi^2 / 6
	protected float heuristicPathWeight(int distanceFromLast) {
		return 1.0f / (distanceFromLast * distanceFromLast);
	}

}

public class AttackStrategy : Strategy {

	public AttackStrategy(int playerNum) : base(playerNum) {
		level1HealthWeight = 0.0f;//10.0f;
		level1AmmoWeight = 0.0f;
		level1ConfrontationWeight = 1.0f;
		level1SuperlevelWeight = 0.0f;// 1.0f;
	}
	
	override public float Level2CostFunction(BlockWorld blockWorld) {
		return 1.0f + Level2DangerDistanceRatio * Level2DangerZone.CheckDanger(blockWorld.Player.I, blockWorld.Player.J);
	}
	override public bool Level2GoalFunction(BlockWorld blockWorld) {
		return blockWorld.Player.I == Level2DangerZone.SourceI
			&& blockWorld.Player.J == Level2DangerZone.SourceJ;
	}
	override public float Level2HeuristicFunction(BlockWorld blockWorld) {

		int playerI = blockWorld.Player.I;
		int playerJ = blockWorld.Player.J;
		int opponentI = Level2DangerZone.SourceI;
		int opponentJ = Level2DangerZone.SourceJ;

		return Util.ManhattanDistance(playerI, playerJ, opponentI, opponentJ);
	}
}

public class RunAwayStrategy : Strategy {

	public RunAwayStrategy(int playerNum) : base(playerNum) {
		level1HealthWeight = 100.0f;
		level1AmmoWeight = 1.0f;
		level1ConfrontationWeight = -1.0f;
		level1SuperlevelWeight = 1.0f;
	}
	
	override public float Level2CostFunction(BlockWorld blockWorld) {
		return 1.0f + Level2DangerDistanceRatio * Level2DangerZone.CheckDanger(blockWorld.Player.I, blockWorld.Player.J);
	}
	override public bool Level2GoalFunction(BlockWorld blockWorld) {
		int playerI = blockWorld.Player.I;
		int playerJ = blockWorld.Player.J;
		int opponentI = Level2DangerZone.SourceI;
		int opponentJ = Level2DangerZone.SourceJ;

		// Check distance
		if (Util.ManhattanDistance(playerI, playerJ, opponentI, opponentJ) <= RunAwayBlockDistance) {
			return false;
		}

		// Enforce no ground above
		bool groundAbove = true;
		for (int j = playerJ; j >= 0; j--) {
			if (blockWorld.CheckGroundByIndex(playerI, j)) {
				groundAbove = false;
				break;
			}
		}
		if (groundAbove) return false;

		// Enforce no danger
		if (Level2DangerZone.CheckDanger(playerI, playerJ) != 0.0f) return false;

		return true;
	}
	override public float Level2HeuristicFunction(BlockWorld blockWorld) {
		
		int playerI = blockWorld.Player.I;
		int playerJ = blockWorld.Player.J;
		int opponentI = Level2DangerZone.SourceI;
		int opponentJ = Level2DangerZone.SourceJ;
		
		return RunAwayBlockDistance - Util.ManhattanDistance(playerI, playerJ, opponentI, opponentJ);
	}
}

public class GetAmmoStrategy : Strategy {

	public GetAmmoStrategy(int playerNum) : base(playerNum) {

		level1HealthWeight = 10.0f;
		level1AmmoWeight = 1.0f;
		level1ConfrontationWeight = 0.0f;
		level1SuperlevelWeight = 1.0f;
	}
	
	override public float Level2CostFunction(BlockWorld blockWorld) {
		return 1.0f + Level2DangerDistanceRatio * Level2DangerZone.CheckDanger(blockWorld.Player.I, blockWorld.Player.J);
	}
	override public bool Level2GoalFunction(BlockWorld blockWorld) {
		return blockWorld.JustCollectedAmmo;
	}
	override public float Level2HeuristicFunction(BlockWorld blockWorld) {
		
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

public class DigDownStrategy : Strategy {

	public DigDownStrategy(int playerNum) : base(playerNum) {
		
		level1HealthWeight = 10.0f;
		level1AmmoWeight = 0.5f;
		level1ConfrontationWeight = 0.0f;
		level1SuperlevelWeight = 1.0f;
	}
	
	override public float Level2CostFunction(BlockWorld blockWorld) {
		return 1.0f + Level2DangerDistanceRatio * Level2DangerZone.CheckDanger(blockWorld.Player.I, blockWorld.Player.J);
	}
	override public bool Level2GoalFunction(BlockWorld blockWorld) {
		return blockWorld.Player.J >= World.WallDepthJ;
	}
	override public float Level2HeuristicFunction(BlockWorld blockWorld) {
		return World.WallDepthJ - blockWorld.Player.J;
	}
	
}
