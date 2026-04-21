namespace KolayCAR.Broker.Domain.Models
{
    public class User
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public UserRoles Role { get; set; }
    }

    public enum UserRoles
    {
        Admin = 1,
        Agency = 2,
        User = 3,
        MobileAPP = 4,
        External = 5,
        Accountancy = 6
    }
}
