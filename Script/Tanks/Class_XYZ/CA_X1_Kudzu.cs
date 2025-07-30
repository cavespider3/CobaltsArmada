using Microsoft.Xna.Framework;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.GameMechanics;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.Coordinates;
using TanksRebirth.GameContent.UI;
using TanksRebirth.GameContent.UI.LevelEditor;
using TanksRebirth.Graphics;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using TanksRebirth.Net;
using CobaltsArmada.Script.Tanks;

namespace CobaltsArmada
{
    /// <summary> A small, fast, aggresive but very aloof tank that constantly calls for renforcements of itself. Comes equipped with a drone.</summary>
    public class CA_X1_Kudzu : CA_ArmadaTank
    {
 
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Kudzu"
        });

        public override string Texture => "assets/textures/tank_kudzu";
        public override int Songs => 5;
  
        public override Color AssociatedColor => Color.Olive;
        public override void PostApplyDefaults()
        {

            Array.Resize(ref AITank.SpecialBehaviors, AITank.SpecialBehaviors.Length + 4);
            for (int i = 0; i < AITank.SpecialBehaviors.Length; i++)
            {
                AITank.SpecialBehaviors[i] = new AiBehavior();
            }

            AITank.Model = CA_Main.Neo_Mobile;
            AITank.Scaling = Vector3.One * 0.81f;
            AITank.Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
            AITank.Parameters.RandomTimerMinMove = 10;
            AITank.Parameters.RandomTimerMaxMove = 30;
            AITank.Parameters.TurretMovementTimer = 20;
            AITank.Parameters.TurretSpeed = 0.03f;
            AITank.Parameters.AimOffset = MathHelper.ToRadians(9);

            AITank.Parameters.AggressivenessBias = 1.2f;
     

            //Clueless
            AITank.Parameters.AwarenessFriendlyShell = 0;
            AITank.Parameters.AwarenessFriendlyMine = 0;
            AITank.Parameters.AwarenessHostileMine = 0;
            AITank.Parameters.AwarenessHostileShell = 0;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(80);

            AITank.Properties.ShootStun = 0;
            AITank.Properties.ShellCooldown = 180;
            AITank.Properties.ShellLimit = 2;
            AITank.Properties.ShellSpeed = 4f;
            AITank.Properties.ShellType = ShellID.Standard;
            AITank.Properties.RicochetCount = 0;


            AITank.Properties.Invisible = false;
            AITank.Properties.Stationary = false;
            AITank.Properties.CanLayTread = false;

            AITank.Properties.TreadVolume = 0.02f;
            AITank.Properties.TreadPitch = 0.7f;
            AITank.Properties.MaxSpeed = 2.3f;

            AITank.Properties.Acceleration = 0.1f;
            AITank.SpecialBehaviors[0].Value = 300f;
            AITank.Parameters.ObstacleAwarenessMovement = 69;
            base.PostApplyDefaults();
        }
        public override void Shoot(Shell shell)
        {

            base.Shoot(shell);
            shell.Properties.FlameColor = AssociatedColor;
        }
        public override void PreUpdate()
        {
            base.PreUpdate();

            //STOP SPAWNING SHIT
            if (LevelEditorUI.IsActive) return;
            if (AITank.IsDestroyed || !GameScene.ShouldRenderAll) return;
            if (AIManager.CountAll(x => x.AiTankType == Type) >= 12)
            {
                AITank.SpecialBehaviors[0].Value = 300f;
                return;
            }

            AITank.SpecialBehaviors[0].Value -= RuntimeData.DeltaTime;

            if (AITank.SpecialBehaviors[0].Value <= 0)
            {
                AITank.SpecialBehaviors[0].Value = Server.ServerRandom.NextFloat(200, 550) * Math.Clamp(float.Lerp(1, 3.25f, Easings.OutCirc(AIManager.CountAll() / 7f)), 0, 1);
 

                //Check to see if within bounds
                if (AITank.Position.X != Math.Clamp(AITank.Position.X, GameScene.MIN_X, GameScene.MAX_X) && AITank.Position.Y != Math.Clamp(AITank.Position.Y, GameScene.MIN_Z, GameScene.MAX_Z)) return;

                var crate = Crate.SpawnCrate(AITank.Position3D + new Vector3(0, 100, 0), 2f);
                crate.TankToSpawn = new TankTemplate()
                {
                    AiTier = Type,
                    IsPlayer = false,
                    Team = AITank.Team
                };
            }
        }
    
    }
}
