using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Properties;
using TanksRebirth.GameContent.UI;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using TanksRebirth.Net;

namespace CobaltsArmada
{
    public class CA_03_Pansy : ModTank 
    {
      
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Pansy"
        });

        public override string Texture => "assets/textures/tank_pansy";
        public override int Songs => 5;
        public override Color AssociatedColor => Color.RoyalBlue;
        public override void PostApplyDefaults(AITank tank)
        {
            base.PostApplyDefaults(tank);
            var aiParams = tank.AiParams;
            var properties = tank.Properties;
            aiParams.MeanderAngle = MathHelper.ToRadians(30);
            aiParams.MeanderFrequency = 15;
            aiParams.TurretMeanderFrequency = 20;
            aiParams.TurretSpeed = 0.025f;
            aiParams.AimOffset = 0.01f;

            aiParams.Inaccuracy = 0.4f;

            aiParams.PursuitLevel = -0.3f;
            aiParams.PursuitFrequency = 240;

            aiParams.ProjectileWarinessRadius_PlayerShot = 40;
            aiParams.MineWarinessRadius_PlayerLaid = 70;

            properties.TurningSpeed = 0.15f;
            properties.MaximalTurn = MathHelper.PiOver2;

            properties.ShootStun = 20;
            properties.ShellCooldown = 150;
            properties.ShellSpread = 0.31f;
            properties.ShellShootCount = 3;
            properties.ShellLimit = 3;

            properties.ShellSpeed = 5.25f;
         
            properties.ShellType = ShellID.Rocket;
            properties.RicochetCount = 0;

            properties.Invisible = false;
            properties.Stationary = false;
            properties.ShellHoming = new();

            properties.TreadPitch = 0.08f;
            properties.MaxSpeed = 1.4f;
            properties.Acceleration = 0.3f;
            properties.Deceleration = 0.6f;

            properties.MineCooldown = 1000;
            properties.MineLimit = 1;
            properties.MineStun = 0;

            aiParams.MinePlacementChance = 0.05f;
            tank.BaseExpValue = 0.1f;
        }

 
    }
}
