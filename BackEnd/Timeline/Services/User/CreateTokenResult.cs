using System;
using Timeline.Entities;

namespace Timeline.Services.User
{
    public class CreateTokenResult
    {
        public string Token { get; set; } = default!;
        public UserEntity User { get; set; } = default!;
    }
}

