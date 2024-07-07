namespace Greenfield.Infrastructure.Hangfire;

/// <summary>
///     Represents a job queue.
/// </summary>
#pragma warning disable CA1711
public sealed record HangfireQueue
#pragma warning restore CA1711
{
    /// <summary>
    ///     Gets the priority queue. Jobs placed in this queue are processed first.
    /// </summary>
    public static readonly HangfireQueue CriticalPriority = new("critical");
    
    /// <summary>
    ///     Gets the default queue.
    /// </summary>
    public static readonly HangfireQueue Default = new("default");
    
    /// <summary>
    ///     Gets the low priority queue. Jobs placed in this queue are processed last.
    /// </summary>
    public static readonly HangfireQueue LowPriority = new("low");
    
    private readonly string _queueName;
    
    private HangfireQueue(string queueName)
    {
        _queueName = queueName;
    }
    
    public static implicit operator string(HangfireQueue? queue) => queue?._queueName ?? Default;
    
    /// <summary>
    ///     Returns a collection of available queues.
    /// </summary>
    /// <returns>An array of queues.</returns>
    public static string[] GetAll()
    {
        return [CriticalPriority.ToString(), Default.ToString(), LowPriority.ToString()];
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        return _queueName;
    }
}