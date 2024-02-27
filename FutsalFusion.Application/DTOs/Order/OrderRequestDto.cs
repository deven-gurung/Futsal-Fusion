namespace FutsalFusion.Application.DTOs.Order;

public class OrderRequestDto
{
    public OrderHeaderDto Order { get; set; }

    public IEnumerable<OrderDetailDto> OrderDetails { get; set; }
}