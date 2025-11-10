using System;

namespace RagApi.Models;

/// <summary>
/// Represents the result of a job matching analysis
/// </summary>
public class JobMatchResult
{
    /// <summary>
    /// Match score between 0-100
    /// </summary>
    public decimal Score { get; set; }

    /// <summary>
    /// Detailed analysis of the match
    /// </summary>
    public string Analysis { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this is a high match (score >= 75)
    /// </summary>
    public bool IsHighMatch => Score >= 75;

    /// <summary>
    /// Indicates whether this is a medium match (score between 50 and 75)
    /// </summary>
    public bool IsMediumMatch => Score >= 50 && Score < 75;

    /// <summary>
    /// Indicates whether this is a low match (score < 50)
    /// </summary>
    public bool IsLowMatch => Score < 50;
}
