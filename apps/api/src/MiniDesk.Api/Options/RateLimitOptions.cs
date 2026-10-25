using System.ComponentModel.DataAnnotations;

namespace MiniDesk.Api.Options;

public class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    [Range(1, 1000)] public int AuthPermitLimit { get; set; } = 5;
    [Range(1, 3600)] public int AuthWindowSeconds { get; set; } = 60;

    [Range(1, 10000)] public int GlobalPermitLimit { get; set; } = 100;
    [Range(1, 3600)] public int GlobalWindowSeconds { get; set; } = 60;
}