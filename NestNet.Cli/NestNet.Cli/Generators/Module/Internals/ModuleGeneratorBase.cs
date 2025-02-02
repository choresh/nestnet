using NestNet.Cli.Infra;
using NestNet.Infra.Helpers;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace NestNet.Cli.Generators
{
    internal partial class ModuleGenerator
    {
        private abstract class ModuleGeneratorBase
        {
            protected ModuleGenerationContext Context { get; set; }
            protected ProjType ProjectType { get; }

            protected ModuleGeneratorBase(ProjType projectType)
            {
                ProjectType = projectType;
            }

            public bool Generate(ModuleGenerationContext context)
            {
                var (projectDir, projectName) = Helpers.GetProjectInfo(ProjectType);
                if (projectDir == null || projectName == null)
                {
                    return false;
                }

                var projectContext = new ProjectContext() {
                    ProjectDir = projectDir,
                    ProjectName = projectName,
                    ModulePath = Path.Combine(projectDir, "Modules", context.ModuleName)
                };

                if (!Helpers.CheckTarDir(projectContext.ModulePath))
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
}