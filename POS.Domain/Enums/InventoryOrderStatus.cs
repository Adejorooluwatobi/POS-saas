namespace POS.Domain.Enums;

public enum InventoryOrderStatus
{
    Draft,        // Created but not yet dispatched
    Dispatched,   // Sent by origin (HQ or Store A)
    Received,     // Store Manager marked as received (quantities entered)
    Approved,     // Store Manager confirmed all quantities match
    Disputed,     // Store Manager flagged shortage/damage
    Resolved      // HQ/Source store accepted the dispute resolution
}
