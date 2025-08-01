using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Localization;
using CobaltsArmada.Script.Tanks;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.TankSystem;

namespace CobaltsArmada
{
    /// <summary> Periwinkles
    /// 
    /// </summary>
    public class CA_02_Perwinkle: CA_ArmadaTank
    {
    
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Periwinkle"
        });

        public override string Texture => "assets/textures/tank_periwinkle";
        public override int Songs => 2;
        public override Color AssociatedColor => new Color(204, 204, 255);
       
        public override void PostApplyDefaults()
        {
            base.PostApplyDefaults();
           // Periwinkles are probably the most generic AITank of the mod

            AITank.UsesCustomModel = true;
            AITank.Model = CA_Main.Neo_Mobile;
            AITank.Scaling = Vector3.One;
            var Parameters = AITank.Parameters;
            var properties = AITank.Properties;
            Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(7);
            Parameters.RandomTimerMinMove = 20;
            Parameters.RandomTimerMaxMove = 60;
            Parameters.TurretMovementTimer = 60;
            Parameters.TurretSpeed = 0.045f;
            Parameters.AimOffset = 0.9f;


            Parameters.AggressivenessBias = 1f;

            Parameters.AwarenessHostileShell = 70;
            Parameters.AwarenessHostileMine = 140;

            properties.TurningSpeed = 0.13f;
            properties.MaximalTurn = MathHelper.PiOver4 / 2;

            properties.ShootStun = 0;
            properties.ShellCooldown = 70;
            properties.ShellLimit = 3;
            properties.ShellSpeed = 4f;
            properties.ShellType = ShellID.Standard;
            properties.RicochetCount = 1;

            properties.Invisible = false;
            properties.Stationary = false;
            properties.CanLayTread = false;

            properties.TreadPitch = 0.2f;
            properties.MaxSpeed = 1.3f;
            properties.Acceleration = 0.1f;
            properties.Deceleration = 0.1f;

            properties.MineCooldown = 60 * 20;
            properties.MineLimit = 1;
            properties.MineStun = 10;
            Parameters.SmartRicochets = true;
            Parameters.PredictsPositions = true;
            Parameters.ChanceMineLay = 0.05f;
            base.PostApplyDefaults();

            //AIParameters aiParams = AITank.Parameters;
            //TankProperties properties = AITank.Properties;

            ////## Visuals ##//

            //AITank.Scaling = Vector3.One;
            //properties.DestructionColor = CA_Main.Dandy;
            //properties.Invisible = false; //1

            ////## Mines ##//

            //aiParams.ObstacleAwarenessMine = 0; //2
            //properties.MineLimit = 0; //3
            //aiParams.RandomTimerMinMine = 0; //4
            //aiParams.RandomTimerMaxMine = 0; //5
            //aiParams.TankAwarenessMine = 0; //6
            //aiParams.ChanceMineLayNearBreakables = 0; //7
            //aiParams.ChanceMineLay = 0; //8
            //properties.MineCooldown = 0; //9
            //properties.MineLimit = 0; //10

            ////## Movement & Navigation ##//

            //properties.Acceleration = 0; //11
            //properties.Deceleration = 0; //12
            //aiParams.MaxAngleRandomTurn = 0; //13
            //aiParams.RandomTimerMaxMove = 0; //14
            //aiParams.RandomTimerMinMove = 0; //15
            //aiParams.AwarenessFriendlyMine = 0; //16
            //aiParams.AwarenessFriendlyShell = 0; //17
            //aiParams.AwarenessHostileMine = 0; //18
            //aiParams.AwarenessHostileShell = 0; //19
            //aiParams.CantShootWhileFleeing = false; //20
            //aiParams.AggressivenessBias = 0; //21
            //aiParams.MaxQueuedMovements = 0; //22
            //properties.MaxSpeed = 0; //23
            
            //properties.TurningSpeed = 0; //26
            //properties.MaximalTurn = 0; //27
            //aiParams.ObstacleAwarenessMovement = 0; //28
            //aiParams.AimOffset = 0; //29

            ////## Shells ##//

            //properties.ShellLimit = 0; //30
            //aiParams.DetectionForgivenessSelf = 0; //31
            //aiParams.DetectionForgivenessFriendly = 0; //32
            //aiParams.DetectionForgivenessHostile = 0; //33
            //properties.RicochetCount = 0; //34
            //aiParams.RandomTimerMaxShoot = 0; //35
            //aiParams.RandomTimerMinShoot = 0; //36
            //properties.ShellCooldown = 0; //37
            //properties.ShellSpeed = 0; //38
            //aiParams.TurretSpeed = 0; //39
            //aiParams.AimOffset = 0; //40
            //aiParams.TankAwarenessShoot = 0; //41
            //properties.ShootStun = 0; //42
        }
    
    }
}
