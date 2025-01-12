using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Properties;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.Localization;

namespace CobaltsArmada
{
    public class CA_08_Eryngium : ModTank 
    {
      
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Eryngium"
        });

        public override string Texture => "assets/textures/tank_eryngium";
        public override int Songs => 3;
        public override Color AssociatedColor => Color.SteelBlue;
        public override void PostApplyDefaults(AITank tank)
        {
            
            base.PostApplyDefaults(tank);
            var aiParams = tank.AiParams;
            var properties = tank.Properties;
            aiParams.TurretMeanderFrequency = 30;
            aiParams.TurretSpeed = 0.02f;
            aiParams.AimOffset = MathHelper.ToRadians(80);
            aiParams.Inaccuracy = MathHelper.ToRadians(25);

            aiParams.TurretSpeed = 0.03f;
            aiParams.AimOffset = 0.18f;

            aiParams.PursuitLevel = -0.2f;
            aiParams.PursuitFrequency = 240;

            aiParams.ProjectileWarinessRadius_PlayerShot = 60;
            aiParams.MineWarinessRadius_PlayerLaid = 160;

            properties.TurningSpeed = 0.06f;
            properties.MaximalTurn = MathHelper.ToRadians(10);

            //RAIL CANNON!
            properties.ShootStun = 220;
            properties.ShellCooldown = 300;
            properties.ShellLimit = 1;
            properties.ShellSpeed = 6f;
            properties.Recoil = 500f;
            properties.ShellType = ShellID.TrailedRocket;
            properties.RicochetCount = 3;
            //we get a little devious
            properties.Invisible = GameProperties.LoadedCampaign.CurrentMissionId>= 50;
            properties.Stationary = false;
            properties.ShellHoming = new();

            properties.TreadPitch = -0.2f;
            properties.MaxSpeed = 1.4f;
            properties.Acceleration = 0.3f;

            properties.MineCooldown = 700;
            properties.MineLimit = 2;
            properties.MineStun = 10;

            aiParams.MinePlacementChance = 0.05f;

            aiParams.BlockWarinessDistance = 45;
            
            tank.BaseExpValue = 0.1f;
        }
       
        public override void Shoot(AITank tank, ref Shell shell)
        {

            base.Shoot(tank, ref shell);
            shell.FlameColor = Color.AliceBlue;
            shell.TrailColor = Color.AliceBlue;
        }
    }
}
