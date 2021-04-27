using System;

namespace Timeline.Services.User
{
    public class CreateUserParams
    {
        public CreateUserParams(string username, string password)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string? Nickname { get; set; }
    }
}
