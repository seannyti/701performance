using mperformancepower.Api.DTOs.Common;
using mperformancepower.Api.DTOs.Inquiry;

namespace mperformancepower.Api.Services.Interfaces;

public interface IInquiryService
{
    Task<InquiryDto> CreateInquiryAsync(CreateInquiryDto dto);
    Task<PagedResultDto<InquiryDto>> GetInquiriesAsync(
        int page, int pageSize, string? status, string? search, DateTime? from, DateTime? to);
    Task<InquiryDto?> GetInquiryAsync(int id);
    Task<InquiryDto?> UpdateStatusAsync(int id, string status);
    Task<InquiryStatsDto> GetStatsAsync();
}
