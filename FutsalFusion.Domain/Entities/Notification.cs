using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class Notification : BaseEntity<Guid>
{
	public string Title { get; set; }

	public string Content { get; set; }
	
	public Guid SenderId { get; set; }
	
	public Guid ReceiverId { get; set; }
	
	public int SenderEntity { get; set; }
	
	public int ReceiverEntity { get; set; }
	
	public bool IsSeen { get; set; } = false;

	[ForeignKey("SenderId")]
	public virtual AppUser? Sender { get; set; }

	[ForeignKey("ReceiverId")]
	public virtual AppUser? Receiver { get; set; }
}