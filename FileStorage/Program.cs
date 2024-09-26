using FileStorage.Model;
using FileStorage.Services;
using Infrastructure.SignatureVerify;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

/*builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("https://localhost:7295",
                    "https://localhost:3000", "http://localhost:3000").AllowAnyOrigin().AllowAnyHeader()
                .AllowAnyMethod();
        });
});*/
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetConnectionString("DefaultConnection") 
                                                            ?? "mongodb://172.20.3.41:27017"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<UploadFileSettings>((serviceProvider) =>
    builder.Configuration.GetSection(nameof(UploadFileSettings)).Get<UploadFileSettings>());

// Add services to the container.
builder.Services.AddSingleton<DatabaseSettings>((serviceProvider) =>
    builder.Configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>());

builder.Services.AddSingleton<FileTypeVerifier>();
builder.Services.AddSingleton<FileValidationService>();
builder.Services.AddSingleton<FileStorageService>();
builder.Services.AddHostedService<AutoDeleteService>(); // Auto delete temp file

var app = builder.Build();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}*/

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();
app.Run();