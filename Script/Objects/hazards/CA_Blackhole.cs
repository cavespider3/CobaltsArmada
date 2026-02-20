using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.RebirthUtils;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.GameContent.Systems.TankSystem.AI;
using TanksRebirth.Graphics;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.Utilities;
using static CobaltsArmada.Script.Tanks.Class_T.DroneParameters;
using static TanksRebirth.GameContent.RebirthUtils.DebugManager;

namespace CobaltsArmada.Script.Objects.hazards
{
    /// <summary>
    /// A special type of hazard capable of reeking havoc on any stage
    /// </summary>
    #nullable enable
    
    public class CA_Blackhole : IAITankDanger
    {
        #region Static/Fixed values
        /// <summary>
        /// The max number of blackholes that can be active at once
        /// </summary>
        public const int MAXBLACKHOLES = 50;
        public static CA_Blackhole[] AllBlackholes { get; } = new CA_Blackhole[MAXBLACKHOLES];

       
        public int Team => Owner is not null ? Owner.Team : TeamID.NoTeam;

        #endregion

        #region Display

        //public Matrix View;
        //public Matrix Projection;
        //public Matrix World;

        //public Model InnerSphere;
        //public Model OuterSphere;

        #endregion

        public Tank? Owner;
        public Vector2 Position { get {
                return Position3D.FlattenZ();
            }
            set { } }
        public float PullForce { get; set; }
        public float LifeTime { get; set; }

        public float pulsetimer = 0f;

        public float Size { get; set; }

        public Vector3 Position3D { get; set; }

        public BoundingSphere KillZone;
        public BoundingSphere PullZone;
        public int Id;

        public CA_Blackhole(Vector3 pos,Tank? owner = null) {
            Position3D = pos;
            AITank.Dangers.Add(this);
            Owner = owner;
            int index = Array.IndexOf(AllBlackholes, null);
            Id = index;
            AllBlackholes[index] = this;
            Size = 0.02f;
            PullForce = 0.3f;
        }

        public void Update()
        {
            if (!GameScene.UpdateAndRender) return;
            if (pulsetimer < 0f)
            {
                var glow2 = GameHandler.Particles.MakeParticle(Position3D, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
                glow2.Scale = Vector3.One * 0.07f * MathF.Max(Size,1) * Explosion.MAGIC_EXPLOSION_NUMBER;
                glow2.Pitch = MathHelper.PiOver2;
                glow2.FaceTowardsMe = false;
                glow2.Alpha = 0f;

                glow2.HasAdditiveBlending = false;
                glow2.Color = Color.Purple;

                glow2.UniqueBehavior = (a) =>
                {
                    glow2.Scale = (1f - glow2.Alpha) * Vector3.One * 0.07f * MathF.Max(Size, 1) * Explosion.MAGIC_EXPLOSION_NUMBER / 2;
                    glow2.Position -= RuntimeData.DeltaTime / 60f * 1f * -Vector3.UnitY;
                    glow2.Alpha += 0.75f * RuntimeData.DeltaTime / 60f / 0.8f;
                    if (glow2.Alpha >= 1f)
                        glow2.Destroy();
                };

                var glow3 = GameHandler.Particles.MakeParticle(Position3D, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
                glow3.Scale = Vector3.One * 0.07f * Size * Explosion.MAGIC_EXPLOSION_NUMBER;
                glow3.Pitch = MathHelper.PiOver2;
                glow3.FaceTowardsMe = false;
                glow3.Alpha = 0f;

                glow3.HasAdditiveBlending = true;
                glow3.Color = Color.Purple;

                glow2.UniqueBehavior = (a) =>
                {
                    glow2.Scale = (1f - glow2.Alpha) * Vector3.One * 0.07f * Size * Explosion.MAGIC_EXPLOSION_NUMBER / 2;
                    glow2.Position -= RuntimeData.DeltaTime / 60f * 1f * -Vector3.UnitY;
                    glow2.Alpha += 0.75f * RuntimeData.DeltaTime / 60f / 1f;
                    if (glow2.Alpha >= 1.2f)
                        glow2.Destroy();
                };

                pulsetimer = 70f - Math.Max(LifeTime/10f,60f);
            }
            pulsetimer -= RuntimeData.DeltaTime;

            Size += ((RuntimeData.DeltaTime / 60f / 2f) + MathF.Floor(LifeTime / 60f) * 0.001f * RuntimeData.DeltaTime) / 1.5f;

            LifeTime += RuntimeData.DeltaTime;
            

            KillZone = new(Position3D, Size * Explosion.MAGIC_EXPLOSION_NUMBER * 0.5f);
           // PullZone = new(Position.ExpandZ(), Size * Explosion.MAGIC_EXPLOSION_NUMBER * MathF.PI);
            try
            {
                if (!IntermissionSystem.IsAwaitingNewMission)
                {
                    foreach (var mine in Mine.AllMines)
                    {
                        if (mine is not null && Vector2.Distance(mine.Position, Position) <= Size * Explosion.MAGIC_EXPLOSION_NUMBER) // magick
                            mine.Remove();
                        else if (mine is not null && Vector2.Distance(mine.Position, Position) <= Size * Explosion.MAGIC_EXPLOSION_NUMBER * Math.PI)
                            mine.Position += mine.Position.DirectionTo(Position) * (float)((Size * Explosion.MAGIC_EXPLOSION_NUMBER * Math.PI) - Vector2.Distance(mine.Position, Position)) /6f * RuntimeData.DeltaTime / 240 * PullForce;


                    }
                    foreach (var block in Block.AllBlocks)
                    {
                        if (block is not null && Vector2.Distance(block.Position, Position) <= Size * Explosion.MAGIC_EXPLOSION_NUMBER && block.Type != BlockID.Hole)
                            block.Destroy();
                        else if (block is not null && Vector2.Distance(block.Position, Position) <= Size * Explosion.MAGIC_EXPLOSION_NUMBER * Math.PI && block.Type != BlockID.Hole)
                            block.Position += block.Position.DirectionTo(Position) * (float)((Size * Explosion.MAGIC_EXPLOSION_NUMBER * Math.PI) - Vector2.Distance(block.Position, Position))/ 6  * RuntimeData.DeltaTime / 240 * PullForce;
                    }
                    foreach (var shell in Shell.AllShells)
                    {
                        if (shell is not null && Vector2.Distance(shell.Position, Position) < Size * Explosion.MAGIC_EXPLOSION_NUMBER)
                            shell.Destroy(Shell.DestructionContext.WithExplosion);
                        else if (shell is not null && Vector2.Distance(shell.Position, Position) <= Size * Explosion.MAGIC_EXPLOSION_NUMBER * 5f)
                            shell.Velocity += shell.Position.DirectionTo(Position) * RuntimeData.DeltaTime / (60 * 5) * PullForce;
                    }
                    foreach (var tank in GameHandler.AllTanks)
                    {
                        if (tank is null || tank.IsDestroyed)
                            continue;
                        if (Vector2.Distance(tank.Position, Position) < Size * Explosion.MAGIC_EXPLOSION_NUMBER)
                        {
                            if (Owner is null)
                                tank.Damage(new TankHurtContextOther(null, TankHurtContextOther.HurtContext.FromIngame, "SUCC"), true);
                            else if (Owner is not null)
                            {
                                tank.Damage(new TankHurtContextOther(Owner, TankHurtContextOther.HurtContext.FromIngame, "SUCC"), true);
                            }
                        }
                        else if (tank is not null && Vector2.Distance(tank.Position, Position) <= Size * Explosion.MAGIC_EXPLOSION_NUMBER * Math.Tau)
                        {
                            tank.KnockbackVelocity += tank.Position.DirectionTo(Position) * RuntimeData.DeltaTime / 60 * PullForce * 0.1f;
                            if (tank.Properties.Stationary) tank.Properties.Stationary = false;
                        }                      
                    }
                    
                }
            }
            catch{ }


            if (LifeTime / 60f > 10f)
            {
                Remove();
            }

        }
        public void DebugRender()
        {
            DebugManager.DrawBoundingSphere(KillZone, Color.Lavender, CameraGlobals.GameView, CameraGlobals.GameProjection);

          //  DebugManager.DrawBoundingSphere(PullZone, Color.Red, CameraGlobals.GameView, CameraGlobals.GameProjection);
        }

        public void Remove()
        {
            AITank.Dangers.Remove(this);
            AllBlackholes[Id] = null;
        }

    }
}
