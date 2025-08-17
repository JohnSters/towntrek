using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TownTrek.Models
{
    /// <summary>
    /// Represents an analytics event for event sourcing
    /// </summary>
    public class AnalyticsEvent
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Type of analytics event (e.g., "BusinessView", "AnalyticsAccess", "Export", "Error")
        /// </summary>
        [Required]
        [StringLength(50)]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// User ID associated with the event (nullable for anonymous events)
        /// </summary>
        [StringLength(450)]
        public string? UserId { get; set; }

        /// <summary>
        /// Business ID associated with the event (nullable for user-level events)
        /// </summary>
        public int? BusinessId { get; set; }

        /// <summary>
        /// Platform where the event occurred (Web, Mobile, API)
        /// </summary>
        [StringLength(20)]
        public string? Platform { get; set; }

        /// <summary>
        /// IP address of the user
        /// </summary>
        [StringLength(45)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent string
        /// </summary>
        [StringLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Session ID for tracking user sessions
        /// </summary>
        [StringLength(100)]
        public string? SessionId { get; set; }

        /// <summary>
        /// JSON serialized event data
        /// </summary>
        public string? EventData { get; set; }

        /// <summary>
        /// Error message for error events
        /// </summary>
        [StringLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Severity level for error events
        /// </summary>
        [StringLength(20)]
        public string? Severity { get; set; }

        /// <summary>
        /// When the event occurred
        /// </summary>
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the event was recorded in the database
        /// </summary>
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey(nameof(BusinessId))]
        public virtual Business? Business { get; set; }
    }
}
