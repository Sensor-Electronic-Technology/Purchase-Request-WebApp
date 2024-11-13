using Webapp;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);
builder.AddBlazorComponents();
builder.AddPurchaseRequestWebApp();
var app = await builder.BuildApp();
app.Run();
