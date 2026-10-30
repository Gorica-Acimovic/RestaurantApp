namespace Restaurant.Api.Services
{
    public class ForecastMetrics
    {
        public static double Mae(IReadOnlyList<double> actual, IReadOnlyList<double> predicted)
        {
            if (actual.Count == 0 || actual.Count != predicted.Count)
            {
                throw new ArgumentException("Actual and predicted values must have the same, non-zero length.");
            }

            return actual.Select((a, i) => Math.Abs(a - predicted[i])).Average();
        }

        public static double Mape(IReadOnlyList<double> actual, IReadOnlyList<double> predicted)
        {
            if (actual.Count == 0 || actual.Count != predicted.Count)
            {
                throw new ArgumentException("Actual and predicted values must have the same, non-zero length.");
            }

            var errors = actual
                .Select((a, i) => (Actual: a, Error: Math.Abs(a - predicted[i])))
                .Where(x => x.Actual > 0)
                .Select(x => x.Error / x.Actual)
                .ToList();

            return errors.Count == 0 ? 0 : 100 * errors.Average();
        }

    }
}
