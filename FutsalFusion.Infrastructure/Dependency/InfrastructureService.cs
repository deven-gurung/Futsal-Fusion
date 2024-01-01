using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Infrastructure.Implementation.Repository.Base;
using FutsalFusion.Infrastructure.Persistence;
using FutsalFusion.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FutsalFusion.Infrastructure.Dependency;

public static class InfrastructureService
{
    public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString,
                b => b.MigrationsAssembly("FutsalFusion.Infrastructure")));

        services.AddScoped<IDbInitializer, DbInitializer>();

        services.AddTransient<IGenericRepository, GenericRepository>();

        return services;
    }
    
}