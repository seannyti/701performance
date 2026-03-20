using mperformancepower.Api.DTOs.Inquiry;

namespace mperformancepower.Api.Services.Interfaces;

public interface IMailService
{
    Task SendInquiryConfirmationAsync(InquiryDto inquiry);
    Task SendAdminNotificationAsync(InquiryDto inquiry);
    Task SendEmailVerificationAsync(string toEmail, string name, string token, string publicBaseUrl);
    Task SendTempPasswordAsync(string toEmail, string tempPassword);
}
