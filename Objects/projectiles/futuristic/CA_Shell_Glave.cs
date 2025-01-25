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

namespace CobaltsArmada
{
    public class CA_Shell_Glave : ModShell
    {

        public override string Texture => "assets/textures/tank_daisy";
        public override string ShootSound => "assets/sfx/touhou_shot.ogg";
        public override string TrailSound => base.TrailSound;

        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Razor Shell"
        });
        public override void OnCreate(Shell shell)
        {
            base.OnCreate(shell);
            shell.Model = CA_Main.Shell_Glave;
            shell.LifeTime = 0f;
            if (shell.Owner is null) return;
            shell.Properties.EmitsSmoke = false;
            shell.Properties.IsDestructible = false;
            
        }
        public override void PostUpdate(Shell shell)
        {
           shell.World = Matrix.CreateScale(2.4f) * Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2 * shell.LifeTime / 15f * (shell.RicochetsRemaining % 2 == 1 ? 1f : -1f) * shell.Velocity.Length()/1.2f, 0, 0) * Matrix.CreateTranslation(shell.Position3D);

            base.PostUpdate(shell);

            //where the fun begins
            if(Math.Floor(shell.LifeTime) % 60f % 10f == 0)
             RenderLeaveTrail(shell);

        }

        private void RenderLeaveTrail(Shell shell)
        {
            // _oldPosition and Position are *not* the same during method call.
            // TODO: make more particles added depending on the positions between 2 distinct frames
            //var numToAdd

            var p = GameHandler.Particles.MakeParticle(
               shell.Position3D, shell.Model, TankGame.WhitePixel);
            p.Yaw = shell.Rotation;
            p.Color = shell.Properties.FlameColor;
            p.HasAddativeBlending = true;
            p.Scale = new(2.4f);
            p.UniqueBehavior = (a) => {
                p.Alpha -= 0.02f * TankGame.DeltaTime;
                if (p.Alpha <= 0f)
                    p.Destroy();
            };
        }
        public override void OnRicochet(Shell shell, Block block)
        {
            if(CA_Main.modifier_Difficulty >= CA_Main.ModDifficulty.Lunatic)
            {
                shell.Velocity *=
                    CA_Main.modifier_Difficulty == CA_Main.ModDifficulty.Lunatic ?
                    CA_Main.modifier_Difficulty == CA_Main.ModDifficulty.Extra ?
                    CA_Main.modifier_Difficulty == CA_Main.ModDifficulty.Phantasm ? 1.175f : 1.10f : 1.05f :1f ;

            }
        }

        public override void PostRender(Shell shell)
        {
            void RenderMeshEffects(int i, ModelMesh mesh)
            {
                for (var j = 0; j < mesh.Effects.Count; j++)
                {
                    var effect = (BasicEffect)mesh.Effects[j];
                    effect.World =
                        shell.World;
                      

                    effect.View = shell.View;
                    effect.Projection = shell.Projection;
                    effect.TextureEnabled = true;

                    effect.SetDefaultGameLighting_IngameEntities();
                    effect.Alpha = 1f;
                }
            }

            for (var i = 0; i < shell.Model.Meshes.Count; i++)
            {
                var mesh = shell.Model.Meshes[i];
                RenderMeshEffects(0, mesh);
                mesh.Draw();
            }

        }

    }
}
