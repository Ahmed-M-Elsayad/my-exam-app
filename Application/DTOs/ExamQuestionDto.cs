namespace ExamApp.Api.Application.DTOs;

/// <summary>
/// السؤال كما يراه الطالب أثناء الامتحان.
/// لا يحتوي على الإجابة الصحيحة أو الشرح.
/// </summary>
public record ExamQuestionDto(
    int Id,
    string Category,
    string Type,
    string Question,
    IReadOnlyList<string>? Options);