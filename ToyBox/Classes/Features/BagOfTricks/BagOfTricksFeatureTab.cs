using ToyBox.Features.BagOfTricks.Cheats;
using ToyBox.Features.BagOfTricks.QoL;

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

        AddFeature(new PreventTrapsFromTriggeringFeature(), CheatsText);
        AddFeature(new ToggleLockJamFeature(), CheatsText);
        AddFeature(new UnlimitedModifierStackingFeature(), CheatsText);

        AddFeature(new DisableRequireMaterialComponent(), CheatsText);
        AddFeature(new DisableNegativePartyLevels(), CheatsText);
        AddFeature(new DisablePartyAbilityDamage(), CheatsText);

        AddFeature(new SpontaneousCasterCopyScrollFeature(), CheatsText);

        AddFeature(new DisableFogOfWarFeature(), CheatsText);
        AddFeature(new RestoreAbilitiesAfterCombatFeature(), CheatsText);
        AddFeature(new RestoreSpellsAfterCombatFeature(), CheatsText);
        AddFeature(new RestoreItemsAfterCombatFeature(), CheatsText);
        AddFeature(new RestAfterCombatFeature(), CheatsText);
    }
}
