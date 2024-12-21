﻿// <auto-generated />
using System;
using ClassroomGroups.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
    [DbContext(typeof(ClassroomGroupsContext))]
    [Migration("20241221023845_ReduceStudentCount")]
    partial class ReduceStudentCount
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.AccountDTO", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("GoogleNameIdentifier")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("PrimaryEmail")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SubscriptionId")
                        .HasColumnType("TEXT");

                    b.Property<int>("SubscriptionKey")
                        .HasColumnType("INTEGER");

                    b.HasKey("Key");

                    b.HasIndex("GoogleNameIdentifier")
                        .IsUnique();

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("SubscriptionKey");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.ClassroomDTO", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("TEXT");

                    b.Property<int>("AccountKey")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.HasIndex("AccountKey");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("Classrooms");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.ColumnDTO", b =>
                {
                    b.Property<int>("FieldKey")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ConfigurationKey")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ConfigurationId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Enabled")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("FieldId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("Key")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Ordinal")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Sort")
                        .HasColumnType("INTEGER");

                    b.HasKey("FieldKey", "ConfigurationKey");

                    b.HasIndex("ConfigurationKey");

                    b.ToTable("Columns");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.ConfigurationDTO", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ClassroomId")
                        .HasColumnType("TEXT");

                    b.Property<int>("ClassroomKey")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("DefaultGroupId")
                        .HasColumnType("TEXT");

                    b.Property<int?>("DefaultGroupKey")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.HasIndex("ClassroomKey");

                    b.HasIndex("DefaultGroupKey")
                        .IsUnique();

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("Configurations");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.FieldDTO", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ClassroomId")
                        .HasColumnType("TEXT");

                    b.Property<int>("ClassroomKey")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Key");

                    b.HasIndex("ClassroomKey");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("Fields");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.GroupDTO", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ConfigurationId")
                        .HasColumnType("TEXT");

                    b.Property<int>("ConfigurationKey")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsLocked")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Ordinal")
                        .HasColumnType("INTEGER");

                    b.HasKey("Key");

                    b.HasIndex("ConfigurationKey");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.StudentDTO", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ClassroomId")
                        .HasColumnType("TEXT");

                    b.Property<int>("ClassroomKey")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.HasIndex("ClassroomKey");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("Students");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.StudentFieldDTO", b =>
                {
                    b.Property<int>("StudentKey")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FieldKey")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("FieldId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("Key")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("StudentId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("StudentKey", "FieldKey");

                    b.HasIndex("FieldKey");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("StudentFields");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.StudentGroupDTO", b =>
                {
                    b.Property<int>("StudentKey")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GroupKey")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("GroupId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("Key")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Ordinal")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("StudentId")
                        .HasColumnType("TEXT");

                    b.HasKey("StudentKey", "GroupKey");

                    b.HasIndex("GroupKey");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("StudentGroups");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.SubscriptionDTO", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("MaxClassrooms")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MaxConfigurationsPerClassroom")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MaxFieldsPerClassroom")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MaxStudentsPerClassroom")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SubscriptionType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("Subscriptions");

                    b.HasData(
                        new
                        {
                            Key = 1,
                            DisplayName = "Free",
                            Id = new Guid("00000000-0000-0000-0000-000000000001"),
                            MaxClassrooms = 2,
                            MaxConfigurationsPerClassroom = 3,
                            MaxFieldsPerClassroom = 5,
                            MaxStudentsPerClassroom = 30,
                            SubscriptionType = "FREE"
                        },
                        new
                        {
                            Key = 2,
                            DisplayName = "Basic",
                            Id = new Guid("00000000-0000-0000-0000-000000000002"),
                            MaxClassrooms = 5,
                            MaxConfigurationsPerClassroom = 20,
                            MaxFieldsPerClassroom = 20,
                            MaxStudentsPerClassroom = 40,
                            SubscriptionType = "BASIC"
                        },
                        new
                        {
                            Key = 3,
                            DisplayName = "Pro",
                            Id = new Guid("00000000-0000-0000-0000-000000000003"),
                            MaxClassrooms = 50,
                            MaxConfigurationsPerClassroom = 50,
                            MaxFieldsPerClassroom = 50,
                            MaxStudentsPerClassroom = 50,
                            SubscriptionType = "PRO"
                        });
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.AccountDTO", b =>
                {
                    b.HasOne("ClassroomGroups.DataAccess.DTOs.SubscriptionDTO", "SubscriptionDTO")
                        .WithMany("Accounts")
                        .HasForeignKey("SubscriptionKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SubscriptionDTO");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.ClassroomDTO", b =>
                {
                    b.HasOne("ClassroomGroups.DataAccess.DTOs.AccountDTO", "AccountDTO")
                        .WithMany("Classrooms")
                        .HasForeignKey("AccountKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AccountDTO");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.ColumnDTO", b =>
                {
                    b.HasOne("ClassroomGroups.DataAccess.DTOs.ConfigurationDTO", "ConfigurationDTO")
                        .WithMany("Columns")
                        .HasForeignKey("ConfigurationKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ClassroomGroups.DataAccess.DTOs.FieldDTO", "FieldDTO")
                        .WithMany("Columns")
                        .HasForeignKey("FieldKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConfigurationDTO");

                    b.Navigation("FieldDTO");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.ConfigurationDTO", b =>
                {
                    b.HasOne("ClassroomGroups.DataAccess.DTOs.ClassroomDTO", "ClassroomDTO")
                        .WithMany("Configurations")
                        .HasForeignKey("ClassroomKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ClassroomGroups.DataAccess.DTOs.GroupDTO", "DefaultGroupDTO")
                        .WithOne()
                        .HasForeignKey("ClassroomGroups.DataAccess.DTOs.ConfigurationDTO", "DefaultGroupKey")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("ClassroomDTO");

                    b.Navigation("DefaultGroupDTO");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.FieldDTO", b =>
                {
                    b.HasOne("ClassroomGroups.DataAccess.DTOs.ClassroomDTO", "ClassroomDTO")
                        .WithMany("Fields")
                        .HasForeignKey("ClassroomKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ClassroomDTO");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.GroupDTO", b =>
                {
                    b.HasOne("ClassroomGroups.DataAccess.DTOs.ConfigurationDTO", "ConfigurationDTO")
                        .WithMany("Groups")
                        .HasForeignKey("ConfigurationKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConfigurationDTO");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.StudentDTO", b =>
                {
                    b.HasOne("ClassroomGroups.DataAccess.DTOs.ClassroomDTO", "ClassroomDTO")
                        .WithMany("Students")
                        .HasForeignKey("ClassroomKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ClassroomDTO");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.StudentFieldDTO", b =>
                {
                    b.HasOne("ClassroomGroups.DataAccess.DTOs.FieldDTO", "FieldDTO")
                        .WithMany("StudentFields")
                        .HasForeignKey("FieldKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ClassroomGroups.DataAccess.DTOs.StudentDTO", "StudentDTO")
                        .WithMany("StudentFields")
                        .HasForeignKey("StudentKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FieldDTO");

                    b.Navigation("StudentDTO");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.StudentGroupDTO", b =>
                {
                    b.HasOne("ClassroomGroups.DataAccess.DTOs.GroupDTO", "GroupDTO")
                        .WithMany("StudentGroups")
                        .HasForeignKey("GroupKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ClassroomGroups.DataAccess.DTOs.StudentDTO", "StudentDTO")
                        .WithMany("StudentGroups")
                        .HasForeignKey("StudentKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GroupDTO");

                    b.Navigation("StudentDTO");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.AccountDTO", b =>
                {
                    b.Navigation("Classrooms");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.ClassroomDTO", b =>
                {
                    b.Navigation("Configurations");

                    b.Navigation("Fields");

                    b.Navigation("Students");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.ConfigurationDTO", b =>
                {
                    b.Navigation("Columns");

                    b.Navigation("Groups");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.FieldDTO", b =>
                {
                    b.Navigation("Columns");

                    b.Navigation("StudentFields");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.GroupDTO", b =>
                {
                    b.Navigation("StudentGroups");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.StudentDTO", b =>
                {
                    b.Navigation("StudentFields");

                    b.Navigation("StudentGroups");
                });

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.SubscriptionDTO", b =>
                {
                    b.Navigation("Accounts");
                });
#pragma warning restore 612, 618
        }
    }
}
