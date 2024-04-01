using FutsalFusion.Application.DTOs.Appointment;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class ReservationController : BaseController<ReservationController>
{
    private readonly IGenericRepository _genericRepository;

    public ReservationController(IGenericRepository genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public IActionResult Index()
    {
        var userId = UserDetail.UserId;

        var user = _genericRepository.GetById<AppUser>(userId);

        var appointments = _genericRepository.Get<Appointment>(x => x.BookedUserId == user.Id);

        var result = appointments.Select(x => new AppointmentsResponseDto()
        {
            AppointmentId = x.Id,
            CourtId = x.BookedCourtId,
            CourtName = _genericRepository.GetById<Court>(x.BookedCourtId).Title,
            TotalPrice = x.TotalPrice.ToString("N2"),
            IsApproved = x.IsApproved,
            ActionDoneUser = x.LastModifiedBy != null ? _genericRepository.GetById<AppUser>(x.LastModifiedBy).FullName : "",
            ActionDoneDate = x.LastModifiedAt != null ? x.LastModifiedAt?.ToString("dd-MM-yyyy") : "",
            IsActionCompleted = x.IsActionComplete,
            TimeSlot = $"{x.TimeSlotStartTime} - {x.TimeSlotEndTime}",
            AppointedDate = x.AppointedDate.ToString("dd-MM-yyyy"),
            BookedDate = x.BookedDate.ToString("dd-MM-yyyy"),
            PaymentStatus = x.IsPaid ? "Paid" : "Not Paid"
        }).ToList();

        return View(result);
    }

    [HttpPost]
    public IActionResult BookingAction(string appointmentId, bool isApproved)
    {
        var appointment = _genericRepository.GetById<Appointment>(Guid.Parse(appointmentId));

        appointment.IsActionComplete = true;
        appointment.IsApproved = isApproved;
        
        appointment.LastModifiedAt = DateTime.Now;
        appointment.LastModifiedBy = UserDetail.UserId;
        
        _genericRepository.Update(appointment);

        var court = _genericRepository.GetById<Court>(appointment.BookedCourtId);
        
        var futsal = _genericRepository.GetById<Futsal>(court.FutsalId);

        var courts = _genericRepository.Get<Court>(x => x.FutsalId == futsal.Id);

        var appointments = _genericRepository.Get<Appointment>(x => courts.Select(z => z.Id).Contains(x.BookedCourtId));

        var result = appointments.Select(x => new AppointmentsResponseDto()
        {
            AppointmentId = x.Id,
            CourtId = x.BookedCourtId,
            CourtName = _genericRepository.GetById<Court>(x.BookedCourtId).Title,
            TotalPrice = x.TotalPrice.ToString("N2"),
            IsApproved = x.IsApproved,
            AppointedUser = _genericRepository.GetById<AppUser>(x.BookedUserId).FullName,
            IsActionCompleted = x.IsActionComplete,
            TimeSlot = $"{x.TimeSlotStartTime} - {x.TimeSlotEndTime}",
            AppointedDate = x.AppointedDate.ToString("dd-MM-yyyy"),
            BookedDate = x.BookedDate.ToString("dd-MM-yyyy"),
            PaymentStatus = x.IsPaid ? "Paid" : "Not Paid"
        }).ToList();

        var htmlData = ConvertViewToString("_AppointmentsList", result, true);

        return Json(new
        {
            htmlData = htmlData
        });
    }
}