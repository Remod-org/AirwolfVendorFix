using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("AirwolfVendorFix", "RFC1920", "1.0.5")]
    [Description("Respawn missing Airwolf and Fishing Village vendors")]
    internal class AirwolfVendorFix : RustPlugin
    {
        private ConfigData configData;
        private readonly string vprefab = "assets/prefabs/npc/bandit/shopkeepers/bandit_conversationalist.prefab";
        private readonly string bprefab = "assets/prefabs/npc/bandit/shopkeepers/boat_shopkeeper.prefab";
        private readonly List<Vector3> boatloc = new List<Vector3>();

        [PluginReference]
        private readonly Plugin GridAPI;

        #region Message
        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);
        private void Message(IPlayer player, string key, params object[] args) => player.Reply(Lang(key, player.Id, args));
        #endregion

        private void Init()
        {
            LoadConfigValues();
            if (configData.Options.autoPlaceVendors)
            {
                FindMonuments();
            }
        }

        // For auto-placement
        private void SpawnVendor(string prefab, Vector3 spawnpos, Quaternion spawnrot, bool killold = false)
        {
            if (killold)
            {
                var hits = Physics.OverlapSphere(spawnpos, 2f);
                for (var i = 0; i < hits.Length; i++)
                {
                    var vendor = hits[i].GetComponentInParent<BaseEntity>();
                    if (vendor == null) continue;
                    if (vendor.ShortPrefabName == "bandit_conversationalist")
                    {
                        vendor.Kill();
                    }
                }
            }
            var newvendor = GameManager.server.CreateEntity(prefab, spawnpos, spawnrot, true);
            newvendor.Spawn();
        }

        // For command placement
        private static void SpawnVendor(string prefab, BasePlayer player)
        {
            var spawnpos = player.transform.position;
            var newvendor = GameManager.server.CreateEntity(prefab, player.transform.position, player.transform.rotation, true);
            newvendor.Spawn();
            newvendor.ToPlayer().viewAngles = player.viewAngles + new Vector3(0, 1.5f, 0);
            newvendor.transform.localEulerAngles = spawnpos + new Vector3(0, 1.5f, 0);
        }

        [ChatCommand("bsp")]
        private void CmdSpawnBoatVendor(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "save":
                        boatloc.Add(player.transform.position);
                        break;
                }
                return;
            }
            List<VehicleVendor> ven = new List<VehicleVendor>();
            Vis.Entities(player.transform.position, 3f, ven);
            foreach (var vendor in ven)
            {
                vendor.Kill();
            }
            SpawnVendor(bprefab, player);
        }

        [ChatCommand("vsp")]
        private void CmdSpawnAirwolfVendor(BasePlayer player, string command, string[] args)
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

        public string PositionToGrid(Vector3 position)
        {
            if (GridAPI != null)
            {
                var g = (string[]) GridAPI.CallHook("GetGrid", position);
                return string.Join("", g);
            }
            else
            {
                // From GrTeleport for display only
                var r = new Vector2(World.Size / 2 + position.x, World.Size / 2 + position.z);
                var x = Mathf.Floor(r.x / 146.3f) % 26;
                var z = Mathf.Floor(World.Size / 146.3f) - Mathf.Floor(r.y / 146.3f);

                return $"{(char)('A' + x)}{z - 1}";
            }
        }
        private void FindMonuments()
        {
            Vector3 extents = Vector3.zero;
            string name = null;
            bool ishapis =  ConVar.Server.level.Contains("Hapis");

            foreach (MonumentInfo monument in UnityEngine.Object.FindObjectsOfType<MonumentInfo>())
            {
                if (monument.name.Contains("power_sub")) continue;// || monument.name.Contains("cave")) continue;
                name = null;

                if (ishapis)
                {
                    var elem = Regex.Matches(monument.name, @"\w{4,}|\d{1,}");
                    foreach (Match e in elem)
                    {
                        if (e.Value.Equals("MONUMENT")) continue;
                        if (e.Value.Contains("Label")) continue;
                        name += e.Value + " ";
                    }
                    name = name.Trim();
                }
                else
                {
                    name = Regex.Match(monument.name, @"\w{6}\/(.+\/)(.+)\.(.+)").Groups[2].Value.Replace("_", " ").Replace(" 1", "").Titleize();
                }

                if (name.Contains("Fishing Village") && configData.Options.placeBoatVendor)
                {
                    // Large Fishing Village
                    if (configData.Options.debug) Puts($"Working on {name}");
                    List<BaseEntity> ents = new List<BaseEntity>();
                    Vis.Entities(monument.transform.position, 80, ents);
                    bool killvendor = false;
                    bool foundvendor = false;
                    Vector3 spawnpos = Vector3.zero;
                    Quaternion spawnrot = new Quaternion();

                    foreach (BaseEntity ent in ents)
                    {
                        if (ent == null) continue;
                        if (string.IsNullOrEmpty(ent.ShortPrefabName)) continue;
                        if (ent.ShortPrefabName.Equals("boat_shopkeeper"))
                        {
                            if (configData.Options.alwaysPlaceVendors)
                            {
                                if (configData.Options.debug) Puts("FOUND VENDOR, KILLING HIM!");
                                killvendor = true;
                                ent.Kill();
                            }
                        }
                        else if (ent.ShortPrefabName.Equals("shopkeeper_vm_invis"))
                        {
                            // Find invisible shopkeepers but verify no bandit_shopkeeper exists there
                            var hit = Physics.OverlapSphere(ent.transform.position, 1f);
                            for (var i = 0; i < hit.Length; i++)
                            {
                                var subitem = hit[i].GetComponentInParent<BaseEntity>();
                                if (subitem == null) continue;
                                if (subitem.ShortPrefabName.Equals("bandit_shopkeeper") || subitem.ShortPrefabName.Equals("boat_shopkeeper"))
                                {
                                    if (configData.Options.debug) Puts($"Found an invisible shopkeeper at {subitem.transform.position}, but there is already an associated NPC.");
                                }
                                else
                                {
                                    foundvendor = true;
                                    if (configData.Options.debug) Puts($"Found invisible shopkeeper with no associated NPC at {spawnpos.ToString()}");
                                    spawnpos = ent.transform.position;
                                    spawnrot = ent.transform.rotation;
                                    continue;
                                }
                            }
                        }
                    }
                    if (killvendor && spawnpos != Vector3.zero)
                    {
                        if (configData.Options.debug) Puts($"Respawning Boat Vendor at {spawnpos}");
                        SpawnVendor(bprefab, spawnpos, spawnrot, true);
                    }
                    else if (foundvendor && spawnpos != Vector3.zero)
                    {
                        if (configData.Options.debug) Puts($"Spawning Boat Vendor at {spawnpos}");
                        SpawnVendor(bprefab, spawnpos, spawnrot);
                    }
                }

                if (name.Contains("Bandit") && configData.Options.placeMiniVendor)
                {
                    List<BaseEntity> ents = new List<BaseEntity>();
                    Vis.Entities(monument.transform.position, 80, ents);
                    bool foundrfb = false;
                    bool foundoor = false;
                    bool foundvendor = false;
                    Vector3 spawnpos = Vector3.zero;
                    Vector3 rfbloc = Vector3.zero;
                    Quaternion spawnrot = new Quaternion();

                    foreach (BaseEntity entity in ents)
                    {
                        if (entity.ShortPrefabName.Equals("rfbroadcaster.static") && !foundrfb)
                        {
                            foundrfb = true;
                            rfbloc = entity.transform.position;
                            spawnpos = Vector3.Lerp(monument.transform.position, rfbloc, 0.975f);

                            var hits = Physics.OverlapSphere(entity.transform.position, 2f);
                            for (var i = 0; i < hits.Length; i++)
                            {
                                var door = hits[i].GetComponentInParent<BaseEntity>();
                                if (door == null) continue;
                                if (door.ShortPrefabName.Equals("bandit_conversationalist"))
                                {
                                    foundvendor = true;
                                    break;
                                }
                                else if (door.ShortPrefabName.Equals("door.hinged.wood.static") && !foundoor)
                                {
                                    foundoor = true;
                                    spawnpos.y -= 1.2f;
                                    var rotation = Quaternion.LookRotation(spawnpos);
                                    spawnrot = rotation *= Quaternion.Euler(0, 180, 0);
                                }
                            }
                        }
                    }
                    if (foundvendor)
                    {
                        if (configData.Options.alwaysPlaceVendors)
                        {
                            SpawnVendor(vprefab, spawnpos, spawnrot, true);
                        }
                    }
                    else if (foundrfb & foundoor)
                    {
                        SpawnVendor(vprefab, spawnpos, spawnrot);
                    }
                }
            }
        }

        #region config
        private class ConfigData
        {
            public Options Options = new Options();
            public VersionNumber Version;
        }

        private class Options
        {
            public bool autoPlaceVendors;
            public bool alwaysPlaceVendors;
            public bool placeBoatVendor;
            public bool placeMiniVendor;
            public bool debug;
        }

        protected override void LoadDefaultConfig()
        {
            Puts("Creating new config file");
            configData = new ConfigData
            {
                Options = new Options()
                {
                    autoPlaceVendors = false,
                    alwaysPlaceVendors = false,
                    placeBoatVendor = true,
                    placeMiniVendor = true,
                    debug = false
                },
                Version = Version,
            };
            SaveConfig(configData);
        }

        private void LoadConfigValues()
        {
            configData = Config.ReadObject<ConfigData>();
            configData.Version = Version;

            SaveConfig(configData);
        }

        private void SaveConfig(ConfigData config)
        {
            Config.WriteObject(config, true);
        }
        #endregion
    }
}
