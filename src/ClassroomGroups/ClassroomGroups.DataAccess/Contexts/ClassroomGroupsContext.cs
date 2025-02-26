using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Authentication.Entities;
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

  public DbSet<ColumnDTO> Columns { get; set; }

  public DbSet<SubscriptionDTO> Subscriptions { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<AccountDTO>().HasIndex(e => e.Id).IsUnique();
    modelBuilder
      .Entity<AccountDTO>()
      .HasMany(e => e.Classrooms)
      .WithOne(e => e.AccountDTO)
      .HasForeignKey(e => e.AccountKey)
      .HasPrincipalKey(e => e.Key)
      .OnDelete(DeleteBehavior.Cascade);
    modelBuilder
      .Entity<AccountDTO>()
      .HasIndex(AccountDTO => AccountDTO.GoogleNameIdentifier)
      .IsUnique();

    modelBuilder.Entity<ClassroomDTO>().HasIndex(classroomDTO => classroomDTO.Id).IsUnique();
    modelBuilder
      .Entity<ClassroomDTO>()
      .HasMany(e => e.Students)
      .WithOne(e => e.ClassroomDTO)
      .HasForeignKey(e => e.ClassroomKey)
      .HasPrincipalKey(e => e.Key)
      .OnDelete(DeleteBehavior.Cascade);
    modelBuilder
      .Entity<ClassroomDTO>()
      .HasMany(e => e.Fields)
      .WithOne(e => e.ClassroomDTO)
      .HasForeignKey(e => e.ClassroomKey)
      .HasPrincipalKey(e => e.Key)
      .OnDelete(DeleteBehavior.Cascade);
    modelBuilder
      .Entity<ClassroomDTO>()
      .HasMany(e => e.Configurations)
      .WithOne(e => e.ClassroomDTO)
      .HasForeignKey(e => e.ClassroomKey)
      .HasPrincipalKey(e => e.Key)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder
      .Entity<ConfigurationDTO>()
      .HasIndex(configurationDTO => configurationDTO.Id)
      .IsUnique();
    modelBuilder
      .Entity<ConfigurationDTO>()
      .HasMany(e => e.Groups)
      .WithOne(e => e.ConfigurationDTO)
      .HasForeignKey(e => e.ConfigurationKey)
      .HasPrincipalKey(e => e.Key)
      .OnDelete(DeleteBehavior.Cascade);
    modelBuilder
      .Entity<ConfigurationDTO>()
      .HasOne(c => c.DefaultGroupDTO)
      .WithOne()
      .HasForeignKey<ConfigurationDTO>(c => c.DefaultGroupKey)
      .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<FieldDTO>().HasIndex(fieldDTO => fieldDTO.Id).IsUnique();
    modelBuilder
      .Entity<FieldDTO>()
      .HasMany(e => e.Configurations)
      .WithMany(e => e.Fields)
      .UsingEntity<ColumnDTO>(
        j =>
          j.HasOne(c => c.ConfigurationDTO)
            .WithMany(c => c.Columns)
            .HasForeignKey(c => c.ConfigurationKey),
        j => j.HasOne(c => c.FieldDTO).WithMany(f => f.Columns).HasForeignKey(c => c.FieldKey),
        j =>
        {
          j.HasKey(t => new { t.FieldKey, t.ConfigurationKey });
        }
      );

    modelBuilder.Entity<StudentDTO>().HasIndex(studentDTO => studentDTO.Id).IsUnique();
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

    modelBuilder
      .Entity<ColumnDTO>()
      .HasOne(c => c.ConfigurationDTO)
      .WithMany(c => c.Columns)
      .HasForeignKey(c => c.ConfigurationKey)
      .OnDelete(DeleteBehavior.Cascade);
    modelBuilder
      .Entity<ColumnDTO>()
      .HasOne(c => c.FieldDTO)
      .WithMany(f => f.Columns)
      .HasForeignKey(c => c.FieldKey)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder
      .Entity<SubscriptionDTO>()
      .HasIndex(subscriptionDTO => subscriptionDTO.Id)
      .IsUnique();
    modelBuilder
      .Entity<SubscriptionDTO>()
      .Property(subscriptionDTO => subscriptionDTO.SubscriptionType)
      .HasConversion<string>();
    modelBuilder
      .Entity<SubscriptionDTO>()
      .HasMany(e => e.Accounts)
      .WithOne(e => e.SubscriptionDTO)
      .HasForeignKey(e => e.SubscriptionKey)
      .HasPrincipalKey(e => e.Key)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<SubscriptionDTO>().HasData(GetSubscriptions());
  }

  private static List<SubscriptionDTO> GetSubscriptions()
  {
    return
    [
      new SubscriptionDTO
      {
        Id = new Guid("00000000-0000-0000-0000-000000000001"),
        DisplayName = "Free",
        Key = 1,
        SubscriptionType = SubscriptionType.FREE,
        MaxClassrooms = 2,
        MaxStudentsPerClassroom = 30,
        MaxFieldsPerClassroom = 5,
        MaxConfigurationsPerClassroom = 3
      },
      new SubscriptionDTO
      {
        Id = new Guid("00000000-0000-0000-0000-000000000002"),
        DisplayName = "Basic",
        Key = 2,
        SubscriptionType = SubscriptionType.BASIC,
        MaxClassrooms = 5,
        MaxStudentsPerClassroom = 40,
        MaxFieldsPerClassroom = 20,
        MaxConfigurationsPerClassroom = 20
      },
      new SubscriptionDTO
      {
        Id = new Guid("00000000-0000-0000-0000-000000000003"),
        DisplayName = "Pro",
        Key = 3,
        SubscriptionType = SubscriptionType.PRO,
        MaxClassrooms = 50,
        MaxStudentsPerClassroom = 50,
        MaxFieldsPerClassroom = 50,
        MaxConfigurationsPerClassroom = 50
      }
    ];
  }
}
