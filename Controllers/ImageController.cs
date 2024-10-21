using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using WebGalleryProject.Models;
using System.Security.Claims;

namespace WebGalleryProject.Controllers
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
                               UserManager<ApplicationUser> userManager)
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
            var technologies = await _technologyCollection.Find(_ => true).ToListAsync(); // Pobieranie technologii

            ViewBag.Tags = tags;
            ViewBag.Categories = categories;
            ViewBag.Technologies = technologies; // Przekazywanie technologii do widoku

            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(AddImageViewModel model)
        {
            // Allowed file types
            var allowedFileTypes = new[] { "image/jpeg", "image/png" };
            const long maxFileSize = 2 * 1024 * 1024; // 2 MB size limit

            // Validate image file
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "Please upload a valid image file.");
            }
            else if (!allowedFileTypes.Contains(model.ImageFile.ContentType))
            {
                ModelState.AddModelError("ImageFile", "Only JPEG and PNG image formats are allowed.");
            }
            else if (model.ImageFile.Length > maxFileSize)
            {
                ModelState.AddModelError("ImageFile", "The image file size must not exceed 2 MB.");
            }

            var selectedTags = new List<ObjectId?> { model.SelectedTag1, model.SelectedTag2, model.SelectedTag3 };
            var distinctTags = selectedTags.Where(t => t.HasValue).Distinct().Count();

            if (distinctTags < selectedTags.Count(t => t.HasValue))
            {
                ModelState.AddModelError(string.Empty, "Please select unique tags.");
            }

            if (ModelState.IsValid)
            {
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ImageFile.CopyToAsync(memoryStream);
                        var base64Image = Convert.ToBase64String(memoryStream.ToArray());

                        var user = await _userManager.GetUserAsync(User);
                        if (user == null)
                        {
                            ModelState.AddModelError(string.Empty, "User not found.");
                            return View(model);
                        }

                        var image = new Image
                        {
                            Name = model.Name,
                            Base64Image = base64Image,
                            Prompt = model.Prompt,
                            IsPublic = model.IsPublic,
                            CreatedDate = DateTime.UtcNow,
                            UserId = user.Id,
                            Tags = selectedTags.Where(t => t.HasValue).Select(t => t.Value).ToList(),
                            CategoryId = model.SelectedCategoryId,
                            TechnologyId = model.SelectedTechnologyId // Przypisanie wybranej technologii
                        };

                        await _imageCollection.InsertOneAsync(image);
                        return RedirectToAction("Profile", "Account");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Please upload a valid image file.");
                }
            }

            // Ponowne ładowanie danych w przypadku błędu
            ViewBag.Tags = await _tagCollection.Find(_ => true).ToListAsync();
            ViewBag.Categories = await _categoryCollection.Find(_ => true).ToListAsync();
            ViewBag.Technologies = await _technologyCollection.Find(_ => true).ToListAsync(); // Ponowne ładowanie technologii

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

            // Zwiększ liczbę wyświetleń
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

            // Calculate average rating
            var answers = await _answerCollection.Find(a => a.ImageId == image.Id).ToListAsync();
            image.AverageRating = answers.Count > 0 ? answers.Average(a => a.RatingAnswer) : 0;

            // Now we need to get the owner's name
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
                AnswersCount = answers.Count, // Liczba odpowiedzi
                OwnerName = owner?.UserName // Ustawiamy imię właściciela
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
        [ValidateAntiForgeryToken]
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

                // Ensure no null values and set tags correctly
                var selectedTags = new List<ObjectId>();
                if (model.SelectedTag1.HasValue)
                    selectedTags.Add(model.SelectedTag1.Value);
                if (model.SelectedTag2.HasValue)
                    selectedTags.Add(model.SelectedTag2.Value);
                if (model.SelectedTag3.HasValue)
                    selectedTags.Add(model.SelectedTag3.Value);

                image.Tags = selectedTags.Distinct().ToList(); // Ensure unique tags
                image.CategoryId = model.SelectedCategoryId;
                image.TechnologyId = model.SelectedTechnologyId;

                await _imageCollection.ReplaceOneAsync(i => i.Id == model.Id, image);

                // Redirect to the details page of the edited image
                return RedirectToAction("Details", new { id = image.Id });
            }

            // Populate ViewBag again if ModelState is invalid
            ViewBag.Tags = await _tagCollection.Find(_ => true).ToListAsync();
            ViewBag.Categories = await _categoryCollection.Find(_ => true).ToListAsync();
            ViewBag.Technologies = await _technologyCollection.Find(_ => true).ToListAsync();

            return View(model);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(string id, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get User ID from claims
            if (userId == null)
            {
                return Forbid(); // If user is not found, forbid the action
            }

            var image = await _imageCollection.Find(i => i.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();
            if (image == null)
            {
                return NotFound();
            }

            // Check if the user is the owner of the image
            if (userId == image.UserId.ToString())
            {
                ModelState.AddModelError(string.Empty, "You cannot comment on your own image.");
                return RedirectToAction("Details", new { id });
            }

            var comment = new Comment
            {
                Id = ObjectId.GenerateNewId(),
                UserId = Guid.Parse(userId), // Convert the userId to Guid
                ImageId = image.Id,
                Content = content,
                CreatedDate = DateTime.UtcNow
            };

            await _commentCollection.InsertOneAsync(comment);
            return RedirectToAction("Details", new { id });
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            // Sprawdź, czy użytkownik już odpowiedział na pytania dla tego zdjęcia
            var existingAnswer = await _answerCollection.Find(a => a.UserId == user.Id && a.ImageId == image.Id).FirstOrDefaultAsync();
            if (existingAnswer != null)
            {
                return RedirectToAction("Details", new { id });
            }

            // Tworzymy nową odpowiedź
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

            // Aktualizacja średniej oceny obrazu
            await UpdateAverageRating(image.Id); // Wywołaj nową metodę

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
        [ValidateAntiForgeryToken]
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

            // Delete associated comments
            await _commentCollection.DeleteManyAsync(c => c.ImageId == image.Id);

            // Delete associated answers
            await _answerCollection.DeleteManyAsync(a => a.ImageId == image.Id);

            // Delete the image
            await _imageCollection.DeleteOneAsync(i => i.Id == image.Id);

            return RedirectToAction("Profile", "Account"); // Redirect to user's profile or wherever appropriate
        }

        private async Task UpdateAverageRating(ObjectId imageId)
        {
            // Pobierz wszystkie odpowiedzi dla danego obrazu
            var answers = await _answerCollection.Find(a => a.ImageId == imageId).ToListAsync();

            // Oblicz średnią ocenę
            double averageRating = answers.Count > 0 ? answers.Average(a => a.RatingAnswer) : 0;

            // Zaktualizuj pole AverageRating w dokumencie Image
            var update = Builders<Image>.Update.Set(i => i.AverageRating, averageRating);
            await _imageCollection.UpdateOneAsync(i => i.Id == imageId, update);
        }

    }
}
