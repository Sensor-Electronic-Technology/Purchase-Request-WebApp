using System.ComponentModel.DataAnnotations;

namespace Webapp.Data;

public class KestrelCustomSettings {
    public const string SectionName="Kestrel";
    [Required]
    public CertificatesSettings Certificates { get; set; } = new();
}

public class CertificatesSettings {
    [Required]
    public DefaultCertSettings Default { get; set; } = new();
}

public class DefaultCertSettings {
    [Required(ErrorMessage = "Certificate Path is required")]
    public string? Path { get; set; }
    [Required(ErrorMessage = "Certificate KeyPath is required")]
    public string? KeyPath { get; set; }

    public bool PathsExist() {
        return File.Exists(this.Path) && File.Exists(this.KeyPath);
    }
}