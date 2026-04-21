using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers
{
    public static class UserMapper
    {
        public static User Map(this Kullanici user) =>
            user != null ? new User
            {
                Id = user.Kullaniciid,
                FirstName = user.Ad,
                LastName = user.Soyad,
                Username = user.Eposta,
                Password = user.Pwd,
                Role = (UserRoles)user.Roleid
            }
            : null;
    }
}
