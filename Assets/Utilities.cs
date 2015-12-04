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

	// Integer exponentiation
	public static int IntPow(int x, uint pow) {
		int ret = 1;
		while (pow != 0) {
			if ((pow & 1) == 1) ret *= x;
			x *= x;
			pow >>= 1;
		}
		return ret;
	}
}