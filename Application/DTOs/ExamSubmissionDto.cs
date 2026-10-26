using System.ComponentModel.DataAnnotations;

namespace ExamApp.Api.Application.DTOs;

public record ExamSubmissionDto(
    [Required]
    [MinLength(1, ErrorMessage = "Submission must contain at least one answer.")]
    [MaxLength(100, ErrorMessage = "Submission cannot exceed 100 answers.")]
    IReadOnlyList<ExamAnswerDto> Answers);

public record ExamAnswerDto(
    [Range(1, int.MaxValue, ErrorMessage = "Invalid question id.")]
    int QuestionId,

    [Range(0, 100, ErrorMessage = "Option index out of range.")]
    int? SelectedOptionIndex,

    [MaxLength(5000, ErrorMessage = "Free text too long.")]
    string? FreeText);