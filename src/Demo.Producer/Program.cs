using System;
using System.Threading;
using Demo.Shared;
using RabbitMQ.Client;
using static System.Console;


WriteLine("##### PRODUCER #####");


const string EXCHANGE_NAME = "demo-exchange";
const string MESSAGE_TYPE = "demo-type-message";
const int TIME_PAUSE = 200;

var produceId = Guid.NewGuid();

var factory = new ConnectionFactory()
{
    HostName = "localhost"
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(
   exchange: EXCHANGE_NAME,
   type: ExchangeType.Topic,
   durable: true,
   autoDelete: false,
   arguments: null);


var messageId = 0;
while(true)
{
    try
    {
        var message = new Message
        {
            ProduceId = produceId,
            MessageId = ++messageId,
            Body = $"I am Nelson Nobre - {DateTime.UtcNow}"
        };

        var properties = MessageBus.CreateProperties(MESSAGE_TYPE);
        await channel.BasicPublishAsync(
            exchange: EXCHANGE_NAME,
            routingKey: MESSAGE_TYPE,
            basicProperties: properties,
            mandatory: true,
            body: message.Serialize());

        WriteLine("--> Sent {0}", message);

        if(TIME_PAUSE > 0)
        {
            WriteLine($"\tWaiting {TIME_PAUSE}ms...");
            Thread.Sleep(TIME_PAUSE);
        }
    }
    catch(Exception exception)
    {
        WriteLine($"Exception {exception.Message}");
    }
}
