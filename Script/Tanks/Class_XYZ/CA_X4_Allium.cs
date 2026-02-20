using CobaltsArmada.Objects.projectiles.futuristic;
using Microsoft.Xna.Framework;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.GameContent.UI.LevelEditor;
using TanksRebirth.Graphics;
using TanksRebirth.Localization;
using static CobaltsArmada.CA_Main;
using CobaltsArmada.Script.Tanks;

namespace CobaltsArmada
{
    public class CA_X4_Allium: CA_ArmadaTank
    {
      

        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Allium"
        });
        public override int Songs => 2;

        public float MutanyTimer;
        public int Team;
        public int TetherLimit;
        public float TetherDistance;


        public override string Texture => "assets/textures/tank_allium";

        public override Color AssociatedColor => Color.BlueViolet;
        public override void PostApplyDefaults()
        {
          
            base.PostApplyDefaults();
            MutanyTimer= 0;
            TetherDistance = 75f;
            TetherLimit =
     CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
     CA_Main.modifier_Difficulty > ModDifficulty.Extra ?
        2 : 1 : 1;

            AITank.DrawParamsTank.Model = CA_Main.Neo_Mobile;
            AITank.DrawParams.Scaling = Vector3.One * 0.95f;
           
            Team = -1;
            AITank.Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
            AITank.Parameters.RandomTimerMinMove = 10;
            AITank.Parameters.RandomTimerMaxMove = 10;
            AITank.Parameters.TurretMovementTimer = 120;
            AITank.Parameters.TurretSpeed = 0.06f;
            AITank.Parameters.AimOffset = MathHelper.ToRadians(33);



            AITank.Parameters.AggressivenessBias = 0.5f;

            AITank.Parameters.AwarenessHostileShell = 0;
            AITank.Parameters.AwarenessFriendlyShell = 0;
            AITank.Parameters.AwarenessHostileMine = 0;
            AITank.Parameters.AwarenessFriendlyMine = 0;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 500;
            AITank.Properties.ShellLimit = 1;
            AITank.Properties.ShellSpeed = 4f;
            AITank.Properties.ShellType = ShellID.Standard;
            AITank.Properties.RicochetCount = 0;

            AITank.Properties.Invisible = false;
            AITank.Properties.Stationary = false;

            AITank.Properties.TreadVolume = 0.1f;
            AITank.Properties.TreadPitch = 0.3f;
            AITank.Properties.MaxSpeed = 1.5f;

            AITank.Properties.Acceleration = 0.04f;

            AITank.Properties.MineCooldown = 0;
            AITank.Properties.MineLimit = 0;
            AITank.Properties.MineStun = 0;

            AITank.Parameters.ObstacleAwarenessMovement = 44;
            AITank.Parameters.DetectionForgivenessSelf = MathHelper.ToRadians(5);
            AITank.Parameters.DetectionForgivenessFriendly = MathHelper.ToRadians(20);
            AITank.Parameters.DetectionForgivenessHostile = MathHelper.ToRadians(20);
            AITank.Parameters.MaxQueuedMovements = 4;
        }
        public override void Shoot(Shell shell)
        {

            base.Shoot(shell);
             shell.Properties.FlameColor = AssociatedColor;
        }
        public override void PreUpdate()
        {
            base.PreUpdate();
            if (LevelEditorUI.IsActive || AITank.IsDestroyed || !GameScene.UpdateAndRender || !CampaignGlobals.InMission) return;
            if (Team < 0f){
                Team = AITank.Team;
            }
        }
        public override void PostUpdate()
        {
            base.PostUpdate();
            if (LevelEditorUI.IsActive || AITank.IsDestroyed || !GameScene.UpdateAndRender || !CampaignGlobals.InMission) return;
            ref Tank[] tanks = ref GameHandler.AllTanks;
            for (int i = 0; i < tanks.Length; i++)
            {
                if (tanks[i] is Tank ai)
                {
                    //Kill Tethers Chain on Phantasm
                    if (ai.IsDestroyed || (ai is AITank ai2 && ai2.AiTankType == Type && CA_Main.modifier_Difficulty <= ModDifficulty.Lunatic) || ai == AITank) continue;

                    if (Vector2.Distance(ai.Position, AITank.Position) > TetherDistance * 2.1f)
                    {
                        Array.Find(CA_Idol_Tether.AllTethers, x => x is not null && x.bindHost == AITank && x.bindTarget == ai && x.Inverse)?.Remove();
                        continue;
                    }
                    //for the Kill Tether to work, the target AITank mustn't already be tethered.

                    if (Array.Find(CA_Idol_Tether.AllTethers, x => x is not null && x.bindTarget == ai && x.Inverse) is null)
                    {
                        if ((AITank.Team == TeamID.NoTeam || ai.Team != (int)Team ) && Array.FindAll(CA_Idol_Tether.AllTethers, x => x is not null && x.bindHost == AITank && x.Inverse).Length < TetherLimit
                            && Array.Find(CA_Idol_Tether.AllTethers, x => x is not null && (x.bindHost == ai || x.bindTarget == ai) && x.Inverse) is null
                            )
                            _ = new CA_Idol_Tether(AITank, ai, true);

                    }
                }
            }
            bool Tethered = Array.FindAll(CA_Idol_Tether.AllTethers, x => x is not null && x.bindHost == AITank).Length > 0;
            AITank.Parameters.AwarenessHostileShell = Tethered ? 0 : 60;
            AITank.Parameters.AwarenessFriendlyShell = Tethered ? 0 : 60;
            AITank.Parameters.AwarenessHostileMine = Tethered ? 0 : 150;
            AITank.Parameters.AwarenessFriendlyMine = Tethered ? 0 : 150;
            AITank.Parameters.AggressivenessBias = Tethered ? 0.9f : 0.1f;
            AITank.Properties.MaxSpeed = Tethered ? 1.2f : 1.9f;
            AITank.Properties.ShellLimit = Tethered ? 0 : 1;
            if (Tethered)
            {
                if(Array.FindAll(CA_Idol_Tether.AllTethers, x => x is not null && x.Inverse && x.bindTarget is PlayerTank && x.bindHost == AITank) is not null)
                {
                    MutanyTimer += RuntimeData.DeltaTime;
                }
                else
                {
                    MutanyTimer += RuntimeData.DeltaTime*4f;
                }
            }
            else
            {
                MutanyTimer -= RuntimeData.DeltaTime*5f;
            }
            MutanyTimer = MathHelper.Clamp(MutanyTimer, 0, 300f);
            AITank.Team = Tethered && MutanyTimer > 300f ? TeamID.Magenta : (int)Team;
           
        }

       
    }
}
