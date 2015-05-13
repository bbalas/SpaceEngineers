﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using VRageMath;

namespace Sandbox.Common.ObjectBuilders.Definitions
{
    [ProtoContract]
    [MyObjectBuilderDefinition]
    public class MyObjectBuilder_ReactorDefinition : MyObjectBuilder_PowerProducerDefinition
    {
        [ProtoMember(1)]
        public Vector3 InventorySize = new Vector3(10, 10, 10);

        [ProtoMember(2)]
        public SerializableDefinitionId FuelId = new SerializableDefinitionId(typeof(MyObjectBuilder_Ingot), "Uranium");
    }
}
