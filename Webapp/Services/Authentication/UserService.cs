using System.Security.Claims;
using Domain.Users;
using Infrastructure.Services;
using SETiAuth.Domain.Shared.Authentication;

namespace Webapp.Services.Authentication;
public class UserService {
    private readonly UserProfileService _profileService;
    private UserSessionDto _session;
    private UserProfile? _profile;
    private ClaimsPrincipal currentUser = new(new ClaimsIdentity());
    
    public UserService(UserProfileService profileService) {
        this._profileService = profileService;
    }
    
    public ClaimsPrincipal GetUser() {
        return currentUser;
    }
    
    public UserProfile? GetProfile() {
        return _profile;
    }
    
    public async Task<UserProfile?> GetProfile(string username) {
        return await this._profileService.GetProfile(username);
    }
    
    public async Task<bool> ProfileExists(string username) {
        return await this._profileService.ProfileExists(username);
    }
    
    public async Task<UserProfile> CreateProfile(UserProfile profile) {
        this._profile=profile;
        return await this._profileService.CreateProfile(profile);
    }
    
    public async Task<bool> UpdateProfile(UserProfile profile) {
        this._profile=profile;
        return await this._profileService.UpdateProfile(profile);
    }
    
    internal async Task SetUser(ClaimsPrincipal user,UserSessionDto? session=null) {
        if (currentUser != user) {
            this._session = session ?? new UserSessionDto() {
                UserAccount=new UserAccountDto() {
                    Username="Anonymous",Role="Anonymous"
                }
            };
            var name = user.Claims.FirstOrDefault(e => e.Type == ClaimTypes.Name)?.Value;
            if (name != null) {
                if (name != "Anonymous") {
                    this._profile = await this._profileService.GetProfile(name);
                } else {
                    this._profile=null;
                }
            } else {
                this._profile=null;
            }
            currentUser = user;
        }
    }
}
