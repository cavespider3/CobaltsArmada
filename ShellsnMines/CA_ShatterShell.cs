using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TanksRebirth.GameContent;
using TanksRebirth.GameContent.GameMechanics;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Internals;
using TanksRebirth.Localization;
using TanksRebirth;
using static TanksRebirth.GameContent.Shell;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TanksRebirth.GameContent.ID;

using TanksRebirth.Internals.Common.Framework;
using static System.Runtime.InteropServices.JavaScript.JSType;
using TanksRebirth.GameContent.RebirthUtils;

namespace CobaltsArmada
{
    public class CA_ShatterShell : ModShell
    {

        public override string Texture => "assets/textures/bullet";
        public override string ShootSound => "assets/sfx/touhou_shot.ogg";
        public override string TrailSound => base.TrailSound;

        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Shatter Shell"
        });
        public override void OnCreate(Shell shell)
        {
            base.OnCreate(shell);
   
            shell.LifeTime = 0f;
            if (shell.Owner is null) return;
            int vibecheck = 0;
            if (shell.Owner is PlayerTank) vibecheck = PlayerTank.ClientTank.PlayerType; else vibecheck = ((AITank)shell.Owner).AiTankType;
            if (vibecheck == ModContent.GetSingleton<CA_05_Poppy>().Type) BurstSize=4;
            if (vibecheck == ModContent.GetSingleton<CA_07_Lavender>().Type) BurstSize = 6;

        }
        int BurstSize = 4;
        uint BurstBounces = 0;

        public override void OnDestroy(Shell shell, DestructionContext context, ref bool playSound)
        {
            base.OnDestroy(shell, context, ref playSound);
            if (shell.Owner is null) return;
            int vibecheck = 0;
            if (shell.Owner is PlayerTank) vibecheck =ShellID.Player; else vibecheck = ShellID.Standard;
            CA_Main.Fire_AbstractShell(shell, BurstSize, vibecheck, BurstBounces, shell.Velocity.Length()/1.1f);
                
        }

    }
}
