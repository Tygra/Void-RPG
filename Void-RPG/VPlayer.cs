﻿#region Discalimer
/*  
 *  The plugin has some features which I got from other authors.
 *  I don't claim any ownership over those elements which were made by someone else.
 *  The plugin has been customized to fit our need at Geldar,
 *  and because of this, it's useless for anyone else.
 *  I know timers are shit, and If someone knows a way to keep them after relog, tell me.
*/
#endregion

#region Refs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terraria;
using TShockAPI;
#endregion

namespace VoidRPG
{
    public class VPlayer
    {
        public int Index { get; set; }
        public TSPlayer TSPlayer { get { return TShock.Players[Index]; } }

        public int facepalmcd { get; set; }
        public int startercd { get; set; }

        public VPlayer(int index)
        {
            this.Index = index;
        }
    }
}