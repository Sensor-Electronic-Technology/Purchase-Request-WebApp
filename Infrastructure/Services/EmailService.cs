using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using MailKit.Net.Smtp;
using MimeKit.Utils;
using Microsoft.Extensions.Logging;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Infrastructure.Services;

public class EmailService {
    public static readonly string FromAddress = "purchase.request@s-et.com";
    //private readonly ILogger<EmailService> _logger;

    /*public EmailService(ILogger<EmailService> logger) {
        _logger = logger;
    }*/
    
    public EmailService() {
        
    }

    public async Task SendEmail(List<string> to, List<string> toCC) {
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
            /*
            builder.HtmlBody = "<style>\n    .button-style{\n        " +
                               "background-color: #70FFDD;\n        " +
                               "width: 150px;\n        height: 100px;\n        " +
                               "font-size: 16px;\n    }\n  \n</style>\n\n\n<h3>Purchase Request</h3>\n<p><a href=\"setihome.seti.com\" target=\"_blank\">View Purchase Request</a></p>\n<form action=\"setihome.seti.com\">\n    <input type=\"submit\" \n           value=\"View Purchase Request\"\n           class=\"button-style\"/>\n</form>\n\n<p><strong>Description</strong>:</p>\n<p>&nbsp;</p>\n<p><strong>Additional Message</strong></p>";
                               */
            builder.HtmlBody=await GetTemplate("Consultant Computer Purchase Request", "This is for the consultant computers", "Sharon, Please see the shopping list here: https://setihome.seti.com");
            message.Body = builder.ToMessageBody();
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        } catch (Exception ex) {
            Console.WriteLine($"Mail Failed, Exception: \n {ex.Message}");
        }
    }

    private async Task<string> GetTemplate(string title, string description, string additional) {
        using StreamReader reader = new(@"C:\Users\aelmendo\RiderProjects\PurchaseRequest\Webapp\wwwroot\EmailTemplateV2\EmailTemplateV2.htm");
        var template=await reader.ReadToEndAsync();
        template=template.Replace("{approver}","Rakesh Jain")
            .Replace("{requester}","Andrew Elmendorf")
            .Replace("{title}", "Consultant Computers")
            .Replace("{description}", "Workstation computers for 2 HQ consultants")
            .Replace("{additional}", "Sharon, Please see the shopping list here: https://a.co/goAegN7");
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
}