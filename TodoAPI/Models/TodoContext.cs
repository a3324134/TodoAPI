using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace TodoAPI.Models
{
    public partial class TodoContext : DbContext
    {
        public TodoContext()
        {
        }

        public TodoContext(DbContextOptions<TodoContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Division> Divisions { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<JobTitle> JobTitles { get; set; }
        public virtual DbSet<TodoList> TodoLists { get; set; }
        public virtual DbSet<UploadFile> UploadFiles { get; set; }
        public virtual DbSet<Role> Roles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Chinese_Taiwan_Stroke_CI_AS");

            modelBuilder.Entity<Division>(entity =>
            {
                entity.ToTable("Division");

                entity.Property(e => e.DivisionId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employee");

                entity.Property(e => e.EmployeeId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Account).IsRequired();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Password).IsRequired();

                entity.HasOne(d => d.Division)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.DivisionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Employee_Division");

                entity.HasOne(d => d.JobTitle)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.JobTitleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Employee_JobTitle");
            });

            modelBuilder.Entity<JobTitle>(entity =>
            {
                entity.ToTable("JobTitle");

                entity.Property(e => e.JobTitleId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TodoList>(entity =>
            {
                entity.HasKey(e => e.TodoId)
                    .HasName("PK__TodoList__958625520C33097B");

                entity.ToTable("TodoList");

                entity.Property(e => e.TodoId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.InsertTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.UpdateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.StartTime)
                    .HasColumnType("date");

                entity.Property(e => e.EndTime)
                    .HasColumnType("date");


                entity.HasOne(d => d.InsertEmployee)
                    .WithMany(p => p.TodoListInsertEmployees)
                    .HasForeignKey(d => d.InsertEmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Todo_InsertIdOfEmployee");

                entity.HasOne(d => d.UpdateEmployee)
                    .WithMany(p => p.TodoListUpdateEmployees)
                    .HasForeignKey(d => d.UpdateEmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Todo_UpdateIdOfEmployee");
            });

            modelBuilder.Entity<UploadFile>(entity =>
            {
                entity.ToTable("UploadFile");

                entity.Property(e => e.UploadFileId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.Src).IsRequired();

                entity.HasOne(d => d.Todo)
                    .WithMany(p => p.UploadFiles)
                    .HasForeignKey(d => d.TodoId)
                    //.OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_File_ToTable");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.RoleId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.EmployeeId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
