﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Sandbox.Common.ObjectBuilders
{
    [ProtoContract]
    [MyObjectBuilderDefinition]
    public class MyObjectBuilder_CompoundCubeBlock : MyObjectBuilder_CubeBlock
    {
        [ProtoMember(1)]
        public MyObjectBuilder_CubeBlock[] Blocks;

        [ProtoMember(2)]
        public ushort[] BlockIds;

        public override void Remap(IMyRemapHelper remapHelper)
        {
            base.Remap(remapHelper);

            foreach (var blockInCompound in Blocks)
                blockInCompound.Remap(remapHelper);
        }
    }
}
