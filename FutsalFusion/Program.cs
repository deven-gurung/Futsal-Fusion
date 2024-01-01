using FutsalFusion.Identity.Dependency;
using FutsalFusion.Infrastructure.Dependency;
using FutsalFusion.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

var configuration = builder.Configuration;

services.AddInfrastructureService(configuration);

services.AddIdentityService(configuration);

services.AddControllersWithViews();

services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

services.ConfigureApplicationCookie(options =>
{
    options.LogoutPath = $"/User/Account/Logout";
    options.LoginPath = $"/User/Account/Login";
    options.AccessDeniedPath = $"/User/Account/AccessDenied";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/User/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=User}/{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();

    dbInitializer.Initialize();
}

app.Run();