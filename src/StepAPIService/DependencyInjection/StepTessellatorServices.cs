using StepAPIService;

namespace Microsoft.Extensions.DependencyInjection
{
    // ReSharper disable once UnusedMember.Global
    public static class StepTessellatorServices
    {
        // ReSharper disable once UnusedMember.Global
        public static void AddStepTessellator(this IServiceCollection services)
            => AddToServiceCollection(services);

        private static void AddToServiceCollection(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TessProcessDispatcher).Assembly));
            services.AddSingleton<TessProcessDispatcher>(_ =>
                new TessProcessDispatcher());
            services.AddSingleton<TessModel.IStepTessellator, Tessellator>();
            services.AddHostedService<TessBackgroundService>();
        }
    }
}
