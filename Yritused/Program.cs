using Microsoft.EntityFrameworkCore;
using Yritused.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(opts => { opts.UseSqlServer(builder.Configuration["ConnectionStrings:YritusedConnection"]); });

var app = builder.Build();

app.UseStaticFiles();
app.MapDefaultControllerRoute();

app.Run();
