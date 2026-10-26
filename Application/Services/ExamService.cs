using ExamApp.Api.Application.Contracts;
using ExamApp.Api.Application.DTOs;
using ExamApp.Api.Domain.Entities;
using ExamApp.Api.Domain.Enums;
using ExamApp.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExamApp.Api.Application.Services;

public class ExamService : IExamService
{
    private readonly ExamDbContext _db;
    private readonly ILogger<ExamService> _logger;

    public ExamService(ExamDbContext db, ILogger<ExamService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ExamResultDto> SubmitAsync(
        ExamSubmissionDto submission,
        CancellationToken ct)
    {
        // لو نفس السؤال اتبعت أكتر من مرة نحسب أول إجابة بس
        var answers = submission.Answers
            .GroupBy(a => a.QuestionId)
            .Select(g => g.First())
            .ToList();

        var submittedIds = answers.Select(a => a.QuestionId).ToList();

        var questions = await _db.Questions
            .AsNoTracking()
            .Where(q => submittedIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, ct);

        var attempt = new ExamAttempt();
        var review = new List<ReviewQuestionDto>();
        var gradedCount = 0;
        var correctCount = 0;

        foreach (var submitted in answers)
        {
            if (!questions.TryGetValue(submitted.QuestionId, out var question))
            {
                _logger.LogWarning("Unknown question id {QuestionId}", submitted.QuestionId);
                continue;
            }

            var selectedIndex = NormalizeIndex(submitted.SelectedOptionIndex, question);
            var isGraded = question.Type is QuestionType.Mcq or QuestionType.TrueFalse;

            if (!isGraded)
            {
                review.Add(BuildUngradedReview(question, submitted.FreeText));
                continue;
            }

            gradedCount++;

            var isCorrect = selectedIndex.HasValue
                && question.CorrectOptionIndex.HasValue
                && selectedIndex.Value == question.CorrectOptionIndex.Value;

            if (isCorrect) correctCount++;

            review.Add(BuildGradedReview(question, selectedIndex, isCorrect));

            attempt.Answers.Add(new ExamAnswer
            {
                QuestionId = question.Id,
                IsCorrect = isCorrect,
                SelectedOptionIndex = selectedIndex,
            });
        }

        var percent = gradedCount > 0
            ? (int)Math.Round((double)correctCount / gradedCount * 100)
            : 0;

        attempt.CorrectCount = correctCount;
        attempt.TotalGraded = gradedCount;
        attempt.Percent = percent;
        attempt.FinishedAt = DateTime.UtcNow;

        _db.ExamAttempts.Add(attempt);
        await _db.SaveChangesAsync(ct);

        return new ExamResultDto(
            correctCount,
            gradedCount,
            percent,
            FeedbackFor(percent),
            review);
    }

    private static int? NormalizeIndex(int? index, Question question)
    {
        if (!index.HasValue) return null;
        if (index.Value < 0) return null;
        if (index.Value >= question.Options.Count) return null;
        return index.Value;
    }

    private static ReviewQuestionDto BuildGradedReview(
        Question question,
        int? selectedIndex,
        bool isCorrect)
    {
        var userChoice = selectedIndex.HasValue
            ? question.Options[selectedIndex.Value]
            : "Not answered";

        var correctAnswer = question.CorrectOptionIndex.HasValue
            && question.CorrectOptionIndex.Value >= 0
            && question.CorrectOptionIndex.Value < question.Options.Count
            ? question.Options[question.CorrectOptionIndex.Value]
            : "N/A";

        return new ReviewQuestionDto(
            question.Id,
            question.Text,
            userChoice,
            correctAnswer,
            question.Explanation,
            isCorrect);
    }

    private static ReviewQuestionDto BuildUngradedReview(
        Question question,
        string? freeText)
    {
        return new ReviewQuestionDto(
            question.Id,
            question.Text,
            string.IsNullOrWhiteSpace(freeText) ? "Not answered" : freeText,
            question.ModelAnswer,
            question.Rubric,
            false);
    }

    private static string FeedbackFor(int percent) => percent switch
    {
        >= 85 => "Excellent.",
        >= 65 => "Good. Review your mistakes.",
        _ => "Needs more revision.",
    };
}