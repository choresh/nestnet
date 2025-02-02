#pragma warning disable IDE0290 // Use primary constructor
using SampleApp.Core.Data;
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;
using SampleApp.Core.Modules.MyModules.Dtos;
using SampleApp.Core.Modules.MyModules.Entities;

namespace SampleApp.Core.Modules.MyModules.Daos
{
    public interface IMyModuleDao: IDao<MyModuleEntity, MyModuleQueryDto>
    {
        // If you add methods to derived class - expose them here.
    }

    [Injectable(LifetimeType.Scoped)]
    public class MyModuleDao : DaoBase<MyModuleEntity, MyModuleQueryDto>, IMyModuleDao
    {
        public MyModuleDao(AppDbContext context)
            : base(context, context.Set<MyModuleEntity>(), "MyModuleId")
        {
        }

        // How to customise this class:
        // 1) You can add here 'custom' methods (methods for operations not supported by the base class).
        // 2) You can override here base class methods if needed:
        //    * Add methods with the same name and signature as in the base class.
        //    * For example:
        //      public override async Task<IEnumerable<MyModuleEntity>> GetAll()
        //      {
        //          // Set your custom implementation here.
        //      }
        // 3) In your methods:
        //    * Base class members '_dbSet' and '_context' are accessible.
        //    * Base class methods (e.g. 'await base.GetAll()') are accesible.
    }
}

#pragma warning restore IDE0290 // Use primary constructor