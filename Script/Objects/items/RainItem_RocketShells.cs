using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Localization;

namespace CobaltsArmada.Script.Objects.items
{
    public class RainItem_RocketShells : RainItem
    {
        public override void OnStart(ref Tank tank) {
            tank.Properties.ShellType = ShellID.TrailedRocket;
            tank.Properties.ShellSpeed += MathF.Log(Stacks) * 0.5f + 0.2f;
            tank.Properties.Recoil += MathF.Log(Stacks) * 2f + 0.5f;
            tank.Properties.ShellCooldown += (uint)(Stacks * 4);
             tank.Properties.RicochetCount = 0;

            TankGame.IngameConsole.Log("Activating the power of " + Name.GetLocalizedString(LangCode.English), ItemColor);
  
        }

        public override void OnShellDestroy(ref Shell shell) {
            new Explosion(shell.Position, MathF.Log(Stacks + shell.Velocity.Length() / 4f) * 2 + 1f,shell.Owner);
        }

        public override Rarity Tier => Rarity.Red;

        public override int Priority => 3;

        public override Color ItemColor => Color.DarkMagenta;

        public override string InternalName => "BoomShells";
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Explosive Rockets"
        });

    }

    
}
