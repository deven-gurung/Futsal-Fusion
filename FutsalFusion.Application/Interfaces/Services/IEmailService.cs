using FutsalFusion.Application.DTOs.Email;

namespace FutsalFusion.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendEmail(EmailActionDto emailAction);
}