﻿// <auto-generated />
using AuthWebApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.EntityFrameworkCore.Storage.Internal;
using System;

namespace AuthWebApi.Migrations.MysqlDb
{
    [DbContext(typeof(MysqlDbContext))]
    partial class MysqlDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.3-rtm-10026");

            modelBuilder.Entity("AuthWebApi.Models.Comments.Comment", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreateDate");

                    b.Property<int>("Likes");

                    b.Property<string>("Message");

                    b.Property<int>("PostId");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("AuthWebApi.Models.Comments.CommentFiles", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CommentId");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.ToTable("Comment_Files");
                });
#pragma warning restore 612, 618
        }
    }
}
