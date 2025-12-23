using ClaimsManagement.DataAccess.Enum;
using ClaimsManagement.Business.Helpers;

namespace ClaimsManagement.Business.Helpers
{
    public static class ClaimHelper
    {
        public static string GenerateClaimNumber()
        {
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month;
            var timestamp = DateTime.UtcNow.ToString("HHmmss");
            var random = new Random().Next(1000, 9999);
            
            return $"CLM{year}{month:D2}{timestamp}{random}";
        }

        public static DateTime CalculateSLADate(Priority priority, DateTime submissionDate)
        {
            var businessDays = priority switch
            {
                Priority.Critical => 1,
                Priority.High => 3,
                Priority.Medium => 5,
                Priority.Low => 10,
                _ => 5
            };

            return DateTimeHelper.AddBusinessDays(submissionDate, businessDays);
        }

        public static bool IsSLABreached(DateTime slaDate)
        {
            return DateTime.UtcNow > slaDate;
        }

        public static string GetPriorityDisplayName(Priority priority)
        {
            return priority switch
            {
                Priority.Critical => "Critical",
                Priority.High => "High",
                Priority.Medium => "Medium",
                Priority.Low => "Low",
                _ => "Unknown"
            };
        }

        public static string GetStatusDisplayName(ClaimStatus status)
        {
            return status switch
            {
                ClaimStatus.Draft => "Draft",
                ClaimStatus.Submitted => "Submitted",
                ClaimStatus.UnderReview => "Under Review",
                ClaimStatus.Approved => "Approved",
                ClaimStatus.Rejected => "Rejected",
                ClaimStatus.Paid => "Paid",
                ClaimStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }

        public static bool CanEditClaim(ClaimStatus status)
        {
            return status == ClaimStatus.Draft;
        }

        public static bool CanCancelClaim(ClaimStatus status)
        {
            return status == ClaimStatus.Draft || status == ClaimStatus.Submitted;
        }

        public static bool RequiresApproval(decimal amount, decimal? categoryLimit)
        {
            if (!categoryLimit.HasValue)
                return true;

            return amount > categoryLimit.Value;
        }

        public static int GetApprovalLevel(decimal amount)
        {
            return amount switch
            {
                <= 1000 => 1,
                <= 5000 => 2,
                <= 10000 => 3,
                _ => 4
            };
        }
    }
}