namespace Domain.Contracts;

public class LoginRequest {
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsDomainUser { get; set; }
}