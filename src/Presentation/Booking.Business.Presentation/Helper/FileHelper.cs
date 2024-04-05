namespace Booking.Presentation.Helper;

public static class FileHelper
{
    public static bool IsFileAvailable(string filePath)
    {
        try
        {
            using (File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                return true;
            }
        }
        catch
        {
            return false; // Файл недоступен (занят или ошибка доступа)
        }
    }
}