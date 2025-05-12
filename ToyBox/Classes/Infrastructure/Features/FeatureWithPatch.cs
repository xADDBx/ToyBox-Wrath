namespace ToyBox;
public abstract class FeatureWithPatch : ToggledFeature {
    private static Dictionary<string, List<Type>>? m_HarmonyCategoryCache;
    protected Harmony HarmonyInstance = null!;
    protected virtual string HarmonyName => $"ToyBox.Feature.{Name}";
    public FeatureWithPatch() {
        HarmonyInstance = new(HarmonyName);
    }
    public void Patch() {
        if (IsEnabled) {
            HandleHarmonyCategoryCache();
            if (m_HarmonyCategoryCache!.TryGetValue(HarmonyName, out var toPatch)) {
                toPatch.Do(type => {
                    HarmonyInstance.CreateClassProcessor(type).Patch();
                });
            }
        }
    }
    public void Unpatch() {
        HarmonyInstance.UnpatchAll(HarmonyName);
    }
    public override void Initialize() {
        Patch();
    }
    public override void Destroy() {
        Unpatch();
    }
    public static void HandleHarmonyCategoryCache() {
        if (m_HarmonyCategoryCache == null) {
            m_HarmonyCategoryCache = new();
            lock (m_HarmonyCategoryCache) {
                if (m_HarmonyCategoryCache.Count == 0) {
                    foreach (var type in AccessTools.GetTypesFromAssembly(typeof(FeatureWithPatch).Assembly)) {
                        List<HarmonyMethod> fromType = HarmonyMethodExtensions.GetFromType(type);
                        HarmonyMethod harmonyMethod = HarmonyMethod.Merge(fromType);
                        if (!string.IsNullOrEmpty(harmonyMethod.category)) {
                            if (!m_HarmonyCategoryCache.TryGetValue(harmonyMethod.category, out var typeList)) {
                                typeList ??= new();
                            }
                            typeList.Add(type);
                            m_HarmonyCategoryCache[harmonyMethod.category] = typeList;
                        }
                    }
                }
            }
        }
    }
}
