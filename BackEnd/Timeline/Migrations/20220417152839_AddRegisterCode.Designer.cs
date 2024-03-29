﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Timeline.Entities;

#nullable disable

namespace Timeline.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20220417152839_AddRegisterCode")]
    partial class AddRegisterCode
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.4");

            modelBuilder.Entity("Timeline.Entities.BookmarkTimelineEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<long>("Rank")
                        .HasColumnType("INTEGER")
                        .HasColumnName("rank");

                    b.Property<long>("TimelineId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("timeline");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user");

                    b.HasKey("Id");

                    b.HasIndex("TimelineId");

                    b.HasIndex("UserId");

                    b.ToTable("bookmark_timelines");
                });

            modelBuilder.Entity("Timeline.Entities.DataEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("BLOB")
                        .HasColumnName("data");

                    b.Property<int>("Ref")
                        .HasColumnType("INTEGER")
                        .HasColumnName("ref");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("tag");

                    b.HasKey("Id");

                    b.HasIndex("Tag")
                        .IsUnique();

                    b.ToTable("data");
                });

            modelBuilder.Entity("Timeline.Entities.HighlightTimelineEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateTime>("AddTime")
                        .HasColumnType("TEXT")
                        .HasColumnName("add_time");

                    b.Property<long?>("OperatorId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("operator_id");

                    b.Property<long>("Order")
                        .HasColumnType("INTEGER")
                        .HasColumnName("order");

                    b.Property<long>("TimelineId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("timeline_id");

                    b.HasKey("Id");

                    b.HasIndex("OperatorId");

                    b.HasIndex("TimelineId");

                    b.ToTable("highlight_timelines");
                });

            modelBuilder.Entity("Timeline.Entities.JwtTokenEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<byte[]>("Key")
                        .IsRequired()
                        .HasColumnType("BLOB")
                        .HasColumnName("key");

                    b.HasKey("Id");

                    b.ToTable("jwt_token");
                });

            modelBuilder.Entity("Timeline.Entities.MigrationEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("migrations");
                });

            modelBuilder.Entity("Timeline.Entities.RegisterCode", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("code");

                    b.Property<bool>("Enabled")
                        .HasColumnType("INTEGER")
                        .HasColumnName("enabled");

                    b.Property<long?>("OwnerId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("owner_id");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("register_code");
                });

            modelBuilder.Entity("Timeline.Entities.TimelineEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Color")
                        .HasColumnType("TEXT")
                        .HasColumnName("color");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_time");

                    b.Property<long>("CurrentPostLocalId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("current_post_local_id");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT")
                        .HasColumnName("description");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("TEXT")
                        .HasColumnName("last_modified");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.Property<DateTime>("NameLastModified")
                        .HasColumnType("TEXT")
                        .HasColumnName("name_last_modified");

                    b.Property<long>("OwnerId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("owner");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT")
                        .HasColumnName("title");

                    b.Property<string>("UniqueId")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("unique_id")
                        .HasDefaultValueSql("lower(hex(randomblob(16)))");

                    b.Property<int>("Visibility")
                        .HasColumnType("INTEGER")
                        .HasColumnName("visibility");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("timelines");
                });

            modelBuilder.Entity("Timeline.Entities.TimelineMemberEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<long>("TimelineId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("timeline");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user");

                    b.HasKey("Id");

                    b.HasIndex("TimelineId");

                    b.HasIndex("UserId");

                    b.ToTable("timeline_members");
                });

            modelBuilder.Entity("Timeline.Entities.TimelinePostDataEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("DataTag")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("data_tag");

                    b.Property<long>("Index")
                        .HasColumnType("INTEGER")
                        .HasColumnName("index");

                    b.Property<string>("Kind")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("kind");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("TEXT")
                        .HasColumnName("last_updated");

                    b.Property<long>("PostId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("post");

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.ToTable("timeline_post_data");
                });

            modelBuilder.Entity("Timeline.Entities.TimelinePostEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<long?>("AuthorId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("author");

                    b.Property<string>("Color")
                        .HasColumnType("TEXT")
                        .HasColumnName("color");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT")
                        .HasColumnName("content");

                    b.Property<string>("ContentType")
                        .HasColumnType("TEXT")
                        .HasColumnName("content_type");

                    b.Property<bool>("Deleted")
                        .HasColumnType("INTEGER")
                        .HasColumnName("deleted");

                    b.Property<string>("ExtraContent")
                        .HasColumnType("TEXT")
                        .HasColumnName("extra_content");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("TEXT")
                        .HasColumnName("last_updated");

                    b.Property<long>("LocalId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("local_id");

                    b.Property<DateTime>("Time")
                        .HasColumnType("TEXT")
                        .HasColumnName("time");

                    b.Property<long>("TimelineId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("timeline");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("TimelineId");

                    b.ToTable("timeline_posts");
                });

            modelBuilder.Entity("Timeline.Entities.UserAvatarEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("DataTag")
                        .HasColumnType("TEXT")
                        .HasColumnName("data_tag");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("TEXT")
                        .HasColumnName("last_modified");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT")
                        .HasColumnName("type");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("user_avatars");
                });

            modelBuilder.Entity("Timeline.Entities.UserConfigurationEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<int>("BookmarkVisibility")
                        .HasColumnType("INTEGER")
                        .HasColumnName("bookmark_visibility");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("user_config");
                });

            modelBuilder.Entity("Timeline.Entities.UserEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("create_time")
                        .HasDefaultValueSql("datetime('now', 'utc')");

                    b.Property<DateTime>("LastModified")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("last_modified")
                        .HasDefaultValueSql("datetime('now', 'utc')");

                    b.Property<string>("Nickname")
                        .HasColumnType("TEXT")
                        .HasColumnName("nickname");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("password");

                    b.Property<string>("UniqueId")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("unique_id")
                        .HasDefaultValueSql("lower(hex(randomblob(16)))");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("username");

                    b.Property<DateTime>("UsernameChangeTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("username_change_time")
                        .HasDefaultValueSql("datetime('now', 'utc')");

                    b.Property<long>("Version")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(0L)
                        .HasColumnName("version");

                    b.HasKey("Id");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("users");
                });

            modelBuilder.Entity("Timeline.Entities.UserPermissionEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Permission")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("permission");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("user_permission");
                });

            modelBuilder.Entity("Timeline.Entities.UserRegisterInfo", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<long?>("IntroducerId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("introducer_id");

                    b.Property<string>("RegisterCode")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("register_code");

                    b.Property<DateTime>("RegisterTime")
                        .HasColumnType("TEXT")
                        .HasColumnName("register_time");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("IntroducerId");

                    b.HasIndex("UserId");

                    b.ToTable("user_register_info");
                });

            modelBuilder.Entity("Timeline.Entities.UserTokenEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateTime?>("CreateAt")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_at");

                    b.Property<bool>("Deleted")
                        .HasColumnType("INTEGER")
                        .HasColumnName("deleted");

                    b.Property<DateTime?>("ExpireAt")
                        .HasColumnType("TEXT")
                        .HasColumnName("expire_at");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("token");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("Token")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("user_token");
                });

            modelBuilder.Entity("Timeline.Entities.BookmarkTimelineEntity", b =>
                {
                    b.HasOne("Timeline.Entities.TimelineEntity", "Timeline")
                        .WithMany()
                        .HasForeignKey("TimelineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Timeline.Entities.UserEntity", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Timeline");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Timeline.Entities.HighlightTimelineEntity", b =>
                {
                    b.HasOne("Timeline.Entities.UserEntity", "Operator")
                        .WithMany()
                        .HasForeignKey("OperatorId");

                    b.HasOne("Timeline.Entities.TimelineEntity", "Timeline")
                        .WithMany()
                        .HasForeignKey("TimelineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Operator");

                    b.Navigation("Timeline");
                });

            modelBuilder.Entity("Timeline.Entities.RegisterCode", b =>
                {
                    b.HasOne("Timeline.Entities.UserEntity", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Timeline.Entities.TimelineEntity", b =>
                {
                    b.HasOne("Timeline.Entities.UserEntity", "Owner")
                        .WithMany("Timelines")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Timeline.Entities.TimelineMemberEntity", b =>
                {
                    b.HasOne("Timeline.Entities.TimelineEntity", "Timeline")
                        .WithMany("Members")
                        .HasForeignKey("TimelineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Timeline.Entities.UserEntity", "User")
                        .WithMany("TimelinesJoined")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Timeline");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Timeline.Entities.TimelinePostDataEntity", b =>
                {
                    b.HasOne("Timeline.Entities.TimelinePostEntity", "Post")
                        .WithMany("DataList")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");
                });

            modelBuilder.Entity("Timeline.Entities.TimelinePostEntity", b =>
                {
                    b.HasOne("Timeline.Entities.UserEntity", "Author")
                        .WithMany("TimelinePosts")
                        .HasForeignKey("AuthorId");

                    b.HasOne("Timeline.Entities.TimelineEntity", "Timeline")
                        .WithMany("Posts")
                        .HasForeignKey("TimelineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");

                    b.Navigation("Timeline");
                });

            modelBuilder.Entity("Timeline.Entities.UserAvatarEntity", b =>
                {
                    b.HasOne("Timeline.Entities.UserEntity", "User")
                        .WithOne("Avatar")
                        .HasForeignKey("Timeline.Entities.UserAvatarEntity", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Timeline.Entities.UserConfigurationEntity", b =>
                {
                    b.HasOne("Timeline.Entities.UserEntity", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Timeline.Entities.UserPermissionEntity", b =>
                {
                    b.HasOne("Timeline.Entities.UserEntity", "User")
                        .WithMany("Permissions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Timeline.Entities.UserRegisterInfo", b =>
                {
                    b.HasOne("Timeline.Entities.UserEntity", "Introducer")
                        .WithMany()
                        .HasForeignKey("IntroducerId");

                    b.HasOne("Timeline.Entities.UserEntity", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Introducer");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Timeline.Entities.UserTokenEntity", b =>
                {
                    b.HasOne("Timeline.Entities.UserEntity", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Timeline.Entities.TimelineEntity", b =>
                {
                    b.Navigation("Members");

                    b.Navigation("Posts");
                });

            modelBuilder.Entity("Timeline.Entities.TimelinePostEntity", b =>
                {
                    b.Navigation("DataList");
                });

            modelBuilder.Entity("Timeline.Entities.UserEntity", b =>
                {
                    b.Navigation("Avatar");

                    b.Navigation("Permissions");

                    b.Navigation("TimelinePosts");

                    b.Navigation("Timelines");

                    b.Navigation("TimelinesJoined");
                });
#pragma warning restore 612, 618
        }
    }
}
