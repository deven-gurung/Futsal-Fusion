using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class Notification : BaseEntity<Guid>
{
    public string Title { get; set; }
    
    public string Content { get; set; }
    
    public string SenderId { get; set; }
    
    public string ReceiverId { get; set; }
    
    public int SenderEntity { get; set; }
    
    public int ReceiverEntity { get; set; }

    public bool IsSeen { get; set; } = false;
}