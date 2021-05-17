using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("AirwolfVendorFix", "RFC1920", "1.0.2")]
    [Description("Respawn missing Airwolf vendor")]
    class AirwolfVendorFix : RustPlugin
    {
        string vprefab = "assets/prefabs/npc/bandit/shopkeepers/bandit_conversationalist.prefab";
        string bprefab = "assets/prefabs/npc/bandit/shopkeepers/boat_shopkeeper.prefab";

        void SpawnVendor(string prefab, BasePlayer player)
        {
            var spawnpos = player.transform.position;
            var newvendor = GameManager.server.CreateEntity(prefab, player.transform.position, player.transform.rotation, true);
            newvendor.Spawn();
            newvendor.ToPlayer().viewAngles = player.viewAngles + new Vector3(0, 1.5f, 0);
            newvendor.transform.localEulerAngles = spawnpos + new Vector3(0, 1.5f, 0);
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
            SpawnVendor(bprefab, player);
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
            SpawnVendor(vprefab, player);
        }
    }
}
