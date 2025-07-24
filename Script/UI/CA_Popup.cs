using CobaltsArmada.Script.Tanks.Class_T;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.Graphics.Shaders;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.Framework.Animation;
using TanksRebirth.Internals.Common.Utilities;
using static TanksRebirth.GameContent.RebirthUtils.DebugManager;

namespace CobaltsArmada.Script.UI
{
    public class CA_Popup
    {
        public Color BannerColor;
        public Texture2D BannerTexture;
        public GradientEffect? Gradient; //priorities over normal color

        public static CA_Popup[] AllPopups = new CA_Popup[50];

        public Animator PopupAnimation;

        public float WaitTime = 2f;
        public float StartDelay = 0f;
        public int Id { get; private set; }

        public delegate void BeginExit();
        public static event BeginExit? OnBeginExit;

        protected float scale_shrink = 1.3f;
        protected Vector2 barPos = Vector2.Zero;

        public CA_Popup(Color color, GradientEffect gradientEffect = null, float offset = 0)
        {
            
            BannerTexture = GameResources.GetGameResource<Texture2D>("Assets/textures/ui/mission_info");
            BannerColor = color;
            Gradient = gradientEffect;
            StartDelay = offset;
            

            int index = Array.IndexOf(AllPopups, null);
            Id = index;
            AllPopups[index] = this;
            Load();
        }

        //It's just like Project Arrhythmia :D
        //only reference the X
        public void Load()
        {

            PopupAnimation = Animator.Create()
                // id = 0
                .WithFrame(new(position2d: Vector2.Zero, duration: TimeSpan.FromSeconds(0), easing: EasingFunction.Linear)) //delay
                .WithFrame(new(position2d: Vector2.Zero, duration: TimeSpan.FromSeconds(StartDelay), easing: EasingFunction.OutExpo)) //delay
                .WithFrame(new(position2d: Vector2.UnitX * WindowUtils.WindowWidth * -0.22f, duration: TimeSpan.FromSeconds(0.05), easing: EasingFunction.OutExpo)) //delay
                .WithFrame(new(position2d: Vector2.UnitX * WindowUtils.WindowWidth * -0.22f, duration: TimeSpan.FromSeconds(WaitTime), easing: EasingFunction.OutExpo)) //move it in

                .WithFrame(new(position2d: Vector2.UnitX * WindowUtils.WindowWidth * 0.2f, duration: TimeSpan.FromSeconds(0.4), easing: EasingFunction.InBack))
                .WithFrame(new());
            ;
            PopupAnimation.OnKeyFrameFinish += PopupAnimation_OnKeyFrameFinish;
         
            PopupAnimation.Run();
       
        }

 
        public void Unload()
        {
            PopupAnimation.OnKeyFrameFinish -= PopupAnimation_OnKeyFrameFinish;         
        }
      
        private void PopupAnimation_OnKeyFrameFinish(KeyFrame frame)
        {
            var frameId = PopupAnimation.KeyFrames.FindIndex(f => f.Equals(frame));
            if (frameId == 3 && Id==0 && this is Modifieralert)
            {
                OnBeginExit?.Invoke(); //sync with other popups
            }
            if(frameId == 5)
            {
                Remove();
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            
            barPos = new Vector2(WindowUtils.WindowWidth - PopupAnimation.CurrentPosition2D.X, (BannerTexture.Height / scale_shrink + 4) * Id  + BannerTexture.Height / scale_shrink);
            DrawUtils.DrawTextureWithShadow(spriteBatch, BannerTexture, barPos.ToResolution(),
               Vector2.UnitY, BannerColor, Vector2.One.ToResolution() / scale_shrink, 1f, Anchor.LeftCenter, shadowDistScale: 0.5f);
            if (Gradient is not null)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, rasterizerState: RenderGlobals.DefaultRasterizer, effect: Gradient);
                DrawUtils.DrawTextureWithShadow(spriteBatch, BannerTexture, barPos.ToResolution(),
                Vector2.UnitY, BannerColor, Vector2.One.ToResolution() / scale_shrink, 1f, Anchor.LeftCenter, shadowDistScale: 0.0f,shadowAlpha:0f);

            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, rasterizerState: RenderGlobals.DefaultRasterizer);
        }
        public virtual void Remove()
        {
            Unload();
            AllPopups[Id] = null;
        }


    }
}
