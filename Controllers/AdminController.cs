using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoWebGallery.Models;

namespace MongoWebGallery.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IMongoCollection<Technology> _technologyCollection;

        public AdminController(IMongoCollection<Technology> technologyCollection)
        {
            _technologyCollection = technologyCollection;
        }

        public async Task<IActionResult> ManageTechnologies()
        {
            var unapprovedTechnologies = await _technologyCollection.Find(t => !t.IsApproved).ToListAsync();
            return View(unapprovedTechnologies);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveTechnology(ObjectId id)
        {
            var filter = Builders<Technology>.Filter.Eq(t => t.Id, id);
            var update = Builders<Technology>.Update.Set(t => t.IsApproved, true);
            await _technologyCollection.UpdateOneAsync(filter, update);

            return RedirectToAction("ManageTechnologies");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTechnology(ObjectId id)
        {
            var filter = Builders<Technology>.Filter.Eq(t => t.Id, id);
            await _technologyCollection.DeleteOneAsync(filter);

            return RedirectToAction("ManageTechnologies");
        }
    }

}
