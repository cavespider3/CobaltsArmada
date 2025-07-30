using CobaltsArmada.Objects.projectiles.futuristic;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.GameContent.UI.LevelEditor;
using TanksRebirth.Graphics;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using TanksRebirth.Net;
using static CobaltsArmada.CA_Main;
using CobaltsArmada.Script.Tanks;

namespace CobaltsArmada
{
    /// <summary>
    /// 
    /// </summary>
    public class CA_X2_CorpseFlower: CA_ArmadaTank
    {
      
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Corpse Flower"
        });

        public override string Texture => "assets/textures/tank_corpse";
        public override int Songs => 2;
        public override Color AssociatedColor => Color.OrangeRed;
        public override void PostApplyDefaults()
        {
            //TANK NO BACK DOWN
            base.PostApplyDefaults();

            AITank.Model = CA_Main.Neo_Mobile;
            AITank.Scaling = Vector3.One * 1.1f;

            AITank.Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(12);
            AITank.Parameters.RandomTimerMinMove = 40;
            AITank.Parameters.RandomTimerMaxMove = 90;
            AITank.Parameters.TurretMovementTimer = 20;
            AITank.Parameters.TurretSpeed = 0.06f;
            AITank.Parameters.AimOffset = MathHelper.ToRadians(5);


            AITank.Parameters.AggressivenessBias = 1f;

            AITank.Parameters.AwarenessHostileShell = 0;
            AITank.Parameters.AwarenessFriendlyShell = 40;
            AITank.Parameters.AwarenessHostileMine = 0;
            AITank.Parameters.AwarenessFriendlyMine = 50;

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

            AITank.Properties.Acceleration = 0.1f;

            AITank.Properties.MineCooldown = 0;
            AITank.Properties.MineLimit = 0;
            AITank.Properties.MineStun = 0;

            AITank.Parameters.ObstacleAwarenessMovement = 44;
           
        }
        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            base.TakeDamage(destroy, context);
            if (!destroy) return;
            new Explosion(AITank.Position, 12.5f, AITank);
        }
        public override void Shoot(Shell shell)
        {
            base.Shoot(shell);
             shell.Properties.FlameColor = AssociatedColor;
        }
        public override void PostUpdate()
        {
            base.PostUpdate();
            if (LevelEditorUI.IsActive || AITank.IsDestroyed || !GameScene.ShouldRenderAll || !CampaignGlobals.InMission) return;
            if (CA_Main.modifier_Difficulty > CA_Main.ModDifficulty.Lunatic)
            {
                ref Tank[] tanks = ref GameHandler.AllTanks;
                float bloat = 0f;
                for (int i = 0; i < tanks.Length; i++)
                {
                    if (tanks[i] is Tank ai)
                    {
                        //Kill Tethers Chain on Phantasm
                        if (ai == AITank || ai.IsDestroyed || AITank.Team != TeamID.NoTeam && ai.Team == AITank.Team) continue;
                        bloat =MathHelper.Clamp(1f-MathF.Max(0f,Vector2.Distance(ai.Position, AITank.Position)-75)/75,0f,1f);
                        if (bloat >= 1)
                        {
                            AITank.Properties.Armor?.Remove();
                            AITank.Damage(new TankHurtContextOther(AITank, TankHurtContextOther.HurtContext.FromIngame, "SUICIDE"), true, AssociatedColor);
                        }
                        else
                        {
                            AITank.Scaling = Vector3.One * (1.1f+(Client.ClientRandom.NextFloat(0.7f,1f)*bloat*0.25f));
                        }
                       
                    }
                }
            }
        }

    }
}
