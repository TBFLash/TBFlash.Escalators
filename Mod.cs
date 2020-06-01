using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimAirport.Modding.Base;
using SimAirport.Modding.Settings;

namespace TBFlash.LongEscalator
{
    public class Mod : BaseMod
    {
        public override string Name => "TBFlash.LongEscalator";

        public override string InternalName => "TBFlash.LongEscalator";

        public override string Description => "";

        public override string Author => "TBFlash";

        public override SettingManager SettingManager { get; set; }

        public override void OnTick()
        {       
        }
    }
}
