using FutsalFusion.Application.DTOs.Order;

namespace FutsalFusion.Application.Interfaces.Services;

public interface IOrderHeaderService
{
    void Update(OrderHeaderDto orderHeader);

    void UpdateStatus(Guid id, string orderStatus, string paymentStatus);

    void UpdatePaymentStatus(Guid id, string sessionId, string paymentIntentId);
}