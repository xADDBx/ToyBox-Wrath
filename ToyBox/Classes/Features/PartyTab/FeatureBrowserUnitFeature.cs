using Kingmaker.EntitySystem.Entities;

namespace ToyBox.Features.PartyTab;

public partial class FeatureBrowserUnitFeature : Feature, INeedContextFeature<UnitEntityData> {
    [LocalizedString("ToyBox_Features_PartyTab_FeatureBrowserUnitFeature_Name", "Unit Feature Browser")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_PartyTab_FeatureBrowserUnitFeature_Description", "Views a Browser containing all the features of the unit in question and adding/removing them")]
    public override partial string Description { get; }
    public override void OnGui() => throw new NotImplementedException();
    public void OnGui(UnitEntityData context) => throw new NotImplementedException();
}
