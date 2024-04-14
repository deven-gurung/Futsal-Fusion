using FutsalFusion.Application.DTOs.Futsal;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class FutsalOwnerController : BaseController<FutsalOwnerController>
{
    private readonly IGenericRepository _genericRepository;

    public FutsalOwnerController(IGenericRepository genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public IActionResult Index()
    {
        var result = (from futsal in _genericRepository.Get<Futsal>()
                    let user = _genericRepository.GetById<AppUser>(futsal.FutsalOwnerId)
                    let courts = _genericRepository.Get<Court>(x => x.FutsalId == futsal.Id)
                    let appointments = _genericRepository.Get<Appointment>(x => courts.Select<Court, Guid>(z => z.Id).Contains(x.BookedCourtId))
                    select new FutsalOwnerResponseDto()
                    {
                        Name = user.FullName,
                        ImageURL = user.ImageURL,
                        Email = user.EmailAddress,
                        FutsalName = futsal.Name,
                        FutsalLocation = $"{futsal.LocationAddress}, {futsal.City}",
                        TotalAppointments = appointments.Count<Appointment>(),
                        IsActive = user.IsActive,
                        PhoneNumber = user.MobileNo
                    }).ToList();

        return View(result);
    }
}