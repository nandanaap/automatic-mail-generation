using t23p0.Services;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();
        
        // Register your Auto Mail Service
        services.AddScoped<IAutoMailService, AutoMailService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Your existing configuration...
    }
}
