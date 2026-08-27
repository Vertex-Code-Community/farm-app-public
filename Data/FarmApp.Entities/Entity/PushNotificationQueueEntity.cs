using FarmApp.Entities.Interfaces;
using FarmApp.Shared.Enums.PushNotification;

namespace FarmApp.Entities.Entity
{
    public class PushNotificationQueueEntity : IBaseEntity<string>
    {
        public string Id { get; set; }
        public string UserId { get; set; } = default!;
        public string? PropertyNoteId { get; set; }
        public PushNotificationType Type { get; set; }
        public DateTime SendAt { get; set; }
        public PushNotificationStatus Status { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public int RetryCount { get; set; }
        public string? Error { get; set; }
    }
}
