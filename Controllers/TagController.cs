using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using WebGalleryProject.Models;
using System.Reflection;
using System.Threading.Tasks;

namespace WebGalleryProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TagController : Controller
    {
        private readonly IMongoCollection<Models.Tag> _tagCollection;

        public TagController(IMongoDatabase database)
        {
            _tagCollection = database.GetCollection<Models.Tag>("Tags");
        }

        public IActionResult Index()
        {
            var tags = _tagCollection.Find(_ => true).ToList();
            return View(tags);
        }

        // GET: Create tag form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create new tag
        [HttpPost]
        public async Task<IActionResult> Create(Models.Tag tag)
        {
            if (ModelState.IsValid)
            {
                var existingTag = await _tagCollection.Find(t => t.Name == tag.Name).FirstOrDefaultAsync();
                if (existingTag == null)
                {
                    await _tagCollection.InsertOneAsync(tag);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Tag o takiej nazwie już istnieje.");
                }
            }
            return View(tag);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var tag = await _tagCollection.Find(t => t.Id == new ObjectId(id)).FirstOrDefaultAsync();
            if (tag == null) return NotFound();

            return View(tag);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Models.Tag updatedTag)
        {
            if (ModelState.IsValid)
            {
                await _tagCollection.ReplaceOneAsync(t => t.Id == updatedTag.Id, updatedTag);
                return RedirectToAction("Index");
            }
            return View(updatedTag);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _tagCollection.DeleteOneAsync(t => t.Id == new ObjectId(id));
            return RedirectToAction("Index");
        }
    }

}
