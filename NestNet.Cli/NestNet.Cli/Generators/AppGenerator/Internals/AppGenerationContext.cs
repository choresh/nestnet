using DbType = NestNet.Infra.Enums.DbType;

namespace NestNet.Cli.Generators.AppGenerator
{
    internal class AppGenerationContext
    {
        // public required string CurrentDir { get; set; }
        public required string BaseProjectName { get; set; }
        public required string CurrProjectName { get; set; }
        public required DbType DbType { get; set; }
        public required string AppPath { get; set; }
    }
}