
global using BreadLibrary.Core.Graphics.Particles;
global using BreadLibrary.Core.Utilities;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using ReLogic.Content;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using Terraria;
global using Terraria.Audio;
global using Terraria.DataStructures;
global using Terraria.GameContent;
global using Terraria.GameContent.Generation;
global using Terraria.Graphics.Effects;
global using Terraria.Graphics.Renderers;
global using Terraria.Graphics.Shaders;
global using Terraria.ID;
global using Terraria.IO;
global using Terraria.Localization;
global using Terraria.ModLoader;
global using Terraria.WorldBuilding;
using System.Runtime.CompilerServices;

[assembly: IgnoresAccessChecksTo("CalamityMod")]
namespace CalamityAdditions
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	
    public partial class CalamityAdditions : Mod
    {
        public CalamityAdditions()
        {
            MusicAutoloadingEnabled = false;
        }
        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {

                Asset<Effect> filterShader = this.Assets.Request<Effect>("Assets/Effects/ProfanedSky");

                Filters.Scene["CalamityAdditions:ProfanedSky"] = new Filter(new ScreenShaderData(filterShader, "Pass1"), EffectPriority.High);


                Asset<Effect> specialShader = this.Assets.Request<Effect>("Assets/Effects/ProfanedConstruct/DirectionalDissolveShader");
                GameShaders.Misc["CalamityAdditions:DirectionalDissolve"] = new MiscShaderData(specialShader, "AutoloadPass");
            }
        }
	}
}
