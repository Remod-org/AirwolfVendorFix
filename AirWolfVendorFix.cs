using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("AirwolfVendorFix", "RFC1920", "1.0.2")]
    [Description("Respawn missing Airwolf vendor")]
    class AirwolfVendorFix : RustPlugin
    {
        string vprefab = "assets/prefabs/npc/bandit/shopkeepers/bandit_conversationalist.prefab";

        void OnServerInitialized()
        {
            var vendors = BaseNetworkable.FindObjectsOfType<VehicleVendor>();
            foreach (var vendor in vendors)
            {
                Puts($"Found {vendor.GetType().ToString()} ({vendor.ShortPrefabName}) @ {vendor.transform.position.ToString()}");
                var x = vendor.conversations.ToSentence();
                Puts($"Conversations: {x}");
                foreach(var item in vendor.vehicleSpawner.objectsToSpawn)
                {
                    Puts($"Sells: {item.prefabToSpawn}");
                }
//                Puts($"{vendor.vehicleSpawner.objectsToSpawn.ToSentence()}");
            }
        }

        [ChatCommand("bsp")]
        void CmdSpawnBoatVendor(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            List<VehicleVendor> ven = new List<VehicleVendor>();
            Vis.Entities(player.transform.position, 3f, ven);
            foreach (var vendor in ven)
            {
                vendor.Kill();
            }

            var spawnpos = player.transform.position;
            var newvendor = GameManager.server.CreateEntity(vprefab, spawnpos, new Quaternion(), true);
            newvendor.Spawn();
            newvendor.ToPlayer().viewAngles = player.viewAngles + new Vector3(0, 1.5f, 0);
            newvendor.transform.localEulerAngles = spawnpos + new Vector3(0, 1.5f, 0);

            var vv = newvendor as VehicleVendor;
            ConversationData cd = new ConversationData() { shortname = "BoatVendor" };
            ConversationData[] cl = new ConversationData[1];
            cl[0] = cd;
            vv.conversations = cl;
        }

        [ChatCommand("vsp")]
        void CmdSpawnAirwolfVendor(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            List<VehicleVendor> ven = new List<VehicleVendor>();
            Vis.Entities(player.transform.position, 3f, ven);
            foreach (var vendor in ven)
            {
                vendor.Kill();
            }

            var spawnpos = player.transform.position;
            var newvendor = GameManager.server.CreateEntity(vprefab, spawnpos, new Quaternion(), true);
            newvendor.Spawn();
            newvendor.ToPlayer().viewAngles = player.viewAngles + new Vector3(0, 1.5f, 0);
            newvendor.transform.localEulerAngles = spawnpos + new Vector3(0, 1.5f, 0);
        }
    }
}
