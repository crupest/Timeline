﻿using System;
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
        public long AuthorId { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public UserEntity Author { get; set; } = default!;

        [Column("content_type"), Required]
        public string ContentType { get; set; } = default!;

        [Column("content")]
        public string? Content { get; set; }

        [Column("extra_content")]
        public string? ExtraContent { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }
    }
}
