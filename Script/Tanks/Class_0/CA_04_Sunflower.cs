using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Localization;

namespace CobaltsArmada
{
    public class CA_04_Sunflower: ModTank 
    {
     
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Sunflower"
        });

        public override string Texture => "assets/textures/tank_sunflower";
        public override int Songs => 2;
        public override Color AssociatedColor => Color.Yellow;
        public override void PostApplyDefaults()
        {
            base.PostApplyDefaults();
            AITank.UsesCustomModel = true;
            AITank.Model = CA_Main.Neo_Mobile;
            AITank.Scaling = Vector3.One * 100.0f;
            AITank.AiParams.MeanderAngle = MathHelper.ToRadians(30);
            AITank.AiParams.MeanderFrequency = 10;
            AITank.AiParams.TurretMeanderFrequency = 20;
            AITank.AiParams.TurretSpeed = 0.06f;
            AITank.AiParams.AimOffset = MathHelper.ToRadians(3);

            AITank.AiParams.Inaccuracy = 0.6f;

            AITank.AiParams.PursuitLevel = 0.9f;
            AITank.AiParams.PursuitFrequency = 500;

            AITank.AiParams.ProjectileWarinessRadius_PlayerShot = 120;
            AITank.AiParams.ProjectileWarinessRadius_AIShot = 100;
            AITank.AiParams.MineWarinessRadius_PlayerLaid = 14;
            AITank.AiParams.MineWarinessRadius_AILaid = 70;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 250;
            AITank.Properties.ShellLimit = 1;
            AITank.Properties.ShellSpeed = 5f;
            AITank.Properties.ShellType = ShellID.Rocket;
            AITank.Properties.RicochetCount = 1;

            AITank.Properties.Invisible = false;
            AITank.Properties.Stationary = false;

            AITank.Properties.CanLayTread = false;
            AITank.Properties.TreadPitch = 0.3f;
            AITank.Properties.MaxSpeed = 1.3f;

            AITank.Properties.Acceleration = 0.1f;

            AITank.Properties.MineCooldown = 0;
            AITank.Properties.MineLimit = 0;
            AITank.Properties.MineStun = 0;

            AITank.AiParams.BlockWarinessDistance = 44;
        }

        public override void Shoot(Shell shell)
        {
            base.Shoot(shell);
             shell.Properties.FlameColor = AssociatedColor;
        }
    }
}
