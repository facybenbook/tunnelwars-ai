/*
 * AStar.cs
 * 
 * Runs A Star search on IJ coordinates, attempting to move the BlockWorld's player
 * to a goal state.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using System.Linq;

// A struct for a path containing a path and a value
public class Path : PriorityQueueNode {
	public float Cost { get { return cost; } }
	public List<BlockWorld> States { get { return states; } }
	
	// Constructor
	public Path(float cost, List<BlockWorld> states) {
		this.cost = cost;
		this.states = states;
	}
	
	// Returns a new path with a state and cost added on BUT SAME ARRAY
	public Path ExtendedPath(BlockWorld newState, float newCost) {
		List<BlockWorld> newStateList = states.ToList();
		newStateList.Add (newState);
		return new Path(cost + newCost, newStateList);
	}
	
	public BlockWorld Last() {
		return states.Last();
	}
	
	float cost;
	List<BlockWorld> states;
}

// Define a delegate for a cost function
public delegate float BlockWorldCostFunction(BlockWorld blockWorld);

// Define delegate for the A* heuristic (under)estimating future cost.
public delegate float FutureCostHeuristic(BlockWorld blockWorld);

// Define delegate for the goal test
public delegate bool GoalStateTest(BlockWorld blockWorld);

public class AStar {

	// The maximum nodes and expansions
	public int MaxNodes { get; set; }
	public int MaxExpansions { get; set; }

	// The model on which to act
	public BlockWorldCostFunction CostFunction { get; set; }

	// The goal state test functino
	public GoalStateTest GoalFunction { get; set; }

	// The heuristic
	public FutureCostHeuristic Heuristic { get; set; }

	public AStar(int maxNodes, int maxExpansions, BlockWorldCostFunction costFunction,
	             GoalStateTest goalFunction, FutureCostHeuristic heuristic) {

		MaxNodes = maxNodes;
		MaxExpansions = maxExpansions;
		CostFunction = costFunction;
		GoalFunction = goalFunction;
		Heuristic = heuristic;
	}

	// Outputs the optimal path
	public Path ComputeBestPath(BlockWorld blockWorld) {

		HeapPriorityQueue<Path> frontier = new HeapPriorityQueue<Path>(MaxNodes);
		HashSet<BlockWorld> explored = new HashSet<BlockWorld>();

		// Initial state, path
		BlockWorld initialState = blockWorld;
		Path initialPath = new Path(0, new List<BlockWorld>() {initialState});

		// Add the initial path to the frontier
		frontier.Enqueue(initialPath, 0 + Heuristic(initialState));


		// Find paths
		int expansions = 0;
		while (frontier.Count > 0) {

			if (expansions > MaxExpansions) {
				return null;
			}

			Path path = frontier.Dequeue();
			BlockWorld lastWorld = path.Last();

			// Check goal
			if (GoalFunction(lastWorld)) {
				return path;
			}

			// Mark as explored
			explored.Add(lastWorld);

			// Iterate over possible actions
			List<BlockWorldAction> possibleActions = lastWorld.ApplicableActions();
			foreach (BlockWorldAction action in possibleActions) {

				// Try the action on a cloned block world
				BlockWorld newWorld = lastWorld.Clone();
				newWorld.Advance(action);

				// Check if explored already
				bool alreadyExplored = false;
				foreach (BlockWorld exploredWorld in explored) {
					if (exploredWorld.PropertiesEqual(newWorld)) {
						alreadyExplored = true;
						break;
					}
				}

				if (!alreadyExplored) {

					// Extend path
					Path newPath = path.ExtendedPath(newWorld, CostFunction(newWorld));

					// Add to frontier
					frontier.Enqueue(newPath, Heuristic(newWorld) + newPath.Cost);
				}
			}

			expansions++;
		}


		Debug.Log("Couldn't find path within constraints");
		return null;
	}
}
