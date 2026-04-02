using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task_flow.Data;
using task_flow.Models;

namespace task_flow.Controllers;

[Authorize]
public class TaskController : Controller
{
  private readonly ApplicationDbContext _context;
  private readonly UserManager<ApplicationUser> _userManager;

  public TaskController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
  {
    _context = context;
    _userManager = userManager;
  }

  public async Task<IActionResult> Index()
  {
    var user = await _userManager.GetUserAsync(User);

    if (user == null)
      return Unauthorized();

    if (User.IsInRole("Admin"))
    {
      return View(await _context.Task.ToListAsync());
    }

    return View(await _context.Task
      .Where(t => t.UserId == user.Id)
      .ToListAsync());
  }

  public IActionResult Create()
  {
    return View();
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(TaskItem task)
  {
    foreach (var error in ModelState)
    {
      var key = error.Key;
      var errors = error.Value.Errors;

      foreach (var e in errors)
      {
        Console.WriteLine($"{key}: {e.ErrorMessage}");
      }
    }
    
    if (!ModelState.IsValid)
      return View(task);

    var user = await _userManager.GetUserAsync(User);

    if (user == null)
      return Unauthorized();

    task.UserId = user.Id;

    _context.Task.Add(task);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
  }

  public async Task<IActionResult> Edit(int id)
  {
    var task = await _context.Task.FindAsync(id);
    if (task == null)
      return NotFound();

    var user = await _userManager.GetUserAsync(User);

    if (user == null)
      return Unauthorized();

    if (task.UserId != user.Id && !User.IsInRole("Admin"))
      return Unauthorized();

    return View(task);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(TaskItem task)
  {
    if (!ModelState.IsValid)
      return View(task);

    var existingTask = await _context.Task.FindAsync(task.Id);
    if (existingTask == null)
      return NotFound();

    var user = await _userManager.GetUserAsync(User);

    if (user == null)
      return Unauthorized();

    if (existingTask.UserId != user.Id && !User.IsInRole("Admin"))
      return Unauthorized();

    existingTask.Title = task.Title;
    existingTask.Description = task.Description;

    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Delete(int id)
  {
    var task = await _context.Task.FindAsync(id);
    if (task == null)
      return NotFound();

    var user = await _userManager.GetUserAsync(User);

    if (user == null)
      return Unauthorized();

    if (task.UserId != user.Id && !User.IsInRole("Admin"))
      return Unauthorized();

    _context.Task.Remove(task);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
  }
}