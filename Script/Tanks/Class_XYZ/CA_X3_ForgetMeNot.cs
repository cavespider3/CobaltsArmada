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

namespace CobaltsArmada
{   
    /// <summary>
    /// A support role AITank that protects nearby allies 
    /// </summary>
    public class CA_X3_ForgetMeNot: ModTank 
    {
      
   
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Forget-Me-Not"
        });

        public override int Songs => 2;


        public override string Texture => "assets/textures/tank_forget";

        public static List<Tank> IdoledTanks = new List<Tank>();
        public override Color AssociatedColor => Color.AliceBlue;
        public override void PostApplyDefaults()
        {
            //TANK NO BACK DOWN
            base.PostApplyDefaults();
            AITank.SpecialBehaviors[0].Value = (
      CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
      CA_Main.modifier_Difficulty > ModDifficulty.Lunatic ?
      CA_Main.modifier_Difficulty > ModDifficulty.Extra ?
       150f : 105f : 75f : 60f) * 2.25f;
            AITank.SpecialBehaviors[1].Value = 
     CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
     CA_Main.modifier_Difficulty > ModDifficulty.Extra ?
        3f : 2f : 1f ;

            if (Difficulties.Types["CobaltArmada_P2"])
            {
                AITank.SpecialBehaviors[0].Value =900f;
            }

                AITank.Model = CA_Main.Neo_Mobile;
            AITank.Scaling = Vector3.One * 100.0f * 0.95f;
            var aiParams = AITank.AiParams;
            var properties = AITank.Properties;
            aiParams.MeanderAngle = MathHelper.ToRadians(40);
            aiParams.MeanderFrequency = 10;
            aiParams.TurretMeanderFrequency = 25;

            aiParams.Inaccuracy = 0.8f;

            aiParams.TurretSpeed = 0.03f;
            aiParams.AimOffset = 0.18f;

            //coward
            aiParams.PursuitLevel = -0.9f;
            aiParams.PursuitFrequency = 40;

            aiParams.ProjectileWarinessRadius_PlayerShot = 60;
            aiParams.MineWarinessRadius_PlayerLaid = 160;

            properties.TurningSpeed = 0.06f;
            properties.MaximalTurn = MathHelper.ToRadians(10);

            properties.ShootStun = 5;
            properties.ShellCooldown = 120;
            properties.ShellLimit = 2;
            properties.ShellSpeed = 3f;
            properties.ShellType = ShellID.Standard;
            properties.RicochetCount = 1;
            aiParams.ShootChance = 0.1f;
            aiParams.DeflectsBullets = true;

            properties.Invisible = false;
            properties.Stationary = false;
            properties.ShellHoming = new();

            properties.TreadPitch = -0.2f;
            properties.MaxSpeed = 1.1f;
            properties.Acceleration = 0.3f;

            aiParams.MinePlacementChance = 0.05f;

            aiParams.BlockWarinessDistance = 45;

            AITank.BaseExpValue = 0.1f;
        }

        public override void PostUpdate()
        {
            base.PostUpdate();

            if (LevelEditorUI.Active || !CampaignGlobals.InMission) return;
            if (AITank.Dead || !GameScene.ShouldRenderAll) return;

            ref Tank[] tanks = ref GameHandler.AllTanks;
            for (int i = 0; i < tanks.Length; i++)
            {
                if (tanks[i] is Tank ai)
                {
                    if (ai.Dead || ai is AITank ai2 && ai2.AiTankType == Type || ai == AITank) continue;

                    //removing the tether limit for FMN tanks

                    //if (Vector2.Distance(ai.Position, AITank.Position) > AITank.SpecialBehaviors[0].Value * 3f)
                    //{
                    //    Array.Find(CA_Idol_Tether.AllTethers, x => x is not null && x.bindHost == AITank && x.bindTarget == ai && !x.Inverse)?.Remove();        
                    //    continue;
                    //}
                    //for the idol buff to work, the target AITank mustn't already be tethered.

                    if (Array.Find(CA_Idol_Tether.AllTethers, x => x is not null && x.bindTarget == ai && !x.Inverse) is null)
                    {
                        if ((AITank.Team == TeamID.NoTeam || ai.Team == AITank.Team) && Array.FindAll(CA_Idol_Tether.AllTethers, x => x is not null && x.bindHost == AITank && !x.Inverse).Length < AITank.SpecialBehaviors[1].Value)
                            _ = new CA_Idol_Tether(AITank, ai);

                    }

                }
            }
           
        }
   


     
    }
}
