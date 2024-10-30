using Domain.PurchaseRequests.Model;
using Domain.Settings;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class PoNumberService {
    private readonly IMongoCollection<PoNumber> _poNumberCollection;
    
    public PoNumberService(IMongoClient client,DatabaseSettings settings) {
        var database = client.GetDatabase("purchasing");
        this._poNumberCollection = database.GetCollection<PoNumber>(settings.PoNumberCollection ?? "po_numbers");
    }
    
    public async Task<PoNumber?> GetNextPoNumber(string initials, int year) {
        var result=await this._poNumberCollection.Find(_ => true)
            .Project(e=>e.Seq)
            .Sort(Builders<PoNumber>.Sort
                .Descending(f=>f.Seq))
            .Limit(1)
            .FirstOrDefaultAsync();
        if (result>0) {
            PoNumber insert = new() { Initials = initials,Year = year };
            insert.Seq = result + 1;
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
        Console.WriteLine("No PoNumber found");
        return null;
    }
    
    public async Task<bool> DeletePoNumber(string poNumber) {
        var filter = Builders<PoNumber>.Filter.Eq(e => e._id, poNumber);
        var result = await this._poNumberCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }
}