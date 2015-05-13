﻿using ProtoBuf;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Common.ObjectBuilders.VRageData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRageMath;

namespace Medieval.ObjectBuilders.Definitions
{
    [ProtoContract]
    [MyObjectBuilderDefinition]
    public class MyObjectBuilder_FireLightBlockDefinition : MyObjectBuilder_CubeBlockDefinition
    {
        [ProtoMember(1)]
        public SerializableBounds LightIntensity = new MyBounds(1.3f, 1.7f, 1.5f);
        [ProtoMember(2)]
        public Vector4 LightColor = new Vector4(0.9f, 0.7f, 0.5f, 1);
        [ProtoMember(3)]
        public SerializableBounds LightRadius = new MyBounds(1, 2, 1.6f);
        [ProtoMember(4)]
        public SerializableBounds LightFalloff = new MyBounds(1, 2, 1.3f);
        [ProtoMember(5)]
        public float ParticleScale = 0.1f;
    }
}
