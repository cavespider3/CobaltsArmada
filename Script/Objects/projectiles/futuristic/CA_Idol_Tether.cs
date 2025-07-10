using Microsoft.Xna.Framework;

using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.UI;
using TanksRebirth.GameContent.UI.MainMenu;
using TanksRebirth.Graphics;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.Internals.Common.Utilities;
using static TanksRebirth.GameContent.RebirthUtils.DebugManager;

namespace CobaltsArmada.Objects.projectiles.futuristic
{
    /// <summary>
    /// P-2 anyone? :tro:
    /// </summary>
    public class CA_Idol_Tether
    {
        public Tank? bindHost;
        public Tank? bindTarget;
        public float Ticker;

        public static CA_Idol_Tether[] AllTethers= new CA_Idol_Tether[GameHandler.MAX_AI_TANKS];
        public int Id { get; private set; }

        public List<Particle> vfx_line = new ();
        public List<Particle> vfx_shield = new();


        public CA_Idol_Tether(Tank host, Tank target)
        {
            SoundPlayer.PlaySoundInstance("Assets/sounds/mine_place.ogg", SoundContext.Effect, 0.8f,pitchOverride:0.7f, gameplaySound: true);
            bindHost = host;
            bindTarget = target;
            int index = Array.IndexOf(AllTethers, null);
            Id = index;
            AllTethers[index] = this;
            for (int i = 0; i < 6; i++)
            {
                float a = MathF.PI / 6 * i;
                float h = 5f;
                Vector3 path = (Vector2.UnitY.Rotate(a)*target.CollisionCircle.Radius*1.1f).ExpandZ()+ Vector3.UnitY * h;
                var p = GameHandler.Particles.MakeParticle(path, TextureGlobals.Pixels[Color.Blue]);
                p.FaceTowardsMe = true;
                p.Scale = new(3f);
                p.Pitch = MathHelper.PiOver4;
                p.Roll = MathHelper.PiOver2;
                p.Alpha = 0.8f;
                p.UniqueBehavior = (a) => {

                    if (p.Alpha <= 0f)
                        p.Destroy();
                };
                vfx_shield.Add(p);
            }
        }
        public bool Valid(Tank tank)
        {
            return tank is not null && !tank.Dead;
        }
        public float distance()
        {
            if (bindHost is null || bindTarget is null) return 0f;
            if (!(Valid(bindHost)&& Valid(bindTarget))) return 0f;
            return Vector2.Distance(bindHost.Position, bindTarget.Position);

        }
        internal void Update()
        {
            if (!GameScene.ShouldRenderAll || (!CampaignGlobals.InMission && !MainMenuUI.Active))
                return;
            if (bindHost is null || bindTarget is null) { Remove(); return; }
            if (!(Valid(bindHost) && Valid(bindTarget))) { Remove(); return; }
            bindTarget.Properties.Immortal = !bindTarget.Dead;

            Ticker += RuntimeData.DeltaTime;
            int bits = (int)Math.Floor(distance() / 8f);
            if (bits != vfx_line.Count)//update the tether
            {
                foreach (var item in vfx_line)
                {
                    item.Alpha = 0f;
                    item?.Destroy();
                }
                vfx_line.Clear();
                for (int i = 0;  i < bits;  i++)
                {
                    float a = MathF.PI / bits * i;
                    a += MathF.PI / (bits * 2f);
                    float h = MathF.Sin(a) * distance() * 0.3f + 5f;
                    Vector3 path = Vector2.Lerp(bindHost.Position, bindTarget.Position, (float)i / bits).ExpandZ() + Vector3.UnitY * h;
                    var p = GameHandler.Particles.MakeParticle(path, TextureGlobals.Pixels[Color.Cyan]);
                    p.FaceTowardsMe = true;
                    p.Scale = new(1.5f);
                    p.Yaw = MathHelper.PiOver4;
                    p.Alpha = 0.5f;
                    p.UniqueBehavior = (a) => {
                        
                        if (p.Alpha <= 0f)
                            p.Destroy();
                    };
                    vfx_line.Add(p);
                }

               


            }
             for (int i = 0; i < bits; i++)
                {
                    float a = MathF.PI / bits * i;
                    a += MathF.PI / (bits * 2f);
                    float h = MathF.Sin(a) * distance() * 0.3f + 5f;
                    Vector3 path = Vector2.Lerp(bindHost.Position, bindTarget.Position, (float)i / bits).ExpandZ() + Vector3.UnitY * h;
                    vfx_line[i].Position = path;
                }
            for (int i = 0; i < vfx_shield.Count; i++)
            {
                float a = (MathF.PI / vfx_shield.Count * i * 2f) + (vfx_shield[i].LifeTime / 60f * MathHelper.PiOver2);
                float h = 5f;
                Vector3 path = (Vector2.UnitY.Rotate(a) * bindTarget.CollisionCircle.Radius * 1.1f).ExpandZ() + Vector3.UnitY * h;
                vfx_shield[i].Position = path+ bindTarget.Position3D;
                vfx_shield[i].Yaw = -a;
            }



        }

        public void Remove()
        {
            foreach (var item in vfx_line)
            {
                item.Alpha = 0f;
                item?.Destroy();
            }
            foreach (var item in vfx_shield)
            {
                item.Alpha = 0f;
                item?.Destroy();
            }
            if (bindTarget is not null) bindTarget.Properties.Immortal = false;
            AllTethers[Id] = null;
        }

    }
}
