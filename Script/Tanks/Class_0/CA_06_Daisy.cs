using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Localization;

namespace CobaltsArmada
{
    public class CA_06_Daisy: ModTank 
    {
      
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Daisy"
        });

        public override string Texture => "assets/textures/tank_daisy";
        public override int Songs => 3;
        public override Color AssociatedColor => Color.Orange;
        public override void PostApplyDefaults()
        {
            base.PostApplyDefaults();
            AITank.UsesCustomModel = true;
            AITank.Model = CA_Main.Neo_Stationary;
            AITank.Scaling = Vector3.One * 100.0f;
            AITank.AiParams.MeanderAngle = MathHelper.ToRadians(30);
            AITank.AiParams.MeanderFrequency = 10;
            AITank.AiParams.TurretMeanderFrequency = 20;
            AITank.AiParams.TurretSpeed = 0.06f;
            AITank.AiParams.AimOffset = MathHelper.ToRadians(3);

            AITank.AiParams.Inaccuracy = 0.6f;

            AITank.AiParams.PursuitLevel = 0.9f;
            AITank.AiParams.PursuitFrequency = 2;

            AITank.AiParams.ProjectileWarinessRadius_PlayerShot = 120;
            AITank.AiParams.ProjectileWarinessRadius_AIShot = 100;
            AITank.AiParams.MineWarinessRadius_PlayerLaid = 14;
            AITank.AiParams.MineWarinessRadius_AILaid = 70;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 300;
            AITank.Properties.ShellLimit = 1;
            AITank.Properties.ShellSpeed = 5f;
            AITank.Properties.ShellType = ModContent.GetSingleton<CA_ShatterBouncer>().Type;
            AITank.Properties.RicochetCount = 2;
            AITank.AiParams.SmartRicochets =true;

            AITank.Properties.Invisible = false;
            AITank.Properties.Stationary = true;

            AITank.Properties.Acceleration = 0.1f;

            AITank.Properties.MineCooldown = 0;
            AITank.Properties.MineLimit = 0;
            AITank.Properties.MineStun = 0;

            AITank.AiParams.BlockWarinessDistance = 44;


        }

        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            base.TakeDamage(destroy, context); 
            if (!destroy) return;
            new Mine(AITank, AITank.Position, 50f,1f);
        }

        public override void Shoot(Shell shell)
        {

            base.Shoot(shell);
             shell.Properties.FlameColor = AssociatedColor;
        }

    }
}
