using System;

namespace AbstractPixel.Core
{
    [Flags]
    public enum PositionConstraints
    {
        None = 0,
        FreezeX = 1 << 0,
        FreezeY = 1 << 1,
        FreezeZ = 1 << 2
    }

    [Flags]
    public enum RotationConstraints
    {
        None = 0,
        FreezeX = 1 << 3,
        FreezeY = 1 << 4,
        FreezeZ = 1 << 5
    }

    public enum ConstraintLockMode
    {
        Permanent,
        Timed
    }
}