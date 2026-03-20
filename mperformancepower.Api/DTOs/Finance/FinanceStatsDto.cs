namespace mperformancepower.Api.DTOs.Finance;

public class FinanceStatsDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalUnitsSold { get; set; }
    public decimal AvgSalePrice { get; set; }
    public decimal ThisMonthRevenue { get; set; }
    public int ThisMonthUnits { get; set; }
    public List<CategoryRevenueDto> RevenueByCategory { get; set; } = [];
    public List<PaymentMethodBreakdownDto> PaymentMethodBreakdown { get; set; } = [];
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = [];
    public List<LenderBreakdownDto> TopLenders { get; set; } = [];
}

public class CategoryRevenueDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Units { get; set; }
}

public class PaymentMethodBreakdownDto
{
    public string Method { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Revenue { get; set; }
}

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Units { get; set; }
}

public class LenderBreakdownDto
{
    public string Lender { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalFinanced { get; set; }
}
