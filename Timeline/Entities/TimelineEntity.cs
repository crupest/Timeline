using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Timeline.Models;

namespace Timeline.Entities
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is entity object.")]
    [Table("timelines")]
    public class TimelineEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// If null, then this timeline is a personal timeline.
        /// </summary>
        [Column("name")]
        public string? Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("owner")]
        public long OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public UserEntity Owner { get; set; } = default!;

        [Column("visibility")]
        public TimelineVisibility Visibility { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        public List<TimelineMemberEntity> Members { get; set; } = default!;

        public List<TimelinePostEntity> Posts { get; set; } = default!;
    }
}
