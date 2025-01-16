using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Localization;

namespace CobaltsArmada
{
    public class CA_02_Perwinkle: ModTank 
    {
      
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Periwinkle"
        });

        public override string Texture => "assets/textures/tank_periwinkle";
        public override int Songs => 3;
        public override Color AssociatedColor => new Color(204, 204, 255);
       
        public override void PostApplyDefaults(AITank tank)
        {
           //Periwinkles are probably the most generic tank of the mod
            base.PostApplyDefaults(tank);
            tank.Model = CA_Main.Neo_Mobile;
            tank.Scaling = Vector3.One * 100.0f;
            var aiParams = tank.AiParams;
            var properties = tank.Properties;
            aiParams.MeanderAngle = MathHelper.ToRadians(7);
            aiParams.MeanderFrequency = 20;
            aiParams.TurretMeanderFrequency = 60;
            aiParams.TurretSpeed = 0.045f;
            aiParams.AimOffset = 0.9f;

            aiParams.Inaccuracy = 0.4f;

            aiParams.PursuitLevel = 1f;
            aiParams.PursuitFrequency = 40;

            aiParams.ProjectileWarinessRadius_PlayerShot = 70;
            aiParams.MineWarinessRadius_PlayerLaid = 140;

            properties.TurningSpeed = 0.13f;
            properties.MaximalTurn = MathHelper.PiOver2;

            properties.ShootStun = 0;
            properties.ShellCooldown = 70;
            properties.ShellLimit = 3;
            properties.ShellSpeed = 4f;
            properties.ShellType = ShellID.Standard;
            properties.RicochetCount = 1;

            properties.Invisible = false;
            properties.Stationary = false;
            

            properties.TreadPitch = 0.2f;
            properties.MaxSpeed = 1.7f;
            properties.Acceleration = 0.1f;
            properties.Deceleration = 0.1f;

            properties.MineCooldown = 60 * 20;
            properties.MineLimit = 1;
            properties.MineStun = 10;

            aiParams.MinePlacementChance = 0.05f;
        }
        public override void Shoot(AITank tank, ref Shell shell)
        {

            base.Shoot(tank, ref shell);
             shell.Properties.FlameColor = AssociatedColor;
        }
    }
}
