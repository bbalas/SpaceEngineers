﻿using ProtoBuf;

namespace Sandbox.Common.ObjectBuilders
{
    [ProtoContract]
    [MyObjectBuilderDefinition]
    public class MyObjectBuilder_Refinery : MyObjectBuilder_ProductionBlock
    {
    }
}
