﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    [Table("timeline_posts")]
    public class TimelinePostEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("local_id")]
        public long LocalId { get; set; }

        [Column("timeline")]
        public long TimelineId { get; set; }

        [ForeignKey(nameof(TimelineId))]
        public TimelineEntity Timeline { get; set; } = default!;

        [Column("author")]
        public long? AuthorId { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public UserEntity? Author { get; set; } = default!;

        [Obsolete("Use post data instead.")]
        [Column("content_type")]
        public string? ContentType { get; set; }

        [Obsolete("Use post data instead.")]
        [Column("content")]
        public string? Content { get; set; }

        [Obsolete("Use post data instead.")]
        [Column("extra_content")]
        public string? ExtraContent { get; set; }

        [Column("deleted")]
        public bool Deleted { get; set; }

        [Column("color")]
        public string? Color { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }

#pragma warning disable CA2227
        public List<TimelinePostDataEntity> DataList { get; set; } = default!;
#pragma warning restore CA2227
    }
}
