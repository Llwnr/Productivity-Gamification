using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Gamification.Infrastructure.Models;

namespace Gamification.Infrastructure.DatabaseService;

public class ProductivityDbContext(DbContextOptions<ProductivityDbContext> options) : DbContext(options){
    public DbSet<Sites> Sites{ get; set; }

    //Defines the schema constraints, namings etc
    protected override void OnModelCreating(ModelBuilder modelBuilder){
        SetupSitesTable(modelBuilder);
        // SetupAnalysisResultsTable(modelBuilder);
    }

    void SetupSitesTable(ModelBuilder modelBuilder){
        var entity = modelBuilder.Entity<Sites>();

        entity.ToTable("sites");

        entity.HasKey(p => p.SiteId);
        
        entity.Property(p => p.SiteId).HasColumnName("site_id");
        entity.Property(p => p.Url).HasColumnName("url");
        entity.Property(p => p.Title).HasColumnName("title");
        entity.Property(p => p.Description).HasColumnName("description");
    }
}
