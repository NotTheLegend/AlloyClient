using System.Collections.Generic;
using AlloyClient.Networking;
using OpenTK.Mathematics;

namespace AlloyClient.Game.Objects.ProjectilePaths;

public class ProjectilePath
{
    private readonly List<ProjectilePathSegment> projectilePathSegments = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProjectilePath" /> class.
    /// </summary>
    public ProjectilePath()
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProjectilePath" /> class.
    /// </summary>
    /// <param name="baseSegment">Starting path segment/</param>
    public ProjectilePath(int lifetimeMs, ProjectilePathSegment baseSegment)
    {
        baseSegment.LifetimeMs = lifetimeMs;
        projectilePathSegments.Add(baseSegment);
        _lifetimeMs += lifetimeMs;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProjectilePath" /> class.
    /// </summary>
    /// <param name="projectilePathSegments">Collection of segments.</param>
    public ProjectilePath(List<ProjectilePathSegment> projectilePathSegments)
    {
        foreach (var segment in projectilePathSegments)
        {
            this.projectilePathSegments.Add(segment.Clone());
            _lifetimeMs += segment.LifetimeMs;
        }
    }

    /// <summary>
    ///     Gets the number of segments in this path.
    /// </summary>
    public int SegmentCount => projectilePathSegments.Count;

    private int _lifetimeMs = 0;
    /// <summary>
    ///     Gets the total lifetime of this path.
    /// </summary>
    public int LifetimeMs => _lifetimeMs;

    /// <summary>
    ///     Gets or sets the projectile info for this path.
    /// </summary>
    public ProjectileInfo Info { get; private set; }

    /// <summary>
    ///     Register a segment to this path.
    /// </summary>
    /// <param name="segment">Segment.</param>
    public void RegisterSegment(ProjectilePathSegment segment)
    {
        projectilePathSegments.Add(segment);
        _lifetimeMs += segment.LifetimeMs;
    }

    /// <summary>
    ///     Calculate the position offset of a projectile on this path based on the time since the projectile started.
    /// </summary>
    /// <param name="relativeElapsed">Time since the projectile started</param>
    /// <returns>Position offset of the projectile from its start position.</returns>
    public Vector2 PositionAt(float relativeElapsed)
    {
        var segmentEnd = 0;
        var segmentsTotal = 0f;
        var startPos = Vector2.Zero; // Origin
        foreach (var segment in projectilePathSegments)
        {
            segmentEnd += segment.LifetimeMs;
            if (relativeElapsed <= segmentEnd)
            {
                var ret = segment.PositionAt(relativeElapsed - segmentsTotal); // Position offset relative to the segment start
                // Console.WriteLine($"Segment pos: {ret} ({segment.Type}: {segment.Speed}, {segment.LifetimeMs}, {segment.Angle})");
                return startPos + ret; // Position offset relative to the path start
            }

            startPos += segment.PositionAtEnd();
            segmentsTotal += segment.LifetimeMs;
        }

        return Vector2.Zero;
    }

    /// <summary>
    ///     Clones itself into a new instance. Required since a behavior references a path, and then that path will be reused,
    ///     so we need a new instance.
    /// </summary>
    /// <returns>A new instance of the path with the same segments.</returns>
    public ProjectilePath Clone()
    {
        return new ProjectilePath(projectilePathSegments);
    }

    public void SetInfo(ProjectileInfo info) {
        Info = info;
        foreach (var segment in projectilePathSegments) {
            _lifetimeMs -= segment.LifetimeMs;
            segment.SetInfo(info);
            _lifetimeMs += segment.LifetimeMs;
        }
    }

    public static ProjectilePath Read(ref SpanReader rdr) {
        var ret = new ProjectilePath();
        var segmentCount = rdr.ReadInt32();
        for (var i = 0; i < segmentCount; i++) {
            var segment = ProjectilePathSegment.ReadNew(ref rdr);
            ret.RegisterSegment(segment);
        }

        return ret;
    }
}

public enum PathType : byte
{
    LinePath,
    WavyPath,
    CirclePath,
    AmplitudePath,
    BoomerangPath,
    AcceleratePath,
    DeceleratePath,
    ChangeSpeedPath,
    CombinedPath
}