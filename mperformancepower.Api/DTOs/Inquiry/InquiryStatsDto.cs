namespace mperformancepower.Api.DTOs.Inquiry;

public class InquiryStatsDto
{
    public int Total { get; set; }
    public int New { get; set; }
    public int InProgress { get; set; }
    public int Resolved { get; set; }
    public double? AvgResponseMinutes { get; set; }
}
