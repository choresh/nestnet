#pragma warning disable IDE0290 // Use primary constructor
using SampleApp.Data;
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;
using SampleApp.Modules.MyModules.Dtos;

namespace SampleApp.Modules.MyModules.Daos
{
    public interface IMyModuleDao: IDao<Entities.MyModule, MyModuleQueryDto>
    {
        // If you add methods to derived class - expose them here.
    }

    [Injectable(LifetimeType.Scoped)]
    public class MyModuleDao : DaoBase<Entities.MyModule, MyModuleQueryDto>, IMyModuleDao
    {
        public MyModuleDao(ApplicationDbContext context)
            : base(context, context.GetDbSet<Entities.MyModule>(), "MyModuleId")
        {
        }

        // How to customise this class:
        // 1) You can add here 'custom' methods (methods for operations not supported by the base class).
        // 2) You can override here base class methods if needed:
        //    * Add methods with the same name and signature as in the base class.
        //    * For example:
        //      public override async Task<IEnumerable<Entities.MyModule>> GetAll()
        //      {
        //          // Set your custom implementation here.
        //      }
        // 3) In your methods:
        //    * Base class members '_dbSet' and '_context' are accessible.
        //    * Base class methods (e.g. 'await base.GetAll()') are accesible.
    }
}

#pragma warning restore IDE0290 // Use primary constructor