namespace Mirror
{
	public class ExponentialMovingAverage
	{
		private readonly float alpha;

		private bool initialized;

		public double Value { get; private set; }

		public double Var { get; private set; }

		public ExponentialMovingAverage(int n)
		{
			alpha = 2f / (float)(n + 1);
		}

		public void Add(double newValue)
		{
			if (initialized)
			{
				double num = newValue - Value;
				Value += (double)alpha * num;
				Var = (double)(1f - alpha) * (Var + (double)alpha * num * num);
			}
			else
			{
				Value = newValue;
				initialized = true;
			}
		}
	}
}
