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
using tainicom.Aether.Physics2D.Dynamics;
using TanksRebirth.Graphics;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.Internals.Common.Framework.Audio;

namespace CobaltsArmada
{
    public class CA_Shell_Glaive : ModShell
    {

        public override string Texture => "assets/textures/tank_daisy";
        public override string ShootSound => "assets/sfx/touhou_shot.ogg";
        public override string TrailSound => base.TrailSound;

        public override LocalizedString Name => new()
        {
            [LangCode.English] = "Razor Shell"
        };
        public override void OnCreate()
        {
            base.OnCreate();

            Shell.DrawParamsShell.Model = CA_Main.Shell_Glaive;
            Shell.LifeTime = 0f;
            if (Shell.Owner is null) return;
            Shell.Properties.Penetration = -1;
            
        }
        public override void PostUpdate()
        {
           Shell.DrawParams.World = Matrix.CreateScale(2.4f) * Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2 * Shell.LifeTime / 15f * (Shell.RicochetsRemaining % 2 == 1 ? 1f : -1f) * Shell.Velocity.Length()/1.2f, 0, 0) * Matrix.CreateTranslation(Shell.Position3D);

            base.PostUpdate();

            //where the fun begins
            if(Math.Floor(Shell.LifeTime) % 60f % 10f == 0)
             RenderLeaveTrail(Shell);

        }

        private void RenderLeaveTrail(Shell shell)
        {
            // _oldPosition and Position are *not* the same during method call.
            // TODO: make more particles added depending on the positions between 2 distinct frames
            //var numToAdd

            var p = GameHandler.Particles.MakeParticle(
               shell.Position3D, shell.DrawParamsShell.Model, TextureGlobals.Pixels[Color.White]);
            p.Yaw = shell.Rotation;
            p.Color = shell.Properties.FlameColor;
            p.HasAdditiveBlending = true;
            p.Scale = new(2.4f);
            p.UniqueBehavior = (a) => {
                p.Alpha -= 0.02f * RuntimeData.DeltaTime;
                if (p.Alpha <= 0f)
                    p.Destroy();
            };
        }
        public override void OnRicochet(Block?block)
        {
            if(CA_Main.modifier_Difficulty >= CA_Main.ModDifficulty.Lunatic)
            {
                Shell.Velocity *=
                    CA_Main.modifier_Difficulty == CA_Main.ModDifficulty.Lunatic ?
                    CA_Main.modifier_Difficulty == CA_Main.ModDifficulty.Extra ?
                    1.10f : 1.05f :1f ;

            }
        }

        public override void PostRender()
        {
            void RenderMeshEffects(int i, ModelMesh mesh)
            {
                for (var j = 0; j < mesh.Effects.Count; j++)
                {
                    var effect = (BasicEffect)mesh.Effects[j];
                    effect.World =
                        Shell.DrawParams.World;
                      

                    effect.View = Shell.DrawParams.View;
                    effect.Projection = Shell.DrawParams.Projection;
                    effect.TextureEnabled = true;

                    effect.SetDefaultGameLighting_IngameEntities();
                    effect.Alpha = 1f;
                }
            }

            for (var i = 0; i < Shell.DrawParamsShell.Model.Meshes.Count; i++)
            {
                var mesh = Shell.DrawParamsShell.Model.Meshes[i];
                RenderMeshEffects(0, mesh);
                mesh.Draw();
            }

        }

    }
}
