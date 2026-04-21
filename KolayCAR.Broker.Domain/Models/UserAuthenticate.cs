using System.ComponentModel.DataAnnotations;

namespace KolayCAR.Broker.Domain.Models
{
    public class UserAuthenticate
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
        public string SecretKey { get; set; }
    }
}
