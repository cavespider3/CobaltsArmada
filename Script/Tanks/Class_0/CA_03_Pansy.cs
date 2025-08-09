using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.UI;
using TanksRebirth.GameContent.UI.LevelEditor;
using TanksRebirth.Graphics;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using TanksRebirth.Net;
using CobaltsArmada.Script.Tanks;

namespace CobaltsArmada
{   
    /// <summary> Pansys will alternate between a spray of fast missles at long range, and a shotgun burst with insane recoil at close ranges
    /// 
    /// </summary>
    public class CA_03_Pansy : CA_ArmadaTank
    {
        
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Pansy"
        });

        public override string Texture => "assets/textures/tank_pansy";
        public override int Songs => 2;
        public override Color AssociatedColor => Color.RoyalBlue;
        public override void PostApplyDefaults()
        {
            base.PostApplyDefaults();
            AITank.UsesCustomModel = true;
            AITank.Model = CA_Main.Neo_Mobile;
            AITank.Scaling = Vector3.One;
            var Parameters = AITank.Parameters;
            var properties = AITank.Properties;
            Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
            Parameters.RandomTimerMaxMove = 26;
            Parameters.RandomTimerMaxMove = 10;
            Parameters.TurretMovementTimer = 20;
            Parameters.TurretSpeed = 0.025f;
            Parameters.AimOffset = 0.2f;

            Parameters.AggressivenessBias = -0.03f;

            Parameters.AwarenessHostileShell = 40;
            Parameters.AwarenessHostileMine = 70;
            Parameters.DetectionForgivenessSelf = MathHelper.ToRadians(5);
            Parameters.DetectionForgivenessFriendly = MathHelper.ToRadians(20);
            Parameters.DetectionForgivenessHostile = MathHelper.ToRadians(20);

            Parameters.TankAwarenessShoot = 50;
            properties.TurningSpeed = 0.15f;
            properties.MaximalTurn = MathHelper.PiOver2;

            properties.ShootStun = 20;
            properties.ShellCooldown = 3;
            //   properties.ShellShootCount = 3;
            properties.ShellLimit = 1;
            //ModContent.GetSingleton<CA_ShatterBouncer>().Type
            properties.ShellSpeed = 5.25f;
         
            properties.ShellType = ShellID.Rocket;
            properties.RicochetCount = 0;

            properties.Invisible = false;
            properties.Stationary = false;
            properties.ShellHoming = new();
            properties.CanLayTread = false;
            properties.TreadPitch = 0.08f;
            properties.MaxSpeed = 1.4f;
            properties.Acceleration = 0.3f;
            properties.Deceleration = 0.6f;
            AITank.Parameters.MaxQueuedMovements = 4;

        }

        public override void PreUpdate()
        {
            base.PreUpdate();
            if (LevelEditorUI.IsActive || AITank.IsDestroyed || !GameScene.ShouldRenderAll || !CampaignGlobals.InMission) return;
            var properties = AITank.Properties;
            if (AITank.TargetTank is not null && AITank.SeesTarget)
            {
                var isInShotgunRange = Vector2.Distance(AITank.Position, AITank.TargetTank.Position) <= 300f;
                //Ruby
                if (!isInShotgunRange)
                {
                    properties.ShootStun = 1;
                    properties.ShellCooldown = 30;
                    properties.ShellLimit = 8;
                    properties.ShellSpeed = 5.6f;
                    properties.ShellType = ShellID.Rocket;
                    properties.ShellShootCount = 1;
                    properties.Recoil = 0f;
                    AITank.Parameters.TankAwarenessShoot = 50;
                }
                else //SHOTGUN
                {
                    AITank.Parameters.TankAwarenessShoot = 140;
                    properties.ShootStun = 40;
                    properties.ShellCooldown = 60;
                    properties.ShellLimit = 15;
                    properties.ShellSpeed = 3f;
                    properties.ShellShootCount = 5;
                    properties.ShellSpread = 0.41f;
                    properties.ShellType = ShellID.Standard;
                    properties.Recoil = 4.1f;
                }

            }

        }
     
    }
}
