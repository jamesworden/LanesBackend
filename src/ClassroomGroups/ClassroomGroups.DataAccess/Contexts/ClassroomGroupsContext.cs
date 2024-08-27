using ClassroomGroups.DataAccess.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.DataAccess.Contexts;

public class ClassroomGroupsContext : DbContext
{
  public ClassroomGroupsContext(DbContextOptions<ClassroomGroupsContext> options)
    : base(options) { }

  public DbSet<AccountDTO> Accounts { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<AccountDTO>().HasIndex(u => u.AccountId).IsUnique();
  }
}
