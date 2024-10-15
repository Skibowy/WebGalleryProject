using MongoDB.Driver;
using WebGalleryProject.Models;
using WebGalleryProject.Settings;


var builder = WebApplication.CreateBuilder(args);

// Pobieranie konfiguracji MongoDB z appsettings.json
var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();

// Dodanie Identity z MongoDB Stores, wykorzystuj�c ConnectionString z MongoDbConfig
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
	.AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
		mongoDbSettings.ConnectionString,
		mongoDbSettings.Name);

// Konfiguracja MongoDB
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
	return new MongoClient(mongoDbSettings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
{
	var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
	return mongoClient.GetDatabase(mongoDbSettings.Name);
});

// Rejestracja kolekcji
builder.Services.AddSingleton<IMongoCollection<Image>>(sp =>
{
	var database = sp.GetRequiredService<IMongoDatabase>();
	return database.GetCollection<Image>("Images");
});

builder.Services.AddSingleton<IMongoCollection<Comment>>(sp =>
{
	var database = sp.GetRequiredService<IMongoDatabase>();
	return database.GetCollection<Comment>("Comments");
});

builder.Services.AddSingleton<IMongoCollection<WebGalleryProject.Models.Tag>>(sp =>
{
	var database = sp.GetRequiredService<IMongoDatabase>();
	return database.GetCollection<WebGalleryProject.Models.Tag>("Tags");
});

builder.Services.AddSingleton<IMongoCollection<Category>>(sp =>
{
	var database = sp.GetRequiredService<IMongoDatabase>();
	return database.GetCollection<Category>("Categories");
});

builder.Services.AddSingleton<IMongoCollection<Technology>>(sp =>
{
	var database = sp.GetRequiredService<IMongoDatabase>();
	return database.GetCollection<Technology>("Technologies");
});

builder.Services.AddSingleton<IMongoCollection<Answer>>(sp =>
{
	var database = sp.GetRequiredService<IMongoDatabase>();
	return database.GetCollection<Answer>("Answers");
});

// Dodanie MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Wstawianie danych testowych
await SeedDatabase(app.Services);

// Middleware konfiguracja
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


async Task SeedDatabase(IServiceProvider serviceProvider)
{
    var categoryCollection = serviceProvider.GetRequiredService<IMongoCollection<Category>>();
    var categories = new List<Category>
    {
        new Category { Name = "Portret" },
        new Category { Name = "Krajobraz" },
        new Category { Name = "Model" },
        new Category { Name = "Natura" },
        new Category { Name = "Abstrakcja" },
        new Category { Name = "Architektura" },
        new Category { Name = "Futurystyka" },
        new Category { Name = "Surrealizm" },
        new Category { Name = "Krask�wka" },
        new Category { Name = "Modernizm" },
        new Category { Name = "Sztuka historyczna" }
    };
    await categoryCollection.InsertManyAsync(categories);

    var tagCollection = serviceProvider.GetRequiredService<IMongoCollection<WebGalleryProject.Models.Tag>>();
    var tags = new List<WebGalleryProject.Models.Tag>
    {
        new WebGalleryProject.Models.Tag { Name = "Fantasy" },
        new WebGalleryProject.Models.Tag { Name = "Kultura popularna" },
        new WebGalleryProject.Models.Tag { Name = "Cyberpunk" },
        new WebGalleryProject.Models.Tag { Name = "Mitologia" },
        new WebGalleryProject.Models.Tag { Name = "Klasyka" },
        new WebGalleryProject.Models.Tag { Name = "Film i serial" },
        new WebGalleryProject.Models.Tag { Name = "Celebryci" },
        new WebGalleryProject.Models.Tag { Name = "Post-apo" },
        new WebGalleryProject.Models.Tag { Name = "Sci-fi" },
        new WebGalleryProject.Models.Tag { Name = "Komiks" }
    };
    await tagCollection.InsertManyAsync(tags);

    var technologyCollection = serviceProvider.GetRequiredService<IMongoCollection<Technology>>();
    var technologies = new List<Technology>
    {
        new Technology { Name = "DALL�E 3 (OpenAI)", Url = "https://openai.com/dall-e-3" },
        new Technology { Name = "MidJourney", Url = "https://www.midjourney.com/home/" },
        new Technology { Name = "Artbreeder", Url = "https://www.artbreeder.com/" },
        new Technology { Name = "RunwayMLy", Url = "https://runwayml.com/" },
        new Technology { Name = "DeepDream (Google)", Url = "https://deepdreamgenerator.com/" }
    };
    await technologyCollection.InsertManyAsync(technologies);
}
