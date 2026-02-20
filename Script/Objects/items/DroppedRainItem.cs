using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;

using System.Text;
using System.Threading.Tasks;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.Systems.ParticleSystem;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.Framework;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Net;
using static TanksRebirth.GameContent.RebirthUtils.DebugManager;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace CobaltsArmada.Script.Objects.items
{
    /// <summary>
    /// An interactable instance of a <seealso cref="RainItem"/>
    /// </summary>
    public class DroppedRainItem
    {
        const int MAXRAINITEMS = 32;
        public static DroppedRainItem[] AllDroppedRainItems = new DroppedRainItem[MAXRAINITEMS];

        public RainItem Data;

        public Vector3 Position;
        public Vector3 Velocity;

        public float Gravity = 8f;
        public bool Grounded;
        public bool Collected = false;
        public BoundingSphere BoundingSphere;
        public int Id;

        #region Visuals

        public List<Particle> vfx_item = new();
        public Color ColorHue;
        #endregion

        public DroppedRainItem(int type, Vector3 position, Vector3? velocity)
        {

            Data = CA_Main.GetRainItem(type);
            Position = position;
            Velocity = velocity ?? new(0, 5f, 0);
            int index = Array.IndexOf(AllDroppedRainItems, null);

            Id = index;

            AllDroppedRainItems[index] = this;
            BoundingSphere = new() { Radius = 14, Center = Position };
            SoundPlayer.PlaySoundInstance(CA_Main.ItemSpawn_Tier2!, SoundContext.Effect, 1.1f);

            ColorHue = Data.ItemColor;
        }

        public void Update()
        {
            float delta = RuntimeData.DeltaTime / 60f;
            bool WasAirborne = !Grounded;

            if (!Grounded)
            {
                var glow1 = GameHandler.Particles.MakeParticle(Position, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/light_particle"));
                glow1.Scale = Vector3.One;

                glow1.HasAdditiveBlending = true;
                glow1.Color = ColorHue;
                glow1.FaceTowardsMe = true;

                glow1.UniqueBehavior = (a) =>
                {
                    glow1.Alpha -= 0.75f * RuntimeData.DeltaTime / 60f;

                    glow1.Scale = Vector3.One * glow1.Alpha;
                    if (glow1.Alpha <= 0)
                        glow1.Destroy();
                };

                var glow2 = GameHandler.Particles.MakeParticle(Position, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/light_particle"));
                glow2.Scale = Vector3.One * 0.2f;
                glow2.FaceTowardsMe = true;
                glow2.Alpha = 0.8f;

                glow2.HasAdditiveBlending = true;
                glow2.Color = Color.White;

                glow2.UniqueBehavior = (a) =>
                {
                    glow2.Alpha -= 0.75f * RuntimeData.DeltaTime / 60f / 0.8f;

                    glow2.Scale = Vector3.One * glow1.Alpha * 0.2f;
                    if (glow2.Alpha <= 0)
                        glow2.Destroy();
                };




            }
            else if (vfx_item.Count > 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    float a = (MathF.PI / 6 * i * 2f) + (vfx_item[i].LifeTime / 60f * MathHelper.PiOver2);
                    float h = -5f;
                    Vector3 path = (Vector2.UnitY.RotatedBy(a) * 10).ExpandZ() + Vector3.UnitY * h;
                    vfx_item[i].Position = path + Position;
                    vfx_item[i].Yaw = -a;
                }
                vfx_item[6].Position = Position + Vector3.UnitY * 4f;
            }


            Grounded = Position.Y <= BoundingSphere.Radius * 0.9f;
            if (Grounded && WasAirborne)
            {
                var glow1 = GameHandler.Particles.MakeParticle(Position, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/bot_hit"));
                glow1.Scale = Vector3.One/2f;
                glow1.Pitch = MathHelper.PiOver4;
                glow1.HasAdditiveBlending = true;
                glow1.Color = ColorHue;
                glow1.FaceTowardsMe = false;

                glow1.UniqueBehavior = (a) =>
                {
                    glow1.Alpha -= 0.75f * RuntimeData.DeltaTime / 4f;

                    glow1.Scale += Vector3.One * RuntimeData.DeltaTime / 30f;
                    if (glow1.Alpha <= 0)
                        glow1.Destroy();
                };

                var glow2 = GameHandler.Particles.MakeParticle(Position, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/bot_hit"));
                glow2.Scale = Vector3.One/2f;
                glow2.Pitch = -MathHelper.PiOver4;
                glow2.HasAdditiveBlending = true;
                glow2.Color = ColorHue;
                glow2.FaceTowardsMe = false;

                glow2.UniqueBehavior = (a) =>
                {
                    glow2.Alpha -= 0.75f * RuntimeData.DeltaTime / 4f;

                    glow2.Scale += Vector3.One * RuntimeData.DeltaTime / 30f;
                    if (glow2.Alpha <= 0)
                        glow2.Destroy();
                };



                for (int i = 0; i < 6; i++)
                {
                    float a = MathF.PI / 6 * i;
                    float h = -5f;
                    Vector3 path = (Vector2.UnitY.RotatedBy(a) * 10f).ExpandZ() + Vector3.UnitY * h;
                    var p = GameHandler.Particles.MakeParticle(path, TextureGlobals.Pixels[ColorHue]);
                    p.Scale = new(2.25f);
                    p.Yaw = MathHelper.PiOver4 + a;
                    p.Roll = MathHelper.PiOver4;
                    p.Pitch = MathHelper.PiOver4 * 1.5f;
                    p.HasAdditiveBlending = false;
                    p.Alpha = 0.5f;
                    p.UniqueBehavior = (a) =>
                    {

                        if (p.Alpha <= 0f)
                            p.Destroy();
                    };
                    vfx_item.Add(p);
                }
                var icon = GameHandler.Particles.MakeParticle(Position + Vector3.UnitY * 4f, GameResources.GetGameResource<Texture2D>("Assets/textures/ui/tnk_ui_lightborder"));
                icon.Scale = new(0.9f);
                 icon.FaceTowardsMe = true;
                icon.HasAdditiveBlending = false;
                icon.Color = ColorHue;
                icon.Alpha = 0.8f;
                icon.UniqueBehavior = (a) =>
                {

                    if (icon.Alpha <= 0f)
                        icon.Destroy();
                };
                vfx_item.Add(icon);
                SoundPlayer.PlaySoundInstance(CA_Main.ItemLand_Tier2!, SoundContext.Effect, 1.1f);

            }

            if (Grounded)
            {
                Position = Position * new Vector3(1f, 0f, 1f) + (Vector3.UnitY * BoundingSphere.Radius * 0.9f);
                Velocity = Vector3.Zero; 
                foreach (var item in GameHandler.AllPlayerTanks)
            {
                if(item is Tank player && !player.IsDestroyed)
                {
                    if (player.Hurtbox.Intersects(BoundingSphere) && !Collected)
                    {
                        Collected = true;
                        CA_Main.AddItem(player,Data);
                        SoundPlayer.PlaySoundInstance(CA_Main.Pickup[Client.ClientRandom.Next(0, CA_Main.Pickup.Length)]!, SoundContext.Effect, 0.8f);
                        Remove();
                        return;
                    }
                }

                }

                foreach (var item in GameHandler.AllAITanks) //HAH
                {
                    if (item is Tank player && !player.IsDestroyed)
                    {
                        if (player.Hurtbox.Intersects(BoundingSphere) && !Collected)
                        {
                            Collected = true;
                            CA_Main.AddItem(player, Data);
                            SoundPlayer.PlaySoundInstance(CA_Main.Pickup[Client.ClientRandom.Next(0, CA_Main.Pickup.Length)]!, SoundContext.Effect, 0.8f);
                            Remove();
                            return;
                        }
                    }

                }
            }
            else
            {
                Velocity -= Vector3.UnitY * Gravity * delta;
                Position += Velocity * RuntimeData.DeltaTime ;
            }


            
           
        }

        public void Remove()
        {
            foreach (var item in vfx_item)
            {
                item.Alpha = 0f;
                item?.Destroy();
            }
            AllDroppedRainItems[Id] = null;
        }

    }


    }
   
