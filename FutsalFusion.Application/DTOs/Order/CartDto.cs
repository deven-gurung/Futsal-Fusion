using System.ComponentModel.DataAnnotations.Schema;

namespace FutsalFusion.Application.DTOs.Order;

public class CartDto
{
    public Guid Id { get; set; }

    public Guid ItemId { get; set; }

    public Guid UserId { get; set; }

    public int Count { get; set; }

    public double Price { get; set; }
}