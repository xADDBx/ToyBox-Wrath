using ToyBox.Classes.Features.BagOfTricks;

namespace ToyBox.Features.BagOfTricks;
public partial class BagOfTricksFeatureTab : FeatureTab {
    [LocalizedString("ToyBox_Features_BagOfTricks_BagOfTricksFeatureTab_BagOfTricksText", "Bag of Tricks")]
    public override partial string Name { get; }
    public BagOfTricksFeatureTab() {
        AddFeature(new EnableAchievementsFeature());
        AddFeature(new SpontaneousCasterCopyScrollFeature());
        AddFeature(new DisableFogOfWarFeature());
        AddFeature(new RestAfterCombatFeature());
        AddFeature(new RestoreAbilitiesAfterCombatFeature());
        AddFeature(new RestoreSpellsAfterCombatFeature());
        AddFeature(new RestoreItemsAfterCombatFeature());
        AddFeature(new ToggleLockJamFeature());
    }
}
