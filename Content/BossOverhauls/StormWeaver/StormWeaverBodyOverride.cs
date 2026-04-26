using CalamityAdditions.Core.NpcBehaviorOverride;
using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.NPCs.StormWeaver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Content.BossOverhauls.StormWeaver
{
    internal class StormWeaverBodyOverride : NPCBehaviorOverride
    {
        public override string TexturePath => this.GetPath();
        public override int NPCType => ModContent.NPCType<StormWeaverBody>();

        public StormWeaverLeg[] Legs;

        public bool ShouldHaveLegs = false;

        public int IndexInList = -1;
        public override void SetStaticDefaults()
        {
            NPCID.Sets.MustAlwaysDraw[NPCType] = true;
        }
        public override bool OnSpawn(NPC NPC, IEntitySource source)
        {
            if (source is not null && source is EntitySource_Parent parentSource && parentSource.Entity is NPC parentNPC && parentNPC.type == ModContent.NPCType<StormWeaverHead>())
            {
                NPC.realLife = parentNPC.whoAmI;

                IndexInList = int.TryParse(source.Context, out int index) ? index : -1;

                if (IndexInList < 3 && IndexInList >= 0)
                {
                    ShouldHaveLegs = true;  
                }
            }

            return false;
        }

        public override void SetDefaults(NPC NPC)
        {
            NPC.Size = new Vector2(120);
            NPC.Opacity = 1;

            CalamityGlobalNPC global = NPC.Calamity();
            NPC.defense = 111;
            global.DR = 0;
            global.unbreakableDR = false;
        }


        public override bool OverrideAI(NPC NPC)
        {
            if(Legs is null && ShouldHaveLegs)
            {
                InitializeLegs(NPC);
            }

            if(Legs is not null)
            {
                for(int i = 0; i< Legs.Length; i++)
                {
                    int flip = (Legs[i].IsLeftLeg ? -1 : 1);
                    Vector2 TargetPos = Legs[i].Anchor + new Vector2(MathF.Sin(Main.GameUpdateCount * 0.05f + NPC.whoAmI)*10, 520).RotatedBy(NPC.rotation + MathHelper.Pi * (Legs[i].IsLeftLeg ? 0 : 1));
                   // Dust.NewDustPerfect(TargetPos, DustID.Cloud, Vector2.Zero);
                    Vector2 Anchor = NPC.Center + new Vector2(0, NPC.height / 2 * flip).RotatedBy(MathHelper.Pi * (Legs[i].IsLeftLeg ? -1 : 1) + NPC.rotation);
                    Legs[i].Update(Anchor, TargetPos);
                   

                }
            }

            return true;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            modifiers._combatTextHidden = false;
        }

        public void InitializeLegs(NPC NPC)
        {
            Legs = new StormWeaverLeg[2];

            var skeleton = new IKSkeleton.JointSetup(220, 0, 360);

            Legs[0] = new StormWeaverLeg(NPC.Center, skeleton, skeleton, skeleton, skeleton);
            Legs[0].IsLeftLeg = true;

            Legs[1] = new StormWeaverLeg(NPC.Center, skeleton, skeleton, skeleton, skeleton);
            Legs[1].IsLeftLeg = false;
        }



        public override bool PreDraw(NPC NPC, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var tex = TextureAssets.Npc[NPCType].Value;
            if(Legs is not null)
            {
                foreach(var leg in Legs)
                {
                    leg.Draw(spriteBatch, screenPos, Color.White);
                }
            }


            Vector2 DrawPos = NPC.Center - screenPos;
            Vector2 Origin = new Vector2(tex.Width/2, tex.Height/1.5f);

            float scale = MathHelper.SmoothStep(0.36f, 0,  IndexInList / (float)StormWeaverHeadOverride.MaxSegmentCount);
            Main.EntitySpriteDraw(tex, DrawPos, null, Color.White, NPC.rotation - MathHelper.PiOver2, Origin, scale, 0);
            //Utils.DrawBorderString(spriteBatch, $"Index in list: {IndexInList}", NPC.Center - screenPos + Vector2.UnitY * 50, Color.White);
            return base.PreDraw(NPC, spriteBatch, screenPos, drawColor);
        }
      
    }
}
