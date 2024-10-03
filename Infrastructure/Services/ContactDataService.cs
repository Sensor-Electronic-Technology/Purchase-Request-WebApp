using Domain.PurchaseRequests.Model;
using Domain.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class ContactDataService {
    private readonly IMongoCollection<Contact> _contactCollection;
    
    public ContactDataService(IMongoClient client, IOptions<DatabaseSettings> options) {
        var database = client.GetDatabase(options.Value.PurchaseRequestDatabase ?? "purchase_req_db");
        this._contactCollection = database.GetCollection<Contact>(options.Value.ContactCollection ?? "contacts");
    }
    
    public ContactDataService(IMongoClient client) {
        var database = client.GetDatabase("purchase_req_db");
        this._contactCollection = database.GetCollection<Contact>("contacts");
    }
    
    public async Task InsertOne<T>(T contact) where T:Contact {
        contact._id=ObjectId.GenerateNewId();
        await this._contactCollection.InsertOneAsync(contact);
    }
    
    public async Task<T> InsertOneV2<T>(T contact) where T:Contact {
        contact._id=ObjectId.GenerateNewId();
        await this._contactCollection.InsertOneAsync(contact);
        return contact;
    }
    
    public async Task InsertMany<T>(IList<T> contacts) where T:Contact {
        await this._contactCollection.InsertManyAsync(contacts);
    }
    
    public async Task<bool> Update<T>(T contact) where T:Contact {
        var result=await this._contactCollection.OfType<T>().ReplaceOneAsync(e=>e._id == contact._id,contact);
        if (result.IsAcknowledged) {
            return result.ModifiedCount>0;
        }
        return false;
    }
    
    public async Task<List<Vendor>> GetVendors() {
        return await this._contactCollection.OfType<Vendor>().Find(_=>true).ToListAsync();
    }

    public async Task<InternalContact> GetInternalContact(InternalContactType type) {
        return await this._contactCollection.OfType<InternalContact>().Find(ic => ic.Type == type).FirstOrDefaultAsync();
    }
}