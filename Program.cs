using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task_flow.Data;
using task_flow.Models;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Read from .env
var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "";
var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "";
var db = Environment.GetEnvironmentVariable("DB_NAME") ?? "";
var user = Environment.GetEnvironmentVariable("DB_USER") ?? "";
var pass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

var connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass}";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => { options.SignIn.RequireConfirmedAccount = false; })
  .AddRoles<IdentityRole>()
  .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews(options =>
  options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddRazorPages();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseMigrationsEndPoint();
}
else
{
  app.UseExceptionHandler("/Home/Error");
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/Home/NotFound");

app.MapControllerRoute(
  name: "areas",
  pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
  name: "default",
  pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
  await DbSeeder.SeedRolesAndAdmin(scope.ServiceProvider);
}

app.Run();