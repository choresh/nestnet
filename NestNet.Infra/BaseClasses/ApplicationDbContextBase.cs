#pragma warning disable IDE0290 // Use primary constructor
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace NestNet.Infra.BaseClasses
{
    public abstract class ApplicationDbContextBase : DbContext
    {
        private readonly IEnumerable<Assembly> _entitiesAssemblies;

        public ApplicationDbContextBase(DbContextOptions options, IEnumerable<Assembly> entitiesAssemblies)
        : base(options)
        {
            _entitiesAssemblies = entitiesAssemblies;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            RegisterEntities(modelBuilder);

        }
        private void RegisterEntities(ModelBuilder modelBuilder)
        {
            foreach (var assembly in _entitiesAssemblies)
            {
                // Register all entity types from the assembly that have Table attribute
                var entityTypes = assembly.GetTypes()
                    .Where(t => t.GetCustomAttributes<TableAttribute>().Any());
                
                foreach (var type in entityTypes)
                {
                    modelBuilder.Entity(type);
                }

                // This will register any additional configurations
                // modelBuilder.ApplyConfigurationsFromAssembly(assembly);
            }
        }
    }
}

#pragma warning restore IDE0290 // Use primary constructor
