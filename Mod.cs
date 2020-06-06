using SimAirport.Modding.Base;
using SimAirport.Modding.Settings;

namespace TBFlash.FastEscalator
{
    public class Mod : BaseMod
    {
        public override string Name => "Fast Escalators";

        public override string InternalName => "TBFlash.FastEscalator";

        public override string Description => "Is a normal escalator not fast enough? Use a Fast one!";

        public override string Author => "TBFlash";

        public override SettingManager SettingManager { get; set; }

        public override void OnTick()
        {
        }
    }
}
