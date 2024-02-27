namespace FutsalFusion.Application.DTOs.Order;

public class OrderDetailDto
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid ItemId { get; set; }

    public int Count { get; set; }

    public double Price { get; set; }
}