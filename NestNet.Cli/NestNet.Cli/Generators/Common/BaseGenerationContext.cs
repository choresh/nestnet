namespace NestNet.Cli.Generators.Common
{
    public abstract class BaseGenerationContext
    {
        public ProjectContext? ProjectContext { get; set; }

        public required string ArtifactName { get; set; }
    }
}