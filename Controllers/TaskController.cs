using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using task_flow.Data;
using task_flow.Models;
using Microsoft.EntityFrameworkCore;

namespace task_flow.Controllers;

[Authorize]
public class TasksController : Controller
{
  private readonly ApplicationDbContext _context;
  private readonly UserManager<ApplicationUser> _userManager;

  public TasksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
  {
    _context = context;
    _userManager = userManager;
  }

  public async Task<IActionResult> Index()
  {
    var user = await _userManager.GetUserAsync(User);
    var tasks = await _context.Tasks
      .Where(t => t.UserId == user.Id)
      .ToListAsync();

    return View(tasks);
  }

  public IActionResult Create() => View();

  [HttpPost]
  public async Task<IActionResult> Create(TaskItem task)
  {
    var user = await _userManager.GetUserAsync(User);
    task.UserId = user.Id;

    _context.Tasks.Add(task);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
  }

  public async Task<IActionResult> Edit(int id)
  {
    var task = await _context.Tasks.FindAsync(id);
    return View(task);
  }

  [HttpPost]
  public async Task<IActionResult> Edit(TaskItem task)
  {
    _context.Tasks.Update(task);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
  }

  public async Task<IActionResult> Delete(int id)
  {
    var task = await _context.Tasks.FindAsync(id);
    _context.Tasks.Remove(task);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
  }
}