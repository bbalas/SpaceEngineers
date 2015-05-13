﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRageMath;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using System.Diagnostics;
using System.Threading;
using Sandbox.Game.World;

namespace Sandbox.Game.Screens.Helpers
{
    public partial class MyGps
    {
        internal static readonly int DROP_NONFINAL_AFTER_SEC = 180;

        //GPS entry may be confirmed or uncorfirmed. Uncorfirmed has valid DiscardAt.
            public string Name;
            public string Description;
            public Vector3D Coords;
            public bool ShowOnHud;
            public TimeSpan? DiscardAt;//final=null. Not final=time at which we should drop it from the list, relative to ElapsedPlayTime
            public int Hash
            {
                get; 
                private set;
            }
            public void UpdateHash()
            {
                int newHash = 1;
                newHash = (newHash * 397) ^ Name.GetHashCode();
                //if (Description!=null)
                //    newHash = (newHash * 397) ^ Description.GetHashCode();
                newHash = (newHash * 397) ^ Coords.GetHashCode();
                Hash = newHash;
            }
            public override int GetHashCode()
            {
                return Hash;
            }
            public override string ToString()
            {
                StringBuilder sb= new StringBuilder("GPS:",256);
                sb.Append(Name);sb.Append(":");
                sb.Append(Coords.X.ToString(System.Globalization.CultureInfo.InvariantCulture));sb.Append(":");
                sb.Append(Coords.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));sb.Append(":");
                sb.Append(Coords.Z.ToString(System.Globalization.CultureInfo.InvariantCulture));sb.Append(":");
                return sb.ToString();
            }

        public void ToClipboard()
        {
            Thread thread = new Thread(() => System.Windows.Forms.Clipboard.SetText(this.ToString()));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        public MyGps(MyObjectBuilder_Gps.Entry builder)
        {
            Name = builder.name;
            Description = builder.description;
            Coords = builder.coords;
            ShowOnHud = builder.showOnHud;
            if (!builder.isFinal)
                SetDiscardAt();
            UpdateHash();
        }
        public MyGps()
        {
            SetDiscardAt();
        }

        public void SetDiscardAt()
        {
            DiscardAt = TimeSpan.FromSeconds(MySession.Static.ElapsedPlayTime.TotalSeconds + DROP_NONFINAL_AFTER_SEC);
        }

    }
}
