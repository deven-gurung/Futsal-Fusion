using FutsalFusion.Application.DTOs.Appointment;

namespace FutsalFusion.Application.DTOs.Futsal;

public class CourtDetailsResponseDto
{
    public Guid FutsalId { get; set; }
    
    public string FutsalName { get; set; }
    
    public Guid CourtId { get; set; }
    
    public string CourtName { get; set; }

    public string CourtDescription { get; set; }
    
    public string Address { get; set; }

    public string OwnerName { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public string EmailAddress { get; set; }
    
    public int OpeningHours { get; set; }
    
    public int ClosingHours { get; set; }
    
    public List<string> CourtImageURL { get; set; }
    
    public List<BookingSlotDto> BookingSlots { get; set; }
    
    public List<FutsalWorkingHours> WorkingHours { get; set; }

    public List<CourtPriceDetails> CourtPrices { get; set; }
}

public class CourtPriceDetails
{
    public string TimePeriod { get; set; }
    
    public string Price { get; set; }
}
