﻿// Copyright < 2021 > Narria (github user Cabarius) - License: MIT
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using ModKit;
using ModKit.DataViewer;
using ModKit.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ModKit.UI;
using static ToyBox.BlueprintExtensions;
using ToyBox.BagOfPatches;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;

namespace ToyBox {
    public class FactsEditor {
        private static Settings Settings => Main.Settings;
        private static bool _showTree = false;
        private static readonly int repeatCount = 1;
        private static readonly FeaturesTreeEditor treeEditor = new();

        private static readonly Dictionary<UnitEntityData, Browser<BlueprintFeature, Feature>> FeatureBrowserDict = new();
        private static readonly Dictionary<UnitEntityData, Browser<BlueprintBuff, Buff>> BuffBrowserDict = new();
        private static readonly Dictionary<UnitEntityData, Browser<BlueprintUnitFact, UnitFact>> AbilityBrowserDict = new();
        private static readonly Browser<BlueprintFeature, FeatureSelectionEntry> FeatureSelectionBrowser = new(Mod.ModKitSettings.searchAsYouType) { IsDetailBrowser = true };
        private static readonly Browser<IFeatureSelectionItem, IFeatureSelectionItem> ParameterizedFeatureBrowser = new(Mod.ModKitSettings.searchAsYouType) { IsDetailBrowser = true };
        public static void BlueprintRowGUI<Item, Definition>(Browser<Definition, Item> browser,
                                                             Item feature,
                                                             Definition blueprint,
                                                             UnitEntityData ch,
                                                             List<Action> todo
                ) where Definition : BlueprintScriptableObject, IUIDataProvider {
            var remainingWidth = ummWidth;
            // Indent
            remainingWidth -= 50;
            var titleWidth = (remainingWidth / (IsWide ? 3.5f : 4.0f)) - 100;
            remainingWidth -= titleWidth;
            string text;
            if (feature is AbilityData maybeSpell2 && maybeSpell2.Blueprint.IsSpell && maybeSpell2.MagicHackData != null) {
                text = maybeSpell2.MagicHackData.Name;
                if (text.IsNullOrEmpty()) text = maybeSpell2.MagicHackData.GetDefaultName();
            } else {
                text = GetTitle(blueprint);
            }
            text = text.MarkedSubstring(browser.SearchText);
            var titleKey = $"{blueprint.AssetGuid}";
            if (feature != null) {
                text = RichText.Bold(text.Cyan());
            }
            if (blueprint is BlueprintFeatureSelection featureSelection
                || blueprint is BlueprintParametrizedFeature parametrizedFeature
                ) {
                if (Browser.DetailToggle(text, blueprint, feature != null ? feature : blueprint, (int)titleWidth))
                    browser.ReloadData();
            } else
                Label(text, Width((int)titleWidth));

            var lastRect = GUILayoutUtility.GetLastRect();

            var mutatorLookup = BlueprintAction.ActionsForType(blueprint.GetType())
                .GroupBy(a => a.name).Select(g => g.FirstOrDefault())
                .ToDictionary(a => a.name, a => a);
            var add = mutatorLookup.GetValueOrDefault("Add".localize(), null);
            var remove = mutatorLookup.GetValueOrDefault("Remove".localize(), null);
            var decrease = mutatorLookup.GetValueOrDefault("<", null);
            var increase = mutatorLookup.GetValueOrDefault(">", null);

            mutatorLookup.Remove("Add".localize());
            mutatorLookup.Remove("Remove".localize());
            mutatorLookup.Remove("<");
            mutatorLookup.Remove(">");
            if (feature != null) {
                bool canDecrease = decrease?.canPerform(blueprint, ch) ?? false;
                bool canIncrease = increase?.canPerform(blueprint, ch) ?? false;
                if ((canDecrease || canIncrease) && feature is UnitFact rankFeature) {
                    var v = rankFeature.GetRank();
                    decrease.BlueprintActionButton(ch, blueprint, () => todo.Add(() => decrease!.action(blueprint, ch, repeatCount)), 60);
                    Space(10f);
                    Label(RichText.Bold(RichText.Orange($"{v}")), Width(30));
                    increase.BlueprintActionButton(ch, blueprint, () => todo.Add(() => increase!.action(blueprint, ch, repeatCount)), 60);
                    Space(17);
                    remainingWidth -= 190;
                } else if (feature is AbilityData maybeSpell && maybeSpell.Blueprint.IsSpell) {
                    var sb = maybeSpell.Spellbook;
                    var level = sb.GetSpellLevel(maybeSpell);
                    if (level > 0) {
                        UI.ActionButton("<", () => {
                            todo.Add(() => {
                                if (maybeSpell.MagicHackData == null && maybeSpell.MetamagicData == null) {
                                    sb.RemoveSpell(maybeSpell.Blueprint);
                                    sb.AddKnown(level - 1, maybeSpell.Blueprint);
                                } else {
                                    sb.RemoveCustomSpell(maybeSpell);
                                    if (maybeSpell.MagicHackData != null) {
                                        maybeSpell.MagicHackData.SpellLevel = level - 1;
                                    } else {
                                        maybeSpell.SpellLevelInSpellbook = level - 1;
                                    }
                                    sb.AddCustomSpell(maybeSpell);
                                }
                                browser.ResetSearch();
                            });
                        }, Width(60));
                    } else {
                        Space(60);
                    }
                    Space(10f);
                    Label(RichText.Bold(RichText.Orange($"{level}")), Width(30));
                    if (level < 10) {
                        UI.ActionButton(">", () => {
                            todo.Add(() => {
                                if (maybeSpell.MagicHackData == null && maybeSpell.MetamagicData == null) {
                                    sb.RemoveSpell(maybeSpell.Blueprint);
                                    sb.AddKnown(level + 1, maybeSpell.Blueprint);
                                } else {
                                    sb.RemoveCustomSpell(maybeSpell);
                                    if (maybeSpell.MagicHackData != null) {
                                        maybeSpell.MagicHackData.SpellLevel = level + 1;
                                    } else {
                                        maybeSpell.SpellLevelInSpellbook = level + 1;
                                    }
                                    sb.AddCustomSpell(maybeSpell);
                                }
                                browser.ResetSearch();
                            });
                        }, Width(60));
                    } else {
                        Space(60);
                    }
                    Space(17);
                    remainingWidth -= 190;
                } else {
                    Space(190);
                    remainingWidth -= 190;
                }
            } else {
                Space(190);
                remainingWidth -= 190;
            }
            var canAdd = add?.canPerform(blueprint, ch) ?? false;
            var canRemove = remove?.canPerform(blueprint, ch) ?? false;
            if (canRemove) {
                remove.BlueprintActionButton(ch, blueprint, () => todo.Add(() => { browser.needsReloadData = !browser.ShowAll; remove.action(blueprint, ch, repeatCount); }), 150);
            }
            if (canAdd) {
                add.BlueprintActionButton(ch, blueprint, () => todo.Add(() => { add.action(blueprint, ch, repeatCount); }), 150);
            }
            remainingWidth -= 178;
            Space(20); remainingWidth -= 20;
            ReflectionTreeView.DetailToggle("", blueprint, feature != null ? feature : blueprint, 0);
            using (VerticalScope(Width(remainingWidth - 100))) {
                try {
                    if (Settings.showAssetIDs)
                        ClipboardLabel(blueprint.AssetGuid.ToString(), AutoWidth());
                    Label(RichText.Green(blueprint.Description.StripHTML().MarkedSubstring(browser.SearchText)), Width(remainingWidth - 100));
                } catch (Exception e) {
                    Mod.Warn($"Error in blueprint: {blueprint.AssetGuid}");
                    Mod.Warn($"         name: {blueprint.name}");
                    Mod.Error(e);
                }
            }
        }
        public static void BlueprintDetailGUI<Item, Definition, K, V>(Definition blueprint, Item feature, UnitEntityData ch, Browser<K, V> browser)
            where Item : UnitFact
            where Definition : BlueprintUnitFact {
            if (ch == null) {
                FeatureSelectionBrowser.ShowAll = true;
                ParameterizedFeatureBrowser.ShowAll = true;
            }
            switch (blueprint) {
                case BlueprintFeatureSelection featureSelection:
                    Browser.OnDetailGUI(blueprint, bp => {
                        FeatureSelectionBrowser.needsReloadData |= browser.needsReloadData;
                        FeatureSelectionBrowser.OnGUI(
                        ch?.FeatureSelectionEntries(featureSelection),
                        () => featureSelection.AllFeatures.OrderBy(f => f.Name),
                        e => e.feature,
                        f => $"{GetSearchKey(f)} " + (Settings.searchDescriptions ? f.Description : ""),
                        f => (new[] { GetTitle(f) }),
                        null,
                        (BlueprintFeature f, FeatureSelectionEntry selectionEntry) => {
                            bool characterHasEntry = ch?.HasFeatureSelection(featureSelection, f) ?? false;
                            var title = GetTitle(f).MarkedSubstring(FeatureSelectionBrowser.SearchText);
                            if (characterHasEntry) title = RichText.Bold(title.Cyan());
                            var titleWidth = (ummWidth / (IsWide ? 3.5f : 4.0f)) - 200;
                            Label(title, Width(titleWidth));
                            78.space();
                            if (selectionEntry != null) {
                                var level = selectionEntry.level;
                                Space(-25);
                                using (VerticalScope(125)) {
                                    using (HorizontalScope(125)) {
                                        if (ch != null) {
                                            Label("sel lvl", 50.width());
                                            if (ValueAdjuster(ref level, 1, 0, 40, false)) {
                                                ch.RemoveFeatureSelection(featureSelection,
                                                    selectionEntry.data,
                                                    f);
                                                ch.AddFeatureSelection(featureSelection, f, level);
                                                FeatureSelectionBrowser.ReloadData();
                                                browser.ReloadData();
                                            }
                                        }
                                    }
                                }
                                20.space();
                                Label($"{selectionEntry.data.Source.Blueprint.GetDisplayName()}",
                                      250.width());
                            } else
                                354.space();
                            if (ch != null) {
                                if (characterHasEntry)
                                    ActionButton("Remove".localize(),
                                                 () => {
                                                     if (selectionEntry == null) return;
                                                     ch.RemoveFeatureSelection(featureSelection, selectionEntry.data, f);
                                                     bool isLast = true;
                                                     foreach (var selection in ch.Progression.Selections) {
                                                         if (selection.Key == featureSelection) {
                                                             foreach (var keyValuePair in selection.Value.SelectionsByLevel.ToList()) {
                                                                 foreach (var feat in keyValuePair.Value.ToList()) {
                                                                     isLast = false;
                                                                 }
                                                             }
                                                         }
                                                     }
                                                     if (isLast) {
                                                         ch.Progression.Features.RemoveFact(featureSelection);
                                                     }
                                                     FeatureSelectionBrowser.needsReloadData = true;
                                                     browser.needsReloadData = true;
                                                 },
                                                     150.width());
                                else
                                    ActionButton("Add".localize(),
                                                 () => {
                                                     bool needsAddSelection = true;
                                                     var progression = ch?.Descriptor()?.Progression;
                                                     if (progression == null) needsAddSelection = false;
                                                     if (progression.Features.HasFact(featureSelection)) needsAddSelection = false;
                                                     var selections = ch?.Descriptor?.Progression.Selections;
                                                     if (needsAddSelection) {
                                                         foreach (var selection in selections) {
                                                             if (selection.Key == featureSelection) {
                                                                 foreach (var keyValuePair in selection.Value.SelectionsByLevel.ToList()) {
                                                                     foreach (var feat in keyValuePair.Value.ToList()) {
                                                                         needsAddSelection = false;
                                                                     }
                                                                 }
                                                             }
                                                         }
                                                     }
                                                     if (needsAddSelection) {
                                                         var source = new FeatureSource();
                                                         ch?.Descriptor()?.Progression.Features.AddFeature(featureSelection).SetSource(source, 1);
                                                     }
                                                     ch.AddFeatureSelection(featureSelection, f);
                                                     FeatureSelectionBrowser.needsReloadData = true;
                                                     browser.needsReloadData = true;
                                                 },
                                                 150.width());
                                15.space();
                            }
                            Label(RichText.Green(f.GetDescription().StripHTML().MarkedSubstring(FeatureSelectionBrowser.SearchText)));
                        },
                        null,
                        100);
                    });
                    break;
                case BlueprintParametrizedFeature parametrizedFeature:
                    Browser.OnDetailGUI(blueprint, (Action<object>)(bp => {
                        ParameterizedFeatureBrowser.needsReloadData |= browser.needsReloadData;
                        ParameterizedFeatureBrowser.OnGUI(
                          ch?.ParameterizedFeatureItems(parametrizedFeature),
                          () => parametrizedFeature.Items.OrderBy(i => i.Name),
                          i => i,
                          i => $"{i.Name} " + (Settings.searchDescriptions ? i.Param?.Blueprint?.GetDescription() : ""),
                          i => (new[] { i.Name
    }),
                          null,
                          (Action<IFeatureSelectionItem, IFeatureSelectionItem>)((IFeatureSelectionItem def, IFeatureSelectionItem item) => {
                              bool characterHasEntry = ch?.HasParameterizedFeatureItem(parametrizedFeature, def) ?? false;
                              var title = def.Name.MarkedSubstring(ParameterizedFeatureBrowser.SearchText);
                              // make the title cyan if we have the item
                              if (characterHasEntry) title = RichText.Bold(title.Cyan());

                              var titleWidth = (ummWidth / (IsWide ? 3.5f : 4.0f));
                              Label(title, Width(titleWidth));
                              25.space();
                              if (ch != null) {
                                  if (characterHasEntry)
                                      ActionButton("Remove".localize(), () => {
                                          ch.RemoveParameterizedFeatureItem(parametrizedFeature, def);
                                          ParameterizedFeatureBrowser.needsReloadData = true;
                                          browser.needsReloadData = true;
                                      }, 150.width());
                                  else
                                      ActionButton("Add".localize(), () => {
                                          ch.AddParameterizedFeatureItem(parametrizedFeature, def);
                                          ParameterizedFeatureBrowser.needsReloadData = true;
                                          browser.needsReloadData = true;
                                      }, 150.width());
                                  15.space();
                              }
                              Label((string)(def.Param?.Blueprint?.GetDescription().StripHTML().MarkedSubstring(ParameterizedFeatureBrowser.SearchText).Green()));
                          }), null, 100);
                    }));
                    break;
            }
        }
        public static List<Action> OnGUI<Item, Definition>(UnitEntityData ch, Browser<Definition, Item> browser, List<Item> fact, string name)
            where Item : UnitFact
            where Definition : BlueprintUnitFact {
            bool updateTree = false;
            List<Action> todo = new();
            if (_showTree) {
                using (HorizontalScope()) {
                    Space(670);
                    Toggle("Show Tree".localize(), ref _showTree, Width(250));
                }
                treeEditor.OnGUI(ch, updateTree);
            } else {
                browser.OnGUI(
                    fact,
                    BlueprintLoader.Shared.GetBlueprintsOfType<Definition>,
                    (feature) => (Definition)feature.Blueprint,
                    (blueprint) => $"{GetSearchKey(blueprint)}" + (Settings.searchDescriptions ? $"{blueprint.Description}" : ""),
                    blueprint => new[] { GetSortKey(blueprint) },
                    () => {
                        using (HorizontalScope()) {
                            var reloadData = false;
                            Toggle("Show GUIDs".localize(), ref Main.Settings.showAssetIDs);
                            20.space();
                            reloadData |= Toggle("Show Internal Names".localize(), ref Settings.showDisplayAndInternalNames);
                            20.space();
                            updateTree |= Toggle("Show Tree".localize(), ref _showTree);
                            20.space();
                            //Toggle("Show Inspector", ref Settings.factEditorShowInspector);
                            //20.space();
                            reloadData |= Toggle("Search Descriptions".localize(), ref Settings.searchDescriptions);
                            if (reloadData) {
                                browser.ResetSearch();
                                FeatureSelectionBrowser.ResetSearch();
                                ParameterizedFeatureBrowser.ResetSearch();
                            }
                        }
                    },
                    (blueprint, feature) => BlueprintRowGUI(browser, feature, blueprint, ch, todo),
                    (blueprint, feature) => {
                        ReflectionTreeView.OnDetailGUI(blueprint);
                        BlueprintDetailGUI(blueprint, feature, ch, browser);
                    }, 50, false, true, 100, 300, "", true);
            }
            return todo;
        }
        public static List<Action> OnGUI(UnitEntityData ch, List<Feature> feature) {
            var featureBrowser = FeatureBrowserDict.GetValueOrDefault(ch, null);
            if (featureBrowser == null) {
                featureBrowser = new Browser<BlueprintFeature, Feature>(Mod.ModKitSettings.searchAsYouType, true) { };
                FeatureBrowserDict[ch] = featureBrowser;
            }
            return OnGUI(ch, featureBrowser, feature, "Features");
        }
        public static List<Action> OnGUI(UnitEntityData ch, List<Buff> buff) {
            var buffBrowser = BuffBrowserDict.GetValueOrDefault(ch, null);
            if (buffBrowser == null) {
                buffBrowser = new Browser<BlueprintBuff, Buff>(Mod.ModKitSettings.searchAsYouType, true);
                BuffBrowserDict[ch] = buffBrowser;
            }
            return OnGUI(ch, buffBrowser, buff, "Buffs");
        }
        public static List<Action> OnGUI(UnitEntityData ch, List<Ability> ability, List<ActivatableAbility> activatable) {
            var abilityBrowser = AbilityBrowserDict.GetValueOrDefault(ch, null);
            var combined = new List<UnitFact>();
            if (abilityBrowser == null) {
                abilityBrowser = new Browser<BlueprintUnitFact, UnitFact>(Mod.ModKitSettings.searchAsYouType, true);
                AbilityBrowserDict[ch] = abilityBrowser;
            }
            combined.AddRange(ability);
            combined.AddRange(activatable);
            return OnGUI(ch, abilityBrowser, combined, "Abilities");
        }
    }
}
