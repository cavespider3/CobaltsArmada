using CobaltsArmada.Script.Objects.hazards;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Internals;
using TanksRebirth.Localization;
using static CobaltsArmada.Script.Tanks.Class_T.DroneParameters;

namespace CobaltsArmada.Script.Objects.items
{
    /// <summary>
    /// Don't.
    /// </summary>
    public class RainItem_BlackHoleMine : RainItem
    {
        public override void OnStart(ref Tank tank)
        {
            TankGame.IngameConsole.Log("Activating the power of " + Name.GetLocalizedString(LangCode.English), ItemColor);
        }

        public override void OnMinePlaced(ref Tank tank, ref Mine mine)
        {
            mine.InactiveColor = new(55, 7, 102);
            mine.ActiveColor = new(136, 22, 247);
            mine.ExplosionRadius = 0.01f;
       
            for (int i = 0; i < 6; i++)
            {
                var glow2 = GameHandler.Particles.MakeParticle(mine.Position3D, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
                glow2.Scale = Vector3.One * 0.6f;
                glow2.Tag = mine;
                glow2.FaceTowardsMe = false;
                glow2.Alpha = 0f;
                glow2.Yaw = MathHelper.Pi / 6 * i;
                glow2.HasAdditiveBlending = true;
                glow2.Color = Color.Purple;

                glow2.UniqueBehavior = (a) =>
                {
                    a.Alpha += RuntimeData.DeltaTime / 60 * 4f;
                    a.Alpha = Math.Clamp(a.Alpha, 0f, 1f);
                    a.Yaw += RuntimeData.DeltaTime / 60 * MathF.PI;
                    if (a.Tag is null || a.Tag is Mine min && (min.Detonated || Mine.AllMines[min.Id] is null))
                    {
                        a.Destroy();
                    }
                };


            }

            var glow4 = GameHandler.Particles.MakeParticle(mine.Position3D, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
            glow4.Scale = new Vector3(1f,1f,1f) * 0.7f;
            glow4.Tag = mine;
            glow4.FaceTowardsMe = false;
            glow4.Alpha = 0f;
            glow4.Pitch = MathHelper.ToRadians(80f);
            glow4.Yaw = MathHelper.ToRadians(90f);
            glow4.HasAdditiveBlending = true;
            glow4.Color = Color.Orange;

            glow4.UniqueBehavior = (a) =>
            {
                a.Alpha += RuntimeData.DeltaTime / 60 * 4f;
                a.Alpha = Math.Clamp(a.Alpha, 0f, 1f);
                if (a.Tag is null || a.Tag is Mine min && (min.Detonated || Mine.AllMines[min.Id] is null))
                {
                    a.Destroy();
                }
            };
            var glow3 = GameHandler.Particles.MakeParticle(mine.Position3D, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
            glow3.Scale = new Vector3(1f, 1f, 1f) * 0.7f;
            glow3.Tag = mine;
            glow3.FaceTowardsMe = false;
            glow3.Alpha = 0f;
            glow3.Pitch = MathHelper.ToRadians(-80f);
            glow3.Yaw = MathHelper.ToRadians(90f);
            glow3.HasAdditiveBlending = true;
            glow3.Color = Color.Orange;

            glow3.UniqueBehavior = (a) =>
            {
                a.Alpha += RuntimeData.DeltaTime / 60 * 4f;
                a.Alpha = Math.Clamp(a.Alpha, 0f, 1f);
                if (a.Tag is null || a.Tag is Mine min && (min.Detonated || Mine.AllMines[min.Id] is null))
                {
                    a.Destroy();
                }
            };

        }

        //public override void OnTankUpdate(ref Tank tank)
        //{
        //    Tank tank2 = tank;
        //    foreach(var mine in Mine.AllMines.Where(x => x is not null && !x.Detonated && x.Owner is not null && x.Owner == tank2))
        //    {
                

        //    }

        //}

        public override void OnMineExplode(ref Mine mine)
        {
            new CA_Blackhole(mine.Position3D + Vector3.UnitY * 2, mine.Owner);
        }
        public override int Priority => 6;

        public override Rarity Tier => Rarity.Blue;

        public override Color ItemColor => Color.DarkMagenta;

        public override string InternalName => "No";
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Void Mines"
        });

    }

    
}
