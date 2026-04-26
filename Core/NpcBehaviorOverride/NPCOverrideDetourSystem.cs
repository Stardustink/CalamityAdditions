using MonoMod.RuntimeDetour;
using System.Reflection;

namespace CalamityAdditions.Core.NpcBehaviorOverride
{
    [Autoload(Side = ModSide.Both)]
    public sealed class NPCOverrideDetourSystem : ModSystem
    {
        private Hook _onSpawnHook;

        private delegate void NPCLoader_OnSpawn_Orig(NPC npc, IEntitySource source);
        private delegate void NPCLoader_OnSpawn_Detour(
          NPCLoader_OnSpawn_Orig orig,
          NPC npc,
          IEntitySource source
      );
        public override void Load()
        {
            On_NPC.AI += AIHook;
            On_NPC.FindFrame += FindFrameHook;

            MethodInfo onSpawnMethod = typeof(NPCLoader).GetMethod("OnSpawn", BindingFlags.Static | BindingFlags.NonPublic);

            if (onSpawnMethod is null)
                throw new MissingMethodException("Could not find NPCLoader.OnSpawn.");

            _onSpawnHook = new Hook(
                onSpawnMethod,
                (NPCLoader_OnSpawn_Detour)OnSpawnHook
            );
        }

        public override void Unload()
        {
            On_NPC.AI -= AIHook;
            On_NPC.FindFrame -= FindFrameHook;
        }

        private void OnSpawnHook(NPCLoader_OnSpawn_Orig orig, NPC npc, IEntitySource source)
        {
            NPCBehaviorOverride ov = npc
                .GetGlobalNPC<NPCOverrideGlobalNPC>()
                .GetOverride(npc);

            // Return true means: "I fully handled spawn behavior; skip old ModNPC.OnSpawn
            // and skip all normal GlobalNPC.OnSpawn hooks."
            if (ov is not null && ov.OnSpawn(npc, source))
                return;

            orig(npc, source);
        }

        private void AIHook(On_NPC.orig_AI orig, NPC self)
        {
            NPCBehaviorOverride ov = self.GetGlobalNPC<NPCOverrideGlobalNPC>().GetOverride(self);

            if (ov is not null && ov.OverrideAI(self))
                return;

            orig(self);
        }

        private void FindFrameHook(On_NPC.orig_FindFrame orig, NPC self)
        {
            NPCBehaviorOverride ov = self.GetGlobalNPC<NPCOverrideGlobalNPC>().GetOverride(self);

            if (ov is not null && ov.OverrideFindFrame(self))
                return;

            orig(self);
        }
    }
}