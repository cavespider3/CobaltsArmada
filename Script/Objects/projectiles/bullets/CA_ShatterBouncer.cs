using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Localization;
using TanksRebirth.GameContent.Tanks.AI;
using TanksRebirth.GameContent.Tanks;
namespace CobaltsArmada
{
    public class CA_ShatterBouncer : ModShell
    {

        public override string Texture => "assets/textures/bullet";
        public override string ShootSound => "assets/sfx/touhou_shot.ogg";
        public override string TrailSound => base.TrailSound;

        public override LocalizedString Name => new()
        {
            [LangCode.English] = "Rico-Shatter Shell"
        };
        public override void OnCreate()
        {
            base.OnCreate();
            Shell.Properties.Penetration = -1;
          
            Shell.LifeTime = 0f;
            if (Shell.Owner is null) return;
            int vibecheck = 0;
            if (Shell.Owner is PlayerTank) vibecheck = PlayerTank.ClientTank.PlayerType; else vibecheck = ((AITank)Shell.Owner).AiTankType;


        }

        public override void OnRicochet(Block?block)
        {
            CA_Main.Fire_AbstractShell(Shell, 4, 1, 0, 3.5f);
        }


    }
}
