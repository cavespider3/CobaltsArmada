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
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.GameUI;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using static TanksRebirth.GameContent.RebirthUtils.DebugManager;
using TanksRebirth.GameContent.Globals;

namespace CobaltsArmada
{
    /// <summary>
    /// Cause we need one
    /// </summary>
    public class Modifieralert
    {

        private Texture2D banner;
        private string Name;
        private Color banner_color;
        private float Anim_Rising;
        private float Anim_Up;
        private float _offset;
        const float Anim_Slide = 0.5f*60f;
        const float Anim_Life = 3f*60f;

        public static Modifieralert[] AllModifiers = new Modifieralert[80];
        public int Id { get; private set; }
        public static Dictionary<string, string> TranslationTable = new()
        {
            ["TanksAreCalculators"] = "Tanks Are Calculators",
            ["PieFactory"] = "Lemon Pie Factory",
            ["UltraMines"] = "Ultra Mines",
            ["BulletHell"] = "東方 Mode",
            ["AllInvisible"] = "All Invisible",
            ["AllStationary"] = "All Stationary",
            ["AllHoming"] = "Seekers",
            ["Armored"] = "Armored",
            ["BumpUp"] = "BumpUp",
            ["Monochrome"] = "Monochrome",
            ["InfiniteLives"] = "Infinite Lives",
            ["MasterModBuff"] = "Master Mod Buff",
            ["MarbleModBuff"] = "Marble Mod Buff",
            ["CobaltArmada_Swap"] = "Armada Mod Buff",
            ["CobaltArmada_GetGud"] = "$Difficulty$",
            ["MachineGuns"] = "Machine Guns",
            ["RandomizedTanks"] = "Randomized Tanks",
            ["ThunderMode"] = "Thunder Mode",
            ["POV"] = "POV Mode",
            ["AiCompanion"] = "AI Companion",
            ["Shotguns"] = "Shotguns",
            ["Predictions"] = "Predictions",
            ["RandomPlayer"] = "Randomized Player",
            ["BulletBlocking"] = "Bullet Blocking",
            ["FFA"] = "Free-for-all",
            ["LanternMode"] = "Lantern",
            ["Disguise"] = "Disguise",
            ["CobaltArmada_TanksOnCrack"] = "NightShaded",
            ["CobaltArmada_Mitosis"] = "Double Trouble",
            ["CobaltArmada_P2"] = "Idol Support"
        };


        public Modifieralert(string text,Color color,float offset)
        {
            banner = GameResources.GetGameResource<Texture2D>("Assets/textures/ui/mission_info");
            banner_color = TranslationTable.ContainsKey(text) ? TranslationTable[text] == "$Difficulty$" ? CA_Main.DifficultyColor(CA_Main.modifier_Difficulty) * 1.2f : color * 1.5f : text == "$START$" ? new Color(0.3f, 0.3f, 0.3f) * 1.5f : color * 1.5f;
            if (text == "CobaltArmada_TanksOnCrack") banner_color = Color.Violet;
            Name = TranslationTable.ContainsKey(text) ? TranslationTable[text] == "$Difficulty$" ? CA_Main.modifier_Difficulty.ToString() : TranslationTable[text] :
                text=="$START$" ? "Active Modifiers:"
  
                : "???";
   

                _offset = offset * 60f;
            int index = Array.IndexOf(AllModifiers, null);
            Id = index;
            AllModifiers[index] = this;
        }

        public void Render(SpriteBatch sb, int interval, Vector2 scale, Anchor aligning)
        {
            Anim_Up += RuntimeData.DeltaTime * (( (Anim_Rising - _offset) >(Anim_Life - Anim_Slide - _offset)) ? -1 : 1f)/60f * (Anim_Rising - _offset > 0 ? 1 : 0f);
            Anim_Rising += RuntimeData.DeltaTime;
            Anim_Up = Math.Clamp(Anim_Up, 0f, 1f);
            float scale_shrink = 1.3f;
            float Anim_Up2 = Math.Clamp(Anim_Up / (Anim_Slide / 60f), 0, 1f);
            var barPos = new Vector2(WindowUtils.WindowWidth-(banner.Width*1.2f/ scale_shrink * Easings.OutCubic(Anim_Up2))+ banner.Width/ scale_shrink, (banner.Height/ scale_shrink + 4)*interval+ banner.Height/ scale_shrink);
            DrawUtils.DrawTextureWithShadow(sb,banner, barPos.ToResolution(),
                Vector2.UnitY, banner_color , Vector2.One.ToResolution()/ scale_shrink, 1f,Anchor.Center, shadowDistScale: 0.5f);
            
            DrawUtils.DrawStringWithShadow(sb,FontGlobals.RebirthFont, (barPos+Vector2.UnitX*3f).ToResolution(), Vector2.UnitY, Name, Color.White, Vector2.One.ToResolution() / scale_shrink, 1f,Anchor.Center, shadowDistScale: 0.5f);
            if (Anim_Rising - _offset > Anim_Life && Anim_Up == 0) Remove();
        }

        public void Remove()
        {

            AllModifiers[Id] = null;
        }
    }
}