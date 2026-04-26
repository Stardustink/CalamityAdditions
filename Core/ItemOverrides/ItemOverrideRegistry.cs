using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Core.ItemOverrides
{

    public sealed class ItemOverrideRegistry : ModSystem
    {
        private static readonly Dictionary<int, ItemBehaviorOverride> OverridesByType = new();

        public override void Unload()
        {
            OverridesByType.Clear();
        }

        public static void Register(ItemBehaviorOverride itemOverride)
        {
            if (itemOverride is null)
                throw new ArgumentNullException(nameof(itemOverride));

            int type = itemOverride.TargetItemType;

            if (type <= ItemID.None)
                throw new ArgumentException($"{itemOverride.GetType().Name} has an invalid TargetItemType.");

            if (OverridesByType.ContainsKey(type))
                throw new Exception($"Duplicate item override registered for item type {type}.");

            OverridesByType[type] = itemOverride;
        }

        public static bool TryGetOverride(Item item, out ItemBehaviorOverride itemOverride)
        {
            if (item is null || item.IsAir)
            {
                itemOverride = null;
                return false;
            }

            return OverridesByType.TryGetValue(item.type, out itemOverride);
        }
    }

}
