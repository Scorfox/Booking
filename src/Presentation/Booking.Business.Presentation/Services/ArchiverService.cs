using Booking.Presentation.Options;
using Microsoft.Extensions.Options;
using Serilog;

namespace Booking.Presentation.Services;

public class ArchiverService : IArchiverService
{
    private readonly ArchiverOptions _archiverOptions;
    private IArchiveStrategy Strategy { get; set; }
    private static readonly object Lock = new(); 
    
    public ArchiverService(IOptions<ArchiverOptions> archiveOptions)
    {
        _archiverOptions = archiveOptions.Value;
    }
    
    public void SetStrategy(IArchiveStrategy strategy)
    {
        Strategy = strategy;
    }

    public void ArchiveLogs()
    {
        lock (Lock)
        {
            //DeleteOldArchives(_archiverOptions.DestinationDirectory);
            Strategy.ArchiveLogs(_archiverOptions.SourceDirectory, _archiverOptions.DestinationDirectory);
        }
    }
    
    private void DeleteOldArchives(string archiveDirectory)
    {
        var files = Directory.GetFiles(archiveDirectory);
        foreach (var file in files)
        {
            try
            {
                var creationTime = File.GetCreationTime(file);
                if (DateTime.Now - creationTime > _archiverOptions.StorageTime)
                {
                    File.Delete(file);
                    Log.Information($"Удален старый архив: {file}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка при удалении файла {file}: {ex.Message}");
            }
        }
    }
}