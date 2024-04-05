using Booking.Presentation.Options;
using Booking.Presentation.Services;
using Microsoft.Extensions.Options;
using Quartz;

namespace Booking.Presentation.Jobs;

public class ArchiveLogsJob : IJob
{
    private ArchiverOptions ArchiverOptions { get; }
    private IArchiverService ArchiverService { get; set; }
    
    public ArchiveLogsJob(IOptions<ArchiverOptions> archiverOptions, IArchiverService archiverService)
    {
        ArchiverOptions = archiverOptions.Value;
        IArchiveStrategy strategy;
        switch (ArchiverOptions.Strategy)
        {
            case "GZip":
                strategy = new GZipArchiveStrategy();
                break;
            case "Zip":
                strategy = new ZipArchiveStrategy();
                break;
            default:
                throw new ArgumentException("Неизвестная стратегия архивирования", nameof(ArchiverOptions.Strategy));
        }
        ArchiverService = archiverService;
        ArchiverService.SetStrategy(strategy);
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        ArchiverService.ArchiveLogs();
    }
}        