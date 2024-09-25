#pragma warning disable IDE0290 // Use primary constructor

namespace NestNet.Infra.Attributes
{
    /// <summary>
    /// Service Lifetimes
    /// 1) Singleton:
    ///     * Creates a single instance for the entire application lifetime
    ///     * Same instance is shared by all requests/components
    ///     * Good for:
    ///         * Stateless services
    ///         * Application-wide configuration
    ///     * Common use cases: Logging service, Configuration service, Cache service
    /// 2) Scoped:
    ///     * Creates new instance for each HTTP request/scope
    ///     * Same instance within the same request
    ///     * Good for:
    ///         * Per-request state
    ///     * Common use cases: DbContext, User-specific services, Request-specific services
    /// 3) Transient:
    ///     * Creates new instance every time service is requested
    ///     * Good for:
    ///         * Lightweight, stateless services
    ///         * Services that need fresh state each time
    ///     * Common use cases: Lightweight services with no shared state
    /// </summary>
    public enum LifetimeType
    {
        Scoped,
        Singleton,
        Transient
    };

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class InjectableAttribute : Attribute
    {
        private readonly LifetimeType _lifetimeType;

        public InjectableAttribute(LifetimeType lifetimeType)
        {
            _lifetimeType = lifetimeType;
        }

        public LifetimeType LifetimeType => _lifetimeType;
    };
}

#pragma warning restore IDE0290 // Use primary constructor

