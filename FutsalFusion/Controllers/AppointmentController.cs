using System.Globalization;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Constants;
using FutsalFusion.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class AppointmentController : BaseController<AppointmentController>
{
    private readonly IGenericRepository _genericRepository;
    
    public AppointmentController(IGenericRepository genericRepository)
    {
        _genericRepository = genericRepository;
    }

    [HttpPost]
    public IActionResult BookSlot([FromBody]AppointmentModel appointmentModel)
    {
        if (!DateTime.TryParseExact(appointmentModel.AppointedDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedAppointedDate))
        {
            return BadRequest("Invalid appointedDate format. Please provide the date in MM/dd/yyyy format.");
        }

        var parsedTimeSlot = new TimeSpan();
        
        if (double.TryParse(appointmentModel.TimeSlot, out var timeSlotHours))
        {
            parsedTimeSlot = TimeSpan.FromHours(timeSlotHours);
        }

        var court = _genericRepository.GetById<Court>(Guid.Parse(appointmentModel.CourtId));

        var futsal = _genericRepository.GetById<Futsal>(court.FutsalId);
        
        var appointment = new Appointment()
        {
            BookedCourtId = Guid.Parse(appointmentModel.CourtId),
            CreatedBy = UserDetail.UserId,
            CreatedAt = DateTime.Now,
            IsActionComplete = false,
            IsApproved = false,
            AppointedDate = parsedAppointedDate,
            BookedDate = DateTime.Now,
            IsPaid = appointmentModel.Price == "Online",
            TimeSlotStartTime = parsedTimeSlot,
            TimeSlotEndTime = TimeSpan.FromHours((int)parsedTimeSlot.TotalHours + 1),
            TotalPrice = decimal.Parse(appointmentModel.Price),
            BookedUserId = UserDetail.UserId,
            IsActive = true,
            NumberOfHours = 1,
        };

        _genericRepository.Insert(appointment);

        var userId = UserDetail.UserId;
        
        var notification = new Notification()
        {
            CreatedAt = DateTime.Now,
            SenderId = userId,
            CreatedBy = userId,
            ReceiverId = futsal.FutsalOwnerId,
            Title = "Booking Alert",
            Content = "You have received a new booking notification",
            IsActive = true,
            SenderEntity = (int)Roles.Player,
            ReceiverEntity = (int)Roles.Futsal,
        };

        _genericRepository.Insert(notification);
        
        TempData["Success"] = "Your appointment has been successfully booked";

        return Json(new
        {
            data = true
        });
    }
}

public class AppointmentModel
{
    public string CourtId { get; set; }
    public string AppointedDate { get; set; }
    public string TimeSlot { get; set; }
    public string Price { get; set; }
    
    public string Payment { get; set; }
}