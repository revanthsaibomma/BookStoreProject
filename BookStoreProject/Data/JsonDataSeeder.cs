using BookStoreProject.DAL;
using BookStoreProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BookStoreProject.Data
{
    public static class JsonDataSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            try
            {
                // ==========================================
                // JSON FILE PATHS
                // ==========================================

                string dataFolder =
                    Path.Combine(
                        environment.ContentRootPath,
                        "Data");

                string genresFile =
                    Path.Combine(
                        dataFolder,
                        "Genres.json");

                string booksFile =
                    Path.Combine(
                        dataFolder,
                        "Books.json");


                // ==========================================
                // CHECK JSON FILES
                // ==========================================

                if (!File.Exists(genresFile))
                {
                    throw new FileNotFoundException(
                        "Genres.json was not found.",
                        genresFile);
                }

                if (!File.Exists(booksFile))
                {
                    throw new FileNotFoundException(
                        "Books.json was not found.",
                        booksFile);
                }


                // ==========================================
                // JSON OPTIONS
                // ==========================================

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };


                // ==========================================
                // READ GENRES.JSON
                // ==========================================

                string genresJson =
                    await File.ReadAllTextAsync(genresFile);

                var genres =
                    JsonSerializer.Deserialize<List<JsonGenre>>(
                        genresJson,
                        options);

                if (genres == null || genres.Count == 0)
                {
                    throw new Exception(
                        "Genres.json does not contain any data.");
                }


                // ==========================================
                // INSERT MISSING GENRES
                // ==========================================

                int genresAdded = 0;

                foreach (var item in genres)
                {
                    if (item.Id <= 0)
                        continue;

                    if (string.IsNullOrWhiteSpace(item.GenreName))
                        continue;


                    bool exists =
                        await context.Genres
                            .AnyAsync(g => g.Id == item.Id);


                    if (!exists)
                    {
                        context.Genres.Add(
                            new Genre
                            {
                                Id = item.Id,
                                GenreName =
                                    item.GenreName.Trim()
                            });

                        genresAdded++;
                    }
                }


                // ==========================================
                // SAVE GENRES
                // ==========================================

                if (genresAdded > 0)
                {
                    await context.Database
                        .OpenConnectionAsync();

                    try
                    {
                        await context.Database
                            .ExecuteSqlRawAsync(
                                "SET IDENTITY_INSERT [Genres] ON");

                        await context.SaveChangesAsync();

                        await context.Database
                            .ExecuteSqlRawAsync(
                                "SET IDENTITY_INSERT [Genres] OFF");
                    }
                    finally
                    {
                        await context.Database
                            .CloseConnectionAsync();
                    }
                }


                // ==========================================
                // READ BOOKS.JSON
                // ==========================================

                string booksJson =
                    await File.ReadAllTextAsync(booksFile);

                var books =
                    JsonSerializer.Deserialize<List<JsonBook>>(
                        booksJson,
                        options);

                if (books == null || books.Count == 0)
                {
                    throw new Exception(
                        "Books.json does not contain any data.");
                }


                // ==========================================
                // INSERT MISSING BOOKS
                // ==========================================

                int booksAdded = 0;

                foreach (var item in books)
                {
                    if (string.IsNullOrWhiteSpace(item.BookName))
                        continue;

                    if (string.IsNullOrWhiteSpace(item.AuthorName))
                        continue;

                    if (item.Price <= 0)
                        continue;

                    if (item.GenreId <= 0)
                        continue;

                    if (item.Stock < 0)
                        continue;


                    // Check genre exists
                    bool genreExists =
                        await context.Genres
                            .AnyAsync(
                                g => g.Id == item.GenreId);

                    if (!genreExists)
                    {
                        Console.WriteLine(
                            $"Skipping book '{item.BookName}' because GenreId {item.GenreId} does not exist.");

                        continue;
                    }


                    // Check duplicate book by name
                    bool bookExists =
                        await context.Books
                            .AnyAsync(
                                b => b.BookName.ToLower()
                                    == item.BookName
                                        .Trim()
                                        .ToLower());

                    if (bookExists)
                    {
                        Console.WriteLine(
                            $"Book already exists: {item.BookName}");

                        continue;
                    }


                    context.Books.Add(
                        new Book
                        {
                            BookName =
                                item.BookName.Trim(),

                            AuthorName =
                                item.AuthorName.Trim(),

                            Price =
                                item.Price,

                            Description =
                                item.Description?.Trim()
                                ?? "",

                            Image =
                                item.Image?.Trim(),

                            GenreId =
                                item.GenreId,

                            Stock =
                                item.Stock
                        });

                    booksAdded++;
                }


                // ==========================================
                // SAVE BOOKS
                // ==========================================

                if (booksAdded > 0)
                {
                    await context.SaveChangesAsync();
                }


                // ==========================================
                // SUCCESS MESSAGE
                // ==========================================

                int totalGenres =
                    await context.Genres.CountAsync();

                int totalBooks =
                    await context.Books.CountAsync();


                Console.WriteLine(
                    "==========================================");

                Console.WriteLine(
                    "JSON DATA SEEDING COMPLETED");

                Console.WriteLine(
                    $"Genres Added : {genresAdded}");

                Console.WriteLine(
                    $"Books Added  : {booksAdded}");

                Console.WriteLine(
                    $"Total Genres : {totalGenres}");

                Console.WriteLine(
                    $"Total Books  : {totalBooks}");

                Console.WriteLine(
                    "==========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "==========================================");

                Console.WriteLine(
                    "JSON DATA SEEDING ERROR");

                Console.WriteLine(
                    ex.ToString());

                Console.WriteLine(
                    "==========================================");

                throw;
            }
        }


        // ==========================================
        // JSON GENRE CLASS
        // ==========================================

        private class JsonGenre
        {
            public int Id { get; set; }

            public string GenreName { get; set; } = "";
        }


        // ==========================================
        // JSON BOOK CLASS
        // ==========================================

        private class JsonBook
        {
            public string BookName { get; set; } = "";

            public string AuthorName { get; set; } = "";

            public double Price { get; set; }

            public int GenreId { get; set; }

            public int Stock { get; set; }

            public string? Image { get; set; }

            public string? Description { get; set; }
        }
    }
}