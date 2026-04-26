using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Content.BossOverhauls.StormWeaver.Attacks
{
    internal static class StormWeaverAttackRegistry
    {
        private static Dictionary<StormWeaverHeadOverride.State, StormWeaverAttack> _attacks;

        public static IReadOnlyDictionary<StormWeaverHeadOverride.State, StormWeaverAttack> Attacks
        {
            get
            {
                _attacks ??= LoadAttacks();
                return _attacks;
            }
        }

        private static Dictionary<StormWeaverHeadOverride.State, StormWeaverAttack> LoadAttacks()
        {
            Dictionary<StormWeaverHeadOverride.State, StormWeaverAttack> attacks = new();

            Type baseType = typeof(StormWeaverAttack);
            Assembly assembly = baseType.Assembly;

            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                if (!baseType.IsAssignableFrom(type))
                    continue;

                if (Activator.CreateInstance(type) is not StormWeaverAttack attack)
                    continue;

                if (attacks.ContainsKey(attack.ID))
                    throw new Exception($"Duplicate StormWeaver attack registered for state {attack.ID}: {type.FullName}");

                attacks.Add(attack.ID, attack);
            }

            return attacks;
        }

        public static StormWeaverAttack Get(StormWeaverHeadOverride.State state)
        {
            if (!Attacks.TryGetValue(state, out StormWeaverAttack attack))
                throw new KeyNotFoundException($"No StormWeaver attack is registered for state {state}");

            return attack;
        }
    }
}
