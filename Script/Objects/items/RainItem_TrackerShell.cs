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
    public class RainItem_TrackerShell : RainItem
    {

        public override void OnStart(ref Tank tank)
        {
            TankGame.IngameConsole.Log("Activating the power of " + Name.GetLocalizedString(LangCode.English), ItemColor);
            tank.Properties.ShellHoming.Power += MathF.Log(Stacks) * 0.1f + 0.05f;
            tank.Properties.ShellHoming.Radius += MathF.Log(Stacks) * 50f + 40f;
            tank.Properties.ShellHoming.Speed = tank.Properties.ShellSpeed;
        }
        public override int Priority => 5;
        public override Color ItemColor => Color.Aquamarine;

        public override string InternalName => "HomingShells";

        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Tracking Shells"
        });

    }


}
