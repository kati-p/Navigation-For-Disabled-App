using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NavigateForDisabledApp.Models;

public partial class NavigateSoftwareDbContext : DbContext
{
    public NavigateSoftwareDbContext()
    {
    }

    public NavigateSoftwareDbContext(DbContextOptions<NavigateSoftwareDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bus> Buses { get; set; }

    public virtual DbSet<Station> Stations { get; set; }

    public virtual DbSet<StationBu> StationBus { get; set; }

    public virtual DbSet<StationGraph> StationGraphs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;user=root;password=;database=NavigateSoftware", Microsoft.EntityFrameworkCore.ServerVersion.Parse("11.1.2-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb3_thai_520_w2")
            .HasCharSet("utf8mb3");

        modelBuilder.Entity<Bus>(entity =>
        {
            entity.HasKey(e => e.BusId).HasName("PRIMARY");

            entity.ToTable("Bus");

            entity.Property(e => e.BusId)
                .ValueGeneratedNever()
                .HasColumnType("int(11) unsigned")
                .HasColumnName("Bus_ID");
            entity.Property(e => e.BusName)
                .HasMaxLength(50)
                .HasColumnName("Bus_Name");
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.StationId).HasName("PRIMARY");

            entity.ToTable("Station");

            entity.Property(e => e.StationId)
                .ValueGeneratedNever()
                .HasColumnType("int(11) unsigned")
                .HasColumnName("Station_ID");
            entity.Property(e => e.StationName)
                .HasMaxLength(50)
                .HasColumnName("Station_Name");
        });

        modelBuilder.Entity<StationBu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.BusId, "Bus_ID");

            entity.HasIndex(e => e.StationId, "Station_ID");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11) unsigned")
                .HasColumnName("ID");
            entity.Property(e => e.BusId)
                .HasColumnType("int(11) unsigned")
                .HasColumnName("Bus_ID");
            entity.Property(e => e.StationId)
                .HasColumnType("int(11) unsigned")
                .HasColumnName("Station_ID");

            entity.HasOne(d => d.Bus).WithMany(p => p.StationBus)
                .HasForeignKey(d => d.BusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Bus_ID");

            entity.HasOne(d => d.Station).WithMany(p => p.StationBus)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Station_ID");
        });

        modelBuilder.Entity<StationGraph>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("StationGraph");

            entity.HasIndex(e => e.StationId, "FK_StationGraph_Station");

            entity.HasIndex(e => e.NearbyStationId, "FK_StationGraph_Station_2");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11) unsigned")
                .HasColumnName("ID");
            entity.Property(e => e.NearbyStationId)
                .HasColumnType("int(11) unsigned")
                .HasColumnName("Nearby_Station_ID");
            entity.Property(e => e.StationId)
                .HasColumnType("int(11) unsigned")
                .HasColumnName("Station_ID");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("User");

            entity.Property(e => e.Id).HasColumnType("int(11) unsigned");
            entity.Property(e => e.Password).HasMaxLength(44);
            entity.Property(e => e.Salt).HasMaxLength(24);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
