using ToyBox.Features.BagOfTricks.Cheats;
using ToyBox.Features.BagOfTricks.QoL;
using ToyBox.Features.BagOfTricks.QualityOfLife;

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
        AddFeature(new DisableTricksterMythicMusicFeature(), QualityOfLifeText);

        AddFeature(new PreventTrapsFromTriggeringFeature(), CheatsText);
        AddFeature(new ToggleLockJamFeature(), CheatsText);
        AddFeature(new UnlimitedModifierStackingFeature(), CheatsText);
        AddFeature(new HighlightHiddenObjectsFeature(), CheatsText);
        AddFeature(new InfiniteAbilitiesFeature(), CheatsText);
        AddFeature(new InfiniteSpellCastsFeature(), CheatsText);
        AddFeature(new DisableRequireMaterialComponentFeature(), CheatsText);
        AddFeature(new DisableNegativePartyLevelsFeature(), CheatsText);
        AddFeature(new DisablePartyAbilityDamageFeature(), CheatsText);
        AddFeature(new InfiniteActionsFeature(), CheatsText);
        AddFeature(new InfiniteItemChargesFeature(), CheatsText);
        AddFeature(new InstantGlobalCrusadeSpellsFeature(), CheatsText);
        AddFeature(new SpontaneousCasterCopyScrollFeature(), CheatsText);
        AddFeature(new IgnoreEquipmentRestrictionsFeature(), CheatsText);
        AddFeature(new DisableArmourMaxDexterityFeature(), CheatsText);
        AddFeature(new DisableArmourSpeedReductionFeature(), CheatsText);
        AddFeature(new DisableArcaneSpellFailureFeature(), CheatsText);
        AddFeature(new IgnoreArmourChecksPenaltyFeature(), CheatsText);
        AddFeature(new NoFriendlyFireAoEFeature(), CheatsText);
        AddFeature(new FreeMetaMagicFeature(), CheatsText);
        AddFeature(new DisableFogOfWarFeature(), CheatsText);
        AddFeature(new RestoreAbilitiesAfterCombatFeature(), CheatsText);
        AddFeature(new RestoreSpellsAfterCombatFeature(), CheatsText);
        AddFeature(new RestoreItemsAfterCombatFeature(), CheatsText);
        AddFeature(new RestAfterCombatFeature(), CheatsText);
        AddFeature(new InstantChangePartyMembersFeature(), CheatsText);
        AddFeature(new EquipmentNoWeightFeature(), CheatsText);
        AddFeature(new AllowItemUseFromInventoryDuringCombatFeature(), CheatsText);
        AddFeature(new IgnoreAlignmentRequirementsForAbilitiesFeature(), CheatsText);
        AddFeature(new IgnoreAllRequirementsForAbilitiesFeature(), CheatsText);
        AddFeature(new IgnorePetSizesForMountingFeature(), CheatsText);
        AddFeature(new AllowAnyUnitAsYourMountFeature(), CheatsText);
        AddFeature(new DisableAttackOfOpportunityFeature(), CheatsText);
        AddFeature(new AllowMovingThroughUnitsFeature(), CheatsText);
    }
}
