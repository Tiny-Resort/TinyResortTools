using System;
using UnityEngine;

public class MapRand
{
	private System.Random rnd;

	public MapRand(int seed)
	{
		rnd = new System.Random(seed);
	}

	public float Sample01()
	{
		return (float)rnd.NextDouble();
	}

	public int Range(int a, int b)
	{
		float t = Sample01();
		return Mathf.FloorToInt(Mathf.Lerp(a, b, t));
	}

	public float Range(float a, float b)
	{
		float t = Sample01();
		return Mathf.Lerp(a, b, t);
	}

	public Vector2 SampleUnitCircle()
	{
		float x = (Sample01() - 0.5f) * 2f;
		float y = (Sample01() - 0.5f) * 2f;
		return new Vector2(x, y);
	}

	public Vector3 SampleUnitSphere()
	{
		float x = (Sample01() - 0.5f) * 2f;
		float y = (Sample01() - 0.5f) * 2f;
		float z = (Sample01() - 0.5f) * 2f;
		return new Vector3(x, y, z);
	}
}
