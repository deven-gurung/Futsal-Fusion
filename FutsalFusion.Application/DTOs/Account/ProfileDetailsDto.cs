namespace FutsalFusion.Application.DTOs.Account;

public class ProfileDetailsDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public string ImageUrl { get; set; }
    
    public string Role { get; set; }
    
    public string Mobile { get; set; }
    
    public string EmailAddress { get; set; }
    
    public string Location { get; set; }
    
    public FriendsRequest FriendsRequest { get; set; }
    
    public List<TeamMembers> TeamMembers { get; set; }
}

public class TeamMembers
{
    public Guid PlayerId { get; set; }
    
    public string ImageUrl { get; set; }
    
    public string Name { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public string? FriendsSince { get; set; }
    
    public int SharedBookingSlots { get; set; }
}

public class FriendsRequest
{
    public Guid PlayerId { get; set; }
    
    public string RequestedPlayers { get; set; }
}