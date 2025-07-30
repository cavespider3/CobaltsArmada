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
using MeltySynth;
using System.Reflection;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.GameContent.Systems.AI;

namespace CobaltsArmada
{
    /// <summary>
    /// Cause we need one
    /// </summary>
    public class VindicationTimer
    {
        public float TimeLeft;
        
        public float BossHPMax;
        public string TimerText="";
        
        public AITank? Owner;

        private float Anim_Rising;
        private float Anim_Up;
        public VindicationTimer(AITank owner,float StartTime=140)
        {
            Owner = owner;
            TimeLeft = StartTime*60f;
        }

        public void Update()
        {
            if (Owner is null) return;
            if (Anim_Up == 1f)
                TimeLeft -= RuntimeData.DeltaTime;
            TimeLeft = MathF.Max(0f, TimeLeft);
            float Realtime = TimeLeft / 60f;
            TimerText = string.Format("{0:00}",Math.Floor(Realtime / 60f))+":"+ string.Format("{0:00}", Math.Floor(Realtime) % 60);
            if (Realtime <= 0f)
            {
                    ref Tank[] tanks = ref GameHandler.AllTanks;
                    for (int i = 0; i < tanks.Length; i++)
                    {

                        if (tanks[i] is null || tanks[i] is not PlayerTank) continue;


                        var plyr = tanks[i] as PlayerTank;
                        if (plyr is null || plyr.IsDestroyed) continue;
                        new Shell(plyr.Position, Vector2.Zero,0, Owner);
                    }
               
            }

        }
        public static float Invlerp(float a,float b, float v)
        {
            return Math.Clamp((v - a) / (b - a),0,1);
        }
        public void Render(SpriteBatch sb, Vector2 position, Vector2 scale, Anchor aligning, Color emptyColor, Color fillColor)
        {
            float Realtime = TimeLeft / 60f;

            if (CampaignGlobals.InMission)
            {
                Anim_Rising += RuntimeData.DeltaTime / 60f;
                Anim_Rising = MathHelper.Clamp(Anim_Rising, 0f, 1f);
            }
            Anim_Up += (CampaignGlobals.InMission && Owner is not null && !Owner.IsDestroyed ? 0.025f:-0.025f) * RuntimeData.DeltaTime;
            Anim_Up = MathHelper.Clamp(Anim_Up, 0f, 1f);
            Vector2 finalpos = position + Vector2.UnitY * 120f*Easings.InBack(1f-Anim_Up);
            
            TankGame.SpriteRenderer.DrawString(FontGlobals.RebirthFont, TimerText, finalpos + (Vector2.UnitY * 20).ToResolution(), Color.Lerp(Color.White, Color.Red,Invlerp(0.7f,1,MathF.Abs(MathF.Floor(Realtime % 1f) - (Realtime % 1f)) )), new Vector2(2f+0.5f* Invlerp(0.7f, 1, MathF.Abs(MathF.Floor(Realtime % 1f) - (Realtime % 1f))) ).ToResolution(), 0f, FontGlobals.RebirthFont.MeasureString(TimerText) / 2f, 0f);
           

            //sb.Draw(TextureGlobals.Pixels[Color.White], finalpos, null, emptyColor, 0f, GameUtils.GetAnchor(aligning, TextureGlobals.Pixels[Color.White].Size()), new Vector2(scale.X, scale.Y), default, 0f);

            //sb.Draw(TextureGlobals.Pixels[Color.White], finalpos - new Vector2((scale.X / 2f) - (scale.X / 2f) * (Hp_Ticked / BossHPMax * Easings.InExpo(Anim_Rising)) ,0), null, Color.White, 0f, GameUtils.GetAnchor(aligning, TextureGlobals.Pixels[Color.White].Size()), new Vector2(scale.X * (Hp_Ticked / BossHPMax * Easings.InExpo(Anim_Rising)), scale.Y), default, 0f);

            //sb.Draw(TextureGlobals.Pixels[Color.White], finalpos - new Vector2((scale.X / 2f) - (scale.X / 2f) * (BossHP / BossHPMax * Easings.InExpo(Anim_Rising)), 0), null, fillColor, 0f, GameUtils.GetAnchor(aligning, TextureGlobals.Pixels[Color.White].Size()), new Vector2(scale.X * (BossHP / BossHPMax * Easings.InExpo(Anim_Rising)), scale.Y), default, 0f);
        }
    }
}