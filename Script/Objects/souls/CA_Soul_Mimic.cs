using Microsoft.Xna.Framework;

using TanksRebirth.GameContent.Tanks;
using TanksRebirth.GameContent.Tanks.AI;

namespace CobaltsArmada
{
    public class CA_Soul_Mimic : CA_Soul
    {
        public int FirstHostIndentity;
        public Tank Owner;
        public bool PlayerOwner;
        public CA_Soul_Mimic(Tank? target, Tank owner) : base(target, owner)
        {
            PlayerOwner = owner is PlayerTank;
            FirstHostIndentity = owner is AITank ai ? ai.AiTankType : (owner as PlayerTank)!.PlayerType;
            Owner = owner;
        }

        public override void OnHaunt(ref Tank tank)
        {
           if(tank is AITank ai)
           {
                ai.AiTankType = FirstHostIndentity;
                ai.Parameters = AIManager.GetAIParameters(FirstHostIndentity);
                ai.Properties = AIManager.GetAITankProperties(FirstHostIndentity);
                ai.DrawParamsTank = Owner.DrawParamsTank;
                ai.DrawParams = Owner.DrawParams;
            }

        }
    }
}
