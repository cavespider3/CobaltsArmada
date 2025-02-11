﻿using Microsoft.Xna.Framework;
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
    public class CA_ShatterBouncer : ModShell
    {

        public override string Texture => "assets/textures/bullet";
        public override string ShootSound => "assets/sfx/touhou_shot.ogg";
        public override string TrailSound => base.TrailSound;

        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Rico-Shatter Shell"
        });
        public override void OnCreate(Shell shell)
        {
            base.OnCreate(shell);
            shell.Properties.IsDestructible = false;

            shell.LifeTime = 0f;
            if (shell.Owner is null) return;
            int vibecheck = 0;
            if (shell.Owner is PlayerTank) vibecheck = PlayerTank.ClientTank.PlayerType; else vibecheck = ((AITank)shell.Owner).AiTankType;


        }

        public override void OnRicochet(Shell shell, Block block)
        {
            CA_Main.Fire_AbstractShell(shell, 4, 1, 0, 3.5f);
        }


    }
}
