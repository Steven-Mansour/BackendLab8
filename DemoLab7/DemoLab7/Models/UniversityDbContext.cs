using System;
using System.Collections.Generic;
using DemoLab7.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace DemoLab7.Models;

public partial class UniversityDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;
    public UniversityDbContext(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public UniversityDbContext(DbContextOptions<UniversityDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=universityDb7;Username=steven;Password=0000");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var schema = _tenantProvider.GetTenantSchema();
        modelBuilder.HasDefaultSchema(schema);
        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_Classes_CourseId");

            entity.HasIndex(e => e.TeacherId, "IX_Classes_TeacherId");

            entity.HasOne(d => d.Course).WithMany(p => p.Classes).HasForeignKey(d => d.CourseId);

            entity.HasOne(d => d.Teacher).WithMany(p => p.Classes).HasForeignKey(d => d.TeacherId);

            entity.HasMany(d => d.Students).WithMany(p => p.Classes)
                .UsingEntity<Dictionary<string, object>>(
                    "ClassesStudent",
                    r => r.HasOne<Student>().WithMany().HasForeignKey("StudentsStudentId"),
                    l => l.HasOne<Class>().WithMany().HasForeignKey("ClassesClassId"),
                    j =>
                    {
                        j.HasKey("ClassesClassId", "StudentsStudentId");
                        j.ToTable("ClassesStudents");
                        j.HasIndex(new[] { "StudentsStudentId" }, "IX_ClassesStudents_StudentsStudentId");
                    });
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.Property(e => e.CourseName).HasMaxLength(200);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
