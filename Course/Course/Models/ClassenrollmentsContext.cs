using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Course.Models;

public partial class ClassenrollmentsContext : DbContext
{
    public ClassenrollmentsContext()
    {
    }

    public ClassenrollmentsContext(DbContextOptions<ClassenrollmentsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=classenrollments;Username=steven;Password=0000");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Classid).HasName("classes_pk");

            entity.Property(e => e.Classid).HasDefaultValueSql("nextval('classes_classid_seq'::regclass)");

            entity.HasOne(d => d.Course).WithMany(p => p.Classes)
                .HasForeignKey(d => d.Courseid)
                .HasConstraintName("courseid");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.Property(e => e.CourseName).HasMaxLength(200);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Enrollmentid).HasName("enrollments_pk");

            entity.Property(e => e.Enrollmentid).HasDefaultValueSql("nextval('enrollments_enrollmentid_seq'::regclass)");

            entity.HasOne(d => d.Class).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.Classid)
                .HasConstraintName("classid");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
