﻿using ProtoBuf;

namespace Sandbox.Common.ObjectBuilders
{
    [ProtoContract]
    [MyObjectBuilderDefinition]
    public class MyObjectBuilder_SignalLight : MyObjectBuilder_CubeBlock
    {
    }
}
