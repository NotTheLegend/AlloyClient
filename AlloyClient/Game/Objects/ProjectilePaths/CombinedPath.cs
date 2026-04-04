#region

using System.Linq;
using OpenTK.Mathematics;
using AlloyClient.Networking;

#endregion

namespace AlloyClient.Game.Objects.ProjectilePaths;

public class CombinedPath : ProjectilePathSegment
{
    private ProjectilePathSegment[] _segments;

    public CombinedPath() : base(PathType.CombinedPath) { }

    public CombinedPath(int? timeOffset = null, params ProjectilePathSegment[] segments)
        : base(PathType.CombinedPath, 0, timeOffset: timeOffset)
    {
        _segments = segments;

        _lifetimeMs = segments.Max(i => i.TimeOffset + i.LifetimeMs);
    }

    public override Vector2 PositionAt(float elapsedLifetimeMs)
    {
        var p = Vector2.Zero;
        if (TimeOffset > 0 && elapsedLifetimeMs < TimeOffset)
            return p;

        elapsedLifetimeMs -= TimeOffset;

        ApplyModifiers(ref elapsedLifetimeMs);

        var deltaX = 0f;
        var deltaY = 0f;

        var count = 0;
        foreach (var segment in _segments)
        {
            if (segment.TimeOffset > 0 && elapsedLifetimeMs < segment.TimeOffset)
                continue;

            var segmentOffset = segment.PositionAt(elapsedLifetimeMs);
            deltaX += segmentOffset.X;
            deltaY += segmentOffset.Y;
            count++;
        }

        p.X = deltaX / count; // Return average deltaX and deltaY
        p.Y = deltaY / count;
        return p;
    }

    public override void Read(ref SpanReader rdr)
    {
        _segments = new ProjectilePathSegment[rdr.ReadByte()];
        for (var i = 0; i < _segments.Length; i++) {
            _segments[i] = ReadNew(ref rdr);
        }

        TimeOffset = rdr.ReadInt32();
        _mods = rdr.ReadInt32();
    }
    
    public override void SetInfo(ProjectileInfo info)
    {
        base.SetInfo(info);
        foreach (var segment in _segments)
        {
            segment.SetInfo(info);
        }
    }
    
    public override ProjectilePathSegment Clone()
    {
        return new CombinedPath(TimeOffset, (ProjectilePathSegment[])_segments.Clone());
    }
}