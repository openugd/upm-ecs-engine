using System;

namespace OpenUGD.ECS.Engine
{
    [Serializable]
    public enum ExitReason
    {
        None = 0,
        Error = 1,
        MaxTick = 2,
        Success = 3,
        Fail = 4
    }
}