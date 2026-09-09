using BookStoreProject.Data;
using BookStoreProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookStoreProject.DAL
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>(options)
    {
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartDetail> CartDetails { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<OrderStatus>().HasData(
                new OrderStatus
                {
                    Id = 1,
                    StatusName = "Pending"
                },
                new OrderStatus
                {
                    Id = 2,
                    StatusName = "Shipped"
                },
                new OrderStatus
                {
                    Id = 3,
                    StatusName = "Delivered"
                },
                new OrderStatus
                {
                    Id = 4,
                    StatusName = "Cancelled"
                }
            );
        }
    }
}