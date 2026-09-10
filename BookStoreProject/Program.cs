using BookStoreProject.DAL;
using BookStoreProject.Data;
using BookStoreProject.Mapping;
using BookStoreProject.Models;
using BookStoreProject.Repository;
using BookStoreProject.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

using QuestPDF.Infrastructure;

namespace BookStoreProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC
            builder.Services.AddControllersWithViews();


            // Repositories
            builder.Services.AddTransient<IBookRepo, BookRepo>();
            builder.Services.AddScoped<IGenreRepo, GenreRepo>();
            builder.Services.AddScoped<ICartRepo, CartRepo>();
            builder.Services.AddScoped<IOrderRepo, OrderRepo>();
            builder.Services.AddScoped<IAdminOrderRepo, AdminOrderRepo>();


            // Services
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IPdfService, PdfService>();
            builder.Services.AddScoped<GeminiService>();

            // Identity Email Sender
            builder.Services.AddTransient<IEmailSender, EmailSender>();


            // AutoMapper
            builder.Services.AddAutoMapper(cfg => { },typeof(Program).Assembly);


            // QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;


            // Database
            var connectionString =
                builder.Configuration.GetConnectionString("AzureCon")
                ?? throw new InvalidOperationException(
                    "Connection string 'AzureCon' not found."
                );
            //var connectionString =
            //    builder.Configuration.GetConnectionString("DefaultConnection")
            //    ?? throw new InvalidOperationException(
            //        "Connection string 'DefaultConnection' not found."
            //    );
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString)
            );


            // Identity
            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            // Identity cookie settings
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });


            // Razor Pages
            builder.Services.AddRazorPages();


            var app = builder.Build();


            // HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();


            // MVC
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            )
            .WithStaticAssets();


            // Identity Razor Pages
            app.MapRazorPages();


            // ==========================================
            // DATABASE SEEDING
            // ==========================================

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var context =
                        services.GetRequiredService<ApplicationDbContext>();

                    var environment =
                        services.GetRequiredService<IWebHostEnvironment>();


                    // Seed Genres and Books from JSON
                    await JsonDataSeeder.SeedAsync(
                        context,
                        environment);


                    // Seed Roles and Admin
                    await RoleSeeder.SeedRolesAndAdminAsync(
                        services);
                }
                catch (Exception ex)
                {
                    var logger =
                        services.GetRequiredService<ILogger<Program>>();

                    logger.LogError(
                        ex,
                        "An error occurred while seeding the database.");
                }
            }


            await app.RunAsync();
        }
    }
}