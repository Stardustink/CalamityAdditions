using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Systems;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.Dusts.WaterSplash;
using CalamityMod.Gores.WaterDroplet;
using CalamityMod;

namespace CalamityAdditions.Content.Biomes.ProfanedBiome
{
    public class ProfanedLava : ModLavaStyle
    {
        public override string WaterfallTexture => "CalamityAdditions/Content/Biomes/ProfanedBiome/ProfanedLavaflow";

        public override int GetSplashDust() => DustID.Firefly;

        public override int GetDropletGore() => ModContent.GoreType<CragsLavaDroplet>();

        public override bool IsLavaActive() => Main.LocalPlayer.InModBiome<ProfanedGully>();

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 2.48f / 4;
            g = 1.05f / 4;
            b = 0.98f / 4;
        }

        public override void InflictDebuff(Player player, int onfireDuration)
        {
            player.AddBuff(ModContent.BuffType<HolyFlames>(), onfireDuration);
        }
    }
}
