using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NServiceBus;

class Program
{
    static async Task Main()
    {
        Console.Title = "Bridge";

        var builder = Host.CreateApplicationBuilder();
        var bridgeConfiguration = new BridgeConfiguration();

        #region create-asb-endpoint-of-bridge
        var sqsBridgeEndpoint = new BridgeEndpoint("Samples.MessagingBridge.SqsEndpoint");
        #endregion

        #region asb-subscribe-to-event-via-bridge
        sqsBridgeEndpoint.RegisterPublisher<MyEvent>("Samples.MessagingBridge.MsmqEndpoint");
        #endregion

        #region asb-bridge-configuration
        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        var region = Environment.GetEnvironmentVariable("AWS_REGION");
        var connectionString = $"AccessKey={accessKey};SecretKey={secretKey};Region={region}";

        var sqsTransport = new SqsTransport()
        {
            S3 = new S3Settings("nservicebussqstest123", connectionString)
        };

        var sqsBridgeTransport = new BridgeTransport(sqsTransport)
        {
            AutoCreateQueues = true
        };

        sqsBridgeTransport.HasEndpoint(sqsBridgeEndpoint);
        bridgeConfiguration.AddTransport(sqsBridgeTransport);
        #endregion

        #region create-msmq-endpoint-of-bridge
        var msmqBridgeEndpoint = new BridgeEndpoint("Samples.MessagingBridge.MsmqEndpoint");
        #endregion

        #region msmq-subscribe-to-event-via-bridge
        msmqBridgeEndpoint.RegisterPublisher<OtherEvent>("Samples.MessagingBridge.SqsEndpoint");
        #endregion

        #region msmq-bridge-configuration
        var msmqBridgeTransport = new BridgeTransport(new MsmqTransport())
        {
            AutoCreateQueues = true
        };

        msmqBridgeTransport.HasEndpoint(msmqBridgeEndpoint);
        bridgeConfiguration.AddTransport(msmqBridgeTransport);
        #endregion

        builder.UseNServiceBusBridge(bridgeConfiguration);

        var host = builder.Build();
        await host.RunAsync();
    }
}