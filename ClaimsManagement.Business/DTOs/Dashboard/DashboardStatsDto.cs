namespace ClaimsManagement.Business.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int SubmittedClaims { get; set; }
        public int UnderReviewClaims { get; set; }
        public int DraftClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public int PaidClaims { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public int PendingApprovals { get; set; }
        public int OverdueClaims { get; set; }
        public int PendingPayments { get; set; }
        public int ApprovedToday { get; set; }
    }
}