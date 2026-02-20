using CobaltsArmada.Script.Tanks.Class_T;
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
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.GameContent.UI.MainMenu;
using TanksRebirth.Internals.Common.Utilities;
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
        public static int SyncEntityDrone;
        public static int SyncCrateSpawn;

        #endregion

        public static void Load()
        {
            SyncNightShade = PacketID.AddPacketId(nameof(SyncNightShade));
            SyncEntityDrone = PacketID.AddPacketId(nameof(SyncEntityDrone));
            SyncCrateSpawn = PacketID.AddPacketId(nameof(SyncCrateSpawn));
            NetPlay.OnReceiveClientPacket += NetPlay_OnReceiveClientPacket;
            NetPlay.OnReceiveServerPacket += NetPlay_OnReceiveServerPacket;
        }

        public static void Unload()
        {
            PacketID.Collection.TryRemove(SyncCrateSpawn);
            PacketID.Collection.TryRemove(SyncEntityDrone);
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
              //  ChatSystem.SendMessage("Mods. Nightshade this tank in particular.", Color.Plum);
                CA_Main.PoisonedTanks.Add(GameHandler.AllTanks[tank]);
                CA_Main.Tank_OnPoisoned(GameHandler.AllTanks[tank]);
            }
           else if(packet == SyncEntityDrone)
            {
            
                bool newdrone = reader.GetBool();

                if (!newdrone)
                {
                    int _drone = reader.GetInt();
                    Vector3 position = reader.GetVector3();
                    Vector3 velocity = reader.GetVector3();

                    float dronerotation = reader.GetFloat();
                    float turretrotation = reader.GetFloat();

                    int task = reader.GetInt();
                    int taskstate = reader.GetInt();
                    ref CA_Drone drone = ref CA_Drone.AllDrones[_drone];

                    drone.Body.Position = position.FlattenZ() / 8f;
                    drone.HoverHeight = position.Y;
                    drone.Velocity = velocity.FlattenZ();
                    drone.HoverSpeed = velocity.Y;

                    drone.Task = (CA_Drone.DroneTask)task;
                    drone.CurrentState = (CA_Drone.TaskState)taskstate;
                }else
                {
                    int _tank = reader.GetInt();
                    Vector3 position = reader.GetVector3();
                    CA_Drone syncdrone = new CA_Drone(GameHandler.AllTanks[_tank],position.FlattenZ());
                }
               
            }
            else if(packet == SyncCrateSpawn)
            {
                int drone = reader.GetInt();
                int id = reader.GetInt();
                int team = reader.GetInt();
                TankTemplate tenk = new TankTemplate() { AiTier = id,Team = team};
                Vector3 P = reader.GetVector3();
                float G = reader.GetFloat();
                var crate = Crate.SpawnCrate(P,G);
                crate.TankToSpawn = tenk;
                CA_Drone.AllDrones[drone].Recruit = crate;

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
            else if(packet == SyncEntityDrone)
            {
                bool Spawn = reader.GetBool();
                message.Put(Spawn);

                message.Put(reader.GetInt());
                message.Put(reader.GetVector3());
                if (!Spawn)
                {
                    message.Put(reader.GetVector3());
                    message.Put(reader.GetFloat());
                    message.Put(reader.GetFloat());
                    message.Put(reader.GetInt());
                    message.Put(reader.GetInt());
                }
                Server.NetManager.SendToAll(message, LiteNetLib.DeliveryMethod.Unreliable, peer);
            }
            else if (packet == SyncCrateSpawn)
            {
                message.Put(reader.GetInt());
                message.Put(reader.GetInt());
                message.Put(reader.GetInt());
                message.Put(reader.GetVector3());
                message.Put(reader.GetFloat());
                Server.NetManager.SendToAll(message, LiteNetLib.DeliveryMethod.ReliableOrdered, peer);
            }
        }
        #endregion
        //Sends a packet to apply nightshade to a tank
        public static void RequestNightshadeTank(Tank target)
        {
            // If the game client is not connected to a server, we don't bother running the future lines.
            if (!IsConnected() || MainMenuUI.IsActive)
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

        //Sends a packet to sync a tank's drone
        //During multiplayer
        public static void SyncDrone(CA_Drone target,bool Spawn)
        {
            // If the game client is not connected to a server, we don't bother running the future lines.
            if (!IsConnected() || MainMenuUI.IsActive)
                return;

            // Constructs a NetDataWriter, which will be our packet.
            NetDataWriter message = new();

            // Here we put the data that goes into our packet of data. We send the packet type first, and not by choice.
            // Tanks Rebirth automatically reads an integer as the first piece of data in the packet's data stream, which becomes the 'int packet' above.
            message.Put(SyncEntityDrone);
            message.Put(Spawn);
            if (!Spawn) message.Put(target.Id); //Drone id in array
            else if (Spawn) message.Put(target.droneOwner!.WorldId);

            message.Put(target.Position3D);  //Drone position in array

            if (!Spawn)
            {
               message.Put(target.Velocity3D); //Drone velocity in array
               message.Put(target.DroneRotation); //Drone's body rotation
               message.Put(target.TurretRotation);
               message.Put((int)target.Task); //for mostly animation stuff
               message.Put((int)target.CurrentState); //for mostly animation stuff
            }


            // This is how our data is sent to the server. We end up handling the data again within the scope of the server.
            // Delivery methods will be covered after the next couple of code blocks.
            NetClient.Send(message, LiteNetLib.DeliveryMethod.Unreliable);
        }

        public static void SyncDroneCrate(CA_Drone drone,Crate crate)
        {
            if (!IsConnected() || MainMenuUI.IsActive)
                return;

            // Constructs a NetDataWriter, which will be our packet.
            NetDataWriter message = new();
            message.Put(SyncCrateSpawn);
            message.Put(drone.Id);
            message.Put(crate.TankToSpawn.AiTier);
            message.Put(crate.TankToSpawn.Team);
            message.Put(crate.Position);
            message.Put(crate.Gravity);
            NetClient.Send(message, LiteNetLib.DeliveryMethod.ReliableOrdered);
        }


    }
}
