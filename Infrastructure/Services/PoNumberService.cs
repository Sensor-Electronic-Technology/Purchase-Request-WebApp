using Domain.PurchaseRequests.Model;
using Domain.Settings;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class PoNumberService {
    private readonly IMongoCollection<PoNumber> _poNumberCollection;
    private readonly ILogger<PoNumberService> _logger;
    
    public PoNumberService(IMongoClient client,DatabaseSettings settings,ILogger<PoNumberService> logger) {
        var database = client.GetDatabase(settings.PurchaseRequestDatabase ?? "purchase_req_db");
        this._poNumberCollection = database.GetCollection<PoNumber>(settings.PoNumberCollection ?? "po_numbers");
        this._logger = logger;
    }
    
    public async Task<PoNumber?> GetNextPoNumber(string initials, int year, ObjectId requestId) {
        var existing = await this.GetRequestPoNumber(requestId);
        if(existing!=null) {
            return existing;
        }
        var result=await this._poNumberCollection.Find(_ => true)
            .Sort(Builders<PoNumber>.Sort
                .Descending(f=>f.Seq))
            .Limit(1)
            .FirstOrDefaultAsync();
        if (result == null) {
            this._logger.LogError("Error: No Po Numbers Found, this indicated a problem with the database");
            return null;
        }
        PoNumber insert = new() { Initials = initials, Year = year, Seq = result.Seq + 1, RequestId = requestId };
        if(insert.Seq/1000>0) {
            insert._id=$"{insert.Year}-{insert.Initials}-{insert.Seq}";
        } else {
            if(insert.Seq/100>0) {
                insert._id=$"{insert.Year}-{insert.Initials}-0{insert.Seq}";
            } else {
                insert._id = insert.Seq/10>0 ? 
                    $"{insert.Year}-{insert.Initials}-00{insert.Seq}" : 
                    $"{insert.Year}-{insert.Initials}-000{insert.Seq}";
            }
        }
        await this._poNumberCollection.InsertOneAsync(insert);
        var filter = Builders<PoNumber>.Filter.Eq(e => e._id, insert._id);
        return await this._poNumberCollection.Find(filter).FirstOrDefaultAsync();
    }
    
    public async Task<PoNumber?> GetRequestPoNumber(ObjectId requestId) {
        return await this._poNumberCollection.Find(e => e.RequestId == requestId).FirstOrDefaultAsync();
    }
    
    public async Task<bool> DeletePoNumber(string poNumber) {
        var filter = Builders<PoNumber>.Filter.Eq(e => e._id, poNumber);
        var result = await this._poNumberCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }
}