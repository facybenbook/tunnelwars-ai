using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class Util {

	// Checks intersection of two rectangles
	public static bool CheckRectIntersect(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4) {

		return x1 < x4 && x2 > x3 && y1 < y4 && y2 > y3;
	}

	// Distance formulas
	public static float Distance(float x1, float y1, float x2, float y2) {
		float dx = x2 - x1;
		float dy = y2 - y1;
		return Mathf.Sqrt(dx * dx + dy * dy);
	}
	public static float ManhattanDistance(float x1, float y1, float x2, float y2) {
		float dx = x2 - x1;
		float dy = y2 - y1;
		return Mathf.Abs(dx) + Mathf.Abs (dy);
	}

	// Returns the distance warped to fit between 0 and 1. Argument k is the real distance for
	// which the bounded distance is 1/2.
	public static float BoundedDistance(float x1, float y1, float x2, float y2, float k=600) {
		
		return 1.0f - 1.0f / ((Distance(x1, y1, x2, y2) / k) + 1.0f);
	}
	public static float BoundedManhattanDistance(float x1, float y1, float x2, float y2, float k=600) {

		return 1.0f - 1.0f / ((ManhattanDistance(x1, y1, x2, y2) / k) + 1.0f);
	}

	// Key class that stores a state and a strategy
	public class Key {

		public State state { get; set; }
		public Strategy strategy { get; set; }

		// Constructors
		public Key (State state1, Strategy strategy1) {
			state = state1;
			strategy = strategy1;
		}

		public Key () {
			state = new State();
			strategy = Strategy.Attack;
		}

		public string ToString ()  {

			string keyString = "";

			// State
			keyString = keyString + state.ToString ();
			keyString = keyString + " ";

			// Strategy
			keyString = keyString + strategy.ToString ();

			return keyString;
		}

		static public Key FromString (string keyString) {

			Key key = new Key ();

			string[] propertyArray = keyString.Split (' ');
			
			for (int i = 0; i < propertyArray.Length; i++) {
				if (i == 0) {
					key.state = State.FromString(propertyArray[0] + " " + propertyArray[1] + " " + propertyArray[2] + " " + propertyArray[3] + " " + propertyArray[4] + " " + propertyArray[5]);
				} else if (i == 6) {
					key.strategy = (Strategy) Enum.Parse(typeof(Strategy),propertyArray[i]);
				}
			}
			
			return key;
		}
	}
}