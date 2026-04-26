using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Core.ItemOverrides
{
    public sealed class ItemOverrideGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => false;

        public override void SetDefaults(Item item)
        {
            if (ItemOverrideRegistry.TryGetOverride(item, out ItemBehaviorOverride itemOverride))
                itemOverride.SetDefaults(item);
        }

        public override bool CanUseItem(Item item, Player player)
        {
            if (ItemOverrideRegistry.TryGetOverride(item, out ItemBehaviorOverride itemOverride))
                return itemOverride.CanUseItem(item, player);

            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            if (ItemOverrideRegistry.TryGetOverride(item, out ItemBehaviorOverride itemOverride))
                itemOverride.UseStyle(item, player, heldItemFrame);
        }

        public override bool? UseItem(Item item, Player player)
        {
            if (ItemOverrideRegistry.TryGetOverride(item, out ItemBehaviorOverride itemOverride))
                return itemOverride.UseItem(item, player);

            return null;
        }

        public override bool Shoot(
            Item item,
            Player player,
            EntitySource_ItemUse_WithAmmo source,
            Vector2 position,
            Vector2 velocity,
            int type,
            int damage,
            float knockback)
        {
            if (ItemOverrideRegistry.TryGetOverride(item, out ItemBehaviorOverride itemOverride))
                return itemOverride.Shoot(item, player, source, position, velocity, type, damage, knockback);

            return true;
        }

        public override void HoldItem(Item item, Player player)
        {
            if (ItemOverrideRegistry.TryGetOverride(item, out ItemBehaviorOverride itemOverride))
                itemOverride.HoldItem(item, player);
        }

        public override void UpdateInventory(Item item, Player player)
        {
            if (ItemOverrideRegistry.TryGetOverride(item, out ItemBehaviorOverride itemOverride))
                itemOverride.UpdateInventory(item, player);
        }

        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            if (ItemOverrideRegistry.TryGetOverride(item, out ItemBehaviorOverride itemOverride))
                itemOverride.ModifyWeaponDamage(item, player, ref damage);
        }

        public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
        {
            if (ItemOverrideRegistry.TryGetOverride(item, out ItemBehaviorOverride itemOverride))
                itemOverride.ModifyWeaponKnockback(item, player, ref knockback);
        }

        public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
        {
            if (ItemOverrideRegistry.TryGetOverride(item, out ItemBehaviorOverride itemOverride))
                itemOverride.ModifyWeaponCrit(item, player, ref crit);
        }

        public override bool AllowPrefix(Item item, int pre)
        {
            if (ItemOverrideRegistry.TryGetOverride(item, out ItemBehaviorOverride itemOverride))
                return itemOverride.AllowPrefix(item, pre);

            return base.AllowPrefix(item, pre);
        }
    }
}
