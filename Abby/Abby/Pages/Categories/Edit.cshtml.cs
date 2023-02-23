using Abby.Data;
using Abby.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Abby.Pages.Categories;
[BindProperties]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    [BindProperty]
    public Category Category { get; set; }
    public EditModel(ApplicationDbContext db)
    {
        _db = db;
    }
    public void OnGet(int id)
    {
        Category = _db.Category.Find(id);
    }
    public async Task<IActionResult> OnPost()
    {
        if (Category.Name == Category.DisplayOrder.ToString())
        {
            ModelState.AddModelError("Category.Name", "The DisplayOrder cannot exactly match the Name.");
        }
        if (ModelState.IsValid)
        {
            _db.Category.Update(Category);
            await _db.SaveChangesAsync();
            TempData["success"] = "Category edited successfully";
            return RedirectToPage("Index");
        }
        return Page();
    }
}