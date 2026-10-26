using ExamApp.Api.Application.DTOs;

namespace ExamApp.Api.Application.Contracts;

public interface IExamService
{
    Task<ExamResultDto> SubmitAsync(ExamSubmissionDto submission, CancellationToken ct);
}