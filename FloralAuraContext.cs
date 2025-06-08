using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using FlowerAura.Models;


namespace FlowerAura.Models;

public partial class FloralAuraContext : DbContext
{
    public FloralAuraContext()
    {
    }

    public FloralAuraContext(DbContextOptions<FloralAuraContext> options)
        : base(options)
    {
    }

  
    public virtual DbSet<Flowers> Flowers { get; set; }
    
    

   
    public virtual DbSet<Feedback> Feedbacks { get; set; }
    public virtual DbSet<Registration> Registrations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=FloralAura;Trusted_Connection=True;MultipleActiveResultSets=true");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.UserId);  

            entity.ToTable("Feedback");

            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(f => f.Registration)
                  .WithOne(r => r.Feedback)  
                  .HasForeignKey<Feedback>(f => f.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("Registration");

            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsFixedLength();
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Mobilenumber)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsFixedLength();

            entity.HasOne(r => r.Feedback)
                  .WithOne(f => f.Registration)
                  .HasForeignKey<Feedback>(f => f.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

public DbSet<FlowerAura.Models.FlowerViewModel> FlowerViewModel { get; set; } = default!;

}