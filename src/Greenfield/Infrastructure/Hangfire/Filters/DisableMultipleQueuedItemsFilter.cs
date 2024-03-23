using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;

namespace Greenfield.Infrastructure.Hangfire.Filters;

/// <summary>
///     Represents an <see cref="IServerFilter" /> that prevents executing the annotated job if one with the same
///     parameters is already running.
/// </summary>
public sealed class DisableMultipleQueuedItemsAttribute : JobFilterAttribute, IServerFilter
{
    private const string MetadataKey = "Metadata";
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan FingerprintTimeout = TimeSpan.FromHours(1);

    public void OnPerformed(PerformedContext filterContext)
    {
        RemoveFingerprint(filterContext.Connection, filterContext.BackgroundJob.Job);
    }

    public void OnPerforming(PerformingContext filterContext)
    {
        if (TryAddFingerprintIfNotExists(filterContext.Connection, filterContext.BackgroundJob.Job))
        {
            return;
        }

        filterContext.Canceled = true;
    }

    private static void RemoveFingerprint(IStorageConnection connection, Job job)
    {
        RemoveFingerprint(connection, GetFingerprintKey(job));
    }

    private static void RemoveFingerprint(IStorageConnection connection, string key)
    {
        var lockFingerprint = GetFingerprintLockKey(key);
        using (connection.AcquireDistributedLock(lockFingerprint, LockTimeout))
        {
            using (var transaction = connection.CreateWriteTransaction())
            {
                transaction.RemoveHash(key);
                transaction.Commit();
            }
        }
    }

    private static string GetFingerprint(Job job)
    {
        if (job.Type is null || job.Method is null)
        {
            return string.Empty;
        }

        var parameters = job.Args.Count == 0 ? string.Empty : string.Join(".", job.Args);

        return $"{job.Type.Name}.{job.Method.Name}.{parameters}";
    }

    private static string GetFingerprintKey(Job job)
    {
        var fingerprint = GetFingerprint(job);

        return $"Fingerprint:{GetSha256(fingerprint)}";
    }

    private static string GetFingerprintLockKey(string key)
    {
        return $"{key}:lock";
    }

    private static string GetSha256(string data)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        var calculatedSignature = BitConverter
            .ToString(hash)
            .Replace("-", string.Empty)
            .ToLowerInvariant();

        return calculatedSignature;
    }

    private static bool TryAddFingerprintIfNotExists(IStorageConnection connection, Job job)
    {
        return TryAddFingerprintIfNotExists(
            connection,
            GetFingerprintKey(job),
            GetFingerprint(job)
        );
    }

    private static bool TryAddFingerprintIfNotExists(IStorageConnection connection, string key, string jobData)
    {
        var lockKey = GetFingerprintLockKey(key);

        using (connection.AcquireDistributedLock(lockKey, LockTimeout))
        {
            var fingerprint = connection.GetAllEntriesFromHash(key);

            if (fingerprint != null &&
                fingerprint.TryGetValue(MetadataKey, out var value) &&
                DateTimeOffset.TryParse(
                    value[..fingerprint[MetadataKey].IndexOf(' ')],
                    null,
                    DateTimeStyles.RoundtripKind,
                    out var timestamp
                ) &&
                DateTimeOffset.UtcNow <= timestamp.Add(FingerprintTimeout))
            {
                return false;
            }

            connection.SetRangeInHash(
                key,
                new Dictionary<string, string>
                {
                    [MetadataKey] = $"{DateTimeOffset.UtcNow:o} {jobData}"
                }
            );

            return true;
        }
    }
}