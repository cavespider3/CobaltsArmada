using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.UI.MainMenu;
using TanksRebirth.Net;
using static TanksRebirth.Net.Client;

namespace CobaltsArmada
{
    public class CA_NetPlay
    {
        #region ServerSidedVariables
        /// <summary>
        /// The packet id for nightshade
        /// </summary>
        public static int SyncNightShade;

        #endregion

        public static void Load()
        {
            SyncNightShade = PacketID.AddPacketId(nameof(SyncNightShade));
            NetPlay.OnReceiveClientPacket += NetPlay_OnReceiveClientPacket;
            NetPlay.OnReceiveServerPacket += NetPlay_OnReceiveServerPacket;
        }

        public static void Unload()
        {
            PacketID.Collection.TryRemove(SyncNightShade);
            NetPlay.OnReceiveClientPacket -= NetPlay_OnReceiveClientPacket;
            NetPlay.OnReceiveServerPacket -= NetPlay_OnReceiveServerPacket;
        }

        #region ReceivingPackets
        private static void NetPlay_OnReceiveClientPacket(int packet, NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (packet == SyncNightShade)
            {
                int tank = reader.GetInt();
                ChatSystem.SendMessage("Mods. Nightshade this tank in particular.", Color.Plum);
                CA_Main.PoisonedTanks.Add(GameHandler.AllTanks[tank]);
                CA_Main.Tank_OnPoisoned(GameHandler.AllTanks[tank]);
            }
        }

        private static void NetPlay_OnReceiveServerPacket(int packet, NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            // We create another packet since we are creating another sequence of data that is identically created and sequenced back to the client(s).
            NetDataWriter message = new();
            // The first thing we put in our packet is the packet identifier, since the client(s) also needs to identify how the data should be processed.
            message.Put(packet);
            // Use a check to verify that the packet is the packet we want before we process data.
            if (packet == SyncNightShade)
            {
                message.Put(reader.GetInt());
                Server.NetManager.SendToAll(message, LiteNetLib.DeliveryMethod.ReliableOrdered, peer);
            }
        }
        #endregion
        //Sends a packet to apply nightshade to a tank
        public static void RequestNightshadeTank(Tank target)
        {
            // If the game client is not connected to a server, we don't bother running the future lines.
            if (!IsConnected() || MainMenuUI.Active)
                return;
            
            // Constructs a NetDataWriter, which will be our packet.
            NetDataWriter message = new();

            // Here we put the data that goes into our packet of data. We send the packet type first, and not by choice.
            // Tanks Rebirth automatically reads an integer as the first piece of data in the packet's data stream, which becomes the 'int packet' above.
            message.Put(SyncNightShade);
            //a -1 means no tank called to spawn a nightshade cloud
            message.Put(target.WorldId);
            // This is how our data is sent to the server. We end up handling the data again within the scope of the server.
            // Delivery methods will be covered after the next couple of code blocks.
            NetClient.Send(message, LiteNetLib.DeliveryMethod.ReliableOrdered);
        }



    }
}
