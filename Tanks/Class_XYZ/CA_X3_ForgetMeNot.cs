using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.UI;
using TanksRebirth.Graphics;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Internals;
using TanksRebirth.Localization;
using TanksRebirth.Net;
using CobaltsArmada.Objects.projectiles.futuristic;
using static CobaltsArmada.CA_Main;

namespace CobaltsArmada
{
    public class CA_X3_ForgetMeNot: ModTank 
    {
      
   
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Forget-Me-Not"
        });


 

        public override string Texture => "assets/textures/tank_forget";

        public static List<Tank> IdoledTanks = new List<Tank>();
        public override Color AssociatedColor => Color.AliceBlue;
        public override void PostApplyDefaults(AITank tank)
        {
            //TANK NO BACK DOWN
            base.PostApplyDefaults(tank);
            tank.SpecialBehaviors[0].Value = (
      CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
      CA_Main.modifier_Difficulty > ModDifficulty.Lunatic ?
      CA_Main.modifier_Difficulty > ModDifficulty.Extra ?
       150f : 105f : 75f : 60f) * 2.25f;


            tank.Model = CA_Main.Neo_Mobile;
            tank.Scaling = Vector3.One * 100.0f * 0.95f;
            var aiParams = tank.AiParams;
            var properties = tank.Properties;
            aiParams.MeanderAngle = MathHelper.ToRadians(40);
            aiParams.MeanderFrequency = 10;
            aiParams.TurretMeanderFrequency = 25;

            aiParams.Inaccuracy = 0.8f;

            aiParams.TurretSpeed = 0.03f;
            aiParams.AimOffset = 0.18f;

            aiParams.PursuitLevel = -0.9f;
            aiParams.PursuitFrequency = 40;

            aiParams.ProjectileWarinessRadius_PlayerShot = 60;
            aiParams.MineWarinessRadius_PlayerLaid = 160;

            properties.TurningSpeed = 0.06f;
            properties.MaximalTurn = MathHelper.ToRadians(10);

            properties.ShootStun = 5;
            properties.ShellCooldown = 30;
            properties.ShellLimit = 2;
            properties.ShellSpeed = 3f;
            properties.ShellType = ShellID.Standard;
            properties.RicochetCount = 1;
            aiParams.ShootChance = 0.5f;

            properties.Invisible = false;
            properties.Stationary = false;
            properties.ShellHoming = new();

            properties.TreadPitch = -0.2f;
            properties.MaxSpeed = 1.2f;
            properties.Acceleration = 0.3f;

            aiParams.MinePlacementChance = 0.05f;

            aiParams.BlockWarinessDistance = 45;

            tank.BaseExpValue = 0.1f;
        }

        public override void PostUpdate(AITank tank)
        {
            base.PostUpdate(tank);

            if (LevelEditor.Active) return;
            if (tank.Dead || !GameSceneRenderer.ShouldRenderAll) return;

            ref Tank[] tanks = ref GameHandler.AllTanks;
            for (int i = 0; i < tanks.Length; i++)
            {
                if (tanks[i] is PlayerTank || tanks[i] is null || tanks[i] as AITank is null) continue;

                var ai = tanks[i] as AITank;
                if (ai is null || ai.Dead || ai.AiTankType == Type || ai == tank) continue;

                if (Vector2.Distance(ai.Position, tank.Position) > tank.SpecialBehaviors[0].Value*3f)
                {
                     Array.Find(CA_Idol_Tether.AllTethers,x => x is not null && x.bindHost == tank && x.bindTarget == ai)?.Remove();
                    continue;
                }               
                //for the idol buff to work, the target tank mustn't already be tethered.
                
                if (Array.Find(CA_Idol_Tether.AllTethers,x => x is not null && x.bindTarget == ai) is null)
                {
                    if( tank.Team == TeamID.NoTeam || ai.Team == tank.Team || CA_Main.modifier_Difficulty >= CA_Main.ModDifficulty.Lunatic)
                        _ = new CA_Idol_Tether(tank, ai); 

                }

            }
        }
   


     
    }
}
