using FutsalFusion.Application.DTOs.Appointment;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Constants;
using FutsalFusion.Domain.Entities;
using FutsalFusion.Domain.Entities.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class AppointmentsController : BaseController<AppointmentsController>
{
    private readonly IGenericRepository _genericRepository;

    public AppointmentsController(IGenericRepository genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public IActionResult Index()
    {
        var userId = UserDetail.UserId;

        var user = _genericRepository.GetById<AppUser>(userId);

        var futsal = _genericRepository.GetFirstOrDefault<Futsal>(x => x.FutsalOwnerId == user.Id);

        var courts = _genericRepository.Get<Court>(x => x.FutsalId == futsal.Id);

        var appointments = _genericRepository.Get<Appointment>(x => courts.Select(z => z.Id).Contains(x.BookedCourtId));

        var result = appointments.Select(x => new AppointmentsResponseDto()
        {
            AppointmentId = x.Id,
            IsActive = x.IsActive,
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

        return View(result);
    }

    [HttpPost]
    public IActionResult BookingAction([FromBody] AppointmentApprovalModel appointmentModel)
    {
        if (string.IsNullOrEmpty(appointmentModel.AppointmentId) || string.IsNullOrEmpty(appointmentModel.IsApproved))
        {
            return Json(new
            {
                success = 0
            });
        }
        
        var appointment = _genericRepository.GetById<Appointment>(Guid.Parse(appointmentModel.AppointmentId));

        appointment.IsActionComplete = true;
        appointment.IsApproved = appointmentModel.IsApproved == "Approved";
        
        appointment.LastModifiedAt = DateTime.Now;
        appointment.LastModifiedBy = UserDetail.UserId;
        
        _genericRepository.Update(appointment);

        var notification = new Notification()
        {
            CreatedAt = DateTime.Now,
            SenderId = UserDetail.UserId,
            CreatedBy = UserDetail.UserId,
            ReceiverId = appointment.BookedUserId,
            Title = "Booking Action Alert",
            Content = "Your booking slot has received a new output.",
            IsActive = true,
            SenderEntity = (int)Roles.Player,
            ReceiverEntity = (int)Roles.Futsal,
        };

        _genericRepository.Insert(notification);
        
        var court = _genericRepository.GetById<Court>(appointment.BookedCourtId);
        
        var futsal = _genericRepository.GetById<Futsal>(court.FutsalId);

        var courts = _genericRepository.Get<Court>(x => x.FutsalId == futsal.Id);

        var appointments = _genericRepository.Get<Appointment>(x => courts.Select(z => z.Id).Contains(x.BookedCourtId));

        var result = appointments.Select(x => new AppointmentsResponseDto()
        {
            AppointmentId = x.Id,
            CourtId = x.BookedCourtId,
            IsActive = x.IsActive,
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
            success = 1,
            htmlData = htmlData
        });
    }
}

public class AppointmentApprovalModel
{
    public string AppointmentId { get; set; }
    
    public  string IsApproved { get; set; }
}