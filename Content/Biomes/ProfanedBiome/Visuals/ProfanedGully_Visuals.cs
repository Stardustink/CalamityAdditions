using Terraria.Graphics.Effects;

namespace CalamityAdditions.Content.Biomes.ProfanedBiome.Visuals
{
    internal class ProfanedGully_Visuals : ModSceneEffect
    {
        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            const string filterKey = "CalamityAdditions:ProfanedSky";

            if (isActive)
            {
                if (!Filters.Scene[filterKey].IsActive())
                    Filters.Scene.Activate(filterKey, player.Center).GetShader().UseColor(Color.LightGoldenrodYellow).UseSecondaryColor(Color.Black).UseOpacity(1).UseTargetPosition(player.Center).UseProgress(0);

                var shader = Filters.Scene[filterKey].GetShader();
                shader.UseIntensity(3);
                shader.UseOpacity(0.7f);
                shader.UseSecondaryColor(Color.Yellow); 


            }
            else
            {
                if (Filters.Scene[filterKey].IsActive())
                    Filters.Scene[filterKey].Deactivate();
            }
        }
        public override bool IsSceneEffectActive(Player player)=> player.InModBiome<ProfanedGully>();
    }
}
