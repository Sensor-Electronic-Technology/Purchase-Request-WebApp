using Domain.PurchaseRequests.Model;
using Domain.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class DepartmentDataService {
    private readonly IMongoCollection<Department> _departmentCollection;
    
    public DepartmentDataService(IMongoClient client,IOptions<DatabaseSettings> options) {
        var database = client.GetDatabase(options.Value.PurchaseRequestDatabase ?? "purchase_req_db");
        this._departmentCollection = database.GetCollection<Department>(options.Value.DepartmentCollection ?? "departments");
    }
    
    public DepartmentDataService(IMongoClient client) {
        var database = client.GetDatabase("purchase_req_db");
        this._departmentCollection = database.GetCollection<Department>("departments");
    }
    
    public async Task InsertOne(Department department) {
        await this._departmentCollection.InsertOneAsync(department);
    }
    
    public async Task InsertMany(IList<Department> departments) {
        await this._departmentCollection.InsertManyAsync(departments);
    }
    
    public async Task<List<Department>> GetDepartments() {
        return await this._departmentCollection.Find(_=>true).ToListAsync();
    }
    
    public async Task<Department> FindDepartmentById(string name) {
        return await this._departmentCollection
            .Find(d=>d.Name == name)
            .FirstOrDefaultAsync();
    }
    
}