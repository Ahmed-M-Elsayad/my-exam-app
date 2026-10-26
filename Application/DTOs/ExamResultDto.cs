namespace ExamApp.Api.Application.DTOs;

public record ExamResultDto(
    int CorrectCount,
    int TotalGraded,
    int Percent,
    string Feedback,
    IReadOnlyList<ReviewQuestionDto> Review);