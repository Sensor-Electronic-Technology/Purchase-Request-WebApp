using Domain.Authentication;

namespace Domain.Contracts;

public class LoginResponse {
    public UserSession? UserSession { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}