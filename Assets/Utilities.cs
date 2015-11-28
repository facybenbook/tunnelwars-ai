using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;

public static class Util {

	public static bool CheckRectIntersect(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4) {
		
		// Note: this cheats
		if ((x1 >= x3 && x1 < x4) || (x2 >= x3 && x2 < x4))
		{
			if ((y1 >= y3 && y1 <= y4) || (y2 >= y3 && y2 <= y4)) return true;
		}
		return false;
	}

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

	// Deep cloning
	public static T DeepClone<T>(T obj)
	{
		using (var ms = new MemoryStream())
		{
			var formatter = new BinaryFormatter();
			formatter.Serialize(ms, obj);
			ms.Position = 0;
			
			return (T) formatter.Deserialize(ms);
		}
	}
}