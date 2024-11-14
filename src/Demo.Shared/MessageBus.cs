using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Demo.Shared;

public static class MessageBus
{
    public static BasicProperties CreateProperties(string messageType)
        => new BasicProperties()
            .SetAppId()
            .SetCorrelationId()
            .SetMessageId()
            .SetContentTypeJson()
            .SetEncodingUTF8()
            .SetMessageType(messageType);

    public static BasicProperties SetAppId(this BasicProperties properties, string? appId = null)
    {
        if(string.IsNullOrWhiteSpace(appId))
        {
            appId = Assembly.GetEntryAssembly()?.GetName()?.Name;
        }

        properties.AppId = appId;

        return properties;
    }

    public static BasicProperties SetMessageId(this BasicProperties properties, string? messageId = null)
    {
        properties.MessageId = messageId ?? Guid.NewGuid().ToString();
        return properties;
    }

    public static BasicProperties SetCorrelationId(this BasicProperties properties, string? correlationId = null)
    {
        if(string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Activity.Current?.Id;

            if(string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = $"guid:{Guid.NewGuid()}";
            }
        }

        properties.CorrelationId = correlationId;

        return properties;
    }

    public static BasicProperties SetContentTypeJson(this BasicProperties properties)
    {
        properties.ContentType = MediaTypeNames.Application.Json;
        return properties;
    }

    public static BasicProperties SetMessageType(this BasicProperties properties, string messageType)
    {
        properties.Type = messageType;
        return properties;
    }

    public static string? GetMessageType(this BasicDeliverEventArgs args)
        => args.BasicProperties?.Type;

    public static BasicProperties SetEncodingUTF8(this BasicProperties properties)
    {
        properties.ContentEncoding = Encoding.UTF8.BodyName;
        return properties;
    }


    public static ReadOnlyMemory<byte> Serialize(this object message)
    {
        var payload = JsonSerializer.Serialize(message);
        return new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(payload));
    }

    public static Encoding GetEncoding(this BasicDeliverEventArgs args)
    {
        if(string.IsNullOrWhiteSpace(args.BasicProperties?.ContentEncoding))
        {
            return Encoding.UTF8;
        }

        return Encoding.GetEncoding(args.BasicProperties.ContentEncoding);
    }

    public static TEvent? Deserialize<TEvent>(this BasicDeliverEventArgs args)
    {
        var encoding = args.GetEncoding();
        var body = args.Body.ToArray();
        var payload = encoding.GetString(body);

        return JsonSerializer.Deserialize<TEvent>(payload);
    }
}
