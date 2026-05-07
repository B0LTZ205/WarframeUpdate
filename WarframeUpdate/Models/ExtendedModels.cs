using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarframeUpdate.Models
{
    /// <summary>
    /// Extended User Profile model
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// UserId is both the primary key and foreign key for one-to-one relationship with ApplicationUser
        /// </summary>
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(15)]
        [Phone]
        public string PhoneNumber { get; set; }

        [MaxLength(500)]
        public string ProfilePictureUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual ApplicationUser User { get; set; }
    }

    /// <summary>
    /// File Upload model for handling user uploads
    /// </summary>
    public class FileUpload
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string OriginalFileName { get; set; }

        [Required]
        [MaxLength(500)]
        public string StoredFileName { get; set; }

        [Required]
        [MaxLength(10)]
        public string FileExtension { get; set; }

        [Required]
        [MaxLength(50)]
        public string FileType { get; set; } // "Image", "Video", "Audio", "PDF", "Text", etc.

        [Required]
        public long FileSizeBytes { get; set; }

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string Description { get; set; }

        public bool IsPublic { get; set; } = false;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }

    /// <summary>
    /// Admin Activity Log model for tracking admin actions
    /// </summary>
    public class AdminActivityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string AdminUserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; }

        [MaxLength(500)]
        public string Details { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AdminUserId")]
        public virtual ApplicationUser AdminUser { get; set; }
    }

    /// <summary>
    /// Pagination helper model
    /// </summary>
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }

    /// <summary>
    /// UserTask model for tracking user tasks
    /// </summary>
    public class UserTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<TaskCompletion> Completions { get; set; }
    }

    /// <summary>
    /// Task Completion tracking model
    /// </summary>
    public class TaskCompletion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserTaskId { get; set; }

        [Required]
        public string CompletedBy { get; set; }

        [MaxLength(500)]
        public string CompletionNotes { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserTaskId")]
        public virtual UserTask UserTask { get; set; }

        [ForeignKey("CompletedBy")]
        public virtual ApplicationUser User { get; set; }
    }

    /// <summary>
    /// Task Status enum
    /// </summary>
    public enum TaskStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }

    /// <summary>
    /// Task Priority enum
    /// </summary>
    public enum TaskPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    /// <summary>
    /// Nightwave Challenge Completion tracking
    /// </summary>
    public class NightwaveCompletion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ChallengeTitle { get; set; }

        [Required]
        [MaxLength(1000)]
        public string ChallengeDescription { get; set; }

        [Required]
        public int Reputation { get; set; }

        [Required]
        public bool IsDaily { get; set; }

        [Required]
        public bool IsElite { get; set; }

        [Required]
        [MaxLength(50)]
        public string ChallengeExpiry { get; set; }

        [Required]
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}