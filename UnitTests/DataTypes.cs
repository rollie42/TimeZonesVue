namespace IntegrationTests
{
    public class LoginDto
    {
        public string Name { get; set; }
        public string Password { get; set; }

    }

    public class RegisterDto
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

    }
}