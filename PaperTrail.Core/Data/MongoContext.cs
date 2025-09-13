using MongoDB.Driver;

namespace PaperTrail.Core.Data;

/// <summary>
/// Provides access to the MongoDB database used by the application.
/// </summary>
public class MongoContext
{
    private const string DatabaseName = "FIWB-PaperTrail";

    public IMongoDatabase Database { get; }

    public IMongoCollection<Models.Attachment> Attachments => Database.GetCollection<Models.Attachment>("Attachments");
    public IMongoCollection<Models.Contract> ImportedContracts => Database.GetCollection<Models.Contract>("ImportedContracts");
    public IMongoCollection<Models.Party> Parties => Database.GetCollection<Models.Party>("Parties");
    public IMongoCollection<Models.Contract> PreviousContracts => Database.GetCollection<Models.Contract>("PreviousContracts");
    public IMongoCollection<Models.Reminder> Reminders => Database.GetCollection<Models.Reminder>("Reminders");

    public MongoContext(IMongoClient client)
    {
        Database = client.GetDatabase(DatabaseName);
    }
}
