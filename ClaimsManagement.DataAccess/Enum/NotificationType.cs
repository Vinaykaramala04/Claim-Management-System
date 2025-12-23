namespace ClaimsManagement.DataAccess.Enum
{
    public enum NotificationType
    {
        ClaimSubmitted = 1,
        ClaimApproved = 2,
        ClaimRejected = 3,
        ClaimPaid = 4,
        ApprovalRequired = 5,
        SLABreach = 6,
        DocumentRequired = 7,
        ClaimUpdate = 8
    }
}