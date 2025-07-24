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
using CobaltsArmada.Script.UI;

namespace CobaltsArmada
{
    /// <summary>
    /// Cause we need one
    /// </summary>
    public class Modifieralert : CA_Popup
    {  
        private string Name;
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
            ["MasterModBuff"] = "Master Mod",
           // ["MarbleModBuff"] = "Marble Mod Buff", //RIP
            ["CobaltArmada_Swap"] = "Armada Mod",
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


        public Modifieralert(string text,Color color,float offset) : base(color,offset:offset)
        {
            BannerColor = TranslationTable.ContainsKey(text) ? TranslationTable[text] == "$Difficulty$" ? CA_Main.DifficultyColor(CA_Main.modifier_Difficulty) * 1.2f : color * 1.5f : text == "$START$" ? new Color(0.3f, 0.3f, 0.3f) * 1.5f : color * 1.5f;
            if (text == "CobaltArmada_TanksOnCrack") BannerColor = Color.Violet;
            Name = TranslationTable.ContainsKey(text) ? TranslationTable[text] == "$Difficulty$" ? CA_Main.modifier_Difficulty.ToString() : TranslationTable[text] :
                text=="$START$" ? "Active Modifiers:"
  
                : "???";
            OnBeginExit += Modifieralert_OnBeginExit;
        }

        private void Modifieralert_OnBeginExit()
        {
            if (Name != "Active Modifiers:")
            {
                PopupAnimation.Seek(4);
            }
        }
        public override void Remove()
        {
            OnBeginExit -= Modifieralert_OnBeginExit;
            base.Remove();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            DrawUtils.DrawStringWithShadow(spriteBatch, FontGlobals.RebirthFont, (barPos+Vector2.UnitX*3f).ToResolution(), Vector2.UnitY, Name, Color.White, Vector2.One.ToResolution() / scale_shrink, 1f,Anchor.LeftCenter, shadowDistScale: 0.5f);
        }
    }
}