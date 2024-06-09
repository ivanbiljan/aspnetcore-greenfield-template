using System.ComponentModel;
using Hangfire;
using Hangfire.States;
using MediatR;

namespace Greenfield.Infrastructure.Hangfire;

public sealed class MediatorWrapper(IMediator mediator)
{
    [DisplayName("{0}")]
    public async Task Send(string displayName, IRequest request)
    {
        await mediator.Send(request);
    }
    
    [DisplayName("{0}")]
    public async Task Send<TResponse>(string displayName, IRequest<TResponse> request)
    {
        await mediator.Send(request);
    }
}

/// <summary>
///     Provides extension methods for the <see cref="IMediator" /> type.
/// </summary>
public static class IMediatorExtensions
{
    /// <summary>
    ///     Enqueues a Hangfire job that will handle the provided mediator request.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator" /> instance used to queue the request.</param>
    /// <param name="displayName">The name used to display the job in Hangfire.</param>
    /// <param name="request">The request.</param>
    /// <param name="queue">
    ///     The <see cref="HangfireQueue" />. Jobs are placed into the default queue if <see langword="null" />
    ///     .
    /// </param>
    /// <param name="enqueueAt">The <see cref="DateTime" /> when the job will be enqueued.</param>
    public static void Enqueue(
        this IMediator mediator,
        string displayName,
        IRequest request,
        HangfireQueue? queue = null,
        DateTime? enqueueAt = null
    )
    {
        queue ??= HangfireQueue.Default;
        var backgroundJobClient = new BackgroundJobClient();
        if (enqueueAt is not null)
        {
            backgroundJobClient.Schedule<MediatorWrapper>(
                queue,
                wrapper => wrapper.Send(displayName, request),
                enqueueAt.Value - DateTime.UtcNow
            );
            
            return;
        }
        
        backgroundJobClient.Create<MediatorWrapper>(
            wrapper => wrapper.Send(displayName, request),
            new EnqueuedState(queue)
        );
    }
    
    /// <summary>
    ///     Enqueues a Hangfire job that will handle the provided request.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator" /> instance used to queue the request.</param>
    /// <param name="displayName">The name used to display the job in Hangfire.</param>
    /// <param name="request">The request.</param>
    /// <param name="queue">
    ///     The <see cref="HangfireQueue" />. Jobs are placed into the default queue if <see langword="null" />
    ///     .
    /// </param>
    /// <param name="enqueueAt">The <see cref="DateTime" /> when the job will be enqueued.</param>
    /// <typeparam name="TResponse">The type of response, as indicated by <paramref name="request" />.</typeparam>
    public static void Enqueue<TResponse>(
        this IMediator mediator,
        string displayName,
        IRequest<TResponse> request,
        HangfireQueue? queue = null,
        DateTime? enqueueAt = null
    )
    {
        queue ??= HangfireQueue.Default;
        var backgroundJobClient = new BackgroundJobClient();
        if (enqueueAt is not null)
        {
            backgroundJobClient.Schedule<MediatorWrapper>(
                queue,
                wrapper => wrapper.Send(displayName, request),
                enqueueAt.Value - DateTime.UtcNow
            );
            
            return;
        }
        
        backgroundJobClient.Create<MediatorWrapper>(
            wrapper => wrapper.Send(displayName, request),
            new EnqueuedState(queue)
        );
    }
}