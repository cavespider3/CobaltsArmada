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


namespace CobaltsArmada
{
    public class CA_PaintMacros
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
              
                float h(byte v,float off = 0f)
                {
                  return MathHelper.Clamp(MathF.Asin(MathF.Sin((v / 255f + off + hue) * MathF.PI / 3f)) * 3f / MathF.PI,0f,1f);
                }
                colors[i] = new Color(h(r)+ h(g,2f)+ h(b, 4f),
                    h(g) + h(b, 2f) + h(r, 4f),
                    h(b) + h(r, 2f) + h(b, 4f));
            }
            t.SetData(colors);
            return t;
        }

    }
}
