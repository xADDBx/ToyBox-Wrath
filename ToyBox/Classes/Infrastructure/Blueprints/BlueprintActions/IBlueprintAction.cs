using Kingmaker.Blueprints;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions; 
public interface IBlueprintAction<in T> where T : SimpleBlueprint { 
    public abstract void OnGui(T blueprint, params object[] parameter); 
}
