using Radzen;

namespace Webapp.Data;

public class RequestAlert {
    public string? Item { get; set; }
    public string? Message { get; set; }
    public bool Okay { get; set; }
    public AlertStyle Style { get; set; }
}