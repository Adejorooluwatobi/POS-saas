namespace POS.Application.DTOs;

public class UpdateGiftCardDto
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public DateOnly? ExpiresAt { get; set; }
}
