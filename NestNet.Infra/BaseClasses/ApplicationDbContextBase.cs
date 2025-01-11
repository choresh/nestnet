#pragma warning disable IDE0290 // Use primary constructor
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestNet.Infra.Attributes;
using NestNet.Infra.Helpers;

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

        // Helper method to get DbSet
        public DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class
        {
            return Set<TEntity>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            DbProvidersHelper.Init(Database.ProviderName!);
            MapDbSets(modelBuilder);
            SetStoreOptionsInAllEntities(modelBuilder);
        }

        private static void SetStoreOptionsInAllEntities(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                SetStoreOptionsInEntity(modelBuilder, entityType);
            }
        }

        private static void SetStoreOptionsInEntity(ModelBuilder modelBuilder, IMutableEntityType entityType)
        {
            var entityBuilder = modelBuilder.Entity(entityType.ClrType);
            var propertyInfos = entityType.ClrType.GetProperties()
                                .Where(p => p.GetCustomAttribute<PropAttribute>() != null);

            foreach (var propertyInfo in propertyInfos)
            {
                SetStoreOptionsInProperty(modelBuilder, entityBuilder, entityType, propertyInfo);
            }
        }

        private static void SetStoreOptionsInProperty(ModelBuilder modelBuilder, EntityTypeBuilder entityBuilder, IMutableEntityType entityType, PropertyInfo propertyInfo)
        {
            var propAttribute = propertyInfo.GetCustomAttribute<PropAttribute>();
            switch (propAttribute?.Store)
            {
                case DbOpt.PrimaryKey:
                    entityBuilder.HasKey(propertyInfo.Name); // Equivalent to [Key] attribute.
                    break;
                case DbOpt.Ignore:
                    entityBuilder.Ignore(propertyInfo.Name); // Equivalent to [NotMapped] attribute.
                    break;
                case DbOpt.Standard:
                    // Normal column, no special configuration needed
                    break;
                default:
                    throw new NotImplementedException();

            }
        }

        private void MapDbSets(ModelBuilder modelBuilder)
        {
            // Get the generic Entity<T> method from ModelBuilder
            var entityMethod = typeof(ModelBuilder).GetMethod("Entity", new Type[] { });

            // Find all entity classes in the specified assemblies
            var types = _entitiesAssemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => (t.GetCustomAttribute<EntityAttribute>() != null));

            // Iterate through all entity types, and add each one to the model
            foreach (var type in types)
            {
                // Use reflection to call modelBuilder.Entity<T>() for each entity type
                entityMethod?.MakeGenericMethod(type).Invoke(modelBuilder, null);

                // If the type inherits from EntityBase, configure the timestamp properties
                if (typeof(EntityBase).IsAssignableFrom(type))
                {
                    // Need to use generic Entity<T> method to access proper configuration methods
                    var genericEntityMethod = typeof(ModelBuilder)
                        .GetMethod(nameof(ModelBuilder.Entity), Type.EmptyTypes)
                        ?.MakeGenericMethod(type);

                    var builder = genericEntityMethod?.Invoke(modelBuilder, null) as EntityTypeBuilder;

                    var currDateFunc = DbProvidersHelper.GetDbProviderHelper().GetCurrDateFunc();

                    // Configure CreatedAt to only set value on insert and never update
                    builder?
                        .Property(nameof(EntityBase.CreatedAt))
                        .HasDefaultValueSql(currDateFunc)
                        .ValueGeneratedOnAdd()
                        .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                    // Configure UpdatedAt to update value on both insert and update
                    builder?
                        .Property(nameof(EntityBase.UpdatedAt))
                        .HasDefaultValueSql(currDateFunc);

                }
                // If the type not inherits from EntityBase, throw exception
                else
                {
                    throw new Exception($"Class {type.Name} marked with [Entity] attribute, is must inherits from class {typeof(EntityBase).Name}");
                }
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateTimestamp();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            UpdateTimestamp();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void UpdateTimestamp()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is EntityBase && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (EntityBase)entry.Entity;
                if (entry.State != EntityState.Added)
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}

#pragma warning restore IDE0290 // Use primary constructor
