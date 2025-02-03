using NestNet.Cli.Generators.Common;

namespace NestNet.Cli.Generators.ModuleGenerator
{
    internal class ModuleGenerationContext : BaseGenerationContext
    {
        public required string PluralizedModuleName { get; set; }
        public required string ParamName { get; set; }
        public required string PluralizedParamName { get; set; }
        public required string KebabCasePluralizedModuleName { get; set; }
        public required string EntityName { get; set; }
        public required string NullableEntityName { get; set; }
        public required string CreateDtoName { get; set; }
        public required string UpdateDtoName { get; set; }
        public required string ResultDtoName { get; set; }
        public required string QueryDtoName { get; set; }
        public required bool GenerateDbSupport { get; set; }
        public required bool GenerateService { get; set; }
        public required bool GenerateController { get; set; }
    }
}