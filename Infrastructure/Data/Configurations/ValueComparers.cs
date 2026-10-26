using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace ExamApp.Api.Infrastructure.Data.Configurations;

public static class ValueComparers
{
    public static readonly ValueComparer<List<string>> StringListComparer = new(
        (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null)
                == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(),
        v => JsonSerializer.Deserialize<List<string>>(
                JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                (JsonSerializerOptions?)null) ?? new List<string>());
}