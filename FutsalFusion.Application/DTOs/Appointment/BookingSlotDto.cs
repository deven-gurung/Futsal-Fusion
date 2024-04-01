namespace FutsalFusion.Application.DTOs.Appointment;

public class BookingSlotDto
{
    public string Day { get; set; }

    public DateTime AppointedDate { get; set; }
    
    public List<SlotsDto> Slots { get; set; }
}

public class SlotsDto
{
    public DateTime AppointedDate { get; set; }
    
    public TimeSpan TimeSlot { get; set; }
    
    public decimal Price { get; set; }
    
    public string Status { get; set; }
}

