using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Localization;

namespace CobaltsArmada.Script.Objects.items
{
    public class RainItem_IgnitedRounds : RainItem
    {
        public override void OnStart(ref Tank tank)
        {
            TankGame.IngameConsole.Log("Activating the power of " + Name[LangCode.English], ItemColor);
        }

        public override void OnTankDestroy(ref Tank owner, ref Tank victim)
        {
            new Explosion(victim.Position, MathF.Log(Stacks * 1.3f) * 2.1f + 1.1f,owner);
        }

        public override Rarity Tier => Rarity.Green;

        public override Color ItemColor => Color.OrangeRed;

        public override string InternalName => "gasoline";
        public override LocalizedString Name => new()
        {
            [LangCode.English] = "Matchstick Shells"
        };

    }

    
}
