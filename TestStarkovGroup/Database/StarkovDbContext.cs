using Microsoft.EntityFrameworkCore;
using TestStarkovGroup.Database.Models;

namespace TestStarkovGroup.Database;

public class StarkovDbContext : DbContext
{
    public DbSet<DepartmentDb> Departments { get; set; }
    public DbSet<EmployeeDb> Employees { get; set; }
    public DbSet<JobTitleDb> JobTitles { get; set; }
    
    public StarkovDbContext(DbContextOptions<StarkovDbContext> options) : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
       
    }
}