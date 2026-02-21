using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;

namespace CobaltsArmada.Script.Objects.items
{
    public class RainItem_FasterShells : RainItem
    {
        public override void OnStart(ref Tank tank)
        {
            TankGame.IngameConsole.Log("Activating the power of " + Name[LangCode.English], ItemColor);
            tank.Properties.ShellSpeed += 0.06f * Stacks;
            //tank.Properties.ShellHoming.Power += 0.08f;
            //tank.Properties.ShellHoming.Power *= 0.25f;
            //tank.Properties.ShellHoming.Radius += 90f;
            //tank.Properties.ShellHoming.Radius *= 0.5f;
            //tank.Properties.ShellHoming.Speed = tank.Properties.ShellSpeed;
        }
        public override Color ItemColor => Color.Yellow;


        public override string InternalName => "FasterShells";

        public override LocalizedString Name => new()
        {
            [LangCode.English] = "Aerodynamic Tips"
        };

    }


}
