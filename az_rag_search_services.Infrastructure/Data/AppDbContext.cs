using az_rag_search_services.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace az_rag_search_services.Intrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<Note> Notes { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
}