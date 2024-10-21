using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using WebGalleryProject.Models;
using System.Threading.Tasks;
using WebGalleryProject.Models;

namespace WebGalleryProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly IMongoCollection<Category> _categoryCollection;

        public CategoryController(IMongoDatabase database)
        {
            _categoryCollection = database.GetCollection<Category>("Categories");
        }

        public IActionResult Index()
        {
            var categories = _categoryCollection.Find(_ => true).ToList();
            return View(categories);
        }

        // GET: Create category form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create new category
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = await _categoryCollection.Find(c => c.Name == category.Name).FirstOrDefaultAsync();
                if (existingCategory == null)
                {
                    await _categoryCollection.InsertOneAsync(category);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Kategoria o takiej nazwie już istnieje.");
                }
            }

            // Zwróć widok "Create" z danymi modelu
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var category = await _categoryCollection.Find(c => c.Id == new ObjectId(id)).FirstOrDefaultAsync();
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category updatedCategory)
        {
            if (ModelState.IsValid)
            {
                await _categoryCollection.ReplaceOneAsync(c => c.Id == updatedCategory.Id, updatedCategory);
                return RedirectToAction("Index");
            }
            return View(updatedCategory);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _categoryCollection.DeleteOneAsync(c => c.Id == new ObjectId(id));
            return RedirectToAction("Index");
        }
    }

}
