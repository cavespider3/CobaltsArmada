
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Numerics;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Globals.Assets;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.GameContent.Systems.TankSystem.AI;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using static TanksRebirth.GameContent.Systems.TankSystem.Tank;

namespace CobaltsArmada.Script.Objects.items
{
    /// <summary>
    /// Copies the properties of ANY tank you kill for 5 (+2 per stack) seconds.
    /// </summary>
    public class RainItem_TankMimic : RainItem
    {
        public float MimicTimer = 0f;
        public bool IsMimic = false;
      
        public override void OnTankUpdate(ref Tank tank)
        {
            MimicTimer -= RuntimeData.DeltaTime;
            if (MimicTimer < 0f && IsMimic)
            {
                tank.Properties = new TankProperties();
                tank.ApplyDefaults(ref tank.Properties);
                IsMimic = false;
                //Return to normal
                    if (tank is AITank ai)
                    {
                        var tierName = TankID.Collection.GetKey(ai.AiTankType)!.ToLower();
                        ai.DrawParamsTank.Model = ModelGlobals.TankEnemy.Asset;
                        var tnkAsset = Tank.Assets[$"tank_" + tierName];
                        ai.DrawParamsTank.TankTexture = tnkAsset!.Duplicate(TankGame.Instance.GraphicsDevice);

                        tank.DrawParams.UsePhong = false;
                        tank.DrawParams.LightPower = 1f;
                        tank.DrawParams.AmbientPower = TankDrawParams.AI_AMB_MUL;
                    }
                    else if(tank is PlayerTank player)
                    {
                        tank.DrawParamsTank.Model = ModelGlobals.TankPlayer.Asset;

                        Texture2D texAsset;

                        texAsset = Tank.Assets[$"plrtank_" + PlayerID.Collection.GetKey(player.PlayerType)!.ToLower()]!;

                        tank.DrawParamsTank.TankTexture = texAsset!.Duplicate(TankGame.Instance.GraphicsDevice);
                        tank.DrawParamsTank.ShadowTexture = GameResources.GetGameResource<Texture2D>("Assets/textures/tank_shadow");
                        tank.DrawParams.UsePhong = true;
                        tank.DrawParams.LightPower = 1f;
                        tank.DrawParams.AmbientPower = TankDrawParams.PLR_AMB_MUL;
                    }
                    tank.InitModelSemantics();
                tank.DoInvisibilityGFXandSFX();
            }

        }
        public override void OnStart(ref Tank tank)
        {
            TankGame.IngameConsole.Log("Activating the power of " + Name.GetLocalizedString(LangCode.English), ItemColor);
            if (tank is AITank) tank.Team = TeamID.NoTeam;
        }

        public override void OnTankDestroy(ref Tank owner, ref Tank victim) {
            //copy anything you kill, this includes your own team...
            if (owner == victim) return; //EXCEPT YOURSELF.
            if (victim is AITank ai2 && ai2.AiTankType >= CA_Main.Dandelion) return;
                //for ai, the parameters themselves are unaffected
            
            MimicTimer = 60f * (5f + Stacks * 2f);
            IsMimic = true;
            if (victim is AITank ai)
            {
                ai.ApplyDefaults(ref owner.Properties);
                if (owner is AITank ai_owner)
                    ai_owner.Parameters = AIManager.GetAIParameters(TankID.Violet);

                owner.DrawParamsTank.Model = victim.DrawParamsTank.Model;
            }
            else if (victim is PlayerTank player)
            {
                player.ApplyDefaults(ref owner.Properties);
                if(owner is AITank ai_owner)
                    ai_owner.Parameters = AIManager.GetAIParameters(TankID.Violet);
                
                owner.DrawParamsTank.Model = ModelGlobals.TankPlayer.Asset;     
            }
            owner.DrawParamsTank.TankTexture = victim.DrawParamsTank.TankTexture;
            owner.DrawParamsTank.ShadowAlpha = victim.DrawParamsTank.ShadowAlpha;
            owner.DrawParamsTank.ShadowTexture = victim.DrawParamsTank.ShadowTexture;
            owner.DrawParams = victim.DrawParams;
            owner.InitModelSemantics();
            owner.DoInvisibilityGFXandSFX();

        }
        public override Rarity Tier => Rarity.Blue;
        public override Color ItemColor => Color.Blue;

        public override string InternalName => "WakeOfVultures";
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Sabotaged Blueprints"
        });

    }

    
}
