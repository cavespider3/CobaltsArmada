using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth.GameContent.ModSupport;

namespace CobaltsArmada.Script.Objects.items
{
    /// <summary>
    /// "internal set;" pisses me off.
    /// </summary>
    public interface IModdedRainItemContent
    {
        TanksMod Mod { get; internal set; }
        int Type { get; internal set; }
    }
}
