namespace NestNet.Infra.Helpers
{
    public static class TestsHelper
    {
        public static void IsValuesExists<TSrc, TTar>(TSrc src, TTar tar, Action<object?, object?> cb)
        {
            foreach (var srcProperty in typeof(TSrc).GetProperties())
            {
                var expectedValue = srcProperty.GetValue(src);
                var tarProperty = typeof(TTar).GetProperty(srcProperty.Name);
                var actualValue = tarProperty?.GetValue(tar);
                cb(expectedValue, actualValue);
            }
        }
    }
}
