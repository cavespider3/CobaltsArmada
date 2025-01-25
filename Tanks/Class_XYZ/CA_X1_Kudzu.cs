using Microsoft.Xna.Framework;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.GameMechanics;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems.Coordinates;
using TanksRebirth.GameContent.UI;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using TanksRebirth.Net;

namespace CobaltsArmada
{
    public class CA_X1_Kudzu: ModTank 
    {
      
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Kudzu"
        });

        public override string Texture => "assets/textures/tank_kudzu";
        public override int Songs => 5;
  
        public override Color AssociatedColor => Color.Olive;
        public override void PostApplyDefaults(AITank tank)
        {
            base.PostApplyDefaults(tank);
           
            tank.Model = CA_Main.Neo_Mobile;
            tank.Scaling = Vector3.One * 100.0f * 0.81f;
            tank.AiParams.MeanderAngle = MathHelper.ToRadians(30);
            tank.AiParams.MeanderFrequency = 20;
            tank.AiParams.TurretMeanderFrequency = 20;
            tank.AiParams.TurretSpeed = 0.03f;
            tank.AiParams.AimOffset = MathHelper.ToRadians(9);

            tank.AiParams.Inaccuracy = 0.6f;

            tank.AiParams.PursuitLevel = 1f;
            tank.AiParams.PursuitFrequency = 40;

            //Clueless
            tank.AiParams.ProjectileWarinessRadius_PlayerShot = 0;
            tank.AiParams.ProjectileWarinessRadius_AIShot = 0;
            tank.AiParams.MineWarinessRadius_PlayerLaid = 0;
            tank.AiParams.MineWarinessRadius_AILaid = 0;

            tank.Properties.TurningSpeed = 0.09f;
            tank.Properties.MaximalTurn = MathHelper.ToRadians(80);

            tank.Properties.ShootStun = 0;
            tank.Properties.ShellCooldown = 180;
            tank.Properties.ShellLimit = 2;
            tank.Properties.ShellSpeed = 4f;
            tank.Properties.ShellType = ShellID.Standard;
            tank.Properties.RicochetCount = 0;

            tank.AiParams.ShootChance = 0.3f;

            tank.Properties.Invisible = false;
            tank.Properties.Stationary = false;
            tank.Properties.CanLayTread = false;

            tank.Properties.TreadVolume = 0.02f;
            tank.Properties.TreadPitch = 0.7f;
            tank.Properties.MaxSpeed = 2.3f;

            tank.Properties.Acceleration = 0.1f;

            tank.AiParams.BlockWarinessDistance = 69;
        }
        public override void Shoot(AITank tank, ref Shell shell)
        {

            base.Shoot(tank, ref shell);
            shell.Properties.FlameColor = AssociatedColor;
        }
        public override void PreUpdate(AITank tank)
        {
            base.PreUpdate(tank);//STOP SPAWNING SHIT
            if (LevelEditor.Active) return;
            if (AIManager.CountAll(x => x.AiTankType == Type) >= Math.Floor(12*(CA_Main.Dif_Scalar_1()/1.5)) || CA_Main.KudzuRegen>0)
            {
                tank.SpecialBehaviors[1].Value = 0f;
                tank.SpecialBehaviors[0].Value = 0f;
                return;
            }
            tank.SpecialBehaviors[0].Value += TankGame.DeltaTime;
            if (tank.SpecialBehaviors[1].Value == 0)
                tank.SpecialBehaviors[1].Value = Server.ServerRandom.NextFloat(200, 550) * Math.Clamp(float.Lerp(1, 3.25f,Easings.OutCirc(AIManager.CountAll() / 7f)), 0, 1) / CA_Main.Dif_Scalar_1();

            if (tank.SpecialBehaviors[0].Value > tank.SpecialBehaviors[1].Value)
            {
                tank.SpecialBehaviors[1].Value = 0f;
                tank.SpecialBehaviors[0].Value = 0f;
                CA_Main.KudzuRegen = 300f / CA_Main.Dif_Scalar_1();
                //Check to see if within bounds
                if (tank.Position.X != Math.Clamp(tank.Position.X, MapRenderer.MIN_X, MapRenderer.MAX_X) && tank.Position.Y != Math.Clamp(tank.Position.Y, MapRenderer.MIN_Y, MapRenderer.MAX_Y)) return;

                var crate = Crate.SpawnCrate(tank.Position3D + new Vector3(0, 100, 0), 2f);
                crate.TankToSpawn = new TankTemplate()
                {
                    AiTier = Type,
                    IsPlayer = false,
                    Team = tank.Team
                };
            }
        }
    }
}
