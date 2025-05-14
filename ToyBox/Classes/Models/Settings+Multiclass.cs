// Copyright < 2021 > Narria (github user Cabarius) - License: MIT
using ModKit.Utility;
using System.Collections.Generic;
using Kingmaker.UnitLogic;
using ModKit;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem;
using Newtonsoft.Json;
using Kingmaker.Utility.UnitDescription;

namespace ToyBox {
    public class ArchetypeOptions : HashSet<string> {
        public const string NoArchetype = "$NoArchetype";

        public bool Contains(BlueprintArchetype arch) => Contains(arch == null ? NoArchetype : arch.HashKey());

        public void Add(BlueprintArchetype arch) => Add(arch == null ? NoArchetype : arch.HashKey());
        public void AddExclusive(BlueprintArchetype arch) { Clear(); Add(arch); }
        public void Remove(BlueprintArchetype arch) => Remove(arch == null ? NoArchetype : arch.HashKey());
        public override string ToString() {
            var result = "{";
            foreach (var arch in this) result += " " + arch + ",";
            result += "}";
            return result;
        }
    }
    public class MulticlassOptions : SerializableDictionary<string, ArchetypeOptions> {
        public const string CharGenKey = @"$CharacterGeneration";
        public static MulticlassOptions Get(UnitDescriptor ch) {
            //Main.Log($"stack: {System.Environment.StackTrace}");
            MulticlassOptions options;
            var key = ch?.Blueprint?.LocalizedName?.String?.GetActualKey();
            if (ch == null || (key != null && charGenLocIds.Contains(key))) {
                options = Main.Settings.multiclassSettings.GetValueOrDefault(CharGenKey, new MulticlassOptions());
                //Mod.Debug($"MulticlassOptions.Get - chargen - options: {options}");
            } else {
                if (ch.HashKey() == null) return null;
                options = Main.Settings.perSave.multiclassSettings.GetValueOrDefault(ch.HashKey(), new MulticlassOptions());
                //Mod.Debug($"MulticlassOptions.Get - {ch.CharacterName} - set: {options}");
            }
            return options;
        }
        private static HashSet<string> charGenLocIds = ["e78a0887-3265-4a46-a365-34b7ba9d1731", "846699aa-bef1-4dd3-9ac8-bce3ea6b3d18", "a06c4ef0-b4a5-4343-a116-bdf68c6cb24e", "1daf8fd5-fb15-4b22-ab80-6245c714c249"];
        public static bool CanSelectClassAsMulticlass(UnitDescriptor ch, BlueprintCharacterClass cl) {
            if (!Main.IsInGame) return true;
            if (ch == null) return true;
            if (cl == null) return false;
            var options = Get(ch);
            var checkMythic = cl.IsMythic;
            var foundIt = false;
            var classCount = 0;
            var selectedCount = 0;
            foreach (var cd in ch.Progression.Classes) {
                var charClass = cd.CharacterClass;
                if (cd.CharacterClass.IsMythic == checkMythic) {
                    classCount += 1;
                    var contains = options.Contains(charClass);
                    if (contains) {
                        selectedCount += 1;
                    }
                    if (charClass == cl && !contains)
                        foundIt = true;
                }
            }
            var result = !foundIt || (classCount - selectedCount > 1);
            //Mod.Trace($"canSelect {cl.Name} - foundIt : {foundIt} count: {classCount} selected: {selectedCount} => {result}");
            return result;
        }
        public static void Set(UnitDescriptor ch, MulticlassOptions options) {
            //modLogger.Log($"stack: {System.Environment.StackTrace}");
            var key = ch?.Blueprint?.LocalizedName?.String?.GetActualKey();
            if (ch == null || (key != null && charGenLocIds.Contains(key)))
                Main.Settings.multiclassSettings[CharGenKey] = options;
            else {
                if (ch.HashKey() == null) return;
                Mod.Debug($"options: {options}");
                Main.Settings.perSave.multiclassSettings[ch.HashKey()] = options;
                Mod.Trace($"multiclass options: {string.Join(" ", Main.Settings.perSave.multiclassSettings)}");
                Settings.SavePerSaveSettings();
            }
        }

        public ArchetypeOptions ArchetypeOptions(BlueprintCharacterClass cl) => this.GetValueOrDefault(cl.HashKey(), new ArchetypeOptions());
        public void SetArchetypeOptions(BlueprintCharacterClass cl, ArchetypeOptions archOptions) => this[cl.HashKey()] = archOptions;
        public bool Contains(BlueprintCharacterClass cl) => ContainsKey(cl.HashKey());
        public ArchetypeOptions Add(BlueprintCharacterClass cl) => this[cl.HashKey()] = new ArchetypeOptions();
        public void Remove(BlueprintCharacterClass cl) => Remove(cl.HashKey());
        public override string ToString() {
            var result = base.ToString() + " {\n";
            foreach (var classEntry in this) {
                result += $"    {classEntry.Key} : {classEntry.Value}\n";
            }
            result += "}";
            return result;
        }
    }
}
