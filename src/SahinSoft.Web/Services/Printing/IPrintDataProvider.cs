using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Services.Printing;

public interface IPrintDataProvider
{
    Task<PrintDataContext> BuildForRetailSaleAsync(int retailSaleId, CancellationToken cancellationToken = default);
    Task<PrintDataContext> BuildForKitchenTicketAsync(int kitchenTicketId, CancellationToken cancellationToken = default);
    Task<PrintDataContext> BuildForZPeriodAsync(int zPeriodId, CancellationToken cancellationToken = default);
    Task<PrintDataContext> BuildForXReportAsync(int branchId, CancellationToken cancellationToken = default);
    PrintDataContext BuildSample(PrintTemplateType type);
}
