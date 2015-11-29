/*
 * AdversarialSearchDynamic.cs
 * 
 * The existing adversarial search is pretty good as it is, but this file explores a
 * couple derivations of it:
 * 
 * 1: Step size is a function of how physically close the danger is to the AI.
 * If danger is very close, a small step size is used in order to greedily avoid the
 * most imminent dangers. If the danger is farther away, the step size may be safely
 * increased.
 * 
 * 2: Step size always starts out small, but increases when game states further forward
 * in time are expanded.
 * 
 */ 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Similar to adversarial search, but preserves the FPS and does not have a max depth
public class AdversarialSearchPreserveFPS : AdversarialSearch {
	
	// Constructor
	public AdversarialSearchPreserveFPS(int player) : base(player) {

		// Override the max search depth to something ridiculous
		maxDepth = 9;
	}

	// Gets an action
	override public List<WorldAction> GetAction(World world) {

		// Set the start time
		startTicks = System.DateTime.Now.Ticks;
		List<WorldAction> retValue = base.GetAction(world);
		return retValue;
	}



	// The ticks at the start of the calculation
	long startTicks;

	// Number of ticks per frame
	const long maxTicks = System.TimeSpan.TicksPerMillisecond * 33;

	// Calculates the utility, but stops when we have reached the maxTicks
	override protected float calculateUtility(World state, int depth, bool isOpponentsTurn, float alpha, float beta, WorldAction prevFillerAction) {
	

		// If FPS caught us then skyrocket the depth
		//if ((System.DateTime.Now.Ticks - startTicks) > maxTicks) depth = 100000;
		if (depth > 4) depth = 100000;

		return base.calculateUtility(state, depth, isOpponentsTurn, alpha, beta, prevFillerAction);
	}
}
