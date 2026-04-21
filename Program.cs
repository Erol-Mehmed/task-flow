using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task_flow.Data;
using task_flow.Models;
using task_flow.Repositories.TaskRepository;
using task_flow.Services.TaskService;
using task_flow.Areas.Admin.Services;
using task_flow.Repositories.ActivityRepository;
using task_flow.Repositories.CommentRepository;
using task_flow.Repositories.TagRepository;
using task_flow.Repositories.WorkspaceRepository;
using task_flow.Services.ActivityService;
using task_flow.Services.CommentService;
using task_flow.Services.TagService;
using task_flow.Services.WorkspaceService;

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

builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseMigrationsEndPoint();
}
else
{
  app.UseExceptionHandler("/Board/Error");
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/Board/NotFound");

app.MapControllerRoute(
  name: "areas",
  pattern: "{area:exists}/{controller=Board}/{action=Index}/{id?}"
);

app.MapControllerRoute(
  name: "default",
  pattern: "{controller=Board}/{action=Index}/{id?}"
);

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
  await DbSeeder.SeedRolesAndAdmin(scope.ServiceProvider);
}

app.Run();