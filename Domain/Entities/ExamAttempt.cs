namespace ExamApp.Api.Domain.Entities;

public class ExamAttempt
{
    public int Id { get; set; }
    public DateTime FinishedAt { get; set; } = DateTime.UtcNow;
    public int CorrectCount { get; set; }
    public int TotalGraded { get; set; }
    public int Percent { get; set; }

    public ICollection<ExamAnswer> Answers { get; set; } = new List<ExamAnswer>();
}

public class ExamAnswer
{
    public int Id { get; set; }
    public int ExamAttemptId { get; set; }
    public int QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public int? SelectedOptionIndex { get; set; }
    public string? FreeText { get; set; }

    // Navigation properties
    public ExamAttempt Attempt { get; set; } = null!;
    public Question Question { get; set; } = null!;
}