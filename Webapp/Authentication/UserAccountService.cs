using Domain.Authentication;
using Visus.Ldap;

namespace Webapp.Authentication;

public class UserAccountService {
    private List<UserAccount> _users;

    public UserAccountService()
    {
        _users = new List<UserAccount>
        {
            new UserAccount{ Username = "admin", Password = "admin", Role = "Administrator" },
            new UserAccount{ Username = "user", Password = "user", Role = "User" },
        };
    }

    //UserAccount? means that the method GetByUserName can return a valid UserAccount object or null. 
    public UserAccount? GetByUserName(string userName) {
        return _users.FirstOrDefault(x => x.Username == userName);
    }
}