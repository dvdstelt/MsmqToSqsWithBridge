using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using NServiceBus;

class Program
{
    static async Task Main()
    {
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var random = new Random();

        Console.Title = "SqsEndpoint";
        var endpointConfiguration = new EndpointConfiguration("Samples.MessagingBridge.SqsEndpoint");
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.UsePersistence<NonDurablePersistence>();

        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        var region = Environment.GetEnvironmentVariable("AWS_REGION");
        var connectionString = $"AccessKey={accessKey};SecretKey={secretKey};Region={region}";
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new Exception("Could not read the 'AzureServiceBus_ConnectionString' environment variable. Check the sample prerequisites.");
        }
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        var sqsTransport = new SqsTransport()
        {
            S3 = new S3Settings("nservicebussqstest123", connectionString)
        };
        endpointConfiguration.UseTransport(sqsTransport);

        var sendOptions = new SendOptions();
        sendOptions.SetDestination("Samples.MessagingBridge.MsmqEndpoint");

        var endpointInstance = await Endpoint.Start(endpointConfiguration);

        Console.WriteLine("Press Enter to send a command");
        Console.WriteLine("Press any other key to exit");

        while (true)
        {
            var key = Console.ReadKey().Key;
            if (key != ConsoleKey.Enter)
            {
                break;
            }

            var prop = new string(Enumerable.Range(0, 3).Select(i => letters[random.Next(letters.Length)]).ToArray());
            await endpointInstance.Send(new MyCommand { Property = prop }, sendOptions);
            Console.WriteLine($"\nCommand with value '{prop}' sent");
        }

        await endpointInstance.Stop();
    }
}
