using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebGalleryProject.Models;
using System.Threading.Tasks;
using MongoDB.Bson;
using WebGalleryProject.Models;

namespace WebGalleryProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TechnologyController : Controller
    {
        private readonly IMongoCollection<Technology> _technologyCollection;

        public TechnologyController(IMongoDatabase database)
        {
            _technologyCollection = database.GetCollection<Technology>("Technologies");
        }

        // Wyświetla listę technologii
        public IActionResult Index()
        {
            var technologies = _technologyCollection.Find(_ => true).ToList();
            return View(technologies);
        }

        // GET: Tworzenie nowej technologii
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tworzenie nowej technologii
        [HttpPost]
        public async Task<IActionResult> Create(Technology technology)
        {
            if (ModelState.IsValid)
            {
                var existingTechnology = await _technologyCollection.Find(t => t.Name == technology.Name).FirstOrDefaultAsync();
                if (existingTechnology == null)
                {
                    await _technologyCollection.InsertOneAsync(technology);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Technologia o takiej nazwie już istnieje.");
                }
            }
            return View(technology);
        }

        // GET: Edytowanie technologii
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var technology = await _technologyCollection.Find(t => t.Id == new ObjectId(id)).FirstOrDefaultAsync();
            if (technology == null) return NotFound();

            return View(technology);
        }

        // POST: Edytowanie technologii
        [HttpPost]
        public async Task<IActionResult> Edit(Technology updatedTechnology)
        {
            if (ModelState.IsValid)
            {
                await _technologyCollection.ReplaceOneAsync(t => t.Id == updatedTechnology.Id, updatedTechnology);
                return RedirectToAction("Index");
            }
            return View(updatedTechnology);
        }

        // POST: Usuwanie technologii
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _technologyCollection.DeleteOneAsync(t => t.Id == new ObjectId(id));
            return RedirectToAction("Index");
        }
    }
}
