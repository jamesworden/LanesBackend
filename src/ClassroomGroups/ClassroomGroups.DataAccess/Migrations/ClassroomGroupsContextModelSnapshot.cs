﻿// <auto-generated />
using System;
using ClassroomGroups.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
    [DbContext(typeof(ClassroomGroupsContext))]
    partial class ClassroomGroupsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("ClassroomGroups.DataAccess.DTOs.AccountDTO", b =>
                {
                    b.Property<int>("AccountKey")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("TEXT");

                    b.Property<string>("GoogleNameIdentifier")
                        .HasColumnType("TEXT");

                    b.Property<string>("PrimaryEmail")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("AccountKey");

                    b.HasIndex("AccountId")
                        .IsUnique();

                    b.ToTable("Accounts");
                });
#pragma warning restore 612, 618
        }
    }
}
