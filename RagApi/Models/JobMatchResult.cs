using System;

namespace RagApi.Models
{
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
        public string Analysis { get; set; }
    }
}
