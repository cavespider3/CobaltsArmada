using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Internals.Common.Framework.Interfaces;
using TanksRebirth.Localization;
using CobaltsArmada.Script.Tanks;
using TanksRebirth.Net;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth;
using CobaltsArmada.Script.Tanks.Class_T;
//Boss AITank
namespace CobaltsArmada
{
    public class CA_Y1_Lotus: CA_ArmadaTank
    {
        /// <summary>
        /// The 1st boss AITank you fight, fought and rematched at mission's 20 and 96(only on extra and above)
        /// while active,
        /// </summary
        public override int Songs => 2;
         public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Lotus"
        });

        public override string Texture => "assets/textures/tank_lotus";
    
        public override Color AssociatedColor => Color.Magenta;
        
        public override void PostApplyDefaults()
        {
            Array.Resize(ref AITank.SpecialBehaviors,2);
            for (int i = 0; i < AITank.SpecialBehaviors.Length; i++)
            {
                AITank.SpecialBehaviors[i] = new TanksRebirth.GameContent.GameMechanics.AiBehavior();
                AITank.SpecialBehaviors[i].Value = 180f;
            }
            AITank.SpecialBehaviors[1].Value = 20 + Server.CurrentClientCount * 5;
            //TANK NO BACK DOWN
            base.PostApplyDefaults();
            AITank.Properties.Armor = new TankArmor(AITank, 20 + Server.CurrentClientCount * 5);
            AITank.Properties.Armor.HideArmor = true;
            AITank.Model = CA_Main.Neo_Boss;
            AITank.Scaling = Vector3.One * 1.3f;

            AITank.Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
            AITank.Parameters.RandomTimerMinMove = 10;
            AITank.Parameters.RandomTimerMaxMove = 10;
            AITank.Parameters.TurretMovementTimer = 20;
            AITank.Parameters.TurretSpeed = 0.06f;
            AITank.Parameters.AimOffset = MathHelper.ToRadians(45);

            AITank.Parameters.AggressivenessBias = 0.1f;

            AITank.Parameters.AwarenessHostileShell = 120;
            AITank.Parameters.AwarenessFriendlyShell = 120;
            AITank.Parameters.AwarenessHostileMine = 10;
            AITank.Parameters.AwarenessFriendlyMine = 50;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 240;
            AITank.Properties.ShellLimit = 2;
            AITank.Properties.ShellSpeed = 4f;
            AITank.Properties.ShellType = ModContent.GetSingleton<CA_Shell_Glaive>().Type;
            AITank.Properties.RicochetCount = 7;



            AITank.Properties.Invisible = false;
            AITank.Properties.Stationary = false;

            AITank.Properties.TreadVolume = 0.1f;
            AITank.Properties.TreadPitch = 0.3f;
            AITank.Properties.MaxSpeed = 1.5f;

            AITank.Properties.Acceleration = 0.1f;

            AITank.Properties.MineCooldown = 0;
            AITank.Properties.MineLimit = 0;
            AITank.Properties.MineStun = 0;

            AITank.Parameters.ObstacleAwarenessMovement = 60;
            AITank.Parameters.DetectionForgivenessSelf = MathHelper.ToRadians(5);
            AITank.Parameters.DetectionForgivenessFriendly = MathHelper.ToRadians(20);
            AITank.Parameters.DetectionForgivenessHostile = MathHelper.ToRadians(20);
            AITank.Parameters.MaxQueuedMovements = 4;
            AITank.Parameters.TankAwarenessShoot = 120;
        }
        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            if(AITank.Properties.Armor!.HitPoints == (int)MathF.Floor(AITank.SpecialBehaviors[1].Value /2))
            {
                //Lore :3
              var Bobbert = new CA_Drone(AITank, AITank.Physics.Position);
              Bobbert.Task = CA_Drone.DroneTask.Recruit;
            }
            base.TakeDamage(destroy, context);

        }
        public override void Shoot(Shell shell)
        {
            base.Shoot( shell);
            shell.SwapTexture(CA_Main.Tank_Y1);
            shell.Properties.FlameColor = AssociatedColor;
        }
        public override bool CustomAI()
        {
            float speed = 5.8f;
            Shell.HomingProperties homing = new() { Cooldown = 0, Power = speed * 0.03f, Speed = speed, Radius = 500f };

            if (!AITank.TanksNearShootAwareness.Any(x => AITank.IsOnSameTeamAs(x.Team))&& !AITank.IsTooCloseToObstacle && (AITank.TargetTank is Tank target && CA_Utils.UnobstructedRaycast(AITank.Position,target.Position,(v2)=>CA_Utils.UnobstructedPosition(v2) ) || AITank.SpecialBehaviors[0].Value <60))
            {
                if (AITank.SpecialBehaviors[0].Value<=35 && (int)AITank.SpecialBehaviors[0].Value % 7 == 0)
                {
                    Vector2 sending = MathUtils.Rotate(Vector2.UnitY, -AITank.TurretRotation + MathHelper.ToRadians(Client.ClientRandom.NextFloat(-45,45)+180f));
                    new Shell(sending * 25f + AITank.Position, sending * speed,ShellID.Rocket,AITank,0,homing,true);

                    if (AITank.SpecialBehaviors[0].Value <= 0) AITank.SpecialBehaviors[0].Value = 120;
                }

                AITank.SpecialBehaviors[0].Value -= RuntimeData.DeltaTime;



            }



            return true;
        }



    }
}
