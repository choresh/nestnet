using Microsoft.Extensions.DependencyInjection;
using NestNet.Infra.Attributes;
using System.Reflection;

namespace NestNet.Infra.Helpers
{
    public static class DependencyInjectionHelper
    {
        /// <summary>
        /// Support Dependency Injection for all classes with [Injectable] attribute
        /// (Daos, Services, etc), within the given assembleis.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembleis"></param>
        public static void RegisterInjetables(IServiceCollection services, IEnumerable<Assembly> assembleis)
        {
            foreach (Assembly assembly in assembleis)
            {
                var injectables = assembly.GetTypes()
                    .Select(t => new
                    {
                       Implementation = t,
                       Attribute = t.GetCustomAttribute<InjectableAttribute>()
                    })
                    .Where(x => x.Attribute != null)
                    .ToList();

                foreach (var injectable in injectables)
                {
                    var interfaces = injectable.Implementation.GetInterfaces();

                    var directInterfaces = interfaces
                        .Except(interfaces.SelectMany(t => t.GetInterfaces()));

                    if (directInterfaces.Count() > 1)
                    {
                        throw new Exception($"Type {injectable.Implementation.Name} is marked as Injectable but have more then one direct interfaces");
                    }

                    var interfaceType = directInterfaces.FirstOrDefault();
                    
                    if (interfaceType == null)
                    {
                        throw new Exception($"Type {injectable.Implementation.Name} is marked as Injectable but does not have any direct interface");
                    }

                    switch (injectable.Attribute.LifetimeType)
                    {
                        case LifetimeType.Transient:
                            services.AddTransient(interfaceType, injectable.Implementation);
                            break;
                        case LifetimeType.Singleton:
                            services.AddSingleton(interfaceType, injectable.Implementation);
                            break;
                        case LifetimeType.Scoped:
                            services.AddScoped(interfaceType, injectable.Implementation);
                            break;
                        default:
                            throw new Exception($"Type {injectable.Implementation.Name} is marked as Injectable but does not have a valid tifetime type");
                    }
                }
            }
        }
    }
}
