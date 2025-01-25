using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TanksRebirth.GameContent;
using TanksRebirth.GameContent.GameMechanics;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Internals;
using TanksRebirth.Localization;
using TanksRebirth;
using static TanksRebirth.GameContent.Shell;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TanksRebirth.GameContent.ID;

using TanksRebirth.Internals.Common.Framework;
using static System.Runtime.InteropServices.JavaScript.JSType;
using TanksRebirth.GameContent.RebirthUtils;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.Net;
using TanksRebirth.Graphics;

namespace CobaltsArmada
{
    public class CA_Shell_Rail : ModShell
    {
       
        public override string Texture =>  "assets/textures/tank_lavender";
        public override string ShootSound => "assets/sfx/pew.ogg";
        public override string TrailSound => base.TrailSound;

        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Laser"
        });
        public override void OnCreate(Shell shell)
        {
            base.OnCreate(shell);
            shell.Model = CA_Main.Shell_Beam;
            shell.Properties.IsDestructible = false;
            //SLOOWWWW
            shell.Velocity = Vector2.Normalize(shell.Velocity)*0.001f;
            shell.LifeTime = 0f;
           
        }



        /// <summary>
        /// Called during a laser raycast
        /// </summary>
        /// <param name="shell">the original shell</param>
        /// <param name="Center">the kill position</param>
        /// <param name="Radius">the radius of the kill box</param>
        public static void CheckCollisions_DeathBeam(Shell shell, Vector2 Center, float Radius)
        {
            var Properties = shell.Properties;


            ref var tankSSpace = ref MemoryMarshal.GetReference((Span<Tank>)GameHandler.AllTanks);

            for (var i = 0; i < GameHandler.AllTanks.Length; i++)
            {
               
                var tank = Unsafe.Add(ref tankSSpace, i);
                if (tank == null || tank.Dead) continue;
               
                if (Vector2.Distance(tank.Position, Center) - tank.CollisionCircle.Radius > Radius) continue;
               
                tank.Damage(new TankHurtContextShell(shell));
            }

            ref var bulletSSpace = ref MemoryMarshal.GetReference((Span<Shell>)AllShells);

            for (var i = 0; i < AllShells.Length; i++)
            {
                ref var bullet = ref Unsafe.Add(ref bulletSSpace, i);
                if (bullet == null || bullet == shell) continue;
                if (Vector2.Distance(bullet.Position, Center)-bullet.HitCircle.Radius > Radius) continue;
                if (bullet.Properties.IsDestructible)
                    bullet.Destroy(DestructionContext.WithShell);
                // if two indestructible bullets come together, destroy them both. too powerful!
                if (bullet is { Properties.IsDestructible: true, } || Properties.IsDestructible) continue;
                // Lasers ignore eachother
                if (bullet is not null && bullet.Type == shell.Type) continue;
                // bullet is sometimes null here? so null safety is key
                bullet?.Destroy(DestructionContext.WithShell);


            }
        }


        public static float DoRaycast(Vector2 start, Vector2 destination, float MAX_DIST = 1000)
        {
            const int PATH_UNIT_LENGTH = 1;


            // 20, 30

            var pathDir = MathUtils.DirectionOf(start, destination).ToRotation();

            var pathPos = start + Vector2.Zero.RotatedByRadians(pathDir);

            pathDir *= PATH_UNIT_LENGTH;

            for (int i = 0; i < MAX_DIST; i++)
            {
                var dummyPos = Vector2.Zero;

                if (pathPos.X < GameSceneRenderer.MIN_X || pathPos.X > GameSceneRenderer.MAX_X)
                {
                    return MAX_DIST;
                }
                if (pathPos.Y < GameSceneRenderer.MIN_Z || pathPos.Y > GameSceneRenderer.MAX_Z)
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

            AITank ai = (AITank)shell.Owner;
            for (var i = 0; i < ai.ShotPathTankCollPoints.Length; i++)
            {
                CheckCollisions_DeathBeam(shell, ai.ShotPathTankCollPoints[i],unforgiveness);
            }
            
        }
        

        float LaserLerp(float x,float offset)
        {
            //The in lerp
            float a = MathHelper.Lerp(0,1,x/(0.5f-offset));
            //the out lerp
            float b = MathHelper.Lerp(1, 0, MathF.Max(0f,x-0.5f-offset)/(0.5f+offset));
            return a >= 1f ? Easings.OutCirc(b): Easings.InOutCubic(a);
        }
        

    public override void PostUpdate(Shell shell)
        {
            if (shell is null) return;
            base.PostUpdate(shell);
            if (shell.Owner is null) return;
            if (shell.Owner is PlayerTank) return;
            AITank ai = (AITank)shell.Owner;
            if (ai.ShotPathRicochetPoints.Length<1) return;
            if (Server.NetManager != null || !Client.IsConnected())
            {
                float Laser_length = Vector2.Distance(shell.Position, ai.ShotPathRicochetPoints[0]);
                float BEW = shell.LifeTime / 60 * MathF.PI * 2f;
                float dur = 1.5f;

                float scaletimer = LaserLerp(MathF.Max(0f, -MathF.Cos(BEW / dur) / 2f + 0.5f), 0.3f);
                float laser_Magnify = Difficulties.Types["PieFactory"] ? 4.4f : 2.1f;

                float reacher = 1.075f;
                shell.World = Matrix.CreateScale(scaletimer * laser_Magnify, scaletimer * laser_Magnify, Laser_length / 8f * reacher) * Matrix.CreateFromYawPitchRoll(-shell.Rotation, 0, 0)
                    * Matrix.CreateTranslation(shell.Position3D) * Matrix.CreateTranslation(Vector3.Normalize(shell.Velocity3D) * Laser_length / 2f * reacher);
                if (scaletimer > 0.2) DoKillcast(shell, laser_Magnify * 1.25f);

                if (dur * 60f < shell.LifeTime)
                {
                    shell.Remove();
                }
            }
        }

    }
}
