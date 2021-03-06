﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Timeline.Models;

namespace Timeline.Entities
{
#pragma warning disable CA2227 // Collection properties should be read only
    // TODO: Create index for this table.
    [Table("timelines")]
    public class TimelineEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("unique_id"), Required]
        public string UniqueId { get; set; } = default!;

        /// <summary>
        /// If null, then this timeline is a personal timeline.
        /// </summary>
        [Column("name")]
        public string? Name { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("name_last_modified")]
        public DateTime NameLastModified { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("owner")]
        public long OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public UserEntity Owner { get; set; } = default!;

        [Column("visibility")]
        public TimelineVisibility Visibility { get; set; }

        [Column("color")]
        public string? Color { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        [Column("last_modified")]
        public DateTime LastModified { get; set; }

        [Column("current_post_local_id")]
        public long CurrentPostLocalId { get; set; }

        public List<TimelineMemberEntity> Members { get; set; } = default!;

        public List<TimelinePostEntity> Posts { get; set; } = default!;
    }
#pragma warning restore CA2227 // Collection properties should be read only
}
