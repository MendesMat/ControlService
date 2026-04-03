namespace ControlService.Application.Commercial.Customers.DTOs;

public class CustomerResponseDto
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string LegalName { get; set; }
    public string? TradeName { get; set; }
    public string? Document { get; set; }
    public string? DocumentType { get; set; }
    public string? Status { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
}
