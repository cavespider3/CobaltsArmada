using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth.GameContent;
using TanksRebirth;
using TanksRebirth.GameContent.ID;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Net;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Graphics;
using tainicom.Aether.Physics2D.Dynamics;


namespace CobaltsArmada
{
    public class CA_Utils
    {

        public static Texture2D HueShift(Texture2D texture, float hue)
        {
            return HueShift(texture, hue, (c) => true);
        }

        public static Texture2D HueShift(Texture2D texture, float hue, Func<Color, bool> condition)
        {
            var t = new Texture2D(TankGame.Instance.GraphicsDevice, texture.Width, texture.Height);
            var colors = new Color[texture.Height * texture.Width];
            texture.GetData(colors);

            for (int i = 0; i < colors.Length; i++)
            {
                if (!condition(colors[i])) continue;
                colors[i].Deconstruct(out byte r, out byte g, out byte b);

                float h(byte v, float off = 0f)
                {
                    return MathHelper.Clamp(MathF.Asin(MathF.Sin((v / 255f + off + hue) * MathF.PI / 3f)) * 3f / MathF.PI, 0f, 1f);
                }
                colors[i] = new Color(h(r) + h(g, 2f) + h(b, 4f),
                    h(g) + h(b, 2f) + h(r, 4f),
                    h(b) + h(r, 2f) + h(b, 4f));
            }
            t.SetData(colors);
            return t;
        }

        public static bool WithinBounds(Vector2 position, bool tankbounds = false)
        {
            return tankbounds ? position.X <= GameScene.TANKS_MAX_X && position.X >= GameScene.TANKS_MIN_X &&
                position.Y <= GameScene.TANKS_MAX_Y && position.Y >= GameScene.TANKS_MIN_Y :
                position.X <= GameScene.MAX_X && position.X >= GameScene.MIN_X &&
                position.Y <= GameScene.MAX_Z && position.Y >= GameScene.MIN_Z;
        }

        public static bool UnobstructedPosition(Vector2 here)
        {
            return !(Block.AllBlocks.Any(x => x is not null && x.Properties.IsCollidable && x.Hitbox.Contains(here / Tank.UNITS_PER_METER)) || WithinBounds(here, true));
        }


        public static bool UnobstructedRaycast(Vector2 start, Vector2 end, Func<Vector2, bool> interception)
        {
            int bits = (int)Math.Ceiling(start.Distance(end) / 8f);
            for(int i = 0; i < bits; i++) {
                Vector2 path = Vector2.Lerp(start, end, (float)i / bits);
                if (interception(path)) return false;
            }
            return true;
        }
    }
}
