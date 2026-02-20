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
    public class RainItem_KineticShells : RainItem
    {
        public override void OnStart(ref Tank tank)
        {
            TankGame.IngameConsole.Log("Activating the power of " + Name.GetLocalizedString(LangCode.English), ItemColor);
        }

        public override void OnShellRicochet(ref Shell shell)
        {
            shell.Velocity += Vector2.Normalize(shell.Velocity) * (Stacks * 0.05f);
        }
        public override Rarity Tier => Rarity.Green;

        public override Color ItemColor => Color.MediumPurple;

        public override string InternalName => "KineticShells";
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Echo Chambered Shells"
        });

    }

    
}
