﻿using ProtoBuf;

namespace Sandbox.Common.ObjectBuilders
{
    [ProtoContract]
    [MyObjectBuilderDefinition]
    public class MyObjectBuilder_ExtendedPistonBase : MyObjectBuilder_PistonBase
    {
    }
}
