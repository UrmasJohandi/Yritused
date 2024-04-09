using Microsoft.EntityFrameworkCore;
using Yritused.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(opts => { opts.UseSqlServer(builder.Configuration["ConnectionStrings:YritusedConnection"]); });

builder.Services.AddScoped<IYritusRepository, EFYritusRepository>();
builder.Services.AddScoped<IOsavotjaRepository, EFOsavotjaRepository>();
builder.Services.AddScoped<IYritusOsavotjaRepository, EFYritusOsavotjaRepository>();
builder.Services.AddMvc(options => options.EnableEndpointRouting = false);

var app = builder.Build();

app.UseStaticFiles();
app.MapDefaultControllerRoute();

app.UseMvc(routes => {
    routes.MapRoute(name: null, template: "{controller}/{action}/{id?}",
        defaults: new
        {
            controller = "Yritus",
            action = "List"
        }
    );
});

app.Run();
