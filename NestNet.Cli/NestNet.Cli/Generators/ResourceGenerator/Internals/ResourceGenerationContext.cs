using NestNet.Cli.Generators.Common;

namespace NestNet.Cli.Generators.ResourceGenerator.Internals
{
    internal class ResourceGenerationContext : BaseGenerationContext
    {
        public required string ParamName { get; set; }
        public required string KebabCaseResourceName { get; set; }
        public required string SampleInputDtoName { get; set; }
        public required string SampleOutputDtoName { get; set; }
        public bool GenerateController { get; set; }
        public bool GenerateConsumer { get; set; }
    }

}