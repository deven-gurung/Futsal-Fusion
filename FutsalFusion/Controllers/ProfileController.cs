using FutsalFusion.Application.DTOs.Account;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Constants;
using FutsalFusion.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FutsalFusion.Controllers;

public class ProfileController : BaseController<ProfileController>
{
    private readonly IGenericRepository _genericRepository;

    public ProfileController(IGenericRepository genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public IActionResult Index()
    {
        var userId = UserDetail.UserId;

        var user = _genericRepository.GetById<AppUser>(userId);

        var role = _genericRepository.GetFirstOrDefault<AppRole>(x => x.Name == Constants.Roles.Player);
        
        var teamMembers = _genericRepository.Get<Team>(x => x.PlayerId == user.Id || x.AssigneeId == user.Id);
        
        var uniqueUserIds = teamMembers
            .SelectMany(x => new List<Guid> { x.PlayerId, x.AssigneeId }) 
            .Distinct()
            .ToList();

        var remainingUsers = 
            _genericRepository.Get<AppUser>(x => !uniqueUserIds.Contains(x.Id) && x.Id != user.Id && x.RoleId == role!.Id).ToList();

        var usersLise = remainingUsers.Select(x => new
        {
            Id = x.Id,
            Value = x.FullName
        }).ToList();
        
        ViewBag.ddlUsers = new SelectList(usersLise, "Id", "Value");
        
        // Assuming uniqueUserIds is a List<Guid> containing the unique user IDs
        List<TeamMembers> assignedPlayers = new List<TeamMembers>();

        foreach (var assigneeId in uniqueUserIds)
        {
            if(assigneeId == user.Id) continue;
            
            // Fetch the assigned user
            var assignedUser = _genericRepository.GetById<AppUser>(assigneeId);

            // Get appointments created by the assigned user or the current user
            var appointments = _genericRepository.Get<Appointment>(x => x.CreatedBy == assignedUser.Id || x.CreatedBy == user.Id).ToList();
    
            // Get appointment details where the appointment IDs match those created by the assigned user or the current user
            var appointmentIds = appointments.Select(z => z.Id);
            var appointmentDetails = _genericRepository.Get<AppointmentDetail>(x => appointmentIds.Contains(x.Id) && (x.PlayerId == assignedUser.Id || x.PlayerId == user.Id)).ToList();

            // Further fetch appointment IDs from the details
            var detailedAppointmentIds = appointmentDetails.Select(x => x.AppointmentId).ToList();

            // Get first matching team member model
            var teamMemberModel = _genericRepository.GetFirstOrDefault<Team>(x => 
                (x.PlayerId == assignedUser.Id && x.AssigneeId == user.Id) || 
                (x.PlayerId == user.Id && x.AssigneeId == assignedUser.Id));

            // Create a new TeamMembers object and populate it
            var teamMember = new TeamMembers()
            {
                PlayerId = assignedUser.Id,
                Name = assignedUser.FullName,
                ImageUrl = assignedUser.ImageURL ?? "sample-profile.png",
                PhoneNumber = assignedUser.MobileNo ?? "",
                SharedBookingSlots = detailedAppointmentIds.Any() ? appointments.Count(x => detailedAppointmentIds.Contains(x.Id)) : 0,
                FriendsSince = teamMemberModel?.CreatedAt.ToString("dd-MM-yyyy")
            };

            // Add to the list of assigned players
            assignedPlayers.Add(teamMember);
        }


        var result = new ProfileDetailsDto()
        {
            Id = user.Id,
            Name = user.FullName,
            ImageUrl = user.ImageURL ?? "sample-profile.png",
            Role = _genericRepository.GetById<AppRole>(user.RoleId).Name,
            Location = "Nepal",
            EmailAddress = user.EmailAddress,
            Mobile = user.MobileNo ?? "",
            FriendsRequest = new()
            {
                PlayerId = user.Id
            },
            TeamMembers = assignedPlayers
        };
        
        return View(result);
    }

    public IActionResult AssignMembers(ProfileDetailsDto profileDetails)
    {
        var assignedMembers = profileDetails.FriendsRequest;

        var assignees = assignedMembers.RequestedPlayers.Split(",");

        foreach (var assignee in assignees)
        {
            var existingTeamMember = _genericRepository.GetFirstOrDefault<Team>(x =>
                (x.PlayerId == Guid.Parse(assignee) && x.AssigneeId == assignedMembers.PlayerId)
                || (x.AssigneeId == Guid.Parse(assignee) && x.PlayerId == assignedMembers.PlayerId));

            if (existingTeamMember != null)
            {
                _genericRepository.Delete(existingTeamMember);
            }

            var assignedMember = _genericRepository.GetById<AppUser>(Guid.Parse(assignee));

            var player = _genericRepository.GetById<AppUser>(assignedMembers.PlayerId);

            var team = new Team()
            {
                PlayerId = player.Id,
                AssigneeId = assignedMember.Id,
                CreatedBy = player.Id,
                IsActive = true,
                CreatedAt = DateTime.Now,
            };

            _genericRepository.Insert(team);
        }

        TempData["Success"] = "Team Member successfully assigned";
        
        return RedirectToAction("Index");
    }
}