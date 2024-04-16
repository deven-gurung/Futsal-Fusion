using FutsalFusion.Application.DTOs.Appointment;
using FutsalFusion.Application.DTOs.Reservation;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Constants;
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
            PaymentStatus = x.IsPaid ? "Paid" : "Not Paid",
            IsActive = x.IsActive
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
            PaymentStatus = x.IsPaid ? "Paid" : "Not Paid",
            IsActive = x.IsActive,
        }).ToList();

        var htmlData = ConvertViewToString("_AppointmentsList", result, true);

        return Json(new
        {
            htmlData = htmlData
        });
    }

    [HttpPost]
    public IActionResult CancelReservation(Guid appointmentId)
    {
        var appointment = _genericRepository.GetById<Appointment>(appointmentId);

        var appointmentDetails = _genericRepository.Get<AppointmentDetail>(x => x.AppointmentId == appointment.Id);

        appointment.IsActive = false;
        appointment.LastModifiedAt = DateTime.Now;
        
        _genericRepository.Update(appointment);
        
        var appointmentDetailsEntityList = appointmentDetails as AppointmentDetail[] ?? appointmentDetails.ToArray();
        
        if (appointmentDetailsEntityList.Any())
        {
            _genericRepository.RemoveMultipleEntity(appointmentDetailsEntityList);
        }
        
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
            PaymentStatus = x.IsPaid ? "Paid" : "Not Paid",
            ActionDoneDate = x.LastModifiedAt?.ToString("dd-MM-yyyy"),
            IsActive = x.IsActive
        }).ToList();

        var htmlData = ConvertViewToString("_AppointmentsList", result, true);

        return Json(new
        {
            htmlData = htmlData
        });
    }

    [HttpGet]
    public IActionResult ApprovedAppointmentsDetails(Guid appointmentId)
    {
        var appointment = _genericRepository.GetById<Appointment>(appointmentId);
        
        var user = _genericRepository.GetById<AppUser>(appointment.BookedUserId);

        var court = _genericRepository.GetById<Court>(appointment.BookedCourtId);

        var futsal = _genericRepository.GetById<Futsal>(court.FutsalId);
        
        var appointmentDetails = _genericRepository.Get<AppointmentDetail>(x => x.AppointmentId == appointment.Id);

        var requestedPlayersDetails = 
            _genericRepository.Get<AppointmentDetail>(x => 
                x.AppointmentId == appointment.Id && 
                x.PlayerStatus == "Requested Player").Select(x => new AssignedPlayers()
            {
                PlayerId = x.PlayerId,
                PlayerName = _genericRepository.GetById<AppUser>(x.PlayerId).FullName,
                ImageUrl = _genericRepository.GetById<AppUser>(x.PlayerId).ImageURL ?? "sample-profile.png",
                PhoneNumber = _genericRepository.GetById<AppUser>(x.PlayerId).MobileNo ?? "",
                Status = x.PlayerStatus,
                IsActive = x.IsActive,
            }).ToList();
        
        var teamMembers = _genericRepository.Get<Team>(x => x.PlayerId == user.Id || x.AssigneeId == user.Id);
        
        var uniqueUserIds = teamMembers
            .SelectMany(x => new List<Guid> { x.PlayerId, x.AssigneeId }) 
            .Distinct()
            .Where(x => !requestedPlayersDetails.Select(z => z.PlayerId).Contains(x))
            .ToList();

        var isUserIncluded = uniqueUserIds.Where(x => x == user.Id);

        if (!isUserIncluded.Any())
        {
            uniqueUserIds.Add(user.Id);
        }
        
        uniqueUserIds.AddRange(requestedPlayersDetails.Where(x => x.IsActive).Select(x => x.PlayerId));
        
        var assignedPlayers = uniqueUserIds.Select(x => new AssignedPlayers()
        {
            PlayerId = x,
            PhoneNumber = _genericRepository.GetById<AppUser>(x).MobileNo ?? "",
            PlayerName = _genericRepository.GetById<AppUser>(x).FullName,
            IsSelected = user.Id == x || appointmentDetails.Any(z => z.PlayerId == x),
            Status = _genericRepository.GetFirstOrDefault<AppointmentDetail>(z => z.PlayerId == x && z.AppointmentId == appointment.Id)?.PlayerStatus ?? "Self"
        }).ToList();

        var requestedPlayers = _genericRepository.GetFirstOrDefault<AppointmentRequest>(x => x.AppointmentId == appointment.Id && x.IsActive);

        var result = new ReservedAppointmentsDto()
        {
            PlayerId = user.Id,
            CourtId = court.Id,
            PlayerName = user.FullName,
            AppointedDate = appointment.AppointedDate.ToString("dd-MM-yyyy"),
            BookedDate = appointment.BookedDate.ToString("dd-MM-yyyy"),
            CourtName = court.Title,
            CourtType = court.Type == 1 ? "Indoor" : "Outdoor",
            FutsalId = futsal.Id,
            FutsalName = futsal.Name,
            TimeSlot = $"{appointment.TimeSlotStartTime} - {appointment.TimeSlotEndTime}",
            AppointmentId = appointment.Id,
            AssignedPlayersList = assignedPlayers,
            NumberOfPlayers = requestedPlayers?.RequestedPlayers ?? 0,
            IsEditable = requestedPlayersDetails.Count == 0 || requestedPlayersDetails.Any(x => x.PlayerId != UserDetail.UserId),
            IsAssignedPlayersRequired = requestedPlayers != null,
            RequestedPlayers = requestedPlayersDetails.Where(x => !x.IsActive).ToList()
        };

        return View(result);
    }

    [HttpGet]
    public IActionResult RequestAction(Guid appointmentId, Guid playerId, bool isApproved)
    {
        var appointment = _genericRepository.GetById<Appointment>(appointmentId);

        var player = _genericRepository.GetById<AppUser>(playerId);

        var appointmentDetail = _genericRepository.GetFirstOrDefault<AppointmentDetail>(x =>
            x.AppointmentId == appointment.Id && x.PlayerId == player.Id && !x.IsActive &&
            x.PlayerStatus == "Requested Player");

        var notification = new Notification()
        {
            SenderId = UserDetail.UserId,
            ReceiverId = player.Id,
            IsActive = true,
            CreatedBy = UserDetail.UserId,
            CreatedAt = DateTime.Now,
            ReceiverEntity = (int)Roles.Player,
            SenderEntity = (int)Roles.Player,
            IsSeen = false,
        };
        
        if (appointmentDetail != null)
        {
            if (isApproved)
            {
                appointmentDetail.IsActive = true;
                
                _genericRepository.Update(appointmentDetail);

                var teamMember = new Team()
                {
                    PlayerId = UserDetail.UserId,
                    AssigneeId = player.Id,
                    CreatedAt = DateTime.Now,
                    CreatedBy = UserDetail.UserId,
                    IsActive = true
                };

                _genericRepository.Insert(teamMember);
                
                notification.AppointmentId = appointment.Id;
                notification.Title = "Request Approved";
                notification.Content =
                    $"Your request for futsal at {appointment.TimeSlotStartTime} - {appointment.TimeSlotEndTime} at {appointment.AppointedDate:dd-MM-yyyy} has been successfully approved";
            }
            else
            {
                _genericRepository.Delete(appointmentDetail);
                
                notification.Title = "Request Rejected / Disapproved";
                notification.Content = "Unfortunately, your request has been disapproved by the organizer.";
            }
        }

        _genericRepository.Insert(notification);

        TempData["Success"] = "Approval Request Successfully Managed";
        
        return RedirectToAction("ApprovedAppointmentsDetails", new { appointmentId = appointment.Id });
    }
    
    [HttpPost]
    public IActionResult ConfirmReservation(ReservedAppointmentsDto reservedAppointment)
    {
        var appointment = _genericRepository.GetById<Appointment>(reservedAppointment.AppointmentId);

        var appointmentDetails = _genericRepository.Get<AppointmentDetail>(x => x.AppointmentId == appointment.Id && x.PlayerStatus == "Self");

        var oldAppointmentDetails = appointmentDetails as AppointmentDetail[] ?? appointmentDetails.ToArray();
        
        if (oldAppointmentDetails.Any())
        {
            _genericRepository.RemoveMultipleEntity(oldAppointmentDetails);
        }
        
        var assignedPlayers = reservedAppointment.AssignedPlayersList.Where(x => x.IsSelected);

        var newAppointmentDetails = assignedPlayers.Select(x => new AppointmentDetail()
        {
            AppointmentId = appointment.Id,
            IsActive = true,
            PlayerId = x.PlayerId,
            PlayerTeam = "",
            PlayerStatus = "Self",
            CreatedAt = DateTime.Now,
            CreatedBy = UserDetail.UserId
        }).ToList();

        _genericRepository.AddMultipleEntity(newAppointmentDetails);

        return Json(new
        {
            data = true
        });
    }

    [HttpPost]
    public IActionResult RequestAppointmentPlayers(ReservedAppointmentsDto reservedAppointmentDetails)
    {
        var userId = UserDetail.UserId;

        var user = _genericRepository.GetById<AppUser>(userId);
        
        var appointment = _genericRepository.GetById<Appointment>(reservedAppointmentDetails.AppointmentId);

        var appointmentRequest = new AppointmentRequest()
        {
            AppointmentId = appointment.Id,
            IsActive = true,
            CreatedAt = DateTime.Now,
            CreatedBy = UserDetail.UserId,
            RequestedPlayers = reservedAppointmentDetails.NumberOfPlayers ?? 0,
        };

        _genericRepository.Insert(appointmentRequest);

        var futsal = _genericRepository.GetById<Futsal>(reservedAppointmentDetails.FutsalId);

        var courts = _genericRepository.Get<Court>(x => x.FutsalId == futsal.Id);

        var appointments = _genericRepository.Get<Appointment>(x =>
            courts.Select(z => z.Id).Contains(x.BookedCourtId) 
            && x.IsApproved && x.IsActionComplete && x.Id != appointment.Id);

        var appointmentDetails =
            _genericRepository.Get<AppointmentDetail>(x => appointments.Select(z => z.Id).Contains(x.AppointmentId));
        
        var teamMembers = _genericRepository.Get<Team>(x => x.PlayerId == user.Id || x.AssigneeId == user.Id);
        
        var uniqueUserIds = teamMembers
            .SelectMany(x => new List<Guid> { x.PlayerId, x.AssigneeId }) 
            .Distinct()
            .ToList();

        var newTeamMembers = appointmentDetails.Select(x => x.PlayerId).Where(z => !uniqueUserIds.Contains(z));

        foreach (var newMember in newTeamMembers)
        {
            var notification = new Notification()
            {
                Title = "Player Request",
                Content = $"Available for a {appointment.TimeSlotStartTime} - {appointment.TimeSlotEndTime} at {appointment.AppointedDate:dd-MM-yyyy}? Contact me for the booking details at (+977) {user.MobileNo}, {user.FullName}.",
                IsActive = true,
                CreatedBy = user.Id,
                ReceiverEntity = (int)Roles.Player,
                SenderEntity = (int)Roles.Player,
                SenderId = user.Id,
                ReceiverId = newMember,
                AppointmentId = appointment.Id
            };

            _genericRepository.Insert(notification);
        }
        
        return Json(new
        {
            data = true
        });
    }
}