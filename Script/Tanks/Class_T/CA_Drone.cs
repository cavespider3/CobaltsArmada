using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TanksRebirth.Internals.Common.Utilities;

using tainicom.Aether.Physics2D.Dynamics;
using TanksRebirth.Internals.Common.Framework;
using TanksRebirth.GameContent.Systems;

using TanksRebirth.Graphics;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.ID;
using TanksRebirth.Net;
using TanksRebirth.GameContent.RebirthUtils;
using TanksRebirth.GameContent.UI.MainMenu;
using TanksRebirth.GameContent;
using TanksRebirth;
using static TanksRebirth.GameContent.RebirthUtils.DebugManager;
using tainicom.Aether.Physics2D.Common;

using TanksRebirth.GameContent.Systems.PingSystem;


using TanksRebirth.GameContent.GameMechanics;
using TanksRebirth.GameContent.Systems.Coordinates;

using Microsoft.Xna.Framework.Input;
using TanksRebirth.Internals.Common;
using MathUtils = TanksRebirth.Internals.Common.Utilities.MathUtils;
using Newtonsoft.Json.Linq;
using Octokit;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.GameContent.Globals.Assets;
using TanksRebirth.Internals;
using TanksRebirth.GameContent.Systems.ParticleSystem;
using System.Reflection;
using TanksRebirth.IO;
using System.Text;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.Internals.Common.Framework.Collisions;
using TanksRebirth.Enums;
using TanksRebirth.GameContent.UI.LevelEditor;


namespace CobaltsArmada.Script.Tanks.Class_T
{
    /// <summary>
    /// A little helper with a lot of depth
    /// </summary>
    public class CA_Drone
    {
        /// <summary>
        /// A list of tasks the drone can do
        /// </summary>
        public enum DroneTask
        {
            /// <summary> The drone is doing nothing.</summary>
            Idle = 0,
            /// <summary> The drone is... get this... dying.</summary>
            Die,
            /// <summary> The drone laying a mine.</summary>
            SetTrap,
            /// <summary> The drone is bringing in backup.</summary>
            Recruit,
            /// <summary> The drone is holding a position</summary>
            Hold,

        }
        public bool[] AssignedTask = new bool[5];
        public enum TaskState
        {
            Start,
            During,
            Finishing,
            Finished,
        }
        // Making sure my code doesn't go nuclear
        private static PropertyInfo[] _dronePropertySkillcache = null;


        private const int MaxDrones = 64;
        public static World DroneCollisions = new(Vector2.Zero);
        public static CA_Drone[] AllDrones { get; } = new CA_Drone[MaxDrones];

        /// <summary>The <see cref="Tank"/> that has ownership over this Drone.</summary>
        public Tank? droneOwner;

        public DroneParameters Parameters = new();

        public BasicEffect TankBasicEffectHandler = new(TankGame.Instance.GraphicsDevice);

        public const float UNITS_TO_METERS = 8f;
        public const float DRN_WIDTH = 9;
        public const float DRN_HEIGHT = 9;
        public const float DRN_BASEHOVER = 13f;
        private float testtimer = 0f;

        #region Fields / Properties
        private float _oldRotation;
        public Body Body { get; set; } = new();

        /// <summary>This <see cref="Tank"/>'s world position. Used to change the actual location of the model relative to the <see cref="View"/> and <see cref="Projection"/>.</summary>
        public Matrix World { get; set; }
        /// <summary>How the <see cref="Model"/> is viewed through the <see cref="Projection"/>.</summary>
        public Matrix View { get; set; }
        /// <summary>The projection from the screen to the <see cref="Model"/>.</summary>
        public Matrix Projection { get; set; }

        public int Id { get; private set; }

        /// <summary>The current speed of this tank.</summary>
        public float Speed { get; set; }
        public float CurShootStun { get; private set; } = 0;
        public float CurShootCooldown { get; private set; } = 0;
        public float CurMineCooldown { get; private set; } = 0;
        public float CurMineStun { get; private set; } = 0;

        private int _oldShellLimit;

        public TankProperties Properties => droneOwner!.Properties;

        public Tank[] TanksSpotted = [];

        public Vector2 TurretPosition => Position + new Vector2(0, 20).Rotate(-TurretRotation);
        public Vector3 TurretPosition3D => new(TurretPosition.X, -3, TurretPosition.Y);


        public Vector2 Position
        {
            get => Body.Position * UNITS_TO_METERS;
            set => Body.Position = value / UNITS_TO_METERS;
        }
        float Gravity = 0;
        float FallSpeed = 0f;
        public Vector2 Velocity = Vector2.Zero;
        public Vector2 KnockbackVelocity = Vector2.Zero;

        /// <summary>The rotation of this <see cref="Tank"/>'s barrel. Generally should not be modified in a player context.</summary>>
        public BoundingBox Worldbox { get; set; }

        /// <summary>The 2D circle-represented hitbox of this <see cref="Tank"/>.</summary>
        public Circle CollisionCircle => new() { Center = Position, Radius = DRN_WIDTH / 2 };

        /// <summary>The 2D rectangle-represented hitbox of this <see cref="Tank"/>.</summary>
        public Rectangle CollisionBox => new((int)(Position.X - DRN_WIDTH / 2 + 3), (int)(Position.Y - DRN_WIDTH / 2 + 2),
            (int)DRN_HEIGHT - 8, (int)DRN_HEIGHT - 4);


        /// <summary>How many <see cref="Mine"/>s this <see cref="Tank"/> owns.</summary>
        public int OwnedMineCount { get; internal set; }

        /// <summary>Whether or not this <see cref="Tank"/> is currently turning.</summary>
        public bool IsTurning { get; internal set; }

        /// <summary>Whether or not this <see cref="Tank"/> is being hovered by the pointer.</summary>
        public bool IsHoveredByMouse { get; internal set; }

        public float TurretRotation { get; set; }

        /// <summary>The rotation of this <see cref="Tank"/>.</summary>
        public float DroneRotation { get; set; }

        /// <summary>The rotation this <see cref="Tank"/> will pivot to.</summary>
        public float TargetTankRotation;

        /// <summary>Whether or not the tank has been destroyed or not.</summary>
        public bool IsDestroyed { get; set; }

        /// <summary>Whether or not this tank is used for ingame purposes or not.</summary>
        public bool IsIngame { get; set; } = true;

        public Vector3 Position3D => Position.ExpandZ() + new Vector3(0f, CurrentHover - Gravity, 0f);
        public Vector3 Velocity3D => Velocity.ExpandZ() + new Vector3(0f, HoverSpeed - Gravity, 0f);
        public Vector3 Scaling = Vector3.One;

        public Vector2 TargetPosition = Vector2.Zero;
        public Vector2 RecruitRequestPos = Vector2.Zero;

        public Crate? Recruit;
        private float _mcg;
        private float _mtm;
        private float _ghost;
        public float MorphCrateGrab { get { return _mcg; } private set { _mcg = MathHelper.Clamp(value, 0f, 1f); } }
        public float MorphTurretMode { get { return _mtm; } private set { _mtm = MathHelper.Clamp(value, 0f, 1f); } }
        public float MorphGhostMode { get { return _ghost; } private set { _ghost = MathHelper.Clamp(value, 0f, 1f); } }

        public float HoverDistance = 20f;
        public float HoverSoftness = 180f;
        public float HoverStrength = 0.02f;

        public float HoverHeight = 0f;
        public float HoverAbove = 13f;

        public float HoverTarget => HoverHeight + HoverAbove;
        public float HoverSpeed = 0;
        public float CurrentHover = 0;

        public int LastTask = -1;

        public float SeekingRotation;
        public float TimeInTask;

        public bool CanIWorkHere()
        {
            foreach (var item in AllDrones)
            {
                if (item is null) continue;
                if (item.Task == DroneTask.Idle) continue;
                if (item.Task == DroneTask.Recruit && Vector2.Distance(TargetPosition, item.RecruitRequestPos) < 50) return false;
                if (Vector2.Distance(TargetPosition, item.TargetPosition) < 80) return false;
            }
            return true;
        }

        public int Ammo;
        public int Magazine => droneOwner is Tank tnk ? tnk.Properties.ShellLimit * 3 : 3;
        public int ClipSize => droneOwner is Tank tnk ? tnk.Properties.ShellLimit : 1;

        public Shell[] OwnedShells = [];


        public float AISeed = 0;
        public float AssignmentPersistance = 0f;
        private DroneTask _task;
        public TaskState CurrentState { get; set; }
        public DroneTask Task
        {
            get { return _task; }
            set
            {
                if (CurrentState != TaskState.Finishing) return;
                AssignedTask[(int)_task] = false;
                _task = value;
                AssignedTask[(int)_task] = true;
                CurrentState = TaskState.Start;
                TimeInTask = 0f;
            }
        }

        private IngamePing[] alreadydone = new IngamePing[70];
        #endregion


        #region Visuals
        public Model Model { get; set; }

        private Texture2D? _droneTexture;


        #region Model Stuff

        internal Matrix[] _boneTransforms;

        internal ModelMesh _ringMeshA;
        internal ModelMesh _ringMeshB;
        internal ModelMesh _wingMesh;
        internal ModelMesh _bodyMesh;

        public bool Flip;

        private bool CustomPaint = false;
        public Color BodyPaint = Color.White;
        public Color NeonPaint = Color.Blue;
        public Color AccentPaint = Color.DarkGray;
        #endregion
        #endregion

        #region DEBUGGING

        private int Debugging_SkillViewer;
      

        #endregion

        public CA_Drone(Tank? owner, Vector2 position = default)
        {
            
            Model = CA_Main.Drone!;
            _droneTexture = CA_Main.Tank_CustomPaint;
            if (owner is not null)
                droneOwner = owner;
            else
            {
                ChatSystem.SendMessage("Drone was spawn orphaned! A random tank has adopted the drone.", Color.IndianRed);
                ref Tank[] tanks = ref GameHandler.AllTanks;

                Tank? target = null;
                var targetPosition = new Vector2(float.MaxValue);
                foreach (var tank in GameHandler.AllTanks)
                {
                    if (tank is not null && !tank.IsDestroyed)
                    {

                        if (GameUtils.Distance_WiiTanksUnits(tank.Position, Position) < GameUtils.Distance_WiiTanksUnits(targetPosition, Position))
                        {
                            target = tank;
                            targetPosition = tank.Position;
                        }
                    }
                }
                droneOwner = target;

            }

            if (droneOwner is AITank ai)
            {
                if (ai.AiTankType == MathHelper.Clamp(ai.AiTankType, CA_Main.Dandelion, CA_Main.Hydrangea))
                {
                    var tierName = TankID.Collection.GetKey(ai.AiTankType)!.ToLower();
                    _droneTexture = Tank.Assets[$"tank_" + tierName];

                }
                else
                {
                    var tierName = TankID.Collection.GetKey(ai.AiTankType)!.ToLower();
                    var colors = new Color[Tank.Assets[$"tank_" + tierName].Width * Tank.Assets[$"tank_" + tierName].Height];

                    Tank.Assets[$"tank_" + tierName].GetData(colors);
                    CustomPaint = true;
                    BodyPaint = colors[0];
                    NeonPaint = colors[Tank.Assets[$"tank_" + tierName].Height * 9];
                    AccentPaint = Color.Lerp(NeonPaint, Color.Black, 0.5f);

                    var t = new Texture2D(TankGame.Instance.GraphicsDevice, Tank.Assets[$"tank_" + tierName].Width, Tank.Assets[$"tank_" + tierName].Height);
                    CA_Main.Tank_CustomPaint!.GetData(colors);

                    for (int i = 0; i < colors.Length; i++)
                    {
                        colors[i].Deconstruct(out byte r, out byte g, out byte b);
                        colors[i] = Color.Lerp(Color.Black, ai.AiTankType != TankID.Obsidian ? BodyPaint : NeonPaint, r / 255f);
                        colors[i] = Color.Lerp(colors[i], ai.AiTankType == TankID.Obsidian ? BodyPaint : NeonPaint, b / 255f * (ai.AiTankType == MathHelper.Clamp(ai.AiTankType, TankID.Bronze, TankID.Obsidian) ? 3f : 1f));
                        colors[i] = Color.Lerp(colors[i], AccentPaint, g / 255f);
                    }
                    t.SetData(colors);
                    _droneTexture = t;
                }
            }
            else if (droneOwner is PlayerTank Plyr)
            {

                var tierName = PlayerID.Collection.GetKey(Plyr.PlayerType)!.ToLower();
                var colors = new Color[Tank.Assets[$"plrtank_" + tierName].Width * Tank.Assets[$"plrtank_" + tierName].Height];

                Tank.Assets[$"plrtank_" + tierName].GetData(colors);
                CustomPaint = true;
                BodyPaint = PlayerID.PlayerTankColors[Plyr.PlayerType];
                NeonPaint = colors[Tank.Assets[$"plrtank_" + tierName].Height * 9];
                AccentPaint = Color.Lerp(NeonPaint, Color.Black, 0.5f);

                var t = new Texture2D(TankGame.Instance.GraphicsDevice, Tank.Assets[$"plrtank_" + tierName].Width, Tank.Assets[$"plrtank_" + tierName].Height);
                CA_Main.Tank_CustomPaint!.GetData(colors);

                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i].Deconstruct(out byte r, out byte g, out byte b);
                    colors[i] = Color.Lerp(Color.Black, NeonPaint, r / 255f);
                    colors[i] = Color.Lerp(colors[i], BodyPaint, b / 255f);
                    colors[i] = Color.Lerp(colors[i], AccentPaint, g / 255f);
                }
                t.SetData(colors);
                _droneTexture = t;

            }


            int index = Array.IndexOf(AllDrones, null);

            Id = index;

            AllDrones[index] = this;
            InitModelSemantics();

            Body = DroneCollisions.CreateCircle(DRN_HEIGHT / UNITS_TO_METERS, 1, position, bodyType: BodyType.Dynamic);
            CurrentHover = 300f;
            OwnedShells = new Shell[ClipSize];
            //assign model and texture data
            if (droneOwner is Tank tnk)
                Parameters = CA_DroneLicenseManager.ApplyDefaultLicense(tnk);
      
        }

       

        public void InitModelSemantics()
        {
            // for some reason Model is null when returning from campaign completion with certain mods.
            if (Model is null)
            {
                Remove();
                return;
            }

            _wingMesh = Model.Meshes["Wings"];
            _ringMeshA = Model.Meshes["HoverRingSentry"];
            _ringMeshB = Model.Meshes["HoverRingEye"];
            _bodyMesh = Model.Meshes["Body"];
            _boneTransforms = new Matrix[Model.Bones.Count];
        }

        public static void CollisionUpdate()
        {
            foreach (var Block in Block.AllBlocks)
            {
                if (Block is null) continue;
                bool valid = false;
                if (DroneCollisions.BodyList.Contains(Block.Physics))
                {
                    DroneCollisions.Remove(Block.Physics);
                }
                foreach (var Drone in AllDrones)
                {
                    
                    if (Drone is null || Drone.droneOwner is null || Drone.droneOwner.IsDestroyed) continue;
                    Vector2 Lookahead = Vector2.Normalize(Drone.Velocity).IsValid() ? Vector2.Normalize(Drone.Velocity) * MathF.PI : Vector2.Zero;
                    float distance = (Vector2.Distance(Block.Position, Drone.Position + (Lookahead * Block.SIDE_LENGTH / 2)) - Block.SIDE_LENGTH*1.2f);
                    var dummy = Drone.Position;
                    Collision.HandleCollisionSimple_ForBlocks(Drone.CollisionBox, Drone.Velocity, ref dummy, out var dir, out var block,
            out bool corner, true, (c) => c.Properties.IsSolid && Drone.Position3D.Y + 1.5f <= c.HeightFromGround);
                    if ((dir != CollisionDirection.None || corner) && distance <= 0)
                    {
                        valid = true;
                    }
                    if (valid)
                    {
                        Drone.Position = dummy;
                    }

                }



            }

        }




        internal void Update()
        {
            if (!GameScene.ShouldRenderAll)
                return;
            if (Recruit is Crate crate && (Crate.crates[crate.id] is null || Crate.crates[crate.id].IsOpening))
            {
                Recruit = null;
            }

            #region ModelAnimationAndVisuals

            Vector2 forward = Vector2.Normalize(Velocity).Rotate(-DroneRotation);
            if (!forward.IsValid()) forward = Vector2.Zero;
            float xRot = MathF.Cos(forward.X) * Velocity.Length() / UNITS_TO_METERS - HoverSpeed / 50f;
            float yRot = MathF.Sin(forward.Y) * Velocity.Length() / UNITS_TO_METERS;
            World = Matrix.CreateScale(Scaling * 100f) * Matrix.CreateFromYawPitchRoll(-yRot * 3f, -xRot + (xRot * Gravity / 50f), 0) * Matrix.CreateFromYawPitchRoll(0, -MathHelper.PiOver2, -DroneRotation - MathHelper.PiOver2) * Matrix.CreateTranslation(Position3D);

            MorphTurretMode += RuntimeData.DeltaTime / 60f * (CurrentState == TaskState.During && Task == DroneTask.Hold ? 2f : -1f) * 5f;
            MorphCrateGrab += RuntimeData.DeltaTime / 60f * (Recruit is Crate && Task == DroneTask.Recruit ? 1f : -1f) * 5f;

            Matrix R1A = Matrix.CreateTranslation(0, 0, -0.01f);
            Matrix R1B = Matrix.CreateScale(6f, 6f, 2f) * Matrix.CreateFromYawPitchRoll(0, 0, MathF.Sin(testtimer * MathF.PI) * 5f) * Matrix.CreateTranslation(0, 0, -0.02f);

            Matrix R2A = Matrix.CreateTranslation(0, 0, -0.02f);
            Matrix R2B = Matrix.CreateScale(8f, 8f, 2f) * Matrix.CreateFromYawPitchRoll(0, 0, MathF.Sin(-testtimer * MathF.PI) * 5f) * Matrix.CreateTranslation(0, 0, -0.1f);

            Matrix R2C = Matrix.CreateScale(2f, 2f, 2f) * Matrix.CreateFromYawPitchRoll(0, 0, MathHelper.WrapAngle(-TurretRotation) * 2f - DroneRotation) * Matrix.CreateTranslation(0, 0, -0.04f);
            Matrix R1C = Matrix.CreateScale(2f, 2f, 2f) * Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, 0, 0) * Matrix.CreateTranslation(0.07f, 0, 0) * Matrix.CreateFromYawPitchRoll(0, 0, MathHelper.WrapAngle(TurretRotation) + DroneRotation) * Matrix.CreateTranslation(0, 0, -0.04f);

            _ringMeshA.ParentBone.Transform = Matrix.Lerp(Matrix.Lerp(R2A, R2B, MorphCrateGrab), R2C, MorphTurretMode) * Matrix.CreateFromYawPitchRoll(yRot * 3f, xRot, 0);
            _ringMeshB.ParentBone.Transform = Matrix.Lerp(Matrix.Lerp(R1A, R1B, MorphCrateGrab), R1C, MorphTurretMode) * Matrix.CreateFromYawPitchRoll(yRot * 3f, xRot, 0);

            _wingMesh.ParentBone.Transform = Matrix.CreateFromYawPitchRoll(-yRot / 4f, -xRot / 4f, 0);
            _bodyMesh.ParentBone.Transform = Matrix.CreateTranslation(0, 0, 0);
            Model!.Root.Transform = World;
            Model!.CopyAbsoluteBoneTransformsTo(_boneTransforms);

            #endregion

            //in the case of a drone doesn't spawn in with an owner
            if (droneOwner is null)
            {
                //ChatSystem.SendMessage("Attempting adoption", Color.Indigo);
                ref Tank[] tanks = ref GameHandler.AllTanks;

                Tank? target = null;
                var targetPosition = new Vector2(float.MaxValue);
                foreach (var tank in GameHandler.AllTanks)
                {
                    if (tank is not null && !tank.IsDestroyed)
                    {

                        if (GameUtils.Distance_WiiTanksUnits(tank.Position, Position) < GameUtils.Distance_WiiTanksUnits(targetPosition, Position))
                        {
                            target = tank;
                            targetPosition = tank.Position;
                        }
                    }
                }
                droneOwner = target;
                return;
            }


            if (droneOwner.IsDestroyed)
            {
                if (Recruit is Crate crate2)
                {
                    crate2.velocity = Velocity.ExpandZ() * 0.5f;
                    crate2.gravity = 4f;
                    Recruit = null;
                }

                Task = DroneTask.Die;
                FallSpeed += 0.03f * RuntimeData.DeltaTime;
                Gravity += FallSpeed * RuntimeData.DeltaTime;
                FallSpeed *= 1.02f;
                if (Position3D.Y < 0)
                {
                    new Explosion(Position, 4, droneOwner, soundPitch: 0.6f);
                    Remove();
                }

                return;
            }


            HoverAbove = DRN_BASEHOVER;

            //like the ai tanks, only the host should do the ai
            if (Client.IsHost() || !Client.IsConnected() && !IsDestroyed || MainMenuUI.IsActive)
            {
               // timeSinceLastAction++;

                if (!MainMenuUI.IsActive)
                    if (!CampaignGlobals.InMission || IntermissionSystem.IsAwaitingNewMission || LevelEditorUI.IsActive)
                        Velocity = Vector2.Zero;
                DroneAI();

                CA_NetPlay.SyncDrone(this, false);
         
            }
            if (!Client.IsHost() && Client.IsConnected() && Recruit is Crate)
            {
                if (CurrentState == TaskState.During)
                {
                    Recruit!.position = Position3D - new Vector3(0, 12, 0);
                    Recruit.gravity = 0f;
                }
                else
                {
                    Recruit.gravity = 2f;
                }
            }

            

            float newHeight = HoverAbove;

            float hurryrise = 1f;
            int hurryammount = 0;
            foreach (var Block in Block.AllBlocks)
            {
                if (Block is null) continue;
                Vector2 Lookahead = Vector2.Normalize(Velocity).IsValid() ? Vector2.Normalize(Velocity) * Block.SIDE_LENGTH / 2f : Vector2.Zero;
                float dist = MathF.Max(0f, (MathF.Min(Vector2.Distance(Block.Position, Position + Lookahead), Vector2.Distance(Block.Position, Position)) - Block.SIDE_LENGTH * (Velocity.Length() * 0.25f)) / (Block.SIDE_LENGTH * (1f + Velocity.Length())));
                float distance = MathHelper.Clamp(dist, 0f, 1f);
                newHeight = MathF.Max(newHeight, Block.HeightFromGround * (1f - distance));

                if (distance < 1f)
                {
                    hurryrise = Math.Min(hurryrise, distance);
                }

            }
            hurryrise = 1f - hurryrise;
            if (float.IsNaN(hurryrise) || CurrentHover > newHeight) hurryrise = 0f;

            if (Task == DroneTask.Recruit)
            {
                newHeight = Block.FULL_SIZE;
            }
            HoverHeight = MathHelper.Lerp(HoverHeight, newHeight * 1.1f, 0.4f);
            HoverSpeed = MathHelper.Lerp(HoverSpeed, MathHelper.Clamp((HoverTarget - CurrentHover) / 5f, -80f, 80f) + MathHelper.Clamp(HoverTarget - CurrentHover, -80f, 80f) * 0.1f, RuntimeData.DeltaTime / 40f + hurryrise * 0.2f);
            if (CurrentHover < 5) HoverSpeed = MathF.Abs(HoverSpeed);
            //AI stuff


            // ChatSystem.SendMessage($"Active:{Position.X}-{Position.Y}", Color.SkyBlue);
            float NextSpeed = MathHelper.Clamp((Vector2.Distance(TargetPosition, Position) - HoverDistance) / HoverSoftness, -1f, 5f) / 0.2f;

            Speed = MathHelper.Lerp(Speed, NextSpeed * (1f - hurryrise * 0.9f), 1f - (hurryrise * 0.4f));

            Speed = MathHelper.Clamp(Speed, -3, 3f);

            CurrentHover = float.Lerp(CurrentHover, CurrentHover + RuntimeData.DeltaTime * HoverSpeed, (HoverSpeed < 0 ? 0.07f : 0.04f) + hurryrise * 0.24f);

            Velocity = Vector2.Lerp(Velocity, Vector2.Normalize(Position.DirectionTo(TargetPosition)) * Speed, HoverStrength + hurryrise);

            if (!Velocity.IsValid()) Velocity = Vector2.Zero;
            Body.LinearVelocity = Velocity / UNITS_TO_METERS;
            if (!Body.LinearVelocity.IsValid()) Body.LinearVelocity = Vector2.Zero;
            if (Vector2.Distance(TargetPosition, Position) > 1f)
                DroneRotation = DroneRotation.AngleLerp(Position.DirectionTo(TargetPosition).ToRotation(), 0.3f * (1f-RuntimeData.DeltaTime) );

            testtimer += RuntimeData.DeltaTime / 60f;

            for (int i = 0; i < OwnedShells.Length; i++)
                if (OwnedShells[i] is Shell s && Shell.AllShells[s.Id] is null) OwnedShells[i] = null;
        }

        // public Tank? TryOverrideTarget()
        //  {
        //     Tank? target = TargetTank;
        //     if (GameHandler.AllPlayerTanks.Any(x => x is not null && x.Team == droneOwner!.Team))
        //     {
        //          foreach (var ping in IngamePing.AllIngamePings)
        //          {
        //             if (ping is null) break;
        //            
        //         }
        //     }
        //     return target;
        //  }
        #region Drone Behavior
        public void DroneAI()
        {
            if (Task == DroneTask.Die) return;
            //Commanding where the drone to go
            if (Task != DroneTask.Idle) goto skip;
            if (droneOwner is AITank aiBuddy)
            {
                if (AssignmentPersistance > 0)
                {
                    AssignmentPersistance -= RuntimeData.DeltaTime;
                }
                else
                {
                    AISeed = Client.ClientRandom.NextFloat(0f, 1f); //it's handled completly by the host
                    AssignmentPersistance = 5f;
                }

                if (aiBuddy.TargetTank is null) goto skip;
                var tanks = GetTanksInPath(Vector2.Normalize(aiBuddy.Position.DirectionTo(aiBuddy.TargetTank.Position)) * new Vector2(1f, -1f), out var ricP, out var tnkCol, false, default, considersRicos: false, missDist: aiBuddy.Parameters.AimOffset, start: droneOwner.Position);
                var findsEnemy2 = tanks.Any(tnk => tnk is not null && (tnk.Team != droneOwner.Team || tnk.Team == TeamID.NoTeam) && tnk != droneOwner);

                Vector2 A = aiBuddy.Position;
                Vector2 B = aiBuddy.TargetTank.Position;
                Vector2.Distance(ref A, ref B, out float C);

                float seedRange = AISeed;
                if (Parameters.TrapsGeneral.CanUse) Parameters.TrapsGeneral.curCooldown -= RuntimeData.DeltaTime;
                if (Parameters.TrapsGeneral.ReadyForUse && (findsEnemy2 || Parameters.TrapsGeneral.GlobalSkill) && C == MathHelper.Clamp(C, Parameters.TrapsGeneral.Minimum, Parameters.TrapsGeneral.Maximum)
                    && seedRange >= 0 && seedRange < Parameters.TrapsGeneral.ChanceToActivate)
                {
                    if (Parameters.TrapsGeneral.RelayTaskToOthers)
                    {
                        var CalledDrones = AllDrones.Where(x => x is not null && x != this && x.Task == DroneTask.Idle && x.droneOwner is AITank && x.droneOwner!.Team != TeamID.NoTeam && x.droneOwner!.Team == droneOwner.Team && x.Parameters.TrapsGeneral.EnabledViaRelay);
                        foreach (var item in CalledDrones)
                        {
                            if (Vector2.Distance(item.droneOwner!.Position, droneOwner!.Position) > Parameters.TrapsGeneral.RelayTaskRange) continue;
                            var signalStrength = Client.ClientRandom.NextFloat(0, 1f) * MathHelper.Clamp(1f - Vector2.Distance(item.droneOwner!.Position, droneOwner!.Position) / Parameters.TrapsGeneral.RelayTaskRange,0,1);
                            if (signalStrength - 1f < 0f && item.droneOwner is AITank ai && ai.TargetTank is Tank && item.Parameters.TrapsGeneral.curCooldown < 0)
                            {
                                item.TargetPosition = Vector2.Lerp(item.droneOwner!.Position, ai.TargetTank.Position, 0.99f);
                                item.TargetPosition += Vector2.UnitY.Rotate(Client.ClientRandom.NextFloat(-1f, 1f) * MathF.Tau)* item.Parameters.TrapsGeneral.Inaccuracy;
                                var dummy = item.TargetPosition;
                                var dummybox = new Rectangle(new Point((int)(item.TargetPosition.X / UNITS_TO_METERS), (int)(item.TargetPosition.Y / UNITS_TO_METERS)),item.CollisionBox.Size);
                                Collision.HandleCollisionSimple_ForBlocks(dummybox,Vector2.Zero, ref dummy, out var dir, out var block, out bool corner,false);
                                if (block is Block) break;
                                item.Task = DroneTask.SetTrap;
                                item.Parameters.TrapsGeneral.curCooldown = item.Parameters.TrapsGeneral.Cooldown;
                                Parameters.TrapsGeneral.curCooldown = Parameters.TrapsGeneral.Cooldown;
                                break;
                            }
                        }

                        goto skip;
                    }
                    TargetPosition = Vector2.Lerp(aiBuddy.Position, aiBuddy.TargetTank.Position, 0.99f);
                    if (CanIWorkHere())
                    {
                        TargetPosition += Vector2.UnitY.Rotate(Client.ClientRandom.NextFloat(-1f, 1f) * MathF.Tau) * Parameters.TrapsGeneral.Inaccuracy;
                        var dummy = TargetPosition;
                        var dummybox = new Rectangle(new Point((int)(TargetPosition.X / UNITS_TO_METERS), (int)(TargetPosition.Y / UNITS_TO_METERS)), CollisionBox.Size);
                        Collision.HandleCollisionSimple_ForBlocks(dummybox, Vector2.Zero, ref dummy, out var dir, out var block, out bool corner, false);
                        if (block is Block) goto skip;
                        Task = DroneTask.SetTrap;
                        Parameters.TrapsGeneral.curCooldown = Parameters.TrapsGeneral.Cooldown;
                    }
                    
                }

                seedRange = AISeed;
                if (Parameters.RecruitGeneral.CanUse) Parameters.RecruitGeneral.curCooldown -= RuntimeData.DeltaTime;
                if (Parameters.RecruitGeneral.ReadyForUse && (findsEnemy2 || Parameters.RecruitGeneral.GlobalSkill) && C == MathHelper.Clamp(C, Parameters.RecruitGeneral.Minimum, Parameters.RecruitGeneral.Maximum)
                    && seedRange >= 0 && seedRange < Parameters.RecruitGeneral.ChanceToActivate)
                {
                    if (Parameters.RecruitGeneral.RelayTaskToOthers)
                    {
                        var CalledDrones = AllDrones.Where(x => x is not null && x != this && x.Task == DroneTask.Idle && x.droneOwner is AITank && x.droneOwner!.Team != TeamID.NoTeam && x.droneOwner!.Team == droneOwner.Team && x.Parameters.RecruitGeneral.EnabledViaRelay);
                        foreach (var item in CalledDrones)
                        {
                            if (Vector2.Distance(item.droneOwner!.Position, droneOwner!.Position) > Parameters.RecruitGeneral.RelayTaskRange) continue;
                            var signalStrength = Client.ClientRandom.NextFloat(0, 1f) * MathHelper.Clamp(1f - Vector2.Distance(item.droneOwner!.Position, droneOwner!.Position) / Parameters.RecruitGeneral.RelayTaskRange, 0, 1);
                            if (signalStrength * 2 > 1f - item.Parameters.RecruitGeneral.ChanceToActivate * 1.25f && item.droneOwner is AITank ai && ai.TargetTank is Tank && item.Parameters.RecruitGeneral.curCooldown < 0)
                            {
                                var places1 = PlacementSquare.Placements.Where(x => x.BlockId == -1).ToArray();
                                var angle1 = MathF.SinCos((Client.ClientRandom.NextFloat(0, 1) * MathF.Tau));
                                item.TargetPosition = (new Vector2(angle1.Sin, angle1.Cos) * 800f) + GameScene.Center.FlattenZ();
                                item.RecruitRequestPos = places1[Client.ClientRandom.Next(0, places1.Length)].Position.FlattenZ();
                                var dummy = item.TargetPosition;
                                var dummybox = new Rectangle(new Point((int)(item.TargetPosition.X / UNITS_TO_METERS), (int)(item.TargetPosition.Y / UNITS_TO_METERS)), item.CollisionBox.Size);
                                Collision.HandleCollisionSimple_ForBlocks(dummybox, Vector2.Zero, ref dummy, out var dir, out var block, out bool corner, false);
                                if (block is Block) break;
                                item.Parameters.RecruitGeneral.curCooldown = item.Parameters.RecruitGeneral.Cooldown;
                                Parameters.RecruitGeneral.curCooldown = Parameters.RecruitGeneral.Cooldown;
                                item.Task = DroneTask.Recruit;

                            }
                        }

                        goto skip;
                    }
                    var places = PlacementSquare.Placements.Where(x => x.BlockId == -1).ToArray();
                    var angle = MathF.SinCos((Client.ClientRandom.NextFloat(0, 1) * MathF.Tau));
                    TargetPosition = (new Vector2(angle.Sin, angle.Cos) * 800f) + GameScene.Center.FlattenZ();
                    RecruitRequestPos = places[Client.ClientRandom.Next(0, places.Length)].Position.FlattenZ();
                    if (CanIWorkHere())
                    {
                        Parameters.RecruitGeneral.curCooldown = Parameters.RecruitGeneral.Cooldown;
                        Task = DroneTask.Recruit;
                    }

                }

                seedRange = AISeed;
                if (Parameters.HoldGeneral.CanUse) Parameters.HoldGeneral.curCooldown -= RuntimeData.DeltaTime;
                if (Parameters.HoldGeneral.ReadyForUse && (findsEnemy2 || Parameters.HoldGeneral.GlobalSkill) && C == MathHelper.Clamp(C, Parameters.HoldGeneral.Minimum, Parameters.HoldGeneral.Maximum) 
                    && seedRange >= 0 && seedRange < Parameters.HoldGeneral.ChanceToActivate)
                {
                    if (Parameters.HoldGeneral.RelayTaskToOthers)
                    {
                        var CalledDrones = AllDrones.Where(x => x is not null && x != this && x.Task == DroneTask.Idle && x.droneOwner is AITank && x.droneOwner!.Team != TeamID.NoTeam && x.droneOwner!.Team == droneOwner.Team && x.Parameters.HoldGeneral.EnabledViaRelay);
                        foreach (var item in CalledDrones)
                        {
                            if (Vector2.Distance(item.droneOwner!.Position, droneOwner!.Position) > Parameters.HoldGeneral.RelayTaskRange) continue;
                            var signalStrength = Client.ClientRandom.NextFloat(0, 1f) * MathHelper.Clamp(1f - Vector2.Distance(item.droneOwner!.Position, droneOwner!.Position) / Parameters.HoldGeneral.RelayTaskRange, 0, 1);
                            if (signalStrength * 2 > 1f - item.Parameters.HoldGeneral.ChanceToActivate * 1.25f && item.droneOwner is AITank ai && ai.TargetTank is Tank && item.Parameters.HoldGeneral.curCooldown < 0)
                            {
                                item.TargetPosition = Vector2.Lerp(item.droneOwner!.Position, ai.TargetTank.Position, 0.99f);
                                if (CanIWorkHere())
                                {
                                    item.TargetPosition += Vector2.UnitY.Rotate(Client.ClientRandom.NextFloat(-1f, 1f) * MathF.Tau) * item.Parameters.HoldGeneral.Inaccuracy;
                                    var dummy = item.TargetPosition;
                                    var dummybox = new Rectangle(new Point((int)(item.TargetPosition.X / UNITS_TO_METERS), (int)(item.TargetPosition.Y / UNITS_TO_METERS)), item.CollisionBox.Size);
                                    Collision.HandleCollisionSimple_ForBlocks(dummybox, Vector2.Zero, ref dummy, out var dir, out var block, out bool corner, false);
                                    if (block is Block) break;
                                    item.Parameters.HoldGeneral.curCooldown = item.Parameters.HoldGeneral.Cooldown;
                                    Parameters.HoldGeneral.curCooldown = Parameters.HoldGeneral.Cooldown;
                                    item.Task = DroneTask.Hold;
                                    break;
                                }

                            }
                        }

                        goto skip;
                    }
                    TargetPosition = Vector2.Lerp(aiBuddy.Position, aiBuddy.TargetTank.Position, 1f);
                    if (CanIWorkHere())
                    {
                        TargetPosition += Vector2.UnitY.Rotate(Client.ClientRandom.NextFloat(-1f, 1f) * MathF.Tau) * Parameters.HoldGeneral.Inaccuracy;
                        var dummy = TargetPosition;
                        var dummybox = new Rectangle(new Point((int)(TargetPosition.X / UNITS_TO_METERS), (int)(TargetPosition.Y / UNITS_TO_METERS)), CollisionBox.Size);
                        Collision.HandleCollisionSimple_ForBlocks(dummybox, Vector2.Zero, ref dummy, out var dir, out var block, out bool corner, false);
                        if (block is Block) goto skip;
                        Task = DroneTask.Hold;
                        Parameters.HoldGeneral.curCooldown = Parameters.HoldGeneral.Cooldown;
                    }
                }

                /*
                                //if (findsEnemy2 && AssignmentPersistance <= 0f && (aiBuddy.AiTankType == TankID.Obsidian || aiBuddy.AiTankType == TankID.Emerald || aiBuddy.AiTankType == TankID.Black))
                                //{
                                //    AssignmentPersistance = aiBuddy.AiTankType == TankID.Emerald ? 900 : 300f;
                                //    Task = DroneTask.SetTrap;
                                //    TargetPosition = aiBuddy.TargetTank.Position + aiBuddy.TargetTank.Velocity*12f;
                                //}

                                ////Bronze will start calling in back up anyways

                                //if ((aiBuddy.SeesTarget || aiBuddy.AiTankType == TankID.Bronze) && AssignmentPersistance <= 0f && (aiBuddy.AiTankType == TankID.Pink || aiBuddy.AiTankType == TankID.White || aiBuddy.AiTankType == TankID.Gold))
                                //    {
                                //    AssignmentPersistance = 2000f;
                                //    Task = DroneTask.Recruit;
                                //    var places = PlacementSquare.Placements.Where(x => x.BlockId == -1).ToArray();
                                //    var angle = MathF.SinCos((Client.ClientRandom.NextFloat(0, 1) * MathF.Tau));
                                //    TargetPosition = (new Vector2(angle.Sin,angle.Cos) * 800f) + GameScene.Center.FlattenZ();
                                //    RecruitRequestPos = places[Client.ClientRandom.Next(0,places.Length)].Position.FlattenZ();
                                //}

                                //if (aiBuddy.SeesTarget && AssignmentPersistance <= 0f && (aiBuddy.AiTankType == TankID.Green || aiBuddy.AiTankType == TankID.Violet || aiBuddy.AiTankType == TankID.Ruby || aiBuddy.AiTankType == TankID.Sapphire))
                                //{
                                //    AssignmentPersistance = 1800f;
                                //    Task = DroneTask.Hold;
                                //    TargetPosition = Vector2.Lerp(aiBuddy.Position, aiBuddy.TargetTank.Position, 1f);
                                //}
                */

               

            }
            else//You have a better Plyr companion in every way with the drone (you could say they have a friend inside me...)
            {

                if (DebuggingEnabled)
                {
                    if (InputUtils.KeyJustPressed(Keys.D1))
                    {
                        Task = DroneTask.Recruit;
                        var places = PlacementSquare.Placements.Where(x => x.BlockId == -1).ToArray();
                        var angle = MathF.SinCos((Client.ClientRandom.NextFloat(0, 1) * MathF.Tau));
                        TargetPosition = (new Vector2(angle.Sin, angle.Cos) * 800f) + GameScene.Center.FlattenZ();
                        RecruitRequestPos = places[Client.ClientRandom.Next(0, places.Length)].Position.FlattenZ();
                        goto skip;
                    }
                    if (InputUtils.KeyJustPressed(Keys.D2))
                    {
                        Task = DroneTask.SetTrap;
                        TargetPosition = (!CameraGlobals.OverheadView ? MatrixUtils.GetWorldPosition(MouseUtils.MousePosition) : PlacementSquare.CurrentlyHovered.Position).FlattenZ();
                        goto skip;
                    }
                    if (InputUtils.KeyJustPressed(Keys.D3))
                    {
                        Task = DroneTask.Hold;
                        TargetPosition = (!CameraGlobals.OverheadView ? MatrixUtils.GetWorldPosition(MouseUtils.MousePosition) : PlacementSquare.CurrentlyHovered.Position).FlattenZ();
                        goto skip;
                    }
                }
                else
                if (
                    GameHandler.AllPlayerTanks.Any(x => x is not null &&
                    x.Team == droneOwner!.Team &&
                    droneOwner is PlayerTank plyer &&
                    plyer == droneOwner))
                {
                    foreach (var ping in IngamePing.AllIngamePings)
                    {

                        if (ping is null) break;

                        if (alreadydone.Any(x => x is not null && x == ping)) continue;
                        else
                        {
                            alreadydone[ping.Id] = null;
                        }

                        if (ping.TrackedTank is Tank target)
                        {

                        }
                        else
                        {
                            switch (ping.PingId)
                            {
                                default: break;
                                case PingID.AvoidHere:
                                case PingID.GroupHere:
                                    Task = DroneTask.SetTrap;
                                    TargetPosition = ping.Position.FlattenZ();
                                    break;
                                case PingID.WatchHere:
                                    Task = DroneTask.Hold;
                                    TargetPosition = ping.Position.FlattenZ();
                                    break;

                            }


                        }
                        alreadydone[ping.Id] = ping;
                    }
                }
            }

        skip:
            
             TimeInTask += RuntimeData.DeltaTime / 60f;
            //behaviors
            switch (Task)
            {
                default: BehaviorIdle(); break;
                case DroneTask.SetTrap: BehaviorSetTrap(); break;
                case DroneTask.Recruit: BehaviorRecruit(); break;
                case DroneTask.Hold: BehaviorPatrol(); break;
            }
        }

        public void BehaviorIdle()
        {
            HoverDistance = 30f;
            HoverSoftness = 80f;
            HoverStrength = 0.02f;
            TargetPosition = droneOwner!.Position;
            CurrentState = TaskState.Finishing;
        }

        public void BehaviorSetTrap()
        {
            HoverDistance = 2f;
            HoverSoftness = 200f;
            HoverStrength = 0.04f;
            HoverAbove = 4f;


            if (CurrentState == TaskState.Start)
            {
                if (Vector2.Distance(Position, TargetPosition) <= HoverDistance && MathF.Abs(Position3D.Y - HoverTarget) < 2f) CurrentState = TaskState.During;
                return;
            }
            if (CurrentState == TaskState.During)
            {
                var M = new Mine(droneOwner, Position, 600);
                Client.SyncMinePlace(Position, 600, M.Id);
                CurrentState = TaskState.Finishing;
                return;
            }
            if (CurrentState == TaskState.Finishing) Task = DroneTask.Idle;

        }

        public void BehaviorPatrol()
        {
            HoverDistance = 1f;
            HoverSoftness = 15f;
            HoverStrength = Vector2.Distance(Position, TargetPosition) / Vector2.Distance(droneOwner!.Position, TargetPosition) > 0.5f ? 0.03f : 0.06f;
            HoverAbove = 11f;
            if (CurrentState == TaskState.Start)
            {

                CurShootCooldown = 60f;
                if (Vector2.Distance(Position, TargetPosition) <= HoverDistance && MathF.Abs(Position3D.Y - HoverTarget) < 2f)
                {
                    Ammo = Magazine;
                    CurrentState = TaskState.During;
                }
                return;
            }
            if (CurrentState == TaskState.During)
            {
                if (TimeInTask > 15f) { CurrentState = TaskState.Finishing; return; }
                var tanks = GetTanksInPath(Vector2.UnitY.Rotate(SeekingRotation), out var ricP, out var tnkCol, false, default, considersRicos: droneOwner is AITank a && a.AiTankType == TankID.Green);
                var findsEnemy2 = tanks.Any(tnk => tnk is not null && (tnk.Team != droneOwner.Team || tnk.Team == TeamID.NoTeam) && tnk != droneOwner);
                // var findsSelf2 = tanks.Any(tnk => tnk is not null && tnk == this);
                // var findsFriendly2 = tanks.Any(tnk => tnk is not null && (tnk.Team == Team && tnk.Team != TeamID.NoTeam));
                // ChatSystem.SendMessage($"{findsEnemy2} {findsFriendly2} | seek: {seeks}", Color.White);
                CurShootCooldown -= RuntimeData.DeltaTime;
                if (findsEnemy2)
                {
                    var findsEnemy4 = tanks.First(tnk => tnk is not null && (tnk.Team != droneOwner.Team || tnk.Team == TeamID.NoTeam) && tnk != droneOwner);
                    TurretRotation = SeekingRotation + MathF.PI;

                    var EmptyMag = OwnedShells.All(shl => shl is not null);
                    if (EmptyMag)
                    {
                        CurShootCooldown = droneOwner is PlayerTank ? 80 : Properties.ShellCooldown * 2f;
                    }
                    if (CurShootCooldown < 0)
                    {
                        CurShootCooldown = droneOwner is PlayerTank ? 50 : Properties.ShellCooldown;
                        var new2d = Vector2.UnitY.Rotate(TurretRotation);
                        var shell = new Shell(TurretPosition, new Vector2(-new2d.X, new2d.Y) * Properties.ShellSpeed,
                        Properties.ShellType, null, Properties.RicochetCount, homing: Properties.ShellHoming, playSpawnSound: true);
                        Velocity = new Vector2(-new2d.X, new2d.Y) * Properties.ShellSpeed * -0.9f;
                        shell.ShootSound!.Instance.Pitch = MathHelper.Clamp(Properties.ShootPitch + 0.3f, -1f, 1f);
                        SoundPlayer.PlaySoundInstance(shell.ShootSound, SoundContext.Effect, 0.3f);
                        DoShootParticles();
                        shell.Owner = droneOwner;
                        Client.SyncShellFire(shell);
                        
                        int index = Array.IndexOf(OwnedShells, null);
                        OwnedShells[index] = shell;
                        Ammo--;
                        if (Ammo <= 0)
                        {
                            CurrentState = TaskState.Finishing;
                            return;
                        }
                    }

                }
                else
                {
                    SeekingRotation += MathHelper.PiOver4 * 0.08f;
                    TurretRotation -= MathHelper.PiOver4 * 0.01f;
                }

                return;
            }
            if (CurrentState == TaskState.Finishing) Task = DroneTask.Idle;

        }

        public void BehaviorRecruit()
        {
            HoverDistance = 40f;
            HoverSoftness = 10f;
            HoverStrength = 0.01f;
            if (CurrentState == TaskState.Start)
            {
                if (Vector2.Distance(Position, TargetPosition) <= HoverDistance)
                {
                    if (Client.IsHost() || !Client.IsConnected())
                    {
                        CurrentState = TaskState.During;
                        Recruit = Crate.SpawnCrate(Position3D - new Vector3(0, 12, 0), 0f);
                        var CallableRecruits = PlayerTank.TankKills.Where((a, b) => b > 0).ToArray();
                        Recruit.TankToSpawn = new TankTemplate()
                        {
                            AiTier = droneOwner is AITank ai && !Difficulties.Types["RandomizedTanks"] ?
                            CallableRecruits.Length > 0 ? Difficulties.Types["MasterModBuff"] ? (CallableRecruits[Client.ClientRandom.Next(0, CallableRecruits.Length)].Key - 1) % 9 + 1 :
                            CallableRecruits[Client.ClientRandom.Next(0, CallableRecruits.Length)].Key :
                            TankID.Brown :
                            Difficulties.Types["MasterModBuff"] ? Client.ClientRandom.Next(TankID.Brown, TankID.Black + 1) : Client.ClientRandom.Next(TankID.Brown, TankID.Obsidian + 1),
                            IsPlayer = false,
                            Team = droneOwner!.Team
                        };
                        if(droneOwner is AITank ai2 && ai2.AiTankType == CA_Main.Kudzu)
                        {
                            Recruit.TankToSpawn.AiTier = CA_Main.Kudzu;
                        }
                        CA_NetPlay.SyncDroneCrate(this, Recruit);
                    }
                }
                return;
            }
            HoverDistance = 4f;
            HoverSoftness = 330f;
            HoverStrength = 0.02f;
            if (CurrentState == TaskState.During)
            {
                Recruit!.position = Position3D - new Vector3(0, 12, 0);
                TargetPosition = RecruitRequestPos;
                if (Vector2.Distance(Position, TargetPosition) <= HoverDistance)
                {
                    Recruit.gravity = 2f;
                    CurrentState = TaskState.Finishing;
                }
                return;
            }

            if (CurrentState == TaskState.Finishing)
            {
                Task = DroneTask.Idle;
            }

        }
        #endregion
        public void DoShootParticles()
        {
            if (!CameraGlobals.IsUsingFirstPresonCamera)
            {
                var hit = GameHandler.Particles.MakeParticle(TurretPosition3D,
                    GameResources.GetGameResource<Texture2D>("Assets/textures/misc/bot_hit"));

                hit.Pitch = -CameraGlobals.DEFAULT_ORTHOGRAPHIC_ANGLE;
                hit.Scale = new(0.5f);
                hit.UniqueBehavior = (part) =>
                {
                    part.Color = Color.Orange;

                    if (part.LifeTime > 1)
                        part.Alpha -= 0.1f * RuntimeData.DeltaTime;
                    if (part.Alpha <= 0)
                        part.Destroy();
                };
            }
            Particle smoke;

            if (CameraGlobals.IsUsingFirstPresonCamera)
            {
                smoke = GameHandler.Particles.MakeParticle(TurretPosition3D,
                    ModelGlobals.Smoke.Asset,
                    GameResources.GetGameResource<Texture2D>("Assets/textures/misc/tank_smokes"));

                smoke.Scale = new(1.25f);
                smoke.Color = new(84, 22, 0, 255);
                smoke.HasAdditiveBlending = false;
            }
            else
            {
                smoke = GameHandler.Particles.MakeParticle(TurretPosition3D, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/tank_smokes"));

                smoke.Roll = -CameraGlobals.DEFAULT_ORTHOGRAPHIC_ANGLE;

                smoke.Scale = new(0.35f);

                smoke.Color = new(84, 22, 0, 255);

                smoke.HasAdditiveBlending = false;
            }

            int achieveable = 80;
            float step = 1;
            smoke.UniqueBehavior = (part) =>
            {
                part.Color.R = (byte)MathUtils.RoughStep(part.Color.R, achieveable, step);
                part.Color.G = (byte)MathUtils.RoughStep(part.Color.G, achieveable, step);
                part.Color.B = (byte)MathUtils.RoughStep(part.Color.B, achieveable, step);

                GeometryUtils.Add(ref part.Scale, 0.004f * RuntimeData.DeltaTime);

                if (part.Color.G == achieveable)
                {
                    part.Color.B = (byte)achieveable;
                    part.Alpha -= 0.04f * RuntimeData.DeltaTime;

                    if (part.Alpha <= 0)
                        part.Destroy();
                }
            };
        }


        

        private List<Tank> GetTanksInPath(Vector2 pathDir, out Vector2[] ricochetPoints, out Vector2[] tankCollPoints,
       bool draw = false, Vector2 offset = default, float missDist = 0.2f, Func<Block, bool> pattern = null, bool doBounceReset = true, bool considersRicos = false, Vector2? start = null)
        {
            const int MAX_PATH_UNITS = 1000;
            const int PATH_UNIT_LENGTH = 8;

            List<Tank> tanks = [];
            List<Vector2> ricoPoints = [];
            List<Vector2> tnkPoints = [];

            pattern ??= c => c.Properties.IsSolid || c.Type == BlockID.Teleporter;

            var whitePixel = TextureGlobals.Pixels[Color.White];
            Vector2 pathPos = (start is Vector2 s ? s : Position) + offset.Rotate(-TurretRotation);
            pathDir.Y *= -1;
            pathDir *= PATH_UNIT_LENGTH;

            int ricochetCount = 0;
            int uninterruptedIterations = 0;

            bool teleported = false;
            int tpTriggerIndex = -1;
            Vector2 teleportedTo = Vector2.Zero;

            var pathHitbox = new Rectangle();

            for (int i = 0; i < MAX_PATH_UNITS; i++)
            {
                uninterruptedIterations++;

                // World bounds check
                if (pathPos.X < GameScene.MIN_X || pathPos.X > GameScene.MAX_X)
                {
                    ricoPoints.Add(pathPos);
                    pathDir.X *= -1;
                    ricochetCount++;
                    if (doBounceReset) uninterruptedIterations = 0;
                }
                else if (pathPos.Y < GameScene.MIN_Z || pathPos.Y > GameScene.MAX_Z)
                {
                    ricoPoints.Add(pathPos);
                    pathDir.Y *= -1;
                    ricochetCount++;
                    if (doBounceReset) uninterruptedIterations = 0;
                }

                // Setup hitbox once
                pathHitbox.X = (int)pathPos.X - 5;
                pathHitbox.Y = (int)pathPos.Y - 5;
                pathHitbox.Width = 8;
                pathHitbox.Height = 8;

                Vector2 dummy = Vector2.Zero;
                Collision.HandleCollisionSimple_ForBlocks(pathHitbox, pathDir, ref dummy, out var dir, out var block, out bool corner, false, pattern);
                if (corner) break;

                if (block is not null)
                {
                    if (block.Type == BlockID.Teleporter && !teleported)
                    {
                        var dest = Block.AllBlocks.FirstOrDefault(bl => bl != null && bl != block && bl.TpLink == block.TpLink);
                        if (dest is not null)
                        {
                            teleported = true;
                            teleportedTo = dest.Position;
                            tpTriggerIndex = i + 1;
                        }
                    }
                    else if (block.Properties.AllowShotPathBounce)
                    {
                        ricoPoints.Add(pathPos);
                        ricochetCount += block.Properties.PathBounceCount;

                        switch (dir)
                        {
                            case CollisionDirection.Up:
                            case CollisionDirection.Down:
                                pathDir.Y *= -1;
                                break;
                            case CollisionDirection.Left:
                            case CollisionDirection.Right:
                                pathDir.X *= -1;
                                break;
                        }

                        if (doBounceReset) uninterruptedIterations = 0;
                    }
                }

                // Delay teleport until next frame
                if (teleported && i == tpTriggerIndex)
                {
                    pathPos = teleportedTo;
                }

                // Check destroy conditions
                bool hitsInstant = i == 0 && Block.AllBlocks.Any(x => x != null && x.Hitbox.Intersects(pathHitbox) && pattern(x));
                bool hitsTooEarly = i < (int)droneOwner!.Properties.ShellSpeed / 2 && ricochetCount > 0;
                bool ricochetLimitReached = ricochetCount > (considersRicos ? droneOwner!.Properties.RicochetCount : 0);

                if (hitsInstant || hitsTooEarly || ricochetLimitReached)
                    break;

                // Check tanks BEFORE moving
                float realMiss = 1f + (missDist * uninterruptedIterations);

                foreach (var enemy in GameHandler.AllTanks)
                {
                    if (enemy is null || enemy.IsDestroyed || tanks.Contains(enemy)) continue;

                    if (i > 15 && GameUtils.Distance_WiiTanksUnits(enemy.Position, pathPos) <= realMiss)
                    {
                        var pathAngle = pathDir.ToRotation();
                        var toEnemy = MathUtils.DirectionTo(pathPos, enemy.Position).ToRotation();

                        if (MathUtils.AbsoluteAngleBetween(pathAngle, toEnemy) >= MathHelper.PiOver2)
                            tanks.Add(enemy);
                    }

                    var pathCircle = new Circle { Center = pathPos, Radius = 4 };
                    if (enemy.CollisionCircle.Intersects(pathCircle))
                    {
                        tnkPoints.Add(pathPos);
                        tanks.Add(enemy);
                    }
                }

                if (draw)
                {
                    var screenPos = MatrixUtils.ConvertWorldToScreen(
                        Vector3.Zero,
                        Matrix.CreateTranslation(pathPos.X, 11, pathPos.Y),
                        CameraGlobals.GameView,
                        CameraGlobals.GameProjection
                    );

                    TankGame.SpriteRenderer.Draw(
                        whitePixel,
                        screenPos,
                        null,
                        Color.White * 0.5f,
                        0,
                        whitePixel.Size() / 2,
                        realMiss,
                        default,
                        default
                    );
                }

                pathPos += pathDir;
            }

            tankCollPoints = [.. tnkPoints];
            ricochetPoints = [.. ricoPoints];
            return tanks;
        }



        public void Remove()
        {
            if (DroneCollisions.BodyList.Contains(Body))
            {
                DroneCollisions.Remove(Body);
            }
            AllDrones[Id] = null;
        }


        internal void Render()
        {
            if (!GameScene.ShouldRenderAll)
                return;
            
            TankGame.Instance.GraphicsDevice.DepthStencilState = RenderGlobals.DefaultStencilState;
            DrawExtras();

            Projection = CameraGlobals.GameProjection;
            View = CameraGlobals.GameView;
            TankGame.SpriteRenderer.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            //  ChatSystem.SendMessage("Bepis", Color.SkyBlue);

            MorphGhostMode += RuntimeData.DeltaTime * (droneOwner!.Properties.Invisible && Task == DroneTask.Idle ? 1f : -1f) / 80f;
            if (!CampaignGlobals.InMission) MorphGhostMode = 0f;


            foreach (ModelMesh mesh in Model.Meshes)
            {
                TankGame.SpriteRenderer.GraphicsDevice.BlendState = mesh.Name.StartsWith("Hover") || mesh.Name.StartsWith("ring") ? BlendState.Additive : BlendState.AlphaBlend;
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = _boneTransforms[mesh.ParentBone.Index];
                    effect.View = CameraGlobals.GameView;
                    effect.Projection = CameraGlobals.GameProjection;
                    effect.TextureEnabled = true;
                    effect.Texture = _droneTexture!;

                    effect.Alpha = (mesh.Name.StartsWith("Hover") || mesh.Name.StartsWith("ring") ? 0.75f : 1f) * 1f - MorphGhostMode;
                }
                mesh.Draw();
            }

        }
        internal void DebugRender()
        {
            if (droneOwner is null) return;
            if (DebugLevel >= DebugManager.Id.FreeCamTest) return;

            //Parameter Toggles
            if (InputUtils.KeyJustPressed(Keys.R)) Debugging_SkillViewer -= 1;
            if (InputUtils.KeyJustPressed(Keys.T)) Debugging_SkillViewer += 1;
            if (Debugging_SkillViewer < 0) Debugging_SkillViewer = 3;
            if (Debugging_SkillViewer > 3) Debugging_SkillViewer = 0;

            var whitePixel = TextureGlobals.Pixels[Color.White];

            if (droneOwner is AITank ai && ai.TargetTank is not null)
               GetTanksInPath(Vector2.Normalize(droneOwner.Position.DirectionTo(ai.TargetTank.Position)) * new Vector2(1f, -1f), out var ricP, out var tnkCol, true, default, considersRicos: false, missDist: ai.Parameters.AimOffset, start: droneOwner.Position);

            Vector3 dOwner = droneOwner.Position3D;
            Vector3 dPos3D = Position3D;

            Vector2 tPos2D = TargetPosition;
            Vector2 dPos2D = Position;

            Vector3 tPos3D = tPos2D.ExpandZ();
            float steps = 5f;
            float distanceOwnerPointer = Vector3.Distance(dPos3D,dOwner);
            float distanceGoalPointer = Vector2.Distance(tPos2D,dPos2D);

            int dOP = (int)Math.Floor(distanceOwnerPointer / steps);
            int dGP = (int)Math.Floor(distanceGoalPointer / steps);

            //Draw the goal pointer
            for (int i = 1; i < dGP; i++)
            {
                Vector3 result = Vector3.Lerp(dPos3D,tPos3D, (float)i / dGP);
                //Color line = Color.Lerp(NeonPaint,BodyPaint,MathF.Sin(( i / 12f + testtimer ) * MathF.PI )/2f+0.5f);
                Color line = Color.Cyan;
                var pathPosScreen = MatrixUtils.ConvertWorldToScreen(Vector3.Zero, Matrix.CreateTranslation(result), CameraGlobals.GameView, CameraGlobals.GameProjection);
                DrawUtils.DrawTextureWithBorder(TankGame.SpriteRenderer, whitePixel, pathPosScreen, Color.Black,
                line, Vector2.One * (i == 0 || i == dGP - 1 ? 5f : 4f), 0f, Anchor.Center, 1f);
            }
            
            for (int i = 1; i < dOP; i++)
            {
                Vector3 result = Vector3.Lerp(dPos3D, dOwner, (float)i / dOP);
                Color line = Color.Magenta;
                var pathPosScreen = MatrixUtils.ConvertWorldToScreen(Vector3.Zero, Matrix.CreateTranslation(result), CameraGlobals.GameView, CameraGlobals.GameProjection);
                DrawUtils.DrawTextureWithBorder(TankGame.SpriteRenderer, whitePixel, pathPosScreen, Color.Black,
                line, Vector2.One * (i == 0 || i == dOP - 1 ? 5f : 4f), 0f, Anchor.Center, 1f);
            }
            //Draw the Owner pointer



            var drawInfo = new Dictionary<string, Color>
            {
                [$"Task:{CurrentState}-{Task}"] = Color.Azure,
                [$"Drone Rotation: {DroneRotation}"] = Color.Azure,
                [$"Velocity:X:{Velocity3D.X},Y:{Velocity3D.Y},Z:{Velocity3D.Z}"] = Color.Azure,
            };


            //for (int i = 0; i < drawInfo.Count; i++)
            //{
            //    var info = drawInfo.ElementAt(i);
            //    var pos = MatrixUtils.ConvertWorldToScreen(Vector3.Up * 20, Matrix.CreateTranslation(Position3D), View, Projection) -
            //        new Vector2(0, (i * 20));
            //    DrawUtils.DrawTextWithBorder(TankGame.SpriteRenderer, FontGlobals.RebirthFont, info.Key, pos,
            //        info.Value, Color.White, new Vector2(0.5f).ToResolution(), 0f, Anchor.TopCenter, 0.6f);
            //}
            if (Debugging_SkillViewer != 0)
            {
                drawInfo.Clear();
                var nodecolor = Color.Red;
                {
                    switch (Debugging_SkillViewer)
                    {
                        default: break;
                        case 1: //trap info
                            foreach (var item in Parameters.TrapsGeneral.GetType()!.GetProperties())
                            {
                                if (item is PropertyInfo property && property.CanWrite && property.CanRead)
                                {
                                    drawInfo.Add($"{property.Name}: {Parameters.TrapsGeneral[property.Name]}", nodecolor);
                                }
                            }
                            break;
                        case 2: //trap info
                            nodecolor = Color.Lime;
                            foreach (var item in Parameters.RecruitGeneral.GetType()!.GetProperties())
                            {
                                if (item is PropertyInfo property && property.CanWrite && property.CanRead)
                                {
                                    drawInfo.Add($"{property.Name}: {Parameters.RecruitGeneral[property.Name]}", nodecolor);
                                }
                            }
                            break;
                        case 3: //trap info
                            nodecolor = Color.Blue;
                            foreach (var item in Parameters.HoldGeneral.GetType()!.GetProperties())
                            {
                                if (item is PropertyInfo property && property.CanWrite && property.CanRead)
                                {
                                    drawInfo.Add($"{property.Name}: {Parameters.HoldGeneral[property.Name]}", nodecolor);

                                }
                            }
                            break;

                    }

                }
                var pos = MatrixUtils.ConvertWorldToScreen(Vector3.Right * 9, Matrix.CreateTranslation(Position3D), View, Projection);
                DrawUtils.DrawTextureWithBorder(TankGame.SpriteRenderer, whitePixel, pos, nodecolor, Color.White, new Vector2(120f, 2f).ToResolution(), 0, Anchor.LeftCenter, 0);
                pos += new Vector2(120f,0).ToResolution();
                DrawUtils.DrawTextureWithBorder(TankGame.SpriteRenderer, whitePixel, pos, nodecolor, Color.White, new Vector2(16f, 2f).ToResolution(), -MathHelper.PiOver4, Anchor.LeftCenter, 0);
                pos += new Vector2(16f,0f).Rotate(-MathHelper.PiOver4).ToResolution();
                for (int i = 0; i < drawInfo.Count; i++)
                {
                    var info = drawInfo.ElementAt(i);
                    var pos2 = pos -
                        new Vector2(0, ((1 + i) * 20));
                    DrawUtils.DrawStringWithBorder(TankGame.SpriteRenderer, FontGlobals.RebirthFont, info.Key, pos2,
                        info.Value, Color.White, new Vector2(0.5f).ToResolution(), 0f, Anchor.TopCenter, 0.6f);
                }

            }
            
           
        }


        //Yoink :3
        private static float Value_WiiTanksUnits(float value) => value * WII_TANKS_UNIT_CONVERSION;
        private float Distance_WiiTanksUnits(Vector2 position, Vector2 endPoint) => Vector2.Distance(position, endPoint) / 0.7f;

        public void DrawAwarenessCircle(BasicEffect effect, float awareness, Color color,Vector2 position)
        {
            if (awareness <= 0 || awareness > 1200)
                return;

            const int circleResolution = 32;

            float radius =awareness;
            float heightOffset = 0.2f; // Slightly above the tank Y to prevent z-fighting

            VertexPositionColor[] vertices = new VertexPositionColor[circleResolution + 1];

            for (int i = 0; i <= circleResolution; i++)
            {
                float angle = MathHelper.TwoPi * i / circleResolution;
                float x = MathF.Cos(angle) * radius;
                float z = MathF.Sin(angle) * radius;

                var worldPos = new Vector3(position.X + x, heightOffset, position.Y + z);
                vertices[i] = new VertexPositionColor(worldPos, color);
            }

            effect.World = Matrix.Identity;
            effect.View = View;
            effect.Projection = Projection;

            effect.VertexColorEnabled = true;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, vertices, 0, circleResolution);
            }
        }

        const float WII_TANKS_UNIT_CONVERSION = 0.71428571428f;
        private void DrawExtras()
        {
            if (droneOwner is null)
                return;

            // did i ever make any good programming choices before this past year or so?
            // this code looks like it was written by a 12 year old with a broken arm - GitHub Copilot
            // even ai hates my code.
            if (DebugManager.DebugLevel < DebugManager.Id.FreeCamTest)
            {
                float calculation = 0f;

                var drawInfo = new Dictionary<(string Name, float Value), (Color color, Vector3 position)>()
                {
                    [(nameof(Parameters.HoldGeneral) + "." + nameof(Parameters.HoldGeneral.Minimum), Parameters.HoldGeneral.Minimum)] = (Color.DarkBlue, droneOwner.Position3D),
                    [(nameof(Parameters.HoldGeneral) + "." + nameof(Parameters.HoldGeneral.Maximum), Parameters.HoldGeneral.Maximum)] = (Color.LightBlue, droneOwner.Position3D),

                    [(nameof(Parameters.RecruitGeneral) + "." + nameof(Parameters.RecruitGeneral.Minimum), Parameters.RecruitGeneral.Minimum)] = (Color.DarkGreen, droneOwner.Position3D),
                    [(nameof(Parameters.RecruitGeneral) + "." + nameof(Parameters.RecruitGeneral.Maximum), Parameters.RecruitGeneral.Maximum)] = (Color.LightGreen, droneOwner.Position3D),

                    [(nameof(Parameters.TrapsGeneral) + "." + nameof(Parameters.TrapsGeneral.Minimum), Parameters.TrapsGeneral.Minimum)] = (Color.DarkRed, droneOwner.Position3D),
                    [(nameof(Parameters.TrapsGeneral) + "." + nameof(Parameters.TrapsGeneral.Maximum), Parameters.TrapsGeneral.Maximum)] = (Color.LightSalmon, droneOwner.Position3D),

                };
 

                // if(Task == DroneTask.Hold) GetTanksInPath(Vector2.UnitY.Rotate(SeekingRotation), out var ricP, out var tnkCol, true, default);
                if (droneOwner is AITank ai && ai.TargetTank is not null)
                {
                    var target = GetTanksInPath(Vector2.Normalize(droneOwner.Position.DirectionTo(ai.TargetTank.Position)) * new Vector2(1f, -1f), out var ricP, out var tnkCol, false, default, considersRicos: false, missDist: ai.Parameters.AimOffset, start: droneOwner.Position);
                    DrawAwarenessCircle(TankBasicEffectHandler, Vector2.Distance(ai.TargetTank.Position, droneOwner.Position), target.Any(x => x.WorldId == ai.TargetTank.WorldId) ? Color.Lime : Color.DarkGray, droneOwner.Position);
                }

                for (int i = 0; i < drawInfo.Count; i++)
                {
                    var info = drawInfo.ElementAt(i);

                    var pos = MatrixUtils.ConvertWorldToScreen(Vector3.Up * 20, Matrix.CreateTranslation(info.Value.position), View, Projection) - new Vector2(0, i * 20);
                    DrawUtils.DrawStringWithBorder(TankGame.SpriteRenderer, FontGlobals.RebirthFont, $"{info.Key.Name}: {info.Key.Value}", pos, info.Value.color, Color.White,
                        Vector2.One * 0.75f, 0f, borderThickness: 0.25f);
                    DrawAwarenessCircle(TankBasicEffectHandler, info.Key.Value, info.Value.color,info.Value.position.FlattenZ());
                }

                drawInfo.Clear();

                drawInfo = new Dictionary<(string Name, float Value), (Color color, Vector3 position)>()
                {
                    
                    [($"Drone Rotation: {DroneRotation}", 0)] = (Color.Black,Position3D),
                    [($"Velocity:X:{Velocity3D.X},Y:{Velocity3D.Y},Z:{Velocity3D.Z}", 0)] = (Color.Black,Position3D),
                    [($"Task:{CurrentState}-{Task}", 0)] = (Color.Black,Position3D),
                    [($"",HoverDistance-HoverSoftness)] = (Color.Yellow, Position3D),
                    [($"{nameof(HoverDistance)}:{HoverDistance}", HoverDistance)] = (Color.Orange, TargetPosition.ExpandZ()),
                    [($"{nameof(HoverSoftness)}:{HoverDistance + HoverSoftness}", HoverDistance+HoverSoftness)] = (Color.Yellow, TargetPosition.ExpandZ()),
                };

                for (int i = 0; i < drawInfo.Count; i++)
                {
                    var info = drawInfo.ElementAt(i);

                    var pos = MatrixUtils.ConvertWorldToScreen(Vector3.Up * 20, Matrix.CreateTranslation(info.Value.position), View, Projection) - new Vector2(0, i * 20);
                    DrawUtils.DrawStringWithBorder(TankGame.SpriteRenderer, FontGlobals.RebirthFont, $"{info.Key.Name}", pos, info.Value.color, Color.White,
                        Vector2.One * 0.75f, 0f, borderThickness: 0.25f);
                    DrawAwarenessCircle(TankBasicEffectHandler, info.Key.Value, info.Value.color, info.Value.position.FlattenZ());
                }

            }


        }



    }


}
