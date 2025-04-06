using UnityEngine;

namespace ToyBox.Features.BagOfTricks;
public partial class BagOfTricksFeatureTab : FeatureTab {
    [LocalizedString("ToyBox_Features_BagOfTricks_BagOfTricksFeatureTab_BagOfTricksText", "Bag of Tricks")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_BagOfTricksFeatureTab_QualityOfLifeText", "Quality of Life")]
    private static partial string QualityOfLifeText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_BagOfTricksFeatureTab_CheatsText", "Cheats")]
    private static partial string CheatsText { get; }
    public BagOfTricksFeatureTab() {
        AddFeature(new EnableAchievementsFeature(), QualityOfLifeText);
        AddFeature(new SpontaneousCasterCopyScrollFeature(), CheatsText);
        AddFeature(new DisableFogOfWarFeature(), CheatsText);
        AddFeature(new RestAfterCombatFeature(), CheatsText);
        AddFeature(new RestoreAbilitiesAfterCombatFeature(), CheatsText);
        AddFeature(new RestoreSpellsAfterCombatFeature(), CheatsText);
        AddFeature(new RestoreItemsAfterCombatFeature(), CheatsText);
        AddFeature(new ToggleLockJamFeature(), CheatsText);
    }
    public override void OnGui() {
        foreach (var (groupName, features) in GetGroups()) {
            using (VerticalScope()) {
                GUILayout.Label(groupName, GUILayout.ExpandWidth(false));
                using (HorizontalScope()) {
                    GUILayout.Space(25);
                    using (VerticalScope()) {
                        foreach (var feature in features) {
                            feature.OnGui();
                        }
                    }
                }
            }
            Div.DrawDiv();
        }
    }
}
