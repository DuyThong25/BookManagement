using BookManagementWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BookManagementWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<Category> Categories { get; set; }

        // OnModelCreating Sử dụng để tạo dữ liệu cho csdl và 
        // Sử dụng migration để truyền dữ liệu xuống cho database
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                    new Category { CategoryId = 1, Name = "Action", DisplayOrder = 1},
                    new Category { CategoryId = 2, Name = "Fiction", DisplayOrder = 2 },
                    new Category { CategoryId = 3, Name = "History", DisplayOrder = 3 }
                );
        }
    }
}
