﻿// <auto-generated />
using System;
using AuthWebApi.DataContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AuthWebApi.Migrations.MysqlDb
{
    [DbContext(typeof(MysqlDbContext))]
    [Migration("20190102104149_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.2-rtm-30932");

            modelBuilder.Entity("DataModelLayer.Models.Comments.Comment", b =>
                {
                    b.Property<long>("UpdatedPostId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate");

                    b.Property<int>("Likes");

                    b.Property<string>("Message");

                    b.Property<int>("UpdatedPostId");

                    b.Property<string>("UpdatedByUserId")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.HasKey("UpdatedPostId");

                    b.HasIndex("UpdatedByUserId");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("DataModelLayer.Models.Comments.CommentFiles", b =>
                {
                    b.Property<long>("UpdatedPostId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CommentId");

                    b.Property<string>("Url");

                    b.HasKey("UpdatedPostId");

                    b.HasIndex("CommentId");

                    b.ToTable("Comment_Files");
                });

            modelBuilder.Entity("DataModelLayer.Models.Entities.UserCommonData", b =>
                {
                    b.Property<int>("UpdatedPostId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("FacebookId");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<string>("UserEmail");

                    b.Property<string>("UpdatedByUserId");

                    b.Property<int>("UserLevel");

                    b.Property<string>("UserName");

                    b.HasKey("UpdatedPostId");

                    b.ToTable("UserCommonData");
                });

            modelBuilder.Entity("DataModelLayer.Models.Posts.Post", b =>
                {
                    b.Property<int>("UpdatedPostId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedDate");

                    b.Property<string>("UpdatedDescription");

                    b.Property<int>("Likes");

                    b.Property<string>("UpdatedPostTtile");

                    b.Property<string>("UpdatedByUserId")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.HasKey("UpdatedPostId");

                    b.HasIndex("UpdatedByUserId");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("DataModelLayer.Models.Posts.PostFiles", b =>
                {
                    b.Property<int>("UpdatedPostId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("UpdatedPostId");

                    b.Property<string>("Url");

                    b.HasKey("UpdatedPostId");

                    b.HasIndex("UpdatedPostId");

                    b.ToTable("PostFiles");
                });

            modelBuilder.Entity("DataModelLayer.Models.Posts.PostLike", b =>
                {
                    b.Property<int>("LikeId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("LikeCount");

                    b.Property<DateTime>("LikeTime");

                    b.Property<int>("UpdatedPostId");

                    b.Property<string>("UpdatedByUserId");

                    b.HasKey("LikeId");

                    b.ToTable("PostLikes");
                });

            modelBuilder.Entity("DataModelLayer.Models.Tikets.ReportEntity", b =>
                {
                    b.Property<int>("UpdatedPostId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AddresedMessage");

                    b.Property<string>("ReportedByUserId");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<int>("EntityId");

                    b.Property<bool>("IsAddressed");

                    b.Property<string>("Message");

                    b.Property<int>("ReportedEntityId");

                    b.HasKey("UpdatedPostId");

                    b.ToTable("ReportEntity");
                });

            modelBuilder.Entity("DataModelLayer.Models.Tikets.TicketFile", b =>
                {
                    b.Property<int>("UpdatedPostId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Url");

                    b.Property<int?>("ReportEntityId");

                    b.Property<int>("ReportedEntityTypeId");

                    b.Property<int?>("TicketsId");

                    b.HasKey("UpdatedPostId");

                    b.HasIndex("ReportEntityId");

                    b.HasIndex("TicketsId");

                    b.ToTable("TicketFile");
                });

            modelBuilder.Entity("DataModelLayer.Models.Tikets.Ticket", b =>
                {
                    b.Property<int>("UpdatedPostId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AddresedMessage");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<bool>("IsAddressed");

                    b.Property<string>("Message");

                    b.Property<string>("TicketIssuerUserId");

                    b.HasKey("UpdatedPostId");

                    b.ToTable("Ticket");
                });

            modelBuilder.Entity("DataModelLayer.Models.Posts.PostFiles", b =>
                {
                    b.HasOne("DataModelLayer.Models.Posts.Post")
                        .WithMany("Files")
                        .HasForeignKey("UpdatedPostId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataModelLayer.Models.Tikets.TicketFile", b =>
                {
                    b.HasOne("DataModelLayer.Models.Tikets.ReportEntity")
                        .WithMany("ReportFiles")
                        .HasForeignKey("ReportEntityId");

                    b.HasOne("DataModelLayer.Models.Tikets.Ticket")
                        .WithMany("TicketFile")
                        .HasForeignKey("TicketsId");
                });
#pragma warning restore 612, 618
        }
    }
}