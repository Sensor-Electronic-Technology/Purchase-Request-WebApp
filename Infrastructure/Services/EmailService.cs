using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.TypeConstants;
using Domain.Settings;
using Microsoft.AspNetCore.Hosting;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Infrastructure.Services;

public class EmailService {
    public static readonly string FromAddress = "purchase.request@s-et.com";
    private EmailSettings _emailSettings;
    private readonly IWebHostEnvironment _environment;
    //private readonly ILogger<EmailService> _logger;

    /*public EmailService(ILogger<EmailService> logger) {
        _logger = logger;
    }*/
    
    public EmailService(EmailSettings emailSettings,IWebHostEnvironment environment) {
        _emailSettings = emailSettings;
        this._environment = environment;
    }
    
    public EmailService() {
        
    }

    public async Task SendEmail(EmailType type,PurchaseRequestInput prInput,List<string> to, List<string> toCC) {
        var client = new SmtpClient();
        try {
            client.CheckCertificateRevocation = false;
            client.ServerCertificateValidationCallback = CertValidationCallback;
            await client.ConnectAsync(this._emailSettings.ServerSettings?.Host ?? "10.92.3.215",
                this._emailSettings.ServerSettings?.Port ?? 25, false);
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Purchase Request", FromAddress));
            foreach (var recipient in to) {
                message.To.Add(new MailboxAddress(recipient, recipient));
            }
            foreach (var recipient in toCC) {
                message.Cc.Add(new MailboxAddress(recipient, recipient));
            }
            message.Subject = $"{prInput.Title}-Purchase Request";
            var builder = new BodyBuilder { HtmlBody = await GenerateMessage(prInput.ApproverName,
                prInput.RequesterName,
                prInput.PrUrl,"View Purchase Request", 
                prInput.Title, prInput.Description,
                prInput.AdditionalComments)
            };

            await using var filestream=File.OpenRead($"{this._environment.WebRootPath}/temp/{prInput.TempFile}");
            await builder.Attachments.AddAsync("PurchaseRequest.pdf",filestream);
            
            foreach (var attachment in prInput.Attachments) {
                //builder.Attachments.Add(attachment.name,attachment.filePath);
                await builder.Attachments.AddAsync(attachment.filePath);
            }
            message.Body = builder.ToMessageBody();
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        } catch (Exception ex) {
            Console.WriteLine($"Mail Failed, Exception: \n {ex.Message}");
        }
    }
    
    private async Task<string> GenerateMessage(string approver,string requester,string prLink,string linkText,string title, string description, string additional) {
        using StreamReader reader = new($"{this._environment.WebRootPath}/{this._emailSettings.TemplatePath}");
        Console.WriteLine($"{this._environment.WebRootPath}/{this._emailSettings.TemplatePath}");
        var template=await reader.ReadToEndAsync();
        template=template.Replace(this._emailSettings.TemplateKeys?.ApproverKey ?? "{approver}",approver)
            .Replace(this._emailSettings.TemplateKeys?.RequesterKey ?? "{requester}",requester)
            .Replace(this._emailSettings.TemplateKeys?.PrLinkKey ?? "{pr_link}",prLink)
            .Replace(this._emailSettings.TemplateKeys?.LinkTextKey ?? "{link_text}",linkText)
            .Replace(this._emailSettings.TemplateKeys?.TitleKey ?? "{title}", title)
            .Replace(this._emailSettings.TemplateKeys?.DescriptionKey ?? "{description}", description)
            .Replace(this._emailSettings.TemplateKeys?.AdditionalKey ?? "{additional}", additional);
        return template;
    }

    private bool CertValidationCallback (object sender, 
        X509Certificate certificate, 
        X509Chain chain, 
        SslPolicyErrors sslPolicyErrors) {
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;
        if (certificate is X509Certificate2 certificate2) {
            var cn = certificate2.GetNameInfo (X509NameType.SimpleName, false);
            var fingerprint = certificate2.Thumbprint;
            var serial = certificate2.SerialNumber;
            var issuer = certificate2.Issuer;
            Console.WriteLine($"Cert: {cn}");
            Console.WriteLine($"Fingerprint: {fingerprint}");
            Console.WriteLine($"Serial: {serial}");
            Console.WriteLine($"Issuer: {issuer}");
            return true;
        }
        return true;
    }
    
        /*public async Task SendEmailTesting(PurchaseRequestInput prInput,List<string> to, List<string> toCC) {
        var client = new SmtpClient();
        try {
            client.CheckCertificateRevocation = false;
            client.ServerCertificateValidationCallback = CertValidationCallback;
            await client.ConnectAsync("10.92.3.215", 25, false);
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Purchase Request", FromAddress));
            foreach (var recipient in to) {
                message.To.Add(new MailboxAddress(recipient, recipient));
            }
            foreach (var recipient in toCC) {
                message.Cc.Add(new MailboxAddress(recipient, recipient));
            }
            message.Subject = "Consultant Computers-Purchase Request";
            var builder = new BodyBuilder();
            builder.HtmlBody=await GetTemplateTesting("Rakesh Jain","Andrew Elmendorf",
                "setihome.seti.com", "Consultant Computer Purchase Request", 
                "This is for the consultant computers", 
                "Sharon, Please see the shopping list here: https://setihome.seti.com");
            message.Body = builder.ToMessageBody();
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        } catch (Exception ex) {
            Console.WriteLine($"Mail Failed, Exception: \n {ex.Message}");
        }
    }*/

    /*private async Task<string> GetTemplateTesting(string approver,string requester,string prLink,string title, string description, string additional) {
        using StreamReader reader = new(@"C:\Users\aelmendo\RiderProjects\PurchaseRequest\Webapp\wwwroot\EmailTemplateV2\EmailTemplateV2.htm");
        var template=await reader.ReadToEndAsync();
        template=template.Replace("{approver}",approver)
            .Replace("{requester}",requester)
            .Replace("{pr_link}",prLink)
            .Replace("{title}", title)
            .Replace("{description}", description)
            .Replace("{additional}", additional);
        return template;
    }*/
}