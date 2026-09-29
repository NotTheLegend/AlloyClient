using System;
using System.Collections.Generic;
using System.Numerics;

namespace AlloyClient.Utils;

/// <summary>
/// Histogram resolution. Each value is the number of sub-bucket bits (2^bits sub-buckets per
/// power of two). Worst-case relative error of a reported percentile is 1 / 2^(bits+1).
/// </summary>
public enum HistogramPrecision {
    /// <summary>~1.6% error.</summary>
    Low = 5,

    /// <summary>~0.39% error.</summary>
    Normal = 7,

    /// <summary>~0.1% error.</summary>
    High = 9,

    /// <summary>~0.024% error.</summary>
    VeryHigh = 11,
}

public sealed class FrameHistogram {

    public readonly record struct Stats(int Fps, double AvgMs, double P90Ms, double P99Ms, double MaxMs);

    private const double NsPerMs = 1_000_000.0;

    private readonly int _windowSeconds;
    private readonly double _windowMs;
    private readonly Queue<double> _samples;
    private readonly int[] _buckets;
    private readonly int _subBits;
    private readonly long _subCount;
    private readonly int _overflow;
    private readonly double _maxBucketMs;

    private double _sampleSum;

    public FrameHistogram(int windowSeconds, int startingCapacity, HistogramPrecision precision = HistogramPrecision.Normal, double maxBucketMs = 1_000d) {
        _windowSeconds = windowSeconds;
        _windowMs = windowSeconds * 1000;
        _subBits = (int)precision;
        _subCount = 1L << _subBits;
        _overflow = IndexOf((long)(maxBucketMs * NsPerMs)) + 1;
        _maxBucketMs = maxBucketMs;

        _samples = new Queue<double>(startingCapacity);
        _buckets = new int[_overflow + 1];
    }

    public void Add(double frameMs) {
        _samples.Enqueue(frameMs);
        _sampleSum += frameMs;
        _buckets[BucketOf(frameMs)]++;

        while (_sampleSum > _windowMs && _samples.Count > 1) {
            var dequeue = _samples.Dequeue();
            _sampleSum -= dequeue;
            _buckets[BucketOf(dequeue)]--;
        }
    }

    public Stats Get() {
        var count = _samples.Count;
        if (count == 0) {
            return default;
        }

        var top90 = Math.Min((int)Math.Ceiling(count * 0.90), count);
        var top99 = Math.Min((int)Math.Ceiling(count * 0.99), count);

        double p90 = 0, p99 = 0;
        bool have90 = false, have99 = false;
        var maxBucket = 0;
        var seen = 0;

        for (var i = 0; i < _buckets.Length; i++) {
            if (_buckets[i] == 0) {
                continue;
            }

            seen += _buckets[i];

            if (!have90 && seen >= top90) {
                p90 = ValueOf(i);
                have90 = true;
            }

            if (!have99 && seen >= top99) {
                p99 = ValueOf(i);
                have99 = true;
            }

            if (seen == count) {
                maxBucket = i;
                break;
            }
        }

        return new Stats(count / _windowSeconds, _sampleSum / count, p90, p99, ValueOf(maxBucket));
    }

    private int BucketOf(double ms) {
        // NaN and negatives fall into bucket 0
        var ns = ms > 0 ? (long)Math.Min(ms * NsPerMs, long.MaxValue / 2d) : 0;
        return Math.Min(IndexOf(ns), _overflow);
    }

    private int IndexOf(long ns) {
        if (ns < (_subCount << 1)) {
            return (int)ns;
        }

        var msb = 63 - BitOperations.LeadingZeroCount((ulong)ns);
        var shift = msb - _subBits;
        return (int)(((long)(shift + 1) << _subBits) + ((ns >> shift) - _subCount));
    }

    private double ValueOf(int bucket) {
        if (bucket >= _overflow) {
            return _maxBucketMs;
        }

        long lower, width;
        if (bucket < (_subCount << 1)) {
            lower = bucket;
            width = 1;
        } else {
            var shift = (bucket >> _subBits) - 1;
            lower = (_subCount + (bucket & (_subCount - 1))) << shift;
            width = 1L << shift;
        }

        return (lower + width * 0.5) / NsPerMs;
    }
}