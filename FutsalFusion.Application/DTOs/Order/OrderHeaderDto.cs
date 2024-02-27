using FutsalFusion.Domain.Entities.Identity;

namespace FutsalFusion.Application.DTOs.Order;

public class OrderHeaderDto
{
    public Guid Id { get; set; }

    public DateTime OrderedDate { get; set; }

    public DateTime ShippedDate { get; set; }

    public Guid UserId { get; set; }

    public double OrderTotal { get; set; }

    public string? OrderStatus { get; set; }

    public string? PaymentStatus { get; set; }

    public string? TrackingNumber { get; set; }

    public string? Carrier { get; set; }

    public DateTime PaymentDate { get; set; }

    public DateTime PaymentDueDate { get; set; }

    public string? SessionId { get; set; }

    public string? PaymentIntentId { get; set; }

    public string Name { get; set; }

    public string PhoneNumber { get; set; }

    public string City { get; set; }

    public string Region { get; set; }
}