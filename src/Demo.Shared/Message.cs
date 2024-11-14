using System;

namespace Demo.Shared;

public record Message
{
    public required Guid ProduceId { get; init; }
    public required int MessageId { get; init; }
    public required string? Body { get; init; }
}
