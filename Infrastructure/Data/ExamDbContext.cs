using System.Text.Json;
using ExamApp.Api.Domain.Entities;
using ExamApp.Api.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ExamApp.Api.Infrastructure.Data;

public class ExamDbContext : DbContext
{
    public ExamDbContext(DbContextOptions<ExamDbContext> options) : base(options)
    {
    }

    public DbSet<Question> Questions => Set<Question>();
    public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();
    public DbSet<ExamAnswer> ExamAnswers => Set<ExamAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Category).HasMaxLength(64).IsRequired();
            entity.Property(q => q.Text).IsRequired();
            entity.Property(q => q.Type).HasConversion<int>();

            // Options بتتخزن كـ JSON string في الداتابيز
            entity.Property(q => q.Options)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                .HasColumnType("TEXT");

            // Value Comparer لمنع تحذير EF Core
            entity.Property(q => q.Options).Metadata
                .SetValueComparer(ValueComparers.StringListComparer);

            entity.HasIndex(q => q.Category);
            entity.HasIndex(q => q.Type);
        });

        modelBuilder.Entity<ExamAttempt>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.HasMany(a => a.Answers)
                .WithOne(ans => ans.Attempt)
                .HasForeignKey(ans => ans.ExamAttemptId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExamAnswer>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}