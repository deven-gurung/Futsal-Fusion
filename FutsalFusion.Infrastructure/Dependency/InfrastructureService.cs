using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Application.Interfaces.Services;
using FutsalFusion.Infrastructure.Implementation.Repository.Base;
using FutsalFusion.Infrastructure.Implementation.Services;
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
        services.AddTransient<IFileUploadService, FileUploadService>();
        services.AddTransient<IMenuService, MenuService>();

        return services;
    }
}