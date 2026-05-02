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
}