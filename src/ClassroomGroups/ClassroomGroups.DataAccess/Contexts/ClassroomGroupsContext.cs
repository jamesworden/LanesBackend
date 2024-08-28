using ClassroomGroups.DataAccess.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.DataAccess.Contexts;

public class ClassroomGroupsContext : DbContext
{
  public ClassroomGroupsContext(DbContextOptions<ClassroomGroupsContext> options)
    : base(options) { }

  public DbSet<AccountDTO> Accounts { get; set; }

  public DbSet<ClassroomDTO> Classrooms { get; set; }

  public DbSet<StudentDTO> Students { get; set; }

  public DbSet<FieldDTO> Fields { get; set; }

  public DbSet<GroupDTO> Groups { get; set; }

  public DbSet<StudentFieldDTO> StudentFields { get; set; }

  public DbSet<StudentGroupDTO> StudentGroups { get; set; }

  public DbSet<ConfigurationDTO> Configurations { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // One to Many
    modelBuilder.Entity<AccountDTO>().HasIndex(e => e.Id).IsUnique();
    modelBuilder
      .Entity<AccountDTO>()
      .HasMany(e => e.Classrooms)
      .WithOne(e => e.AccountDTO)
      .HasForeignKey(e => e.AccountKey)
      .HasPrincipalKey(e => e.Key);

    modelBuilder.Entity<ClassroomDTO>().HasIndex(classroomDTO => classroomDTO.Id).IsUnique();
    modelBuilder
      .Entity<ClassroomDTO>()
      .HasMany(e => e.Students)
      .WithOne(e => e.ClassroomDTO)
      .HasForeignKey(e => e.ClassroomKey)
      .HasPrincipalKey(e => e.Key);
    modelBuilder
      .Entity<ClassroomDTO>()
      .HasMany(e => e.Fields)
      .WithOne(e => e.ClassroomDTO)
      .HasForeignKey(e => e.ClassroomKey)
      .HasPrincipalKey(e => e.Key);
    modelBuilder
      .Entity<ClassroomDTO>()
      .HasMany(e => e.Configurations)
      .WithOne(e => e.ClassroomDTO)
      .HasForeignKey(e => e.ClassroomKey)
      .HasPrincipalKey(e => e.Key);

    modelBuilder
      .Entity<ConfigurationDTO>()
      .HasIndex(configurationDTO => configurationDTO.Id)
      .IsUnique();
    modelBuilder
      .Entity<ConfigurationDTO>()
      .HasMany(e => e.Groups)
      .WithOne(e => e.ConfigurationDTO)
      .HasForeignKey(e => e.ConfigurationKey)
      .HasPrincipalKey(e => e.Key);

    // Many to Many
    modelBuilder.Entity<StudentDTO>().HasIndex(studentDTO => studentDTO.Id).IsUnique();

    modelBuilder.Entity<FieldDTO>().HasIndex(fieldDTO => fieldDTO.Id).IsUnique();
    modelBuilder
      .Entity<StudentFieldDTO>()
      .HasIndex(studentFieldDTO => studentFieldDTO.Id)
      .IsUnique();
    modelBuilder
      .Entity<StudentDTO>()
      .HasMany(e => e.Fields)
      .WithMany(e => e.Students)
      .UsingEntity<StudentFieldDTO>(
        l =>
          l.HasOne(e => e.FieldDTO).WithMany(e => e.StudentFields).HasForeignKey(e => e.FieldKey),
        r =>
          r.HasOne(e => e.StudentDTO)
            .WithMany(e => e.StudentFields)
            .HasForeignKey(e => e.StudentKey),
        j =>
        {
          j.HasKey(t => new { t.StudentKey, t.FieldKey });
        }
      );

    modelBuilder.Entity<GroupDTO>().HasIndex(groupDTO => groupDTO.Id).IsUnique();
    modelBuilder
      .Entity<StudentGroupDTO>()
      .HasIndex(studentGroupDTO => studentGroupDTO.Id)
      .IsUnique();
    modelBuilder
      .Entity<StudentDTO>()
      .HasMany(e => e.Groups)
      .WithMany(e => e.Students)
      .UsingEntity<StudentGroupDTO>(
        l =>
          l.HasOne(e => e.GroupDTO).WithMany(e => e.StudentGroups).HasForeignKey(e => e.GroupKey),
        r =>
          r.HasOne(e => e.StudentDTO)
            .WithMany(e => e.StudentGroups)
            .HasForeignKey(e => e.StudentKey),
        j =>
        {
          j.HasKey(t => new { t.StudentKey, t.GroupKey });
        }
      );
  }
}
