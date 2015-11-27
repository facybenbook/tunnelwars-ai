/*
 * Agents.cs
 * 
 * Describes agents to control players in the Tunnel Wars game.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// All agents should conform to the interface below
public interface IAgent {

	// This method, given a world should return a list of world actions
	List<WorldAction> GetAction(World world);
}

// This class provides a base for all agents that control a single player
public abstract class PlayerAgentBase : IAgent {

	// Chain the constructor `: base(player)` while subclassing
	public PlayerAgentBase(int player) {

		// Assign actions to respond to
		if (player == 1) {
			leftAction = WorldAction.P1Left;
			rightAction = WorldAction.P1Right;
			jumpAction = WorldAction.P1Jump;
			fireAction = WorldAction.P1Fire;
			
		} else if (player == 2) {
			leftAction = WorldAction.P2Left;
			rightAction = WorldAction.P2Right;
			jumpAction = WorldAction.P2Jump;
			fireAction = WorldAction.P2Fire;
		}
	}

	abstract public List<WorldAction> GetAction(World world);

	protected WorldAction leftAction;
	protected WorldAction rightAction;
	protected WorldAction jumpAction;
	protected WorldAction fireAction;
}

// An agent to control a player with WASDF
public class WASDFAgent : PlayerAgentBase {

	public WASDFAgent(int player) : base(player) {
	
		w = false;
		a = false;
		d = false;
		f = false;

		wPrev = false;
		fPrev = false;
	}
	
	override public List<WorldAction> GetAction(World world) {

		wPrev = w;
		fPrev = f;

		w = Input.GetAxis("Player 2 Jump") > 0.25f;
		a = Input.GetAxis("Player 2 Horizontal") > 0.25f;
		d = Input.GetAxis("Player 2 Horizontal") < -0.25f;
		f = Input.GetAxis("Player 2 Fire") > 0.25f;

		List<WorldAction> actions = new List<WorldAction>();

		if (a) actions.Add(leftAction);
		if (d) actions.Add(rightAction);
		if (w && !wPrev) actions.Add(jumpAction);
		if (f && !fPrev) actions.Add(fireAction);

		return actions;
	}


	bool w;
	bool a;
	bool d;
	bool f;

	bool wPrev;
	bool fPrev;
}
