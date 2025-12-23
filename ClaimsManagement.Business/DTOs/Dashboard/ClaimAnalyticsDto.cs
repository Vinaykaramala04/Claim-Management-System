namespace ClaimsManagement.Business.DTOs.Dashboard
{
    public class ClaimAnalyticsDto
    {
        public Dictionary<string, int> ClaimsByStatus { get; set; } = new();
        public Dictionary<string, int> ClaimsByType { get; set; } = new();
        public Dictionary<string, decimal> AmountByCategory { get; set; } = new();
        public Dictionary<string, int> ClaimsByMonth { get; set; } = new();
        public List<TopClaimant> TopClaimants { get; set; } = new();
        public double AverageProcessingTime { get; set; }
    }

    public class TopClaimant
    {
        public string UserName { get; set; } = string.Empty;
        public int ClaimCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}