﻿using Kingmaker.Blueprints;
using Kingmaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Modding;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Armies.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.AI.Blueprints;
using Kingmaker.AI.Blueprints.Considerations;
using System.IO;
using System.Runtime.Serialization;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;

namespace ToyBox.classes.Infrastructure.Blueprints {
    [Serializable]
    public class BlueprintIdCache {
        public string CachedGameVersion = "";
        public HashSet<(string, string)> UmmList = new();
        public HashSet<(string, string)> OmmList = new();
        public Dictionary<Type, HashSet<BlueprintGuid>> IdsByType = new();

        public static HashSet<Type> CachedIdTypes = new() {
                typeof(BlueprintEtude), typeof(BlueprintArea), typeof(BlueprintItemEnchantment),
                typeof(BlueprintBuff), typeof(BlueprintLeaderSkill), typeof(BlueprintPortrait),
                typeof(BlueprintSpellbook), typeof(BlueprintAbility), typeof(BlueprintAreaEnterPoint),
                typeof(BlueprintUnit), typeof(BlueprintAiAction), typeof(Consideration), 
                typeof(BlueprintBrain), typeof(BlueprintFeature), typeof(BlueprintUnitFact)
        };

        private static BlueprintIdCache _instance;
        public static BlueprintIdCache Instance {
            get {
                if (_instance == null) {
                    Load();
                }
                return _instance;
            }
        }

        private static bool? _needsCacheRebuilt = null;
        public static bool NeedsCacheRebuilt {
            get {
                if (_needsCacheRebuilt.HasValue) return _needsCacheRebuilt.Value && !isRebuilding;

                bool gameVersionChanged = GameVersion.GetVersion() != Instance.CachedGameVersion;

                var ummSet = Instance.UmmList.ToHashSet();
                bool ummModsChanged = !(ummSet.Count == UnityModManagerNet.UnityModManager.modEntries.Count);
                if (!ummModsChanged) {
                    foreach (var modEntry in UnityModManagerNet.UnityModManager.modEntries) {
                        if (!ummSet.Contains(new(modEntry.Info.Id, modEntry.Info.Version))) {
                            ummModsChanged = true;
                            break;
                        }
                    }
                }

                var ommSet = Instance.OmmList.ToHashSet();
                bool ommModsChanged = !(ommSet.Count == OwlcatModificationsManager.s_Instance.AppliedModifications.Count());
                if (!ommModsChanged) {
                    foreach (var modEntry in OwlcatModificationsManager.s_Instance.AppliedModifications) {
                        if (!ommSet.Contains(new(modEntry.Manifest.UniqueName, modEntry.Manifest.Version))) {
                            ommModsChanged = true;
                            break;
                        }
                    }
                }
                ModKit.Mod.Log($"Test for BPId Cache constincy: Game Version Changed: {gameVersionChanged}; UMM Mod Changed: {ummModsChanged}; OMM Mod Changed: {ommModsChanged}");
                _needsCacheRebuilt = gameVersionChanged || ummModsChanged || ommModsChanged;
                return _needsCacheRebuilt.Value && !isRebuilding;
            }
        }

        public static bool isRebuilding = false;
        internal static void RebuildCache(List<SimpleBlueprint> blueprints) {
            if (!Main.Settings.toggleUseBPIdCache) return;
            ModKit.Mod.Log("Starting to BPId Cache");
            isRebuilding = true;

            // Header
            Instance.CachedGameVersion = GameVersion.GetVersion();

            Instance.UmmList.Clear();
            foreach (var modEntry in UnityModManagerNet.UnityModManager.modEntries) {
                Instance.UmmList.Add(new(modEntry.Info.Id, modEntry.Info.Version));
            }

            Instance.OmmList.Clear();
            foreach (var modEntry in OwlcatModificationsManager.s_Instance.AppliedModifications) {
                Instance.OmmList.Add(new(modEntry.Manifest.UniqueName, modEntry.Manifest.Version));
            }

            //Ids
            Instance.IdsByType.Clear();
            foreach (var type in CachedIdTypes) {
                HashSet<BlueprintGuid> idsForType = new();
                foreach (var bp in blueprints) {
                    if (type.IsInstanceOfType(bp)) {
                        idsForType.Add(bp.AssetGuid);
                    }
                }
                Instance.IdsByType[type] = idsForType;
            }

            Instance.Save();
            _needsCacheRebuilt = false;
            isRebuilding = false;
            ModKit.Mod.Log("Finished rebuilding BPId Cache");
        }
        internal void Save() {
            using var stream = new FileStream(EnsureFilePath(), FileMode.Create, FileAccess.Write);
            using var writer = new BinaryWriter(stream, Encoding.UTF8);

            writer.Write(CachedGameVersion);

            writer.Write(UmmList.Count);
            foreach (var (item1, item2) in UmmList) {
                writer.Write(item1);
                writer.Write(item2);
            }

            writer.Write(OmmList.Count);
            foreach (var (item1, item2) in OmmList) {
                writer.Write(item1);
                writer.Write(item2);
            }

            writer.Write(IdsByType.Count);
            foreach (var kvp in IdsByType) {
                writer.Write(kvp.Key.AssemblyQualifiedName);
                writer.Write(kvp.Value.Count);
                foreach (var guid in kvp.Value) {
                    writer.Write(guid.ToString());
                }
            }
        }
        private static void Load() {
            try {
                using var stream = new FileStream(EnsureFilePath(), FileMode.Open, FileAccess.Read);
                using var reader = new BinaryReader(stream, Encoding.UTF8);

                BlueprintIdCache result = new();
                result.CachedGameVersion = reader.ReadString();

                int ummCount = reader.ReadInt32();
                for (int i = 0; i < ummCount; i++) {
                    result.UmmList.Add((reader.ReadString(), reader.ReadString()));
                }

                int ommCount = reader.ReadInt32();
                for (int i = 0; i < ommCount; i++) {
                    result.OmmList.Add((reader.ReadString(), reader.ReadString()));
                }

                int dictCount = reader.ReadInt32();
                for (int i = 0; i < dictCount; i++) {

                    string typeName = reader.ReadString();
                    Type type = Type.GetType(typeName);
                    if (type == null) ModKit.Mod.Error(new SerializationException($"Type {typeName} not found.").ToString());

                    int listCount = reader.ReadInt32();
                    var guidList = new HashSet<BlueprintGuid>();
                    for (int j = 0; j < listCount; j++) {
                        guidList.Add(BlueprintGuid.Parse(reader.ReadString()));
                    }
                    result.IdsByType[type] = guidList;
                }
                _instance = result;
            } catch (FileNotFoundException) {
                _instance = new();
                _instance.Save();
            }
        }

        private static string EnsureFilePath() {
            var userConfigFolder = Path.Combine(Main.ModEntry.Path, "UserSettings");
            Directory.CreateDirectory(userConfigFolder);
            return Path.Combine(userConfigFolder, "BlueprintIdCache.bin");
        }
    }
}
