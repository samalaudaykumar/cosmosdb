using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using dotenv.net;

public class Program
{
    public static async Task Main(string[] args)
    {
        string databaseName = "myDatabase";
        string containerName = "myContainer";

        // Load environment variables
        DotEnv.Load();
        var envVars = DotEnv.Read();

        string cosmosDbAccountUrl = envVars["DOCUMENT_ENDPOINT"];
        string accountKey = envVars["ACCOUNT_KEY"];

        if (string.IsNullOrEmpty(cosmosDbAccountUrl) || string.IsNullOrEmpty(accountKey))
        {
            Console.WriteLine("Please set the DOCUMENT_ENDPOINT and ACCOUNT_KEY environment variables.");
            return;
        }

        // Create Cosmos client
        CosmosClient client = new CosmosClient(
            accountEndpoint: cosmosDbAccountUrl,
            authKeyOrResourceToken: accountKey
        );

        try
        {
            // Create database if not exists
            Database database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            Console.WriteLine($"Created or retrieved database: {database.Id}");

            // Create container if not exists
            Container container = await database.CreateContainerIfNotExistsAsync(
                id: containerName,
                partitionKeyPath: "/id"
            );
            Console.WriteLine($"Created or retrieved container: {container.Id}");

            // Create item
            Product newItem = new Product
            {
                id = Guid.NewGuid().ToString(),
                name = "Sample Item",
                description = "This is a sample item in my Azure Cosmos DB exercise."
            };

            // Insert item
            ItemResponse<Product> createResponse = await container.CreateItemAsync(
                item: newItem,
                partitionKey: new PartitionKey(newItem.id)
            );

            Console.WriteLine($"Created item with ID: {createResponse.Resource.id}");
            Console.WriteLine($"Request charge: {createResponse.RequestCharge} RUs");
        }
        catch (CosmosException ex)
        {
            Console.WriteLine($"Cosmos DB Error: {ex.StatusCode} - {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Error: {ex.Message}");
        }
    }
}

// Product model
public class Product
{
    public string? id { get; set; }
    public string? name { get; set; }
    public string? description { get; set; }
}
