﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Timeline.Entities;

namespace Timeline.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Timeline.Entities.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("EncryptedPassword")
                        .IsRequired()
                        .HasColumnName("password");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasMaxLength(26);

                    b.Property<string>("RoleString")
                        .IsRequired()
                        .HasColumnName("roles");

                    b.Property<long>("Version")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("version")
                        .HasDefaultValue(0L);

                    b.HasKey("Id");

                    b.ToTable("users");
                });

            modelBuilder.Entity("Timeline.Entities.UserAvatar", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<byte[]>("Data")
                        .HasColumnName("data");

                    b.Property<DateTime>("LastModified")
                        .HasColumnName("last_modified");

                    b.Property<string>("Type")
                        .HasColumnName("type");

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("user_avatars");
                });

            modelBuilder.Entity("Timeline.Entities.UserAvatar", b =>
                {
                    b.HasOne("Timeline.Entities.User")
                        .WithOne("Avatar")
                        .HasForeignKey("Timeline.Entities.UserAvatar", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
