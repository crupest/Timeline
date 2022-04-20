using System;
using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    public class HttpRegisterCodeRegisterRequest
    {
        [Required, Username]
        public string Username { get; set; } = default!;

        [Required, MinLength(1)]
        public string Password { get; set; } = default!;

        [Nickname]
        public string? Nickname { get; set; }

        [Required]
        public string RegisterCode { get; set; } = default!;
    }
}

