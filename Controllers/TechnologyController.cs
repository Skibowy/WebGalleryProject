using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoWebGallery.Models;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MongoWebGallery.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TechnologyController : Controller
    {
        private readonly IMongoCollection<Technology> _technologyCollection;

        public TechnologyController(IMongoDatabase database)
        {
            _technologyCollection = database.GetCollection<Technology>("Technologies");
        }

        public IActionResult Index()
        {
            var technologies = _technologyCollection.Find(_ => true).ToList();
            return View(technologies);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

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

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var technology = await _technologyCollection.Find(t => t.Id == new ObjectId(id)).FirstOrDefaultAsync();
            if (technology == null) return NotFound();

            return View(technology);
        }

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

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _technologyCollection.DeleteOneAsync(t => t.Id == new ObjectId(id));
            return RedirectToAction("Index");
        }
    }
}
