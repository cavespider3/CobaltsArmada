

using TanksRebirth.GameContent;

using TanksRebirth.GameContent.ModSupport;

using TanksRebirth.Localization;

using static TanksRebirth.GameContent.Shell;

using TanksRebirth.GameContent.ID;
using TanksRebirth.Internals.Common.Framework.Audio;


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

        public override void OnCreate()
        {
            base.OnCreate();
          
            Shell.LifeTime = 0f;
            if (Shell.Owner is null) return;
            int vibecheck = 0;
            if (Shell.Owner is PlayerTank) vibecheck = PlayerTank.ClientTank.PlayerType; else vibecheck = ((AITank)Shell.Owner).AiTankType;
            if (vibecheck == ModContent.GetSingleton<CA_05_Poppy>().Type) BurstSize=4;
            if (vibecheck == ModContent.GetSingleton<CA_07_Lavender>().Type) BurstSize = 6;

        }
        int BurstSize = 4;
        uint BurstBounces = 0;

        public override void OnDestroy(DestructionContext context, ref bool playSound)
        {
            base.OnDestroy(context, ref playSound);
            if (Shell.Owner is null) return;
            int vibecheck = 0;
            if (Shell.Owner is PlayerTank) vibecheck =ShellID.Player; else vibecheck = ShellID.Standard;

            // EDGE CASE
            if (context == DestructionContext.WithExplosion || context == DestructionContext.WithMine) return;
            switch (CA_Main.modifier_Difficulty)
            {
                case CA_Main.ModDifficulty.Easy:
                    if (context != DestructionContext.WithObstacle) return; break;

                case CA_Main.ModDifficulty.Normal:
                    if (context == DestructionContext.WithShell) return; break;

                case CA_Main.ModDifficulty.Hard:
                case CA_Main.ModDifficulty.Lunatic:
                    if (context == DestructionContext.WithShell && CA_Y2_NightShade.PoisonedTanks.Find(x => x == Shell.Owner) is null) return;break;

                case CA_Main.ModDifficulty.Extra:
                case CA_Main.ModDifficulty.Phantasm:
                    if (CA_Y2_NightShade.PoisonedTanks.Find(x => x == Shell.Owner) is not null) BurstSize += CA_Main.modifier_Difficulty == CA_Main.ModDifficulty.Phantasm? 2:1;
                    if (CA_Y2_NightShade.PoisonedTanks.Find(x => x == Shell.Owner) is not null) BurstBounces = CA_Main.modifier_Difficulty == CA_Main.ModDifficulty.Phantasm ? 1u : 0; break;
                default:
                break;
            }
            
            CA_Main.Fire_AbstractShell(Shell, BurstSize, vibecheck, BurstBounces, Shell.Velocity.Length()/1.1f);
                
        }

    }
}
