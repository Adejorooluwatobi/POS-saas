namespace POS.Application.DTOs;

public class CreateTillSessionDto
{
    public Guid TerminalId { get; set; }
    public decimal OpeningFloat { get; set; }
}
