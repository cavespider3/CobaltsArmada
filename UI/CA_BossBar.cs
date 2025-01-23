using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth.Internals.Common.Utilities;
using FontStashSharp;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Properties;

namespace CobaltsArmada
{
    /// <summary>
    /// Cause we need one
    /// </summary>
    public class BossBar
    {
        public float BossHP;
        private float Old_BossHP;
        public float Hp_Ticked;
        public float Hp_Ticked_Ouched;
        public float BossHPMax;
        public string Name;
        public string Subtitle;
        public AITank? Owner;

        private float Anim_Rising;
        private float Anim_Up;
        public BossBar(AITank owner, string name, string subtitle)
        {
            Owner = owner;
            if (owner.Properties.Armor is not null)
            {
               
                BossHPMax = MathF.Ceiling(owner.SpecialBehaviors[2].Value);
                BossHP = BossHPMax;
                
            }
            Name = name;
            Subtitle = subtitle;
        }

        public void Update()
        {
            if (Owner is null) return;
            BossHP = Owner.SpecialBehaviors[2].Value;
        }
        public void Render(SpriteBatch sb, Vector2 position, Vector2 scale, Anchor aligning, Color emptyColor, Color fillColor)
        {
            
            if (Old_BossHP>BossHP)
            {
                Hp_Ticked_Ouched = 30f;

            }
            
            Hp_Ticked_Ouched -= TankGame.DeltaTime;
            if(Hp_Ticked_Ouched<=0 && Hp_Ticked> BossHP)
            {
                Hp_Ticked -= 0.01f* BossHPMax * TankGame.DeltaTime;
            }else if (Hp_Ticked < BossHP)
            {
                Hp_Ticked = BossHP;
            }
            if (GameProperties.InMission)
            {
                Anim_Rising += TankGame.DeltaTime / 60f;
                Anim_Rising = MathHelper.Clamp(Anim_Rising, 0f, 1f);
            }
            Anim_Up += (GameProperties.InMission ? 0.025f:-0.025f) * TankGame.DeltaTime;
            Anim_Up = MathHelper.Clamp(Anim_Up, 0f, 1f);
            Vector2 finalpos = position + Vector2.UnitY * 120f*Easings.InBack(1f-Anim_Up);

            string _Title = Name + (Subtitle.Length == 0 ? "" : " - " + Subtitle);
           TankGame.SpriteRenderer.DrawString(TankGame.TextFont, _Title, finalpos - (Vector2.UnitY * 20).ToResolution(), Color.White, new Vector2(0.6f).ToResolution(), 0f, TankGame.TextFont.MeasureString(_Title)/2f,0f);
           

            sb.Draw(TankGame.WhitePixel, finalpos, null, emptyColor, 0f, GameUtils.GetAnchor(aligning, TankGame.WhitePixel.Size()), new Vector2(scale.X, scale.Y), default, 0f);

            sb.Draw(TankGame.WhitePixel, finalpos - new Vector2((scale.X / 2f) - (scale.X / 2f) * (Hp_Ticked / BossHPMax * Easings.InExpo(Anim_Rising)) ,0), null, Color.White, 0f, GameUtils.GetAnchor(aligning, TankGame.WhitePixel.Size()), new Vector2(scale.X * (Hp_Ticked / BossHPMax * Easings.InExpo(Anim_Rising)), scale.Y), default, 0f);

            sb.Draw(TankGame.WhitePixel, finalpos - new Vector2((scale.X / 2f) - (scale.X / 2f) * (BossHP / BossHPMax * Easings.InExpo(Anim_Rising)), 0), null, fillColor, 0f, GameUtils.GetAnchor(aligning, TankGame.WhitePixel.Size()), new Vector2(scale.X * (BossHP / BossHPMax * Easings.InExpo(Anim_Rising)), scale.Y), default, 0f);
            Old_BossHP = BossHP;
        }
    }
}