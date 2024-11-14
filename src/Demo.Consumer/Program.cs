using System;
using Demo.Shared;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static System.Console;


WriteLine("##### CONSUMER #####");


const string EXCHANGE_NAME = "demo-exchange";
const string QUEUE_NAME = "demo-queue";
const string MESSAGE_TYPE = "demo-type-message";


var factory = new ConnectionFactory()
{
    HostName = "localhost",
    AutomaticRecoveryEnabled = true
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(
    exchange: EXCHANGE_NAME,
    type: ExchangeType.Topic,
    durable: true,
    autoDelete: false,
    arguments: null);

await channel.QueueDeclareAsync(
    queue: QUEUE_NAME,
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

// Attach Queue to Exchange
await channel.QueueBindAsync(
    queue: QUEUE_NAME,
    exchange: EXCHANGE_NAME,
    routingKey: MESSAGE_TYPE);

// Set QoS
await channel.BasicQosAsync(
    0,
    1, // Total message receive same time
    false); // [ false per consumer | true per channel ]

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (sender, eventArgs) =>
{
    try
    {
        var message = eventArgs.Deserialize<Message>()
            ?? throw new NotSupportedException("The message is not supported by consummer");

        WriteLine($"Received[{eventArgs.GetMessageType()}] {message}");
        await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
    }
    catch(Exception exception)
    {
        await channel.BasicNackAsync(eventArgs.DeliveryTag, false, true);
        WriteLine($"Exception {exception.Message}");
    }
};



await channel.BasicConsumeAsync(
    queue: QUEUE_NAME,
    autoAck: false,
    consumer: consumer);

WriteLine("Consumer runing...");
ReadLine();
