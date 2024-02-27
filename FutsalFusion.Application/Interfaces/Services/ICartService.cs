using FutsalFusion.Application.DTOs.Order;

namespace FutsalFusion.Application.Interfaces.Services;

public interface ICartService
{
    int IncrementCount(CartDto cart, int count);

    int DecrementCount(CartDto cart, int count);
}