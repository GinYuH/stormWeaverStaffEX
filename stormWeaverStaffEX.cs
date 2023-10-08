using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace stormWeaverStaffEX
{
    public class stormWeaverStaffEX : Mod
    {
        public static stormWeaverStaffEX instance;

        public override void Load()
        {
            instance = this;
}
        public override void Unload()
        {
            instance = null;
}
    }

    /*public class SWconfig : ModConfig
    {
        public static SWconfig Instance;
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Label("Body Segment Distance")]
        //[BackgroundColor(192, 54, 64, 192)]
        //[SliderColor(224, 165, 56, 128)]
        [Range(0, 50)]
        [DefaultValue(18)]
        [Tooltip("Changes the distance between body segments")]
        public int BodyDist { get; set; }

        [Label("Tail Segment Distance")]
        //[BackgroundColor(192, 54, 64, 192)]
        //[SliderColor(224, 165, 56, 128)]
        [Range(0, 50)]
        [DefaultValue(18)]
        [Tooltip("Changes the distance between tail segments")]
        public int TailDist { get; set; }
    }*/
}