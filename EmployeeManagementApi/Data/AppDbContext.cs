using EmployeeManagementApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }

   
}