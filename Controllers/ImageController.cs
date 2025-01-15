using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoWebGallery.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MongoWebGallery.Controllers
{
    public class ImageController : Controller
    {
        private readonly IMongoCollection<Image> _imageCollection;
        private readonly IMongoCollection<Comment> _commentCollection;
        private readonly IMongoCollection<Models.Tag> _tagCollection;
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMongoCollection<Technology> _technologyCollection;
        private readonly IMongoCollection<Answer> _answerCollection;
        private readonly UserManager<ApplicationUser> _userManager;

        public ImageController(IMongoCollection<Image> imageCollection,
                       IMongoCollection<Comment> commentCollection,
                       IMongoCollection<Models.Tag> tagCollection,
                       IMongoCollection<Category> categoryCollection,
                       IMongoCollection<Technology> technologyCollection,
                       IMongoCollection<Answer> answerCollection,
                       UserManager<ApplicationUser> userManager
                       )
        {
            _imageCollection = imageCollection;
            _commentCollection = commentCollection;
            _tagCollection = tagCollection;
            _categoryCollection = categoryCollection;
            _technologyCollection = technologyCollection;
            _answerCollection = answerCollection;
            _userManager = userManager;
        }


        [Authorize]
        public async Task<IActionResult> AddImage()
        {
            var tags = await _tagCollection.Find(_ => true).ToListAsync();
            var categories = await _categoryCollection.Find(_ => true).ToListAsync();
            var technologies = await _technologyCollection.Find(_ => true).ToListAsync();

            ViewBag.Tags = tags;
            ViewBag.Categories = categories;
            ViewBag.Technologies = technologies;

            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddImage(AddImageViewModel model)
        {
            var allowedFileTypes = new[] { "image/jpeg", "image/png" };
            const long maxFileSize = 2 * 1024 * 1024;

            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "Proszę przesłać prawidłowy plik obrazu.");
            }
            else if (!allowedFileTypes.Contains(model.ImageFile.ContentType))
            {
                ModelState.AddModelError("ImageFile", "Dozwolone formaty to tylko JPEG i PNG.");
            }
            else if (model.ImageFile.Length > maxFileSize)
            {
                ModelState.AddModelError("ImageFile", "Rozmiar pliku obrazu nie może przekroczyć 2 MB.");
            }

            var selectedTags = new List<ObjectId?> { model.SelectedTag1, model.SelectedTag2, model.SelectedTag3 };
            var distinctTags = selectedTags.Where(t => t.HasValue).Distinct().Count();

            if (distinctTags < selectedTags.Count(t => t.HasValue))
            {
                ModelState.AddModelError(string.Empty, "Proszę wybrać unikalne tagi.");
            }

            if (string.IsNullOrWhiteSpace(model.SelectedTechnologyId?.ToString()))
            {
                if (string.IsNullOrWhiteSpace(model.NewTechnology))
                {
                    ModelState.AddModelError("NewTechnology", "Proszę podać nazwę nowej technologii.");
                }

                if (string.IsNullOrWhiteSpace(model.NewTechnologyUrl))
                {
                    ModelState.AddModelError("NewTechnologyUrl", "Proszę podać URL dla nowej technologii.");
                }
            }

            if (ModelState.IsValid)
            {
                ObjectId? technologyId = model.SelectedTechnologyId;

                if (!string.IsNullOrWhiteSpace(model.NewTechnology))
                {
                    var newTechnology = new Technology
                    {
                        Name = model.NewTechnology,
                        Url = model.NewTechnologyUrl,
                        IsApproved = false,
                        SubmittedBy = (await _userManager.GetUserAsync(User))?.Id
                    };

                    await _technologyCollection.InsertOneAsync(newTechnology);
                    technologyId = newTechnology.Id;
                }

                using (var memoryStream = new MemoryStream())
                {
                    await model.ImageFile.CopyToAsync(memoryStream);
                    var base64Image = Convert.ToBase64String(memoryStream.ToArray());

                    var image = new Image
                    {
                        Name = model.Name,
                        Base64Image = base64Image,
                        Prompt = model.Prompt,
                        IsPublic = model.IsPublic,
                        CreatedDate = DateTime.UtcNow,
                        UserId = (await _userManager.GetUserAsync(User))?.Id ?? Guid.Empty,
                        Tags = new List<ObjectId?> { model.SelectedTag1, model.SelectedTag2, model.SelectedTag3 }
                                    .Where(t => t.HasValue).Select(t => t.Value).ToList(),
                        CategoryId = model.SelectedCategoryId,
                        TechnologyId = technologyId ?? ObjectId.Empty
                    };

                    await _imageCollection.InsertOneAsync(image);
                    return RedirectToAction("Profile", "Account");
                }
            }

            ViewBag.Tags = await _tagCollection.Find(_ => true).ToListAsync();
            ViewBag.Categories = await _categoryCollection.Find(_ => true).ToListAsync();
            ViewBag.Technologies = await _technologyCollection.Find(t => t.IsApproved).ToListAsync();

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            var image = await _imageCollection.Find(i => i.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();

            if (image == null)
            {
                return NotFound();
            }

            image.ViewCount++;
            await _imageCollection.ReplaceOneAsync(i => i.Id == image.Id, image);

            var tags = await _tagCollection.Find(t => image.Tags.Contains(t.Id)).ToListAsync();
            var category = await _categoryCollection.Find(c => c.Id == image.CategoryId).FirstOrDefaultAsync();
            var technology = await _technologyCollection.Find(t => t.Id == image.TechnologyId).FirstOrDefaultAsync();
            var comments = await _commentCollection.Find(c => c.ImageId == image.Id).SortBy(c => c.CreatedDate).ToListAsync();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Forbid();
            }

            var userAnswer = await _answerCollection.Find(a => a.UserId == user.Id && a.ImageId == image.Id).FirstOrDefaultAsync();

            var answers = await _answerCollection.Find(a => a.ImageId == image.Id).ToListAsync();
            image.AverageRating = answers.Count > 0 ? answers.Average(a => a.RatingAnswer) : 0;

            var owner = await _userManager.FindByIdAsync(image.UserId.ToString());

            var viewModel = new ImageDetailsViewModel
            {
                Image = image,
                Tags = tags,
                Category = category,
                Technology = technology,
                Comments = comments,
                HasUserAnswered = userAnswer != null,
                UserAnswer = userAnswer,
                AnswersCount = answers.Count,
                OwnerName = owner?.UserName
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var image = await _imageCollection.Find(i => i.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();
            if (image == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Id != image.UserId)
            {
                return Forbid();
            }

            var tags = await _tagCollection.Find(_ => true).ToListAsync();
            var categories = await _categoryCollection.Find(_ => true).ToListAsync();
            var technologies = await _technologyCollection.Find(_ => true).ToListAsync();

            var model = new EditImageViewModel
            {
                Id = image.Id,
                Name = image.Name,
                Prompt = image.Prompt,
                IsPublic = image.IsPublic,
                SelectedTag1 = image.Tags.ElementAtOrDefault(0),
                SelectedTag2 = image.Tags.ElementAtOrDefault(1),
                SelectedTag3 = image.Tags.ElementAtOrDefault(2),
                SelectedCategoryId = image.CategoryId,
                SelectedTechnologyId = image.TechnologyId
            };

            ViewBag.Tags = tags;
            ViewBag.Categories = categories;
            ViewBag.Technologies = technologies;

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(EditImageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var image = await _imageCollection.Find(i => i.Id == model.Id).FirstOrDefaultAsync();

                if (image == null)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.Id != image.UserId)
                {
                    return Forbid();
                }

                image.Name = model.Name;
                image.Prompt = model.Prompt;
                image.IsPublic = model.IsPublic;

                var selectedTags = new List<ObjectId>();
                if (model.SelectedTag1.HasValue)
                    selectedTags.Add(model.SelectedTag1.Value);
                if (model.SelectedTag2.HasValue)
                    selectedTags.Add(model.SelectedTag2.Value);
                if (model.SelectedTag3.HasValue)
                    selectedTags.Add(model.SelectedTag3.Value);

                image.Tags = selectedTags.Distinct().ToList();
                image.CategoryId = model.SelectedCategoryId;
                image.TechnologyId = model.SelectedTechnologyId;

                await _imageCollection.ReplaceOneAsync(i => i.Id == model.Id, image);

                return RedirectToAction("Details", new { id = image.Id });
            }

            ViewBag.Tags = await _tagCollection.Find(_ => true).ToListAsync();
            ViewBag.Categories = await _categoryCollection.Find(_ => true).ToListAsync();
            ViewBag.Technologies = await _technologyCollection.Find(_ => true).ToListAsync();

            return View(model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddComment(string id, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Forbid();
            }

            var image = await _imageCollection.Find(i => i.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();
            if (image == null)
            {
                return NotFound();
            }

            if (userId == image.UserId.ToString())
            {
                ModelState.AddModelError(string.Empty, "Nie możesz komentować swojego własnego obrazu.");
                return RedirectToAction("Details", new { id });
            }

            var comment = new Comment
            {
                Id = ObjectId.GenerateNewId(),
                UserId = Guid.Parse(userId),
                ImageId = image.Id,
                Content = content,
                CreatedDate = DateTime.UtcNow
            };

            await _commentCollection.InsertOneAsync(comment);

            var update = Builders<Image>.Update.AddToSet(i => i.Comments, comment.Id);
            await _imageCollection.UpdateOneAsync(i => i.Id == image.Id, update);
            return RedirectToAction("Details", new { id });
        }

        private async Task UpdateAverageRating(ObjectId imageId)
        {
            var ratings = await _answerCollection.Find(a => a.ImageId == imageId).ToListAsync();
            var averageRating = ratings.Any() ? ratings.Average(a => a.RatingAnswer) : 0;

            var update = Builders<Image>.Update.Set(i => i.AverageRating, averageRating);
            await _imageCollection.UpdateOneAsync(i => i.Id == imageId, update);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddAnswer(string id, AnswerViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Forbid();
            }

            var image = await _imageCollection.Find(i => i.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();
            if (image == null)
            {
                return NotFound();
            }

            var existingAnswer = await _answerCollection.Find(a => a.UserId == user.Id && a.ImageId == image.Id).FirstOrDefaultAsync();
            if (existingAnswer != null)
            {
                return RedirectToAction("Details", new { id });
            }

            var answer = new Answer
            {
                Id = ObjectId.GenerateNewId(),
                UserId = user.Id,
                ImageId = image.Id,
                BoolAnswer1 = model.BoolAnswer1,
                BoolAnswer2 = model.BoolAnswer2,
                BoolAnswer3 = model.BoolAnswer3,
                BoolAnswer4 = model.BoolAnswer4,
                RatingAnswer = model.RatingAnswer,
                CreatedDate = DateTime.UtcNow
            };

            await _answerCollection.InsertOneAsync(answer);

            var update = Builders<Image>.Update.AddToSet(i => i.Answers, answer.Id);
            await _imageCollection.UpdateOneAsync(i => i.Id == image.Id, update);

            await UpdateAverageRating(image.Id);
            return RedirectToAction("Details", new { id });
        }


        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            var image = await _imageCollection.Find(i => i.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();

            if (image == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Id != image.UserId)
            {
                return Forbid();
            }

            return View(image);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var image = await _imageCollection.Find(i => i.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();

            if (image == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Id != image.UserId)
            {
                return Forbid();
            }

            await _commentCollection.DeleteManyAsync(c => c.ImageId == image.Id);
            await _answerCollection.DeleteManyAsync(a => a.ImageId == image.Id);

            await _imageCollection.DeleteOneAsync(i => i.Id == image.Id);

            return RedirectToAction("Profile", "Account");
        }

        [Authorize]
        public async Task<IActionResult> Download(string id)
        {
            var image = await _imageCollection.Find(i => i.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();

            if (image == null)
            {
                return NotFound();
            }

            var imageBytes = Convert.FromBase64String(image.Base64Image);
            var contentType = "image/jpeg";
            var fileName = $"{image.Name}.jpg";

            if (image.Base64Image.StartsWith("data:image/png"))
            {
                contentType = "image/png";
                fileName = $"{image.Name}.png";
            }

            return File(imageBytes, contentType, fileName);
        }

    }
}