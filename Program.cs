using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MongoWebGallery.Models;
using MongoWebGallery.Settings;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
        mongoDbSettings.ConnectionString,
        mongoDbSettings.Name)
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    return new MongoClient(mongoDbSettings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
{
    var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
    return mongoClient.GetDatabase(mongoDbSettings.Name);
});

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

builder.Services.AddSingleton<IMongoCollection<MongoWebGallery.Models.Tag>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<MongoWebGallery.Models.Tag>("Tags");
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

builder.Services.AddSingleton<IMongoCollection<ApplicationUser>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<ApplicationUser>("Users");
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

await SeedDatabase(app.Services);

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
    using (var scope = serviceProvider.CreateScope())
    {
        var scopedServiceProvider = scope.ServiceProvider;

        var roleManager = scopedServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scopedServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
        }

        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = "User" });
        }

        var adminEmail = "admin@gmail.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        var user1Email = "user1@gmail.com";
        if (await userManager.FindByEmailAsync(user1Email) == null)
        {
            var user1 = new ApplicationUser
            {
                UserName = user1Email,
                Email = user1Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user1, "User@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user1, "User");
            }
        }

        var user2Email = "user2@gmail.com";
        if (await userManager.FindByEmailAsync(user2Email) == null)
        {
            var user2 = new ApplicationUser
            {
                UserName = user2Email,
                Email = user2Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user2, "User@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user2, "User");
            }
        }

        var categoryCollection = scopedServiceProvider.GetRequiredService<IMongoCollection<Category>>();
        var existingCategories = await categoryCollection.CountDocumentsAsync(_ => true);
        if (existingCategories == 0)
        {
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
                new Category { Name = "Kraskówka" },
                new Category { Name = "Modernizm" },
                new Category { Name = "Sztuka historyczna" }
            };
            await categoryCollection.InsertManyAsync(categories);
        }

        var tagCollection = scopedServiceProvider.GetRequiredService<IMongoCollection<MongoWebGallery.Models.Tag>>();
        var existingTags = await tagCollection.CountDocumentsAsync(_ => true);
        if (existingTags == 0)
        {
            var tags = new List<MongoWebGallery.Models.Tag>
            {
                new MongoWebGallery.Models.Tag { Name = "Fantasy" },
                new MongoWebGallery.Models.Tag { Name = "Kultura popularna" },
                new MongoWebGallery.Models.Tag { Name = "Cyberpunk" },
                new MongoWebGallery.Models.Tag { Name = "Mitologia" },
                new MongoWebGallery.Models.Tag { Name = "Klasyka" },
                new MongoWebGallery.Models.Tag { Name = "Film i serial" },
                new MongoWebGallery.Models.Tag { Name = "Celebryci" },
                new MongoWebGallery.Models.Tag { Name = "Post-apo" },
                new MongoWebGallery.Models.Tag { Name = "Sci-fi" },
                new MongoWebGallery.Models.Tag { Name = "Komiks" }
            };
            await tagCollection.InsertManyAsync(tags);
        }

        var technologyCollection = scopedServiceProvider.GetRequiredService<IMongoCollection<Technology>>();
        var existingTechnologies = await technologyCollection.CountDocumentsAsync(_ => true);
        if (existingTechnologies == 0)
        {
            var technologies = new List<Technology>
            {
                new Technology { Name = "DALL·E 3 (OpenAI)", Url = "https://openai.com/dall-e-3" },
                new Technology { Name = "MidJourney", Url = "https://www.midjourney.com/home/" },
                new Technology { Name = "Artbreeder", Url = "https://www.artbreeder.com/" },
                new Technology { Name = "RunwayMLy", Url = "https://runwayml.com/" },
                new Technology { Name = "DeepDream (Google)", Url = "https://deepdreamgenerator.com/" }
            };
            await technologyCollection.InsertManyAsync(technologies);
        }
    }
}
