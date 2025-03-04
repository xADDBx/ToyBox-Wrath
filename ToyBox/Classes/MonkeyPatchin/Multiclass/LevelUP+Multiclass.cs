﻿// borrowed shamelessly and enhanced from Bag of Tricks https://www.nexusmods.com/pathfinderkingmaker/mods/26, which is under the MIT License

using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
//using Kingmaker.Controllers.GlobalMap;
using Kingmaker.EntitySystem.Entities;
//using Kingmaker.UI._ConsoleUI.Models;
//using Kingmaker.UI.RestCamp;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.Utility;
using ModKit;
using System;
using System.Linq;
using ToyBox.classes.Infrastructure;
//using Kingmaker.UI._ConsoleUI.GroupChanger;
//using Kingmaker.UI.LevelUp.Phase;

namespace ToyBox.Multiclass {
    internal static class LevelUp {
        public static Settings settings => Main.Settings;
        public static Player player => Game.Instance.Player;

        [HarmonyPatch(typeof(LevelUpController), MethodType.Constructor, new Type[] {
            typeof(UnitEntityData),
            typeof(bool),
            typeof(LevelUpState.CharBuildMode),
            typeof(bool)})]
        private static class LevelUpController_ctor_Patch {
            [HarmonyPrefix, HarmonyPriority(Priority.First)]
            private static bool Prefix(LevelUpController __instance) {
                if (Main.Enabled) {
                    MultipleClasses.levelUpController = __instance;
                }
                return true;
            }
        }
#if true
        /*     public static void UpdateProgression(
                                    [NotNull] LevelUpState state,
                                    [NotNull] UnitDescriptor unit,
                                    [NotNull] BlueprintProgression progression)
        */
        [HarmonyPatch(typeof(LevelUpHelper), nameof(LevelUpHelper.UpdateProgression))]
        [HarmonyPatch(new Type[] { typeof(LevelUpState), typeof(UnitDescriptor), typeof(BlueprintProgression) })]
        private static class LevelUpHelper_UpdateProgression_Patch {
            public static bool Prefix([NotNull] LevelUpState state, [NotNull] UnitDescriptor unit, [NotNull] BlueprintProgression progression) {
                if (!settings.toggleMulticlass) return true;
                var progressionData = unit.Progression.SureProgressionData(progression);
                var level = progressionData.Level;
                var nextLevel = progressionData.Blueprint.CalcLevel(unit);
                progressionData.Level = nextLevel;
                // TODO - this is from the mod but we need to figure out if max level 20 still makes sense with mythic levels
                // int maxLevel = 20 // unit.Progression.CharacterLevel;
                // if (nextLevel > maxLevel)
                //     nextLevel = maxLevel;
                //Mod.Debug($"LevelUpHelper_UpdateProgression_Patch - {unit.CharacterName.orange()} - class: {state.SelectedClass} level: {level} nextLvl: {nextLevel}");
                if (level >= nextLevel || progression.ExclusiveProgression != null && state.SelectedClass != progression.ExclusiveProgression)
                    return false;
                if (!progression.GiveFeaturesForPreviousLevels)
                    level = nextLevel - 1;
                for (var lvl = level + 1; lvl <= nextLevel; ++lvl) {
                    //                    if (!AllowProceed(progression)) break;
                    var levelEntry = progressionData.GetLevelEntry(lvl);
                    //Mod.Debug($"    LevelUpHelper_UpdateProgression_Patch - {string.Join(", ", levelEntry.Features.Select(f => f.name.yellow()))}");

                    LevelUpHelper.AddFeaturesFromProgression(state, unit, levelEntry.Features, (FeatureSource)progression, lvl);
                }
                return false;
            }
            private static bool AllowProceed(BlueprintProgression progression) {
                // SpellSpecializationProgression << shouldn't be applied more than once per character level
                if (!Main.Enabled || Main.multiclassMod == null) return false;
                return Main.multiclassMod.UpdatedProgressions.Add(progression);
                // TODO - what is the following and does it still matter?
                // || progression.AssetGuid != "fe9220cdc16e5f444a84d85d5fa8e3d5";
            }
        }
#endif

        // Do not proceed the spell selection if the caster level was not changed
        [HarmonyPatch(typeof(ApplySpellbook))]
        private static class ApplySpellbookPatch {
            [HarmonyPatch(nameof(ApplySpellbook.Apply), new Type[] { typeof(LevelUpState), typeof(UnitDescriptor) })]
            [HarmonyPrefix]
#if false
            public static bool Prefix(LevelUpState state, UnitDescriptor unit) {
                if (!settings.toggleMulticlass) return true;

                if (state.SelectedClass == null) {
                    return false;
                }
                var component1 = state.SelectedClass.GetComponent<SkipLevelsForSpellProgression>();
                if (component1 != null && component1.Levels.Contains(state.NextClassLevel)) {
                    return false;
                }

                var classData = unit.Progression.GetClassData(state.SelectedClass);
                if (classData?.Spellbook == null) {
                    return false;
                }

                var spellbook1 = unit.DemandSpellbook(classData.Spellbook);
                if (state.SelectedClass.Spellbook && state.SelectedClass.Spellbook != classData.Spellbook) {
                    var spellbook2 = unit.Spellbooks.FirstOrDefault(s => s.Blueprint == state.SelectedClass.Spellbook);
                    if (spellbook2 != null) {
                        foreach (var allKnownSpell in spellbook2.GetAllKnownSpells()) {
                            spellbook1.AddKnown(allKnownSpell.SpellLevel, allKnownSpell.Blueprint);
                        }

                        unit.DeleteSpellbook(state.SelectedClass.Spellbook);
                    }
                }

                var casterLevelAfter = CasterHelpers.GetRealCasterLevel(unit, spellbook1.Blueprint); // Calculates based on progression which includes class selected in level up screen
                spellbook1.AddLevelFromClass(classData.CharacterClass); // This only adds one class at a time and will only ever increase by 1 or 2
                var isNewSpellCaster = (spellbook1.IsStandaloneMythic && casterLevelAfter == 2) || casterLevelAfter == 1;
                var spellSelectionData = state.DemandSpellSelection(spellbook1.Blueprint, spellbook1.Blueprint.SpellList);
                if (spellbook1.Blueprint.SpellsKnown != null) {
                    for (var index = 0; index <= 10; ++index) {
                        var spellsKnown = spellbook1.Blueprint.SpellsKnown;
                        var expectedCount = spellsKnown.GetCount(casterLevelAfter, index);
                        var actual = CasterHelpers.GetActualSpellsLearnedForClass(unit, spellbook1, index);
                        int learnabl = spellbook1.GetSpellsLearnableOfLevel(index).Count();
                        int spelladd = Math.Max(0, Math.Min(expectedCount - actual, learnabl));
#if DEBUG
                        Mod.Trace($"Spellbook {spellbook1.Blueprint.Name}: Granting {spelladd} spells of spell level:{index} based on expected={expectedCount}, actual={actual}, learnable={learnabl}");
#endif
                        spellSelectionData.SetLevelSpells(index, spelladd);
                    }
                }
                var maxSpellLevel = spellbook1.MaxSpellLevel;
                if (spellbook1.Blueprint.SpellsPerLevel > 0) {
                    if (isNewSpellCaster) {
                        spellSelectionData.SetExtraSpells(0, maxSpellLevel);
                        spellSelectionData.ExtraByStat = true;
                        spellSelectionData.UpdateMaxLevelSpells(unit);
                    }
                    else {
                        spellSelectionData.SetExtraSpells(spellbook1.Blueprint.SpellsPerLevel, maxSpellLevel);
                    }
                }
                foreach (var component2 in spellbook1.Blueprint.GetComponents<AddCustomSpells>()) {
                    ApplySpellbook.TryApplyCustomSpells(spellbook1, component2, state, unit);
                }

                return false;
            }
#endif
            public static bool Apply(LevelUpState state, UnitDescriptor unit) {
                if (!settings.toggleMulticlass) return true;

                if (state.SelectedClass == null)
                    return false;
                var component1 = state.SelectedClass.GetComponent<SkipLevelsForSpellProgression>();
                if (component1 != null && component1.Levels.Contains(state.NextClassLevel))
                    return false;
                var classData = unit.Progression.GetClassData(state.SelectedClass);
                if (classData == null || classData.Spellbook == null)
                    return false;
                Mod.Debug($"ApplySpellbook.Apply".Orange());
                var spellbook1 = unit.DemandSpellbook(classData.Spellbook);
                if ((bool)(SimpleBlueprint)state.SelectedClass.Spellbook && state.SelectedClass.Spellbook != classData.Spellbook) {
                    var spellbook2 = unit.Spellbooks.FirstOrDefault(s => s.Blueprint == state.SelectedClass.Spellbook);
                    if (spellbook2 != null) {
                        foreach (var allKnownSpell in spellbook2.GetAllKnownSpells())
                            spellbook1.AddKnown(allKnownSpell.SpellLevel, allKnownSpell.Blueprint);
                        unit.DeleteSpellbook(state.SelectedClass.Spellbook);
                    }
                }
                var casterLevelAfter = CasterHelpers.GetRealCasterLevel(unit, spellbook1.Blueprint); // Calculates based on progression which includes class selected in level up screen
                spellbook1.AddLevelFromClass(classData.CharacterClass); // This only adds one class at a time and will only ever increase by 1 or 2
                var isNewSpellCaster = (spellbook1.IsStandaloneMythic && casterLevelAfter == 2) || casterLevelAfter == 1;
                var classLevel1 = classData.CharacterClass.IsMythic ? spellbook1.CasterLevel : spellbook1.BaseLevel;
                var classLevel2 = classData.CharacterClass.IsMythic ? spellbook1.CasterLevel : spellbook1.BaseLevel;
                Mod.Debug($"classLevel1: {classLevel1} classLevel2: {classLevel2} casterLevelAfter:{casterLevelAfter}");
                var spellSelectionData = state.DemandSpellSelection(spellbook1.Blueprint, spellbook1.Blueprint.SpellList);
                if (spellbook1.Blueprint.SpellsKnown != null) {
                    var blueprintSpellsTable = spellbook1.Blueprint.SpellsKnown;
                    if (classData.CharacterClass.IsMythic
                        && unit.Facts.Get((Func<UnitFact, bool>)(x => x.Blueprint is BlueprintFeatureSelectMythicSpellbook))
                               ?.Blueprint is BlueprintFeatureSelectMythicSpellbook blueprint2) {
                        blueprintSpellsTable = blueprint2.SpellKnownForSpontaneous;
                        if (blueprintSpellsTable != null) {
                            classLevel1 = spellbook1.MythicLevel - 1;
                            classLevel2 = spellbook1.MythicLevel;
                        } else {
                            PFLog.Default.Error("Mythic Spellbook {0} doesn't contains SpellKnownForSpontaneous table!",
                                                blueprint2);
                        }
                    }
                    for (var index = 0; index <= 10; ++index) {
                        var spellsKnown = spellbook1.Blueprint.SpellsKnown;
                        var expectedCount = spellsKnown.GetCount(casterLevelAfter, index);
                        var actual = CasterHelpers.GetActualSpellsLearnedForClass(unit, spellbook1, index);
                        int learnabl = spellbook1.GetSpellsLearnableOfLevel(index).Count();
                        int spelladd = Math.Max(0, Math.Min(expectedCount - actual, learnabl));
#if DEBUG
                        Mod.Trace($"Spellbook {spellbook1.Blueprint.Name}: Granting {spelladd} spells of spell level:{index} based on expected={expectedCount}, actual={actual}, learnable={learnabl}");
#endif
                        spellSelectionData.SetLevelSpells(index, spelladd);
                    }
                }
                var maxSpellLevel = spellbook1.MaxSpellLevel;
                if (spellbook1.Blueprint.SpellsPerLevel > 0) {
                    if (isNewSpellCaster) {
                        spellSelectionData.SetExtraSpells(0, maxSpellLevel);
                        spellSelectionData.ExtraByStat = true;
                        spellSelectionData.UpdateMaxLevelSpells(unit);
                    } else {
                        spellSelectionData.SetExtraSpells(spellbook1.Blueprint.SpellsPerLevel, maxSpellLevel);
                    }
                }

                foreach (var component2 in spellbook1.Blueprint.GetComponents<AddCustomSpells>())
                    ApplySpellbook.TryApplyCustomSpells(spellbook1, component2, state, unit);
                return false;
            }
        }


        // Fixed a vanilla PFK bug that caused dragon bloodline to be displayed in Magus' feats tree
        [HarmonyPatch(typeof(ApplyClassMechanics), nameof(ApplyClassMechanics.ApplyProgressions))]
        private static class ApplyClassMechanics_ApplyProgressions_Patch {
            public static bool Prefix(LevelUpState state, UnitDescriptor unit) {
                if (!settings.toggleMulticlass) return true;
                Mod.Debug($"ApplyClassMechanics_ApplyProgressions_Patch - {unit.CharacterName.Orange()} - class: {state.SelectedClass} nextLevel: {state.NextClassLevel}");
                BlueprintCharacterClass blueprintCharacterClass = null;
                if (unit.TryGetPartyMemberForLevelUpVersion(out var ch)
                    && ch.TryGetClass(state.SelectedClass, out var cl)
                    && state.NextClassLevel >= cl.Level
                    )
                    blueprintCharacterClass = state.SelectedClass;
                //var blueprintCharacterClass = state.NextClassLevel <= 1 ? state.SelectedClass : (BlueprintCharacterClass)null;
                var features = unit.Progression.Features.Enumerable;
                var progressions = features.Select(f => f.Blueprint).OfType<BlueprintProgression>().ToList();  // this ToList is important because it prevents mutation exceptions
                foreach (var blueprintProgression in progressions) {
                    var p = blueprintProgression;
                    Mod.Debug($"    prog: {p.name.Yellow()}");
                    if (blueprintCharacterClass != null
                        // && p.Classes.Contains<BlueprintCharacterClass>(blueprintCharacterClass)) 
                        && p.IsChildProgressionOf(unit, blueprintCharacterClass) // Mod Line replacing above
                        ) {
                        var feature = unit.Progression.Features.Enumerable.FirstItem(f => f.Blueprint == p);
                        feature?.SetSource(blueprintCharacterClass, state.NextClassLevel);
                        Mod.Debug($"    feature: {feature.Name.Cyan()} - levlel: {state.NextClassLevel}");
                        //feature?.SetSource(blueprintCharacterClass, 1);
                    }
                    LevelUpHelper.UpdateProgression(state, unit, p);
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(UnitHelper), nameof(UnitHelper.CopyInternal))]
        private static class UnitProgressionData_CopyFrom_Patch {
            private static void Postfix(UnitEntityData unit, UnitEntityData __result) {
                if (!settings.toggleMulticlass) return;
                // When upgrading, this method will be used to copy a UnitEntityData, which involves copying UnitProgressionData
                // By default, the CharacterLevel of the copied UnitProgressionData is equal to the sum of all non-mythical class levels
                //  If the character level is not equal to this default value, there will be problems (for example, when it is lower than the default value, you may not be able to upgrade until you reach level 20, because the sum of non-mythical class levels has exceeded 20 in advance)
                // Fix this.

                var UnitProgressionData_CharacterLevel = AccessTools.Property(typeof(UnitProgressionData), nameof(UnitProgressionData.CharacterLevel));
                Mod.Trace($"UnitProgressionData_CopyFrom_Patch - {unit.CharacterName.Orange()} - {UnitProgressionData_CharacterLevel}");

                UnitProgressionData_CharacterLevel.SetValue(__result.Descriptor.Progression, unit.Descriptor.Progression.CharacterLevel);
                __result.Descriptor.Progression.MythicLevel = unit.Descriptor.Progression.MythicLevel;
            }
        }
    }
}
