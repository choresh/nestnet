namespace NestNet.Cli.Installation
{
    public enum PathOperation
    {
        Set,
        Remove
    }

    public static class PathModifier
    {
        private const string PATH_KEY = "PATH";

        public static void ModifyPath(string targetDir, bool allUsers, PathOperation pathOperation)
        {
            var messageDetails = $"(pathOperation: {pathOperation}, allUsers: {allUsers}, targetDir: {targetDir})";
            Console.WriteLine($"Start to modify PATH environment variable {messageDetails}");

            try
            {
                var envTarget = allUsers
                    ? EnvironmentVariableTarget.Machine
                    : EnvironmentVariableTarget.User;

                var paths = GetExistsPaths(allUsers, envTarget);

                switch (pathOperation)
                {
                    case PathOperation.Set:
                        HandlePathSet(targetDir, envTarget, paths);
                        break;
                    case PathOperation.Remove:
                        HandlePathRemove(targetDir, envTarget, paths);
                        break;
                    default:
                        throw new ArgumentException($"Invalid PathOperation ('{pathOperation}')");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to modify PATH environment variable {messageDetails}, error: {ex.Message}");
                throw;
            }

            Console.WriteLine($"Finised to modify PATH environment variable {messageDetails}");
        }

        private static string[] GetExistsPaths(bool allUsers, EnvironmentVariableTarget envTarget)
        {
            string[] paths;

            if (!allUsers)
            {
                // For 'user' scope, we need to filter out machine PATH entries
                var userValue = Environment.GetEnvironmentVariable(PATH_KEY, EnvironmentVariableTarget.User) ?? string.Empty;
                var machineValue = Environment.GetEnvironmentVariable(PATH_KEY, EnvironmentVariableTarget.Machine) ?? string.Empty;
                var machinePaths = machineValue.Split(';', StringSplitOptions.RemoveEmptyEntries);
                paths = userValue.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                .Except(machinePaths, StringComparer.OrdinalIgnoreCase)
                                .ToArray();
            }
            else
            {
                // For 'machine' scope, we use the combined PATH entries
                var oldValue = Environment.GetEnvironmentVariable(PATH_KEY, envTarget) ?? string.Empty;
                paths = oldValue.Split(';', StringSplitOptions.RemoveEmptyEntries);
            }

            return paths;
        }

        private static void HandlePathRemove(string targetDir, EnvironmentVariableTarget envTarget, string[] paths)
        {
            if (paths.Contains(targetDir))
            {
                var newPaths = paths.Where(p => p != targetDir);
                string newValue = string.Join(';', newPaths);
                Environment.SetEnvironmentVariable(PATH_KEY, newValue, envTarget);
            }
        }

        private static void HandlePathSet(string targetDir, EnvironmentVariableTarget envTarget, string[] paths)
        {
            if (!paths.Contains(targetDir))
            {
                var newPaths = paths.Append(targetDir);
                string newValue = string.Join(';', newPaths);
                Environment.SetEnvironmentVariable(PATH_KEY, newValue, envTarget);
            }
        }
    }
}