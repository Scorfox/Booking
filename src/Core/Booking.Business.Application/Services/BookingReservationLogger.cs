
namespace Booking.Business.Application.Services;

public class BookingReservationLogger : IBookingReservationLogger
{
    private readonly Queue<string> _logQueue = new();
    private readonly object _lockObject = new();

    public void AddLog(string logMessage)
    {
        // Блокировка для обеспечения безопасного доступа к очереди логов
        lock (_lockObject)
        {
            _logQueue.Enqueue(logMessage);
        }
    }

    public void ProcessLogs()
    {
        while (true)
        {
            string? logMessage = null;
            
            // Блокировка для безопасного доступа к очереди логов
            lock (_lockObject)
            {
                if (_logQueue.Count > 0)
                {
                    logMessage = _logQueue.Dequeue();
                }
            }

            if (logMessage != null)
            {
                // Здесь можно обработать лог, например, записать его в файл или отправить по сети
                Console.WriteLine(logMessage);
            }
          
        }
    }
}