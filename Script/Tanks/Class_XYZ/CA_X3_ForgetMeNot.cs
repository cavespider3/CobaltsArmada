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
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.UI.LevelEditor;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.GameContent.Systems.AI;
using CobaltsArmada.Script.Tanks;

namespace CobaltsArmada
{   
    /// <summary>
    /// A support tank that protects nearby allies 
    /// </summary>
    public class CA_X3_ForgetMeNot: CA_ArmadaTank
    {
      
   
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Forget-Me-Not"
        });

        public override int Songs => 2;

        public float TetherDistance = 0;
        public int TetherLimit = 1;

        public override string Texture => "assets/textures/tank_forget";

        public static List<Tank> IdoledTanks = new List<Tank>();
        public override Color AssociatedColor => Color.AliceBlue;
        public override void PostApplyDefaults()
        {

            //TANK NO BACK DOWN
            base.PostApplyDefaults();
            TetherDistance = (
      CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
      CA_Main.modifier_Difficulty > ModDifficulty.Lunatic ?
      CA_Main.modifier_Difficulty > ModDifficulty.Extra ?
       150f : 105f : 75f : 60f) * 2.25f;
            TetherLimit =( 
     CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
     CA_Main.modifier_Difficulty > ModDifficulty.Extra ?
        3 : 2 : 1 );

            if (Modifiers.Map[M_IDOL])
            {
                TetherDistance =900f;
            }

                AITank.DrawParamsTank.Model = CA_Main.Neo_Mobile;
            AITank.DrawParams.Scaling = Vector3.One * 0.95f;
            var Parameters = AITank.Parameters;
            var properties = AITank.Properties;
            Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(40);
            Parameters.RandomTimerMinMove = 10;
            Parameters.RandomTimerMaxMove = 10;
            Parameters.TurretMovementTimer = 25;


            Parameters.TurretSpeed = 0.03f;
            Parameters.AimOffset = 0.18f;

            //coward
            Parameters.AggressivenessBias = -0.9f;


            Parameters.AwarenessHostileShell = 60;
            Parameters.AwarenessHostileMine = 160;

            properties.TurningSpeed = 0.06f;
            properties.MaximalTurn = MathHelper.ToRadians(10);

            properties.ShootStun = 5;
            properties.ShellCooldown = 120;
            properties.ShellLimit = 2;
            properties.ShellSpeed = 3f;
            properties.ShellType = ShellID.Standard;
            properties.RicochetCount = 1;
            Parameters.RandomTimerMinShoot = 30;
            Parameters.RandomTimerMaxShoot = 120;

            Parameters.DeflectsBullets = true;

            properties.Invisible = false;
            properties.Stationary = false;
            properties.ShellHoming = new();

            properties.TreadPitch = -0.2f;
            properties.MaxSpeed = 1.1f;
            properties.Acceleration = 0.3f;

            Parameters.ObstacleAwarenessMovement = 45;
            AITank.Parameters.DetectionForgivenessSelf = MathHelper.ToRadians(5);
            AITank.Parameters.DetectionForgivenessFriendly = MathHelper.ToRadians(20);
            AITank.Parameters.DetectionForgivenessHostile = MathHelper.ToRadians(20);
            AITank.Parameters.MaxQueuedMovements = 4;
            AITank.Parameters.TankAwarenessShoot = 50;
        }

        public override void PostUpdate()
        {
            base.PostUpdate();

            if (LevelEditorUI.IsActive || !CampaignGlobals.InMission) return;
            if (AITank.IsDestroyed || !GameScene.UpdateAndRender) return;

            ref Tank[] tanks = ref GameHandler.AllTanks;
            for (int i = 0; i < tanks.Length; i++)
            {
                if (tanks[i] is Tank ai)
                {
                    if (ai.IsDestroyed || ai is AITank ai2 && ai2.AiTankType == Type || ai == AITank) continue;

                    //removing the tether limit for FMN tanks

                    //if (Vector2.Distance(ai.Position, AITank.Position) > TetherDistance * 3f)
                    //{
                    //    Array.Find(CA_Idol_Tether.AllTethers, x => x is not null && x.bindHost == AITank && x.bindTarget == ai && !x.Inverse)?.Remove();        
                    //    continue;
                    //}
                    //for the idol buff to work, the target AITank mustn't already be tethered.

                    if (Array.Find(CA_Idol_Tether.AllTethers, x => x is not null && x.bindTarget == ai && !x.Inverse) is null)
                    {
                        if ((AITank.Team == TeamID.NoTeam || ai.Team == AITank.Team) && Array.FindAll(CA_Idol_Tether.AllTethers, x => x is not null && x.bindHost == AITank && !x.Inverse).Length < TetherLimit)
                            _ = new CA_Idol_Tether(AITank, ai);

                    }

                }
            }
          
        }
   


     
    }
}
