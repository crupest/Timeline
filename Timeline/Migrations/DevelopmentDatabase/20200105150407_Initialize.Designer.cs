﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Timeline.Entities;

namespace Timeline.Migrations.DevelopmentDatabase
{
    [DbContext(typeof(DevelopmentDatabaseContext))]
    [Migration("20200105150407_Initialize")]
    partial class Initialize
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.0");

            modelBuilder.Entity("Timeline.Entities.TimelineEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnName("create_time")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnName("description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnName("name")
                        .HasColumnType("TEXT");

                    b.Property<long>("OwnerId")
                        .HasColumnName("owner")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Visibility")
                        .HasColumnName("visibility")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("timelines");
                });

            modelBuilder.Entity("Timeline.Entities.TimelineMemberEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("INTEGER");

                    b.Property<long>("TimelineId")
                        .HasColumnName("timeline")
                        .HasColumnType("INTEGER");

                    b.Property<long>("UserId")
                        .HasColumnName("user")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TimelineId");

                    b.HasIndex("UserId");

                    b.ToTable("timeline_members");
                });

            modelBuilder.Entity("Timeline.Entities.TimelinePostEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("INTEGER");

                    b.Property<long>("AuthorId")
                        .HasColumnName("author")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .HasColumnName("content")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnName("last_updated")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Time")
                        .HasColumnName("time")
                        .HasColumnType("TEXT");

                    b.Property<long>("TimelineId")
                        .HasColumnName("timeline")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("TimelineId");

                    b.ToTable("timeline_posts");
                });

            modelBuilder.Entity("Timeline.Entities.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("EncryptedPassword")
                        .IsRequired()
                        .HasColumnName("password")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(26);

                    b.Property<string>("RoleString")
                        .IsRequired()
                        .HasColumnName("roles")
                        .HasColumnType("TEXT");

                    b.Property<long>("Version")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("version")
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(0L);

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("users");
                });

            modelBuilder.Entity("Timeline.Entities.UserAvatar", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Data")
                        .HasColumnName("data")
                        .HasColumnType("BLOB");

                    b.Property<string>("ETag")
                        .HasColumnName("etag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(30);

                    b.Property<DateTime>("LastModified")
                        .HasColumnName("last_modified")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .HasColumnName("type")
                        .HasColumnType("TEXT");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("user_avatars");
                });

            modelBuilder.Entity("Timeline.Entities.UserDetail", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Nickname")
                        .HasColumnName("nickname")
                        .HasColumnType("TEXT")
                        .HasMaxLength(26);

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("user_details");
                });

            modelBuilder.Entity("Timeline.Entities.TimelineEntity", b =>
                {
                    b.HasOne("Timeline.Entities.User", "Owner")
                        .WithMany("Timelines")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Timeline.Entities.TimelineMemberEntity", b =>
                {
                    b.HasOne("Timeline.Entities.TimelineEntity", "Timeline")
                        .WithMany("Members")
                        .HasForeignKey("TimelineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Timeline.Entities.User", "User")
                        .WithMany("TimelinesJoined")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Timeline.Entities.TimelinePostEntity", b =>
                {
                    b.HasOne("Timeline.Entities.User", "Author")
                        .WithMany("TimelinePosts")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Timeline.Entities.TimelineEntity", "Timeline")
                        .WithMany("Posts")
                        .HasForeignKey("TimelineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Timeline.Entities.UserAvatar", b =>
                {
                    b.HasOne("Timeline.Entities.User", null)
                        .WithOne("Avatar")
                        .HasForeignKey("Timeline.Entities.UserAvatar", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Timeline.Entities.UserDetail", b =>
                {
                    b.HasOne("Timeline.Entities.User", null)
                        .WithOne("Detail")
                        .HasForeignKey("Timeline.Entities.UserDetail", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
