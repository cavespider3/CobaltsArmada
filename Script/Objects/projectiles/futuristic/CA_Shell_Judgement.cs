using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TanksRebirth.GameContent;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Internals;
using TanksRebirth;
using static TanksRebirth.GameContent.Shell;
using static TanksRebirth.GameContent.Mine;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.Graphics;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.UI.MainMenu;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Internals.Common.Framework.Collisions;
using TanksRebirth.GameContent.ID;


namespace CobaltsArmada
{/// <summary>
 /// JUDGEMENT
 /// </summary>
    #nullable enable
    public class CA_OrbitalStrike : IAITankDanger
    {
       // private Vector2 _oldPosition;

        // this used to be 1500. why?
        /// <summary>The maximum shells allowed at any given time.</summary>
        private const int MaxLasers = 200;

        public static CA_OrbitalStrike[] AllLasers { get; } = new CA_OrbitalStrike[MaxLasers];

        public bool IsPlayerSourced { get; set; }
        public Vector2 Position { get; set; }

        public Vector3 Position3D => Position.ExpandZ() + new Vector3(0, 11, 0);

        /// <summary> The laser's behavior while it's on the field </summary>
        public Action<CA_OrbitalStrike>? UniqueBehavior;

        public Matrix View;
        public Matrix Projection;
        public Matrix World;

        public Model Model;

        public OggAudio? PrepSound;
        public OggAudio? FireSound;

        /// <summary> The time it takes for the laser to warm up </summary>
        public float WarningTime;
        /// <summary> The time the laser will last before fading </summary>
        public float ActiveTime;
        public float LifeTime;
        public float WarningForgiveness;
    
        /// <summary> The range og</summary>
        public float Radius;

        private bool Firing;

        public Tank? Owner;
        public Texture2D? _LaserTexture;

        public enum TeamkillContext
        {
            ///<summary>Anyone not on the same team</summary>
            NotTeam,
            /// <summary>Anyone but the owner</summary>
            NotMyself,
            /// <summary>Why?</summary>
            OnlyMyself,
            /// <summary>that Tank is fucking stupid</summary>
            OnlyTeam,
            /// <summary>I DONT CARE</summary>
            All,
        }
        public TeamkillContext context { get; private set; }
        public int Id { get; private set; }

        public int Team => Owner is not null ? Owner.Team : TeamID.NoTeam;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"> the position of the laser</param>
        /// <param name="owner"> the AITank that fired it</param>
        /// <param name="warningtime"> how long before the laser is fired</param>
        /// <param name="warningforgiveness">the amplified size of the warning marker</param>
        /// <param name="firetime"> how long the laser is active for</param>
        public CA_OrbitalStrike(Vector2 position , Tank owner , float radius =2f, float warningtime=1.5f , float warningforgiveness = 0f , float firetime=2f , TeamkillContext friendlyfire = TeamkillContext.NotMyself)
        {
            SoundPlayer.PlaySoundInstance("Assets/sounds/mine_place.ogg", SoundContext.Effect, 0.8f,pitchOverride:0.8f);
            TankGame.ClientLog.Write("Calling Orbital Strike", TanksRebirth.Internals.LogType.Info);
            WarningForgiveness = MathF.Max(0f, warningforgiveness);
            Position = position;
            Model = CA_Main.Shell_Beam;
            _LaserTexture =CA_Main.Beam;
            AITank.Dangers.Add(this);
            IsPlayerSourced = owner is PlayerTank;

            Owner = owner;

            int index = Array.IndexOf(AllLasers, null);

            Id = index;

            AllLasers[index] = this;

            context = friendlyfire;

            WarningTime = warningtime * 60f;
            ActiveTime = firetime * 60f;
            Radius = radius;
            LifeTime = 0f;
            Hitbox.Radius = radius * 2f;
            Hitbox.Center = position;
        }

        float LaserLerp(float x, float offset)
        {
            //The in lerp
            float a = MathHelper.Lerp(0, 1, x / (0.5f - offset));
            //the out lerp
            float b = MathHelper.Lerp(1, 0, MathF.Max(0f, x - 0.5f - offset) / (0.5f + offset));
            return a >= 1f ? Easings.OutCirc(b) : Easings.InOutCubic(a);
        }

      
        const float Laser_length = 90f;

        public Circle Hitbox = new();

        internal void Update()
        {
            if (!GameScene.ShouldRenderAll || (!CampaignGlobals.InMission && !MainMenuUI.Active))
                return;

            float Ani_ObliterateScale = MathF.Max(0, (LifeTime - WarningTime) / 60 * MathF.PI * 2f);
            float scaletimer = LaserLerp(MathF.Max(0f, -MathF.Cos(Ani_ObliterateScale / (ActiveTime/60f)) / 2f + 0.5f), 0.3f);
            World = Matrix.CreateScale(scaletimer * Radius * MathF.PI, scaletimer * Radius * MathF.PI, Laser_length) * Matrix.CreateFromYawPitchRoll(0, MathHelper.PiOver2, 0) * Matrix.CreateTranslation(Position3D);
          

            //SERVER SIDED BS
          
                Hitbox.Radius =scaletimer * Radius * MathF.PI;
                LifeTime += RuntimeData.DeltaTime;
                if(!Firing && Ani_ObliterateScale>0f)
                {
                    SoundPlayer.PlaySoundInstance("Assets/sounds/mine_explode.ogg", SoundContext.Effect, 0.5f,pitchOverride:1f);
                    Firing = true;
                    var ring = GameHandler.Particles.MakeParticle(Position3D + Vector3.UnitY * 0.01f, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
                    ring.Scale = new(0.2f * Radius + WarningForgiveness * 0.035f );
                    ring.Pitch = MathHelper.PiOver2;
                    ring.HasAddativeBlending = true;
                    ring.Color = Color.Cyan;
                    
                    ring.UniqueBehavior = (a) =>
                    {
                        ring.Alpha -= 0.1f * RuntimeData.DeltaTime;

                        GeometryUtils.Add(ref ring.Scale, (0.03f + WarningForgiveness*0.035f) * RuntimeData.DeltaTime);
                        if (ring.Alpha <= 0)
                            ring.Destroy();
                    };
                    var ring2 = GameHandler.Particles.MakeParticle(Position3D + Vector3.UnitY * 0.01f, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
                    ring2.Scale = new(0.2f * Radius + WarningForgiveness * 0.035f);
                    ring2.Pitch = MathHelper.PiOver2;
                    ring2.HasAddativeBlending = true;
                    ring2.Color = Color.Cyan;
                    
                    ring2.UniqueBehavior = (a) =>
                    {
                        ring2.Alpha -= 0.1f * RuntimeData.DeltaTime;

                        GeometryUtils.Add(ref ring2.Scale, (0.06f + WarningForgiveness * 0.035f) * RuntimeData.DeltaTime);
                        if (ring2.Alpha <= 0)
                            ring2.Destroy();
                    };
                }
                if(Ani_ObliterateScale>0f)
                {
                    CheckCollisions_DeathBeam();
                }
                if ((Math.Floor(LifeTime) % 60f) % 20f == 0 && LifeTime < WarningTime)
                {
                    var EasierWarning = GameHandler.Particles.MakeParticle(Position3D + Vector3.UnitY * 0.01f, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
                    EasierWarning.Scale = new(0.34f * Radius + WarningForgiveness * 0.035f);
                    EasierWarning.Roll = MathHelper.PiOver2;
                    EasierWarning.HasAddativeBlending = true;
                    EasierWarning.Color = Color.Cyan;

                    EasierWarning.UniqueBehavior = (a) =>
                    {
                        EasierWarning.Alpha -= 0.06f * RuntimeData.DeltaTime;

                        EasierWarning.Position+= Vector3.UnitY * (0.8f + WarningForgiveness * 0.035f) * RuntimeData.DeltaTime;
                        if (EasierWarning.Alpha <= 0)
                            EasierWarning.Destroy();
                    };

                    var ring = GameHandler.Particles.MakeParticle(Position3D + Vector3.UnitY * 0.01f, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
                    ring.Scale = new(0.34f * Radius + WarningForgiveness * 0.035f );
                    ring.Pitch = MathHelper.PiOver2;
                    ring.HasAddativeBlending = true;
                    ring.Color = Color.Red;
                    SoundPlayer.PlaySoundInstance("Assets/sounds/mine_trip.ogg", SoundContext.Effect, 0.2f,pitchOverride:(LifeTime/WarningTime)-0.5f);
                    ring.UniqueBehavior = (a) =>
                    {
                        ring.Alpha -= 0.08f * RuntimeData.DeltaTime;

                        GeometryUtils.Add(ref ring.Scale, (0.01f + WarningForgiveness * 0.035f) * RuntimeData.DeltaTime);
                        if (ring.Alpha <= 0)
                            ring.Destroy();
                    };
                }
               

            



            if (WarningTime + ActiveTime <= LifeTime)
            {
                Remove();
            }

        }

        public void Remove()
        {
            AITank.Dangers.Remove(this);
            AllLasers[Id] = null;
        }

        internal void Render()
        {

            View = CameraGlobals.GameView;
            Projection = CameraGlobals.GameProjection;
            //DebugManager.DrawDebugString(TankGame.SpriteRenderer, $"DetonationTime: {DetonateTime}/{DetonateTimeMax}\nNearDestructibles: {IsNearDestructibles}\nId: {Id}", MatrixUtils.ConvertWorldToScreen(Vector3.Zero, World, View, Projection) - new Vector2(0, 20), 1, centered: true);
            TankGame.SpriteRenderer.GraphicsDevice.BlendState = BlendState.Additive;
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = World;
                    effect.View = CameraGlobals.GameView;
                    effect.Projection = CameraGlobals.GameProjection;

                    effect.LightingEnabled = false;

                    effect.TextureEnabled = true;

                    effect.Texture = _LaserTexture;
                    effect.Alpha = 1f;
                
                    //if (IsOpening)
                    //effect.Alpha -= fadeScale;
                    
                }
                mesh.Draw();

            }
            
        }
          //  OnPostRender?.Invoke(this);
        
    

    /// <summary>
    /// Called during a laser raycast
    /// </summary>
    /// <param name="shell">the original shell</param>
    /// <param name="Center">the kill position</param>
    /// <param name="Radius">the radius of the kill box</param>
    public void CheckCollisions_DeathBeam()
        {

            ref var tankSSpace = ref MemoryMarshal.GetReference((Span<Tank>)GameHandler.AllTanks);

            for (var i = 0; i < GameHandler.AllTanks.Length; i++)
            {
                var tank = Unsafe.Add(ref tankSSpace, i);
                if (tank == null || tank.Dead) continue;
                if (context == TeamkillContext.NotTeam && tank.Team == Owner?.Team) continue;
                if (context == TeamkillContext.OnlyTeam && tank.Team != Owner?.Team) continue;
                if (context == TeamkillContext.NotMyself && tank == Owner) continue;
                if (context == TeamkillContext.OnlyMyself && tank != Owner) continue;
                
                if (Vector2.Distance(tank.Position, Position) - tank.CollisionCircle.Radius > Radius) continue;
               
                tank.Damage(new TankHurtContextOther(tank,TankHurtContextOther.HurtContext.FromIngame,"JUDGED"),true);
            }

            ref var bulletSSpace = ref MemoryMarshal.GetReference((Span<Shell>)AllShells);

            for (var i = 0; i < AllShells.Length; i++)
            {
                ref var bullet = ref Unsafe.Add(ref bulletSSpace, i);
                if (bullet == null) continue;
                if (context == TeamkillContext.NotTeam && bullet.Owner?.Team == Owner?.Team) continue;
                if (context == TeamkillContext.OnlyTeam && bullet.Owner?.Team != Owner?.Team) continue;
                if (context == TeamkillContext.NotMyself && bullet.Owner == Owner) continue;
                if (context == TeamkillContext.OnlyMyself && bullet.Owner != Owner) continue;
                if (Vector2.Distance(bullet.Position, Position) -bullet.HitCircle.Radius > Hitbox.Radius) continue;
                bullet?.Remove();


            }
            ref var mineSSpace = ref MemoryMarshal.GetReference((Span<Mine>)AllMines);
            for (var i = 0; i < AllMines.Length; i++)
            {
                ref var mine = ref Unsafe.Add(ref mineSSpace, i);
                if (mine == null) continue;
                if (context == TeamkillContext.NotTeam && mine.Owner?.Team == Owner?.Team) continue;
                if (context == TeamkillContext.OnlyTeam && mine.Owner?.Team != Owner?.Team) continue;
                if (context == TeamkillContext.NotMyself && mine.Owner == Owner) continue;
                if (context == TeamkillContext.OnlyMyself && mine.Owner != Owner) continue;
                if (Vector2.Distance(mine.Position, Position) - 6f > Hitbox.Radius) continue;
                mine?.Detonate();
            }

        }


       /* public static float DoRaycast(Vector2 start, Vector2 destination, float MAX_DIST = 1000)
        {
            const int PATH_UNIT_LENGTH = 1;


            // 20, 30

            var pathDir = MathUtils.DirectionOf(start, destination).ToRotation();

            var pathPos = start + Vector2.Zero.RotatedByRadians(pathDir);

            pathDir *= PATH_UNIT_LENGTH;

            for (int i = 0; i < MAX_DIST; i++)
            {
                var dummyPos = Vector2.Zero;

                if (pathPos.X < MapRenderer.MIN_X || pathPos.X > MapRenderer.MAX_X)
                {
                    return MAX_DIST;
                }
                if (pathPos.Y < MapRenderer.MIN_Y || pathPos.Y > MapRenderer.MAX_Y)
                {
                    return MAX_DIST;
                }

                var pathHitbox = new Rectangle((int)pathPos.X, (int)pathPos.Y, 1, 1);

                // Why is velocity passed by reference here lol
                Collision.HandleCollisionSimple_ForBlocks(pathHitbox, start, ref dummyPos, out var dir, out var block, out bool corner, false);

                switch (dir)
                {
                    case CollisionDirection.Up:
                    case CollisionDirection.Down:
                        return Vector2.Distance(pathPos, start);
                    case CollisionDirection.Left:
                    case CollisionDirection.Right:
                        return Vector2.Distance(pathPos, start);
                }
             

                pathPos += Vector2.Normalize(MathUtils.DirectionOf(start, destination));
                
             

            }
            return MAX_DIST;
        }

        public static void DoKillcast(Shell shell,float unforgiveness=5.3f,bool draw = false)
        {
            if (shell.Owner is null) return;
            if (shell.Owner is PlayerTank) return;
            CheckCollisions_DeathBeam(shell, shell.Position,unforgiveness);
        }
        

        
        

    public override void PostUpdate(Shell shell)
        {
            base.PostUpdate(shell);
            if (shell.Owner is null) return;
            if (shell.Owner is PlayerTank) return;
            float Warning = 3f*60f;
            float laser_Magnify = 6f;
            if ((Math.Floor(shell.LifeTime) % 60f) % 20f == 0 && shell.LifeTime<Warning)
            {
                var ring = GameHandler.Particles.MakeParticle(shell.Position3D + Vector3.UnitY*0.01f, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
                ring.Scale = new(0.6f);
                ring.Roll = MathHelper.PiOver2;
                ring.HasAddativeBlending = true;
                ring.Color = Color.Red;

                ring.UniqueBehavior = (a) => {
                    ring.Alpha -= 0.08f * RuntimeData.DeltaTime;

                    GeometryUtils.Add(ref ring.Scale, 0.01f * RuntimeData.DeltaTime);
                    if (ring.Alpha <= 0)
                        ring.Destroy();
                };
            }
            AITank ai = (AITank)shell.Owner;
            float Laser_length =64f;
            float BEW = MathF.Max(0, ((shell.LifeTime - Warning)/60) * MathF.PI*2f);
            float dur = 1.5f;
            float scaletimer = LaserLerp(MathF.Max(0f,-MathF.Cos(BEW/ dur) /2f + 0.5f),0.3f);
            
         
            shell.World = Matrix.CreateScale(scaletimer*laser_Magnify, scaletimer* laser_Magnify, Laser_length) * Matrix.CreateFromYawPitchRoll(0, MathHelper.PiOver2, 0)
                * Matrix.CreateTranslation(shell.Position3D);
            if (scaletimer > 0.2) DoKillcast(shell, laser_Magnify);
           
            if (dur * 60f< shell.LifeTime - Warning)
            {
                shell.Remove();
            }
        }
       */
    }
}
