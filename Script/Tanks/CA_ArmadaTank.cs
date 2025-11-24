using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems;


namespace CobaltsArmada.Script.Tanks
{
    public abstract class CA_ArmadaTank : ModTank
    {
        
        public bool BossTank;
       
        public override void PostApplyDefaults()
        {
            AITank.UsesCustomModel = true;
            AITank.Model = AITank.Properties.Stationary ? AITank.AiTankType == CA_Main.Dandelion ? CA_Main.Neo_Remote! : CA_Main.Neo_Stationary! : BossTank ? CA_Main.Neo_Boss! : CA_Main.Neo_Mobile!;
            AITank.Properties.CanLayTread = false;
            base.PostApplyDefaults();
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            //AITank.World.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            //AITank.World = Matrix.CreateScale(100f * scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(translation);
            //AITank.Model!.Root.Transform = AITank.World;

        }
      
    }
}
