using Microsoft.EntityFrameworkCore;
using NestNet.Infra.BaseClasses;
using System.Reflection;

namespace SampleApp.Worker.Data
{
    public class ApplicationDbContext : ApplicationDbContextBase
    {
         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(
                options,
                [Assembly.GetExecutingAssembly()] // If your entities not located (only) at current assembly - customise here
            )
        {
        }
    }
}