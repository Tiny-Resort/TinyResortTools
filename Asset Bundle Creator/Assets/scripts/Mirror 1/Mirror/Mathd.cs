namespace Mirror
{
	public static class Mathd
	{
		public static double LerpUnclamped(double a, double b, double t)
		{
			return a + (b - a) * t;
		}

		public static double Clamp01(double value)
		{
			if (value < 0.0)
			{
				return 0.0;
			}
			if (!(value > 1.0))
			{
				return value;
			}
			return 1.0;
		}

		public static double InverseLerp(double a, double b, double value)
		{
			if (a == b)
			{
				return 0.0;
			}
			return Clamp01((value - a) / (b - a));
		}
	}
}
