namespace NestNet.Infra.Helpers
{
    public static class ConfigHelper
	{
        /// <summary>
        /// Get value of specific config key.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static string GetConfigParam(string[] args, string name, string? defaultValue = null)
        {
            // Check command line args first
            var arg = args.FirstOrDefault(a => a.StartsWith($"{name}="));
            if (arg != null)
            {
                return arg.Split('=')[1];
            }

            // If not in args, check environment variable
            return Environment.GetEnvironmentVariable(name) ??
                (defaultValue ?? throw new InvalidOperationException($"{name} is not set"));
        }
	}
}
