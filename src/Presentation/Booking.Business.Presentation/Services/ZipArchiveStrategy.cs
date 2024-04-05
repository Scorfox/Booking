using System.IO.Compression;
using Booking.Presentation.Helper;
using Serilog;

namespace Booking.Presentation.Services;

public class ZipArchiveStrategy : IArchiveStrategy
{
    public void ArchiveLogs(string sourceDirectory, string destinationPath)
    {
        // Создаём временную директорию для хранения незанятых файлов
        var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        
        foreach (var filePath in Directory.GetFiles(sourceDirectory))
        {
            if (FileHelper.IsFileAvailable(filePath))
            {
                // Копируем незанятые файлы во временную директорию
                var fileName = Path.GetFileName(filePath);
                var tempFilePath = Path.Combine(tempDirectory, fileName);
                File.Copy(filePath, tempFilePath);
            }
        }

        // Создаём архив из временной директории
        if (Directory.Exists(tempDirectory))
        {
            ZipFile.CreateFromDirectory(tempDirectory, destinationPath + DateTimeOffset.Now.ToString("yyyyMMdd_HHmm") + ".zip");
            Log.Information("Архив создан успешно.");
        }
    }
}
