using NestNet.Cli.Infra;

namespace NestNet.Cli.Generators.Common
{
    public abstract class MultiProjectsGeneratorBase<TContext> where TContext : BaseGenerationContext
    {
        protected TContext Context { get; set; }
        protected ProjectType ProjectType { get; }

        protected MultiProjectsGeneratorBase(ProjectType projectType) 
        {
            ProjectType = projectType;
        }

        public bool Generate(TContext context, string parentFolderName)
        {
            var (projectDir, projectName) = Helpers.GetProjectInfo(ProjectType);
            if (projectDir == null || projectName == null)
            {
                return false;
            }

            var projectContext = new ProjectContext()
            {
                ProjectDir = projectDir,
                ProjectName = projectName,
                TargetPath = Path.Combine(projectDir, parentFolderName, context.ArtifactName)
            };

            if (!Helpers.CheckTarDir(projectContext.TargetPath))
            {
                return false;
            }

            // Atttach project-specific enrichment.
            context.ProjectContext = projectContext;

            Context = context;

            // Call dervid class.
            DoGenerate();

            // Detach the project-specific enrichment.
            context.ProjectContext = null;

            return true;
        }

        public abstract void DoGenerate();

        protected string GetDirectoryName(string path)
        {
            var dirName = Path.GetDirectoryName(path);
            if (dirName == null)
            {
                throw new Exception($"Directory name not found, path: {path}");
            }
            return dirName;
        }
    }
}