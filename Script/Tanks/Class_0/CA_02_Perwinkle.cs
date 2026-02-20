using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Localization;
using CobaltsArmada.Script.Tanks;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.GameContent.Systems.TankSystem.AI;

namespace CobaltsArmada
{
    /// <summary> Periwinkles are rather average tanks, with the small exception of marking invisible tanks (for about 10 minutes... missions shouldn't last that long though)
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
            AITank.DrawParamsTank.Model = CA_Main.Neo_Mobile!;
           

            AIParameters aiParams = AITank.Parameters;
            TankProperties properties = AITank.Properties;

            //## Visuals ##//

        
            properties.Invisible = false; //1

            //## Mines ##//

            aiParams.ObstacleAwarenessMine = 210; //2
            properties.MineLimit = 0; //3
            aiParams.RandomTimerMinMine = 0; //4
            aiParams.RandomTimerMaxMine = 0; //5
            aiParams.TankAwarenessMine = 100; //6
            aiParams.ChanceMineLayNearBreakables = 0; //7
            aiParams.ChanceMineLay = 0; //8
            properties.MineCooldown = 0; //9
            properties.MineLimit = 0; //10

            //## Movement & Navigation ##//

            properties.Acceleration = 0.04f; //11
            properties.Deceleration = 0.01f; //12
            aiParams.MaxAngleRandomTurn = MathHelper.ToRadians(60); //13
        
            aiParams.RandomTimerMaxMove = 30; //14
            aiParams.RandomTimerMinMove = 10; //15
            aiParams.AwarenessFriendlyMine = 0; //16
            aiParams.AwarenessFriendlyShell = 120; //17
            aiParams.AwarenessHostileMine = 130; //18
            aiParams.AwarenessHostileShell = 80; //19
            aiParams.CantShootWhileFleeing = true; //20
            aiParams.AggressivenessBias = 0.3f; //21
            aiParams.MaxQueuedMovements = 4; //22
            properties.MaxSpeed = 1.3f; //23

            properties.TurningSpeed = 0.08f; //26
            properties.MaximalTurn = MathHelper.ToRadians(30); //27
            aiParams.ObstacleAwarenessMovement = 80; //28
            aiParams.AimOffset = MathHelper.ToRadians(15); //29
            aiParams.PredictsPositions = true;
            aiParams.Rememberance = 3600;
            

            //## Shells ##//

            properties.ShellLimit = 7; //30
            aiParams.DetectionForgivenessSelf = MathHelper.ToRadians(5); //31
            aiParams.DetectionForgivenessFriendly = MathHelper.ToRadians(15); //32
            aiParams.DetectionForgivenessHostile = MathHelper.ToRadians(35); //33
            properties.RicochetCount = 0; //34
            aiParams.RandomTimerMaxShoot = 45; //35
            aiParams.RandomTimerMinShoot = 2; //36
            properties.ShellCooldown = 40; //37
            properties.ShellSpeed = 7f; //38
            aiParams.TurretSpeed = 0.05f; //39
            aiParams.TurretMovementTimer = 80; //40
            aiParams.TankAwarenessShoot = 130; //41
            properties.ShootStun = 20; //42
            properties.Recoil = 1f;

            properties.ShellType = ShellID.TrailedRocket;
            properties.ShellHoming = new() { Cooldown = 20,HeatSeeks= true, Radius = 500f,Power = properties.ShellSpeed * 0.03f, Speed = properties.ShellSpeed};

        }

        public override bool CustomAI()
        {
            if(AITank.TargetTank is Tank tank && tank.Properties.Invisible)
            {
                tank.TimeSinceLastAction = -6000;
            }
            return true;
        }
    }
}
