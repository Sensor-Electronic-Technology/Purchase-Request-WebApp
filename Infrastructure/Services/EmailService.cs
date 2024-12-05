using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.Dto.ActionInputs;
using Domain.PurchaseRequests.TypeConstants;
using Domain.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using SetiFileStore.FileClient;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Infrastructure.Services;

public class EmailService {
    private static readonly string FromAddress = "purchase.request@s-et.com";
    private EmailSettings _emailSettings;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<EmailService> _logger;
    
    public EmailService(EmailSettings emailSettings,IWebHostEnvironment environment,ILogger<EmailService> logger) {
        _emailSettings = emailSettings;
        this._environment = environment;
        this._logger=logger;
    }
    
    public EmailService() {
        
    }

    public async Task SendRequestEmail(byte[] htmlBody,PurchaseRequestInput prInput,List<string> to, List<string> toCC) {
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
            using var stream = new MemoryStream(htmlBody);
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();
            html = html.Replace("{prLink}",$"<a href=\"{prInput.PrUrl}\">Request Link</a>");
            var builder = new BodyBuilder { 
                HtmlBody = html
            };
            builder.Attachments.Add($"{prInput.Title}-PurchaseRequest.pdf", prInput.TempFile);
            foreach (var attachment in prInput.Attachments) { 
                builder.Attachments.Add(attachment.Name, attachment.Data);
            }
            message.Body = builder.ToMessageBody();
            await client.SendAsync(message);
        } catch (Exception ex) {
            this._logger.LogError("Mail Failed, Exception: \\n {ExMessage}", ex.Message);
        } finally {
            await client.DisconnectAsync(true);
        }
    }
    
    public async Task SendCancellationEmail(byte[] htmlBody,string title,List<string> to, List<string> toCC) {
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
            message.Subject = $"{title}-Purchase Request Cancellation";
            using var stream = new MemoryStream(htmlBody);
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();
            var builder = new BodyBuilder { 
                HtmlBody = html
            };
            message.Body = builder.ToMessageBody();
            await client.SendAsync(message);
        } catch (Exception ex) {
            this._logger.LogError("Mail Failed, Exception: \\n {ExMessage}", ex.Message);
        } finally {
            await client.DisconnectAsync(true);
        }
    }
    
    public async Task SendApprovalEmail(byte[] htmlBody,
        string title,
        bool approved,string? url,
        byte[] prDoc,
        List<FileData> attachments,List<string> to, List<string> toCC) {
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
            message.Subject = $"{title}-Purchase Request {(approved ? "Approved" : "Rejected")}";
            using var stream = new MemoryStream(htmlBody);
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();
            //html=html.Replace("<body>", "<body style=\"background-color: rgb(89, 174, 207);\">");
            if (approved && !string.IsNullOrWhiteSpace(url)) {
                html = html.Replace("{prLink}",$"<a href=\"{url}\">Order Link</a>");
            }
            var builder = new BodyBuilder { 
                HtmlBody = html
            };
            builder.Attachments.Add($"{title}-PR.pdf", prDoc);
            foreach (var attachment in attachments) { 
                builder.Attachments.Add(attachment.Name, attachment.Data);
            }
            message.Body = builder.ToMessageBody();
            await client.SendAsync(message);
        } catch (Exception ex) {
            this._logger.LogError("Mail Failed, Exception: \\n {ExMessage}", ex.Message);
        } finally {
            await client.DisconnectAsync(true);
        }
    }
    
    public async Task SendOrderEmail(byte[] htmlBody, string title, byte[] prDoc, List<FileData> attachments,List<string> to, List<string> toCC) {
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
            message.Subject = $"{title}-Purchase Request Ordered";
            using var stream = new MemoryStream(htmlBody);
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();
            var builder = new BodyBuilder { 
                HtmlBody = html
            };
            builder.Attachments.Add($"{title}-PR.pdf", prDoc);
            foreach (var attachment in attachments) { 
                builder.Attachments.Add(attachment.Name, attachment.Data);
            }
            message.Body = builder.ToMessageBody();
            await client.SendAsync(message);
        } catch (Exception ex) {
            this._logger.LogError("Mail Failed, Exception: \\n {ExMessage}", ex.Message);
        } finally {
            await client.DisconnectAsync(true);
        }
    }
    
    public async Task SendReceivedEmail(byte[] htmlBody, string title, byte[] prDoc,bool isPartial, List<FileData> attachments,List<string> to, List<string> toCC) {
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
            message.Subject = $"{title}-Purchase Request Delivery {(isPartial ? "Partial" : "Complete")}";
            using var stream = new MemoryStream(htmlBody);
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();
            var builder = new BodyBuilder { 
                HtmlBody = html
            };
            builder.Attachments.Add($"{title}-PR.pdf", prDoc);
            foreach (var attachment in attachments) { 
                builder.Attachments.Add(attachment.Name, attachment.Data);
            }
            message.Body = builder.ToMessageBody();
            await client.SendAsync(message);
        } catch (Exception ex) {
            this._logger.LogError("Mail Failed, Exception: \\n {ExMessage}", ex.Message);
        } finally {
            await client.DisconnectAsync(true);
        }
    }
    
    public async Task SendTestEmail(string to) {
        var client = new SmtpClient();
        try {
            client.CheckCertificateRevocation = false;
            client.ServerCertificateValidationCallback = CertValidationCallback;
            await client.ConnectAsync(this._emailSettings.ServerSettings?.Host ?? "10.92.3.215",
                this._emailSettings.ServerSettings?.Port ?? 25, false);
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Purchase Request", FromAddress));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = $"Test-Purchase Request";
            var builder = new BodyBuilder {
                HtmlBody = "<h1>Test Email</h1>"
            };
            message.Body = builder.ToMessageBody();
            await client.SendAsync(message);
        } catch (Exception ex) {
            this._logger.LogError("Mail Failed, Exception: \\n {ExMessage}", ex.Message);
        } finally {
            await client.DisconnectAsync(true);
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
}