// See https://aka.ms/new-console-template for more information


using System.DirectoryServices.AccountManagement;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Authentication;
using Domain.Contracts;
using Domain.Settings;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;

//await CreateSettings();
//await RoleAccountTesting();
await TestLoginApi();

async Task TestLoginApi() {
	var client = new HttpClient();
	//client.BaseAddress = new Uri("http://172.20.4.20:5000/api");
	
	var response = await client.PostAsJsonAsync("http://172.20.4.20:5000/api/login",new LoginRequest(){
			Username = "aelmendo", 
			Password = "Drizzle234!",
			IsDomainUser = true 
	}); 
	Console.WriteLine(response.StatusCode);
	var session=await response.Content.ReadFromJsonAsync<LoginResponse>();
	Console.WriteLine(JsonSerializer.Serialize<UserSession>(session?.UserSession, new JsonSerializerOptions() { WriteIndented = true }));
}


async Task RoleAccountTesting() {
	var client=new MongoClient("mongodb://172.20.3.41:27017");
	var database=client.GetDatabase("auth_db");
	var collection=database.GetCollection<UserAccount>("user_accounts");
	UserAccount account = new UserAccount() {
		Username = "aelmendorf",
		Email = "aelmendorf@seti.com",
		Role = Role.User.Name,
		IsDomainAccount = true
	};
	await collection.InsertOneAsync(account);
	Console.WriteLine("Account inserted, check database");
}

async Task CreateSettings() {
	var client=new MongoClient("mongodb://172.20.3.41:27017");
	var database=client.GetDatabase("settings_db");
	var collection=database.GetCollection<LoginServerSettings>("login_settings");
	var settings = new LoginServerSettings() {
		HostIp = "172.20.3.5",
		_id = ObjectId.GenerateNewId(),
		IsLatest = true,
		UserName = "elmendorfal@seti.com",
		Password = "!23seti"
	};
	await collection.InsertOneAsync(settings);
	Console.WriteLine("Settings created, check database");
}

void LoginTest() {
	using (PrincipalContext context = new PrincipalContext(ContextType.Domain, "172.20.3.5", "elmendorfal@seti.com", "!23seti")) {
		UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, "aelmendo");
		if (user != null) {
			Console.WriteLine($"User: {user.SamAccountName} - {user.DisplayName} - {user.EmailAddress} - {user.UserPrincipalName}");
			if (context.ValidateCredentials("aelmendo@seti.com", "Drizzle123!")) {
				Console.WriteLine("User authenticated");
			} else {
				Console.WriteLine("Authentication failed");
			}
		} else {
			Console.WriteLine("User is null");
		}
	}
}


			
