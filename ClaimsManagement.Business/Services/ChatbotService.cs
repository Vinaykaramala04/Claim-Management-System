using ClaimsManagement.Business.Interfaces.IServices;

namespace ClaimsManagement.Business.Services
{
    public class ChatbotService : IChatbotService
    {
        public async Task<string> ProcessMessageAsync(string message, int userRole, int userId)
        {
            var lowerMessage = message.ToLower();

            // Intent detection and response generation
            if (lowerMessage.Contains("create") && lowerMessage.Contains("claim"))
                return GetClaimCreationHelp();
            
            if (lowerMessage.Contains("status") || lowerMessage.Contains("check"))
                return GetStatusCheckHelp();
            
            if (lowerMessage.Contains("document") || lowerMessage.Contains("upload"))
                return GetDocumentHelp();
            
            if (lowerMessage.Contains("policy") || lowerMessage.Contains("rule") || lowerMessage.Contains("limit"))
                return GetPolicyHelp();
            
            if (lowerMessage.Contains("approval") || lowerMessage.Contains("approve") || lowerMessage.Contains("process"))
                return GetApprovalHelp();

            return GetGeneralHelp(userRole);
        }

        private string GetClaimCreationHelp()
        {
            return @"📝 **Creating a New Claim:**

1. Click 'Create New Claim' button on dashboard
2. Select claim type (Travel, Medical, Office, etc.)
3. Choose expense category
4. Fill in details:
   • Title (brief description)
   • Amount
   • Incident date
   • Description
5. Upload supporting documents
6. Submit for review

The claim will get a unique number and be sent to agents for review.";
        }

        private string GetStatusCheckHelp()
        {
            return @"🔍 **Check Claim Status:**

• Go to 'My Claims' section in the menu
• Find your claim by number or date
• Status meanings:
  - **Draft**: Not submitted yet
  - **Submitted**: Under agent review
  - **Under Review**: Being processed
  - **Approved**: Ready for payment
  - **Paid**: Completed
  - **Rejected**: Needs revision

Click on any claim to see detailed status and comments.";
        }

        private string GetDocumentHelp()
        {
            return @"📎 **Document Requirements:**

**Required for all claims:**
• Original receipts/invoices
• Proof of payment

**Additional by type:**
• **Travel**: Boarding passes, hotel bills, taxi receipts
• **Medical**: Medical reports, prescriptions, bills
• **Office**: Purchase orders, delivery notes

**File formats:** PDF, JPG, PNG, DOC, DOCX
**Max size:** 3MB per file

Upload documents when creating or editing your claim.";
        }

        private string GetPolicyHelp()
        {
            return @"📋 **Claim Policies:**

**General Rules:**
• Submit claims within 30 days of expense
• All receipts must be original or certified copies
• Personal expenses are not reimbursable

**Approval Limits:**
• Up to $500: Agent approval
• $500-$2000: Manager approval
• Above $2000: Admin approval

**Common Categories:**
• Travel, Medical, Office Supplies, Training, Equipment

Need specific policy information? Ask about a particular expense type.";
        }

        private string GetApprovalHelp()
        {
            return @"⚡ **Approval Process:**

**Step 1:** Agent Review (1-2 days)
• Checks documents and policy compliance
• May request additional information

**Step 2:** Manager Approval (2-3 days)
• Reviews amount and business justification
• Approves or rejects with comments

**Step 3:** Payment Processing (3-5 days)
• Admin processes approved claims
• Payment issued to your account

**Total Time:** Usually 5-10 business days

You'll get notifications at each step!";
        }

        private string GetGeneralHelp(int userRole)
        {
            return userRole switch
            {
                2 => @"👨💼 **Agent Help:**

• Review submitted claims in 'All Claims'
• Check documents and policy compliance
• Update status to 'Under Review' or 'Approved'
• Add comments for clarification
• Escalate high-value claims to managers

What specific task do you need help with?",

                3 => @"👩💼 **Manager Help:**

• Review claims in 'Pending Approvals'
• Approve claims within your authority
• Reject with detailed feedback
• Monitor team claim statistics
• Handle escalated cases

What would you like to know about the approval process?",

                4 => @"🔧 **Admin Help:**

• Process payments for approved claims
• Manage users and system settings
• View system-wide analytics
• Handle policy exceptions
• Manage expense categories

What administrative task can I help you with?",

                _ => @"🤖 **I'm here to help!**

I can assist you with:
• **Creating claims** - Step-by-step guidance
• **Checking status** - Understanding claim progress
• **Document requirements** - What files you need
• **Policies** - Rules and limits
• **Approval process** - How claims get approved

Try asking:
• ""How do I create a travel claim?""
• ""What documents do I need?""
• ""Why was my claim rejected?""
• ""How long does approval take?""

What would you like to know?"
            };
        }
    }
}