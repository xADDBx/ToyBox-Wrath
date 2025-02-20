using Kingmaker;
using Kingmaker.AI.Blueprints.Considerations;
using Kingmaker.AI.Blueprints;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Armies.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Items;
using Kingmaker.Modding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System.Runtime.Serialization;
using System.Text;

namespace ToyBox.Infrastructure.Blueprints;
public class BlueprintIdCache {
    private static readonly Lazy<BlueprintIdCache> _instance = new Lazy<BlueprintIdCache>(() => {
        var cache = Load();
        if (cache == null) {
            cache = new();
            cache.Save();
        }
        return cache;
    });
    public static BlueprintIdCache Instance => _instance.Value;
    public string CachedGameVersion = "";
    public HashSet<(string, string)> UmmList = new();
    public HashSet<(string, string)> OmmList = new();
    public Dictionary<Type, HashSet<BlueprintGuid>> IdsByType = new();
    public static HashSet<Type> CachedIdTypes = new() {
                typeof(BlueprintItem), typeof(BlueprintItemWeapon), typeof(BlueprintItemArmor),
                typeof(BlueprintEtude), typeof(BlueprintArea), typeof(BlueprintItemEnchantment),
                typeof(BlueprintBuff), typeof(BlueprintLeaderSkill), typeof(BlueprintPortrait),
                typeof(BlueprintSpellbook), typeof(BlueprintAbility), typeof(BlueprintAreaEnterPoint),
                typeof(BlueprintUnit), typeof(BlueprintAiAction), typeof(Consideration),
                typeof(BlueprintBrain), typeof(BlueprintFeature), typeof(BlueprintUnitFact),
                typeof(BlueprintAreaPreset)
        };

    private static bool? m_NeedsCacheRebuilt = null;
    public static bool NeedsCacheRebuilt {
        get {
            if (m_NeedsCacheRebuilt.HasValue) return m_NeedsCacheRebuilt.Value && !IsRebuilding;

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
            Log($"Test for BPId Cache constincy: Game Version Changed: {gameVersionChanged}; UMM Mod Changed: {ummModsChanged}; OMM Mod Changed: {ommModsChanged}");
            m_NeedsCacheRebuilt = gameVersionChanged || ummModsChanged || ommModsChanged;
            return m_NeedsCacheRebuilt.Value && !IsRebuilding;
        }
    }

    public static bool IsRebuilding = false;
    internal static void RebuildCache(List<SimpleBlueprint> blueprints) {
        if (!Settings.UseBPIdCache) return;
        Log("Starting to build BPId Cache");
        IsRebuilding = true;
        try {
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
        } catch (Exception ex) {
            Error(ex.ToString());
        }
        m_NeedsCacheRebuilt = false;
        IsRebuilding = false;
        Log("Finished rebuilding BPId Cache");
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
    private static BlueprintIdCache? Load() {
        try {
            Trace("Started loading BPId Cache");
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
                if (type == null) {
                    throw new SerializationException($"BPId Cache references {typeName}, but the type couldn't be found not found.");
                }

                int listCount = reader.ReadInt32();
                var guidList = new HashSet<BlueprintGuid>();
                for (int j = 0; j < listCount; j++) {
                    guidList.Add(BlueprintGuid.Parse(reader.ReadString()));
                }
                result.IdsByType[type] = guidList;
            }
            Trace("Finished loading BPId Cache");
            return result;
        } catch (FileNotFoundException) {
            Debug("No BPId Cache found, creating new.");
            return null;
        } catch (SerializationException ex) {
            Error(ex);
            return null;
        }
    }

    private static string EnsureFilePath() {
        var userConfigFolder = Path.Combine(Main.ModEntry.Path, "Settings");
        Directory.CreateDirectory(userConfigFolder);
        return Path.Combine(userConfigFolder, "BlueprintIdCache.bin");
    }
}
