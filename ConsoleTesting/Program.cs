// See https://aka.ms/new-console-template for more information
using Domain.Users;
using MongoDB.Driver;
using SETiAuth.Domain.Shared.Constants;
using Infrastructure.Services;

//Console.WriteLine($"Address: {HttpClientConstants.LoginApiUrl}");
//await TestMongoIdString();
//await TestMongoQueryIdString();
await TestSendEmail();

async Task TestSendEmail() {
    EmailService emailService = new();
    await emailService.SendEmail(["rakesh@s-et.com"], ["aelmendorf@s-et.com","space@s-et.com"]);
}

async Task TestMongoIdString() {
    var client=new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<UserProfile>("user_profiles");
    UserProfile profile = new() { _id = "aelmendo", Email = "" };
    await collection.InsertOneAsync(profile);
    Console.WriteLine("Check database");
}

async Task TestMongoQueryIdString() {
    var client=new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<UserProfile>("user_profiles");
    collection.Find(e=>e._id=="aelmendo").ToList().ForEach(e=>Console.WriteLine(e._id));
}
