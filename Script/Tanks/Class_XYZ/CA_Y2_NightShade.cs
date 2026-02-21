using CobaltsArmada.Script.Tanks;
using CobaltsArmada.Script.Tanks.Class_T;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.GameMechanics;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.GameContent.UI;
using TanksRebirth.GameContent.UI.LevelEditor;
using TanksRebirth.Graphics;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.Framework;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.Internals.Common.Framework.Interfaces;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using TanksRebirth.Net;
using static CobaltsArmada.CA_Main;
//Boss AITank
namespace CobaltsArmada
{
    public class CA_Y2_NightShade : CA_ArmadaTank
    {
        /// <summary>
        /// The 2nd boss AITank you fight, fought and rematched at mission's 40 and 97(only on extra and above).
        /// function, while active, this AITank calls for backup. any tanks closeby are poisioned, becoming much more aggressive
        /// </summary
        public override int Songs => 1;
        public override bool HasSong => true;
        public override LocalizedString Name => new()
        {
            [LangCode.English] = "Nightshade"
        };

        public override LocalizedString Description => new()
        {
            [LangCode.English] = "A boss tank that inflicts nearby ally tanks with nightshade. Comes with two drones that either bring in backup, or drop nightshade bombs. Has slow natural regeneration, and releases a shockwave when below 50% health."
        };

        public override string Texture => "assets/textures/tank_nightshade";

        public override Color AssociatedColor => Color.DarkViolet;

       

        public override void OnLoad()
        {
            base.OnLoad();
        }
        public int Health;
        public float RegenTimer;
        public float PhaseFlag = 0;
        public float PoisonShock = 0;
        public float PoisonShockTimer = 0;

        public float NightNadeTimer = 0f;
        public Vector2 GloomPulse = Vector2.Zero;
        public override void PostApplyDefaults()
        {

            Health = (Modifiers.Map[Modifiers.RANDOM_ENEMY] ? 3 : 15) + Server.CurrentClientCount * 10;
            //TANK NO BACK DOWN
            base.PostApplyDefaults();
            AITank.Properties.Armor = new TankArmor(AITank, Health);
            AITank.Properties.Armor.HideArmor = true;
            // CA_Main.boss = new BossBar(AITank, "Nightshade", "The Infector");

            PoisonShockTimer =
           CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
           CA_Main.modifier_Difficulty > ModDifficulty.Lunatic ?
           CA_Main.modifier_Difficulty > ModDifficulty.Extra ?
            150f : 105f : 75f : 60f;

            AITank.DrawParamsTank.Model = CA_Main.Neo_Boss;
            AITank.DrawParams.Scaling = Vector3.One * 1.03f;

            AITank.Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
            AITank.Parameters.RandomTimerMinMove = 10;
            AITank.Parameters.RandomTimerMaxMove = 20;
            AITank.Parameters.TurretMovementTimer = 20;
            AITank.Parameters.TurretSpeed = 0.06f;
            AITank.Parameters.AimOffset = MathHelper.ToRadians(20);

            AITank.Parameters.MaxQueuedMovements = 5;

            AITank.Parameters.AggressivenessBias = 0.1f;

            AITank.Parameters.AwarenessHostileShell = 15;
            AITank.Parameters.AwarenessFriendlyShell = 40;
            AITank.Parameters.AwarenessHostileMine = 100;
            AITank.Parameters.AwarenessFriendlyMine = 50;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 250;
            AITank.Properties.ShellLimit = 3;
            AITank.Properties.ShellSpeed = 2.7f;
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
            AITank.Parameters.DetectionForgivenessSelf = MathHelper.ToRadians(5);
            AITank.Parameters.DetectionForgivenessFriendly = MathHelper.ToRadians(20);
            AITank.Parameters.DetectionForgivenessHostile = MathHelper.ToRadians(20);
            AITank.Parameters.TankAwarenessShoot = 120;
        }
        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            if(AITank.Properties.Armor is not null)
            {
                RegenTimer = 0;
                PhaseFlag = AITank.Properties.Armor!.HitPoints <= (int)MathF.Floor(Health / 2) || PhaseFlag == 1f ? 1f : 0f;

            }
            base.TakeDamage(destroy, context);
        }

       

        public override void PostUpdate()
        {
          
            base.PostUpdate();
            if (LevelEditorUI.IsActive || AITank.IsDestroyed || !GameScene.UpdateAndRender || !CampaignGlobals.InMission) return;
            bool Phase2 = PhaseFlag == 1;
            Vector2 smokey = Vector2.One.RotatedBy(Client.ClientRandom.NextFloat(-MathF.PI, MathF.PI)) * Client.ClientRandom.NextFloat(0.1f, 130);
            var smoke = GameHandler.Particles.MakeParticle(AITank.Position3D + smokey.ExpandZ(),
                GameResources.GetGameResource<Texture2D>("Assets/textures/misc/tank_smokes"));
            smoke.Pitch = -CameraGlobals.DEFAULT_ORTHOGRAPHIC_ANGLE;
            smoke.Scale = new(0.8f * Client.ClientRandom.NextFloat(0.1f, 1f));
            smoke.Color = Color.DarkViolet;
            smoke.HasAdditiveBlending = false;
            smoke.UniqueBehavior = (part) => {

                GeometryUtils.Add(ref part.Scale, -0.004f * RuntimeData.DeltaTime);
                part.Position += Vector3.UnitY * 1f * RuntimeData.DeltaTime;
                part.Alpha -= 0.04f * RuntimeData.DeltaTime;

                if (part.Alpha <= 0)
                    part.Destroy();

            };

            if (LevelEditorUI.IsActive) return;
            RegenTimer += RuntimeData.DeltaTime;
            // it has regen shield
            if (RegenTimer > GetValueByDifficulty(60 * 3, 60 * 2, 60 * 1.5, 60 * 1) * (Phase2 ? 0.75f : 1f) &&
                AITank.Properties.Armor!.HitPoints < Health)
            {
                AITank.Properties.Armor!.HitPoints += 1;
                RegenTimer = 0;
            }

            if (Phase2)
            {
                NightNadeTimer += RuntimeData.DeltaTime;

                if (NightNadeTimer > 600)
                {
                    
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 smokey2 = Vector2.One.RotatedBy(Client.ClientRandom.NextFloat(-MathF.PI, MathF.PI)) * (Client.ClientRandom.NextFloat(-2,2)+ (NightNadeTimer - 595)*3);
                        var smoke2 = GameHandler.Particles.MakeParticle(GloomPulse.ExpandZ() + smokey2.ExpandZ() + Vector3.UnitY *3,
                            GameResources.GetGameResource<Texture2D>("Assets/textures/misc/tank_smokes"));
                        smoke2.Pitch = -CameraGlobals.DEFAULT_ORTHOGRAPHIC_ANGLE;
                        smoke2.Scale = new(0.8f * Client.ClientRandom.NextFloat(0.1f, 1f));
                        smoke2.Color = Color.DarkViolet;
                        smoke2.HasAdditiveBlending = false;
                        smoke2.UniqueBehavior = (part) => {

                            GeometryUtils.Add(ref part.Scale, -0.004f * RuntimeData.DeltaTime);
                            part.Position += Vector3.UnitY * 1f * RuntimeData.DeltaTime;
                            part.Alpha -= 0.04f * RuntimeData.DeltaTime;

                            if (part.Alpha <= 0)
                                part.Destroy();

                        };
                    }
                    ref Tank[] tanks2 = ref GameHandler.AllTanks;
                    for (int i = 0; i < tanks2.Length; i++)
                    {
                        if (tanks2[i] is Tank ai)
                        {

                            if (ai.IsDestroyed || AITank == ai || ai.Team != AITank.Team && AITank.Team != TeamID.NoTeam ||
                                ai is AITank ai2 && (ai2.AiTankType == NightShade || ai2.AiTankType == Lily)) continue;

                            if (Vector2.Distance(ai.Position, AITank.Position3D.FlattenZ()) < ((NightNadeTimer - 595) * 3 - 2f) ||
                                Vector2.Distance(ai.Position, AITank.Position3D.FlattenZ()) > ((NightNadeTimer - 595) * 3 + 2f)) continue;
                            PoisonTank(ai);
                        }
                    }

                    if(NightNadeTimer > 600 + 300)
                    {
                        NightNadeTimer = 0;
                    }
                }
                else
                {
                    GloomPulse = AITank.Position;
                 }

                //if(NightNadeTimer > 300)
                //{
                //    CA_Utils.CreateNightShadeGrenade(GameHandler.Particles, AITank.Position3D + Vector3.UnitY * 20f, (Vector2.One.Rotate(Client.ClientRandom.NextFloat(-MathF.PI, MathF.PI)).ExpandZ() + Vector3.Up) * new Vector3(1,3,1), AITank.Team);
                //    NightNadeTimer = 0f;
                //}
            }


            PoisonShockTimer += RuntimeData.DeltaTime;
            if (PoisonShock == 0)
                PoisonShock = 5;

            if (PoisonShockTimer > PoisonShock)
            {
                PoisonShock = 0f;
                PoisonShockTimer = 0f;
                ref Tank[] tanks = ref GameHandler.AllTanks;
                for (int i = 0; i < tanks.Length; i++)
                {
                    if (tanks[i] is Tank ai)
                    {

                        if (ai.IsDestroyed || AITank == ai || ai.Team != AITank.Team && AITank.Team != TeamID.NoTeam ||
                            ai is AITank ai2 && (ai2.AiTankType == NightShade || ai2.AiTankType == Lily)) continue;

                        if (Vector2.Distance(ai.Position, AITank.Position3D.FlattenZ()) > 130) continue;
                        PoisonTank(ai);
                    }
                }
            }
            foreach(CA_Drone drone in CA_Drone.AllDrones)
            {
                if(drone is not null && drone.droneOwner == AITank)
                {
                    drone.Parameters.ValidRecruits = AITank.Properties.Armor!.HitPoints <= (int)MathF.Floor(Health / 2) || PhaseFlag == 1 ?
                        CA_Main.GetValueByDifficulty<int[]>(
                            [TankID.Pink,TankID.Yellow,TankID.Marine,TankID.Violet], //normal phase 2
                            [TankID.Pink, TankID.Yellow, TankID.Marine, TankID.Violet], //hard phase 2
                            [TankID.Pink, TankID.Yellow, TankID.Marine, TankID.Violet], //lunatic phase 2
                            [TankID.Obsidian,TankID.Black,Lavender,TankID.Amethyst,TankID.Sapphire,TankID.Gold,Carnation,ForgetMeNot,CorpseFlower,Allium] //extra phase 2 (You will suffer)
                            ) :
                            CA_Main.GetValueByDifficulty<int[]>(
                            [TankID.Brown,TankID.Marine,TankID.Ash,TankID.Yellow], //normal phase 1
                            [TankID.Brown, TankID.Marine, TankID.Ash, TankID.Yellow], //hard phase 1
                            [TankID.Bronze,TankID.Silver,Dandelion,Periwinkle], //lunatic phase 1
                            [TankID.Pink, TankID.Yellow, TankID.Marine, TankID.Violet] //extra phase 1
                            );
                }

            }
   

    
          
        
    }
        

     


    }
}
