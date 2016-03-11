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
public abstract class KeyboardAgent : PlayerAgentBase {

	public KeyboardAgent(int player) : base(player) {

		right = false;
		left = false;
		up = false;
		fire = false;

		upPrev = false;
		firePrev = false;
	}

	override public List<WorldAction> GetAction(World world) {

		upPrev = up;
		firePrev = fire;

		assignKeys();

		List<WorldAction> actions = new List<WorldAction>();

		if (left) actions.Add(leftAction);
		if (right) actions.Add(rightAction);
		if (up && !upPrev) actions.Add(jumpAction);
		if (fire && !firePrev) actions.Add(fireAction);

		return actions;
	}

	abstract protected void assignKeys();

	protected bool up;
	protected bool left;
	protected bool right;
	protected bool fire;

	bool upPrev;
	bool firePrev;
}

public class WASDFAgent : KeyboardAgent {

	public WASDFAgent(int player) : base(player) {}

	protected override void assignKeys() {

		up = Input.GetAxis("Player 2 Jump") > 0.25f;
		left = Input.GetAxis("Player 2 Horizontal") < -0.25f;
		right = Input.GetAxis("Player 2 Horizontal") > 0.25f;
		fire = Input.GetAxis("Player 2 Fire") > 0.25f;
	}
}

public class ArrowsAgent : KeyboardAgent {

	public ArrowsAgent(int player) : base(player) {}

	protected override void assignKeys() {

		up = Input.GetAxis("Player 1 Jump") > 0.25f;
		left = Input.GetAxis("Player 1 Horizontal") < -0.25f;
		right = Input.GetAxis("Player 1 Horizontal") > 0.25f;
		fire = Input.GetAxis("Player 1 Fire") > 0.25f;
	}
}