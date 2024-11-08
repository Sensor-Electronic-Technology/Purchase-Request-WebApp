using Radzen;

namespace Webapp.Data;

public class UserActionAlert:IEquatable<UserActionAlert> , IComparable<UserActionAlert> {
    public string? Item { get; set; }
    public string? Message { get; set; }
    public bool Okay { get; set; }
    public AlertStyle Style { get; set; }
    
    public bool Equals(UserActionAlert? other) {
        return this.Okay == other?.Okay;
    }

    public int CompareTo(UserActionAlert? other) {
        if(other == null) return -1;
            
        if(this.Okay && other.Okay) {
            return 1;
        } else if (this.Okay && !other.Okay) {
            return -1;
        } else {
            return 0;
        }
    }
}