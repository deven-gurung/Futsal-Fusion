namespace FutsalFusion.Application.DTOs.Order;

public class CartRequestDto
{
    public OrderHeaderDto Order { get; set; }

    public IEnumerable<CartDto> CartList { get; set; }
}