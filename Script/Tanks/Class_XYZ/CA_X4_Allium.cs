using CobaltsArmada.Objects.projectiles.futuristic;
using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.UI.LevelEditor;
using TanksRebirth.Graphics;
using TanksRebirth.Localization;
using static CobaltsArmada.CA_Main;

namespace CobaltsArmada
{
    public class CA_X4_Allium: ModTank 
    {
      

        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Allium"
        });
        public override int Songs => 2;

        public override string Texture => "assets/textures/tank_allium";

        public override Color AssociatedColor => Color.BlueViolet;
        public override void PostApplyDefaults()
        {

            base.PostApplyDefaults();
            AITank.SpecialBehaviors[0].Value = (
      CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
      CA_Main.modifier_Difficulty > ModDifficulty.Lunatic ?
      CA_Main.modifier_Difficulty > ModDifficulty.Extra ?
       150f : 105f : 75f : 60f)*1.3f;
            AITank.SpecialBehaviors[1].Value =
     CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
     CA_Main.modifier_Difficulty > ModDifficulty.Extra ?
        3f : 2f : 1f;

            AITank.Model = CA_Main.Neo_Mobile;
            AITank.Scaling = Vector3.One * 100.0f * 0.95f;

            AITank.SpecialBehaviors[2].Value = -1;
            AITank.AiParams.MeanderAngle = MathHelper.ToRadians(30);
            AITank.AiParams.MeanderFrequency = 10;
            AITank.AiParams.TurretMeanderFrequency = 120;
            AITank.AiParams.TurretSpeed = 0.06f;
            AITank.AiParams.AimOffset = MathHelper.ToRadians(3);

            AITank.AiParams.Inaccuracy = 0.6f;

            AITank.AiParams.PursuitLevel = 0.5f;
            AITank.AiParams.PursuitFrequency = 20;

            AITank.AiParams.ProjectileWarinessRadius_PlayerShot = 0;
            AITank.AiParams.ProjectileWarinessRadius_AIShot = 0;
            AITank.AiParams.MineWarinessRadius_PlayerLaid = 0;
            AITank.AiParams.MineWarinessRadius_AILaid = 0;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 500;
            AITank.Properties.ShellLimit = 1;
            AITank.Properties.ShellSpeed = 4f;
            AITank.Properties.ShellType = ShellID.Standard;
            AITank.Properties.RicochetCount = 0;

            AITank.AiParams.ShootChance = 0.01f;

            AITank.Properties.Invisible = false;
            AITank.Properties.Stationary = false;

            AITank.Properties.TreadVolume = 0.1f;
            AITank.Properties.TreadPitch = 0.3f;
            AITank.Properties.MaxSpeed = 1.5f;

            AITank.Properties.Acceleration = 0.04f;

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
        public override void PreUpdate()
        {
            if (LevelEditorUI.Active || AITank.Dead || !GameScene.ShouldRenderAll || !CampaignGlobals.InMission) return;
            base.PreUpdate();
            if (AITank.SpecialBehaviors[2].Value < 0f){
                AITank.SpecialBehaviors[2].Value = AITank.Team;
            }
        }
        public override void PostUpdate()
        {
            base.PostUpdate();
            if (LevelEditorUI.Active || AITank.Dead || !GameScene.ShouldRenderAll || !CampaignGlobals.InMission) return;
            ref Tank[] tanks = ref GameHandler.AllTanks;
            for (int i = 0; i < tanks.Length; i++)
            {
                if (tanks[i] is Tank ai)
                {
                    //Kill Tethers Chain on Phantasm
                    if (ai.Dead || (ai is AITank ai2 && ai2.AiTankType == Type && CA_Main.modifier_Difficulty <= ModDifficulty.Lunatic) || ai == AITank) continue;

                    if (Vector2.Distance(ai.Position, AITank.Position) > AITank.SpecialBehaviors[0].Value * 2.1f)
                    {
                        Array.Find(CA_Idol_Tether.AllTethers, x => x is not null && x.bindHost == AITank && x.bindTarget == ai && x.Inverse)?.Remove();
                        continue;
                    }
                    //for the Kill Tether to work, the target AITank mustn't already be tethered.

                    if (Array.Find(CA_Idol_Tether.AllTethers, x => x is not null && x.bindTarget == ai && x.Inverse) is null)
                    {
                        if ((AITank.Team == TeamID.NoTeam || ai.Team != (int)AITank.SpecialBehaviors[2].Value ) && Array.FindAll(CA_Idol_Tether.AllTethers, x => x is not null && x.bindHost == AITank && x.Inverse).Length < AITank.SpecialBehaviors[1].Value
                            && Array.Find(CA_Idol_Tether.AllTethers, x => x is not null && (x.bindHost == ai || x.bindTarget == ai) && x.Inverse) is null
                            )
                            _ = new CA_Idol_Tether(AITank, ai, true);

                    }
                }
            }
            bool Tethered = Array.FindAll(CA_Idol_Tether.AllTethers, x => x is not null && x.bindHost == AITank).Length > 0;
            AITank.AiParams.ProjectileWarinessRadius_PlayerShot = Tethered ? 0 : 60;
            AITank.AiParams.ProjectileWarinessRadius_AIShot = Tethered ? 0 : 60;
            AITank.AiParams.MineWarinessRadius_PlayerLaid = Tethered ? 0 : 150;
            AITank.AiParams.MineWarinessRadius_AILaid = Tethered ? 0 : 150;
            AITank.AiParams.PursuitLevel = Tethered ? 0.9f : 0.1f;
            AITank.AiParams.PursuitFrequency = Tethered ? 3 : 75;
            AITank.Properties.MaxSpeed = Tethered ? 1.2f : 1.7f;
            AITank.AiParams.ShootChance = Tethered ? 0.00f : 0.01f;
            AITank.Properties.ShellLimit = Tethered ? 0 : 1;
            AITank.Team = Tethered ? TeamID.Magenta : (int)AITank.SpecialBehaviors[2].Value;

        }

       
    }
}
