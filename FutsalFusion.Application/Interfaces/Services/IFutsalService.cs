using FutsalFusion.Application.DTOs.Futsal;

namespace FutsalFusion.Application.Interfaces.Services;

public interface IFutsalService
{
    List<FutsalResponseDto> Get();

    FutsalDetailResponseDto GetById(Guid futsalId);

    Task Upsert(FutsalRequestDto futsal);
}