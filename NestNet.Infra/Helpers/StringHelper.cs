namespace NestNet.Infra.Helpers
{
    public static class StringHelper
    {
        public static string ToCamelCase(string str)
        {
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public static string ToPascalCase(string str)
        {
            return char.ToUpperInvariant(str[0]) + str.Substring(1);
        }
    }
}
