using DentalLink.Application.Common.Interfaces;
using DentalLink.Domain.Constants;
using DentalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DentalLink.Infrastructure.BackgroundServices;

public class TelegramReminderWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelegramReminderWorker> _logger;

    public TelegramReminderWorker(
        IServiceProvider serviceProvider,
        ILogger<TelegramReminderWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TelegramReminderWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in TelegramReminderWorker loop.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("TelegramReminderWorker stopped.");
    }

    private async Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramNotificationService>();

        var now = DateTimeOffset.UtcNow;

        var pendingNotifications = await context.Notifications
            .Include(n => n.Appointment).ThenInclude(a => a.Patient)
            .Include(n => n.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
            .Include(n => n.Appointment).ThenInclude(a => a.Cabinet)
            .Include(n => n.Appointment).ThenInclude(a => a.Slot)
            .Include(n => n.Type)
            .Where(n => n.StatusId == LookupIds.NotificationStatus.Pending && n.ScheduledAt <= now)
            .OrderBy(n => n.ScheduledAt)
            .Take(25)
            .ToListAsync(cancellationToken);

        if (pendingNotifications.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} pending notifications.", pendingNotifications.Count);

        foreach (var notification in pendingNotifications)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var appt = notification.Appointment;
            var text = BuildMessageText(notification);

            var sent = await telegramService.SendMessageAsync(
                notification.TelegramChatId,
                text,
                cancellationToken);

            notification.SentAt = DateTimeOffset.UtcNow;
            notification.StatusId = sent ? LookupIds.NotificationStatus.Sent : LookupIds.NotificationStatus.Failed;
            notification.ResponsePayload = sent ? "Delivered successfully" : "Delivery failed";
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static string BuildMessageText(Notification notification)
    {
        var appt = notification.Appointment;
        var date = appt.Slot.SlotDate.ToString("dd.MM.yyyy");
        var time = appt.Slot.StartTime.ToString("HH:mm");
        var doctor = appt.Doctor.User.FullName;
        var cabinet = appt.Cabinet.Name;
        var num = appt.AppointmentNumber;

        return notification.TypeId switch
        {
            var type when type == LookupIds.NotificationType.Confirmation =>
                $"<b>Стоматологическая клиника DentalLink 🦷</b>\n\n" +
                $"Здравствуйте, {appt.Patient.FullName}!\n" +
                $"Ваша запись подтверждена.\n\n" +
                $"📌 Номер записи: <b>{num}</b>\n" +
                $"👨‍⚕️ Врач: {doctor}\n" +
                $"🚪 {cabinet}\n" +
                $"🗓 Дата: <b>{date}</b> в <b>{time}</b>\n\n" +
                $"Будем рады видеть Вас!",

            var type when type == LookupIds.NotificationType.Reminder24Hours =>
                $"<b>Напоминание о визите в DentalLink 🦷</b>\n\n" +
                $"Уважаемый(ая) {appt.Patient.FullName}, напоминаем о записи на завтра:\n" +
                $"🗓 Дата: <b>{date}</b> в <b>{time}</b>\n" +
                $"👨‍⚕️ Врач: {doctor} ({cabinet})\n" +
                $"Пожалуйста, подтвердите визит или предупредите нас при изменении планов.",

            var type when type == LookupIds.NotificationType.Reminder2Hours =>
                $"<b>Экспресс-напоминание DentalLink 🦷</b>\n\n" +
                $"Ждём Вас через 2 часа в <b>{time}</b> ({cabinet}, врач: {doctor}).",

            var type when type == LookupIds.NotificationType.Cancellation =>
                $"<b>Стоматологическая клиника DentalLink 🦷</b>\n\n" +
                $"Ваша запись <b>{num}</b> на {date} в {time} успешно отменена.",

            _ => $"Уведомление по записи #{num} на {date} в {time}."
        };
    }
}
