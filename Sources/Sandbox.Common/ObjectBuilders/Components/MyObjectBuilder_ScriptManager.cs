﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using VRage.Serialization;

namespace Sandbox.Common.ObjectBuilders
{
    [ProtoContract]
    [MyObjectBuilderDefinition]
    public class MyObjectBuilder_ScriptManager : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(1)]
        public SerializableDictionary<string, object> variables=new SerializableDictionary<string,object>();
    }
}

