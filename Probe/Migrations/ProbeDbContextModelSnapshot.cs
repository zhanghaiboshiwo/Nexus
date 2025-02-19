﻿// <auto-generated />
using Aiursoft.Probe.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace Aiursoft.Probe.Migrations
{
    [DbContext(typeof(ProbeDbContext))]
    partial class ProbeDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Aiursoft.Pylon.Models.Probe.File", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ContextId")
                        .HasColumnType("int");

                    b.Property<string>("FileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UploadTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("ContextId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("Aiursoft.Pylon.Models.Probe.Folder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("ContextId")
                        .HasColumnType("int");

                    b.Property<string>("FolderName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ContextId");

                    b.ToTable("Folders");
                });

            modelBuilder.Entity("Aiursoft.Pylon.Models.Probe.ProbeApp", b =>
                {
                    b.Property<string>("AppId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("AppId");

                    b.ToTable("Apps");
                });

            modelBuilder.Entity("Aiursoft.Pylon.Models.Probe.Site", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AppId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("FolderId")
                        .HasColumnType("int");

                    b.Property<bool>("OpenToDownload")
                        .HasColumnType("bit");

                    b.Property<bool>("OpenToUpload")
                        .HasColumnType("bit");

                    b.Property<string>("SiteName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AppId");

                    b.HasIndex("FolderId");

                    b.ToTable("Sites");
                });

            modelBuilder.Entity("Aiursoft.Pylon.Models.Probe.File", b =>
                {
                    b.HasOne("Aiursoft.Pylon.Models.Probe.Folder", "Context")
                        .WithMany("Files")
                        .HasForeignKey("ContextId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Aiursoft.Pylon.Models.Probe.Folder", b =>
                {
                    b.HasOne("Aiursoft.Pylon.Models.Probe.Folder", "Context")
                        .WithMany("SubFolders")
                        .HasForeignKey("ContextId");
                });

            modelBuilder.Entity("Aiursoft.Pylon.Models.Probe.Site", b =>
                {
                    b.HasOne("Aiursoft.Pylon.Models.Probe.ProbeApp", "Context")
                        .WithMany("Sites")
                        .HasForeignKey("AppId");

                    b.HasOne("Aiursoft.Pylon.Models.Probe.Folder", "Root")
                        .WithMany()
                        .HasForeignKey("FolderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
