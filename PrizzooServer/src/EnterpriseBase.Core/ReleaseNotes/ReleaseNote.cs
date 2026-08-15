using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities.Auditing;

namespace EnterpriseBase.ReleaseNotes
{
    /// <summary>
    /// Stores release notes for the application. Managed by host admin.
    /// Not tenant-specific — all tenants see the same release notes.
    /// </summary>
    public class ReleaseNote : CreationAuditedEntity<Guid>
    {
        [Required]
        [StringLength(20)]
        public string Version { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// JSON array of feature descriptions, e.g. ["Feature 1", "Feature 2"]
        /// </summary>
        [Required]
        public string Features { get; set; }

        public bool IsActive { get; set; }

        public ReleaseNote()
        {
            IsActive = true;
        }
    }
}
