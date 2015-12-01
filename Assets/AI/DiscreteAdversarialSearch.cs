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

// Define a struct for a action-filler tuple
public struct ActionWithFiller {
	public readonly WorldAction Action;
	public readonly WorldAction FillerAction;
	public ActionWithFiller(WorldAction action, WorldAction filler) {
		Action = action; FillerAction = filler;
	} 
}

// Define a delegate for a World-evaluating heuristic function
public delegate float WorldHeuristic(World world); 

class DiscreteAdversarialSearch {

	// The number of frames for each action
	public int StepSize { get; set; }

	// The number of actions into the future to search
	public int SearchDepth { get; set; }

	// The heuristic function
	public WorldHeuristic Heuristic { get; set; }

	// Constructor
	public DiscreteAdversarialSearch(int playerNum, int stepSize=4, int searchDepth=4) {
		init(playerNum, stepSize, searchDepth);
	}

	// Searches the world for an action
	public ActionWithFiller ComputeBestAction(World world) {
		WorldAction bestAction = WorldAction.NoAction;
		WorldAction bestFillerAction = WorldAction.NoAction;

		return new ActionWithFiller(bestAction, bestFillerAction);
	}

	

	// The number of the player to search on behalf of
	int playerNum;

	// Default heuristic returns 0
	float defaultHeuristic(World world) {
		return 0.0f;
	}

	protected void init(int playerNum, int stepSize, int searchDepth) {

		this.playerNum = playerNum;
		StepSize = stepSize;
		SearchDepth = searchDepth;
		Heuristic = new WorldHeuristic(defaultHeuristic);
	}
}