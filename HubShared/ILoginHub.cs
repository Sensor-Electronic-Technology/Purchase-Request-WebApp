using Domain.Authentication;

namespace HubShared;

public interface ILoginHub {
    Task<UserAccount> Login(string username, string password);
}