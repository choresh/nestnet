using Microsoft.EntityFrameworkCore;
using NestNet.Infra.BaseClasses;
using System.Reflection;

namespace GlobalE.Payments.Manager.Core.Data
{
    public class AppDbContext : AppDbContextBase
    {
         public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(
                options,
                [Assembly.GetExecutingAssembly()] // If your entities not located (only) at current assembly - customise here
            )
        {
        }
    }
}