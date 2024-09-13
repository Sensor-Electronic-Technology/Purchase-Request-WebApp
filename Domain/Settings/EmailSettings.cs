namespace Domain.Settings;

public class EmailSettings {
    public TemplateKeys? TemplateKeys { get; set; }
    public ServerSettings? ServerSettings { get; set; }
    public string? TemplatePath { get; set; }
}

public class TemplateKeys {
    public string? ApproverKey { get; set; }
    public string? RequesterKey { get; set; }
    public string? LinkTextKey { get; set; }
    public string? PrLinkKey { get; set; }
    public string? TitleKey { get; set; }
    public string? DescriptionKey { get; set; }
    public string? AdditionalKey { get; set; }

}

public class ServerSettings {
    public string? Host { get; set; }
    public int Port { get; set; }
    public string? FromAddress { get; set; }
}