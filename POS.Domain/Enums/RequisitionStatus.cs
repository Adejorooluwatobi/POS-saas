namespace POS.Domain.Enums;

public enum RequisitionStatus
{
    Pending,              // Submitted, awaiting General review
    UnderReview,          // General opened it
    Approved,             // General approved, fulfillment orders created
    PartiallyFulfilled,   // Some orders received and approved
    FullyFulfilled,       // All quantities received and approved
    Rejected,             // General rejected the request
    Cancelled             // StoreManager cancelled before review
}
