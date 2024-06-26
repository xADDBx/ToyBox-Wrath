﻿// Copyright < 2021 > Narria (github user Cabarius) - License: MIT

using Kingmaker;
using Kingmaker.AI.Blueprints.Considerations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Armies.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Crusade.GlobalMagic;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using ModKit;
using System;
using System.Collections.Generic;
using System.Linq;
using ToyBox.BagOfPatches;

namespace ToyBox {
    public static partial class BlueprintActions {
        private static Dictionary<BlueprintParametrizedFeature, IFeatureSelectionItem[]> parametrizedSelectionItems = new();
        public static IFeatureSelectionItem ParametrizedSelectionItems(this BlueprintParametrizedFeature feature, int index) {
            if (parametrizedSelectionItems.TryGetValue(feature, out var value)) return index < value.Length ? value[index] : null;
            var tmp = feature?.Items;
            if (tmp == null) return null;
            value = tmp.OrderBy(x => x.Name).ToArray();
            parametrizedSelectionItems[feature] = value;
            return index < value.Length ? value[index] : null;
        }

        public static void InitializeActionsWrath() {
            var flags = Game.Instance.Player.UnlockableFlags;

            // Features
            BlueprintAction.Register<BlueprintFeature>("Add".localize(),
                                                       (bp, ch, n, index) => ch.Progression.Features.AddFeature(bp),
                                                       (bp, ch, index) => !ch.Progression.Features.HasFact(bp));

            BlueprintAction.Register<BlueprintFeature>("Remove".localize(),
                                                       (bp, ch, n, index) => ch.Progression.Features.RemoveFact(bp),
                                                       (bp, ch, index) => ch.Progression.Features.HasFact(bp));
            BlueprintAction.Register<BlueprintFeature>("<".localize(),
                                           (bp, ch, n, index) => ch.Progression.Features.GetFact(bp)?.RemoveRank(),
                                           (bp, ch, index) => {
                                               var feature = ch.Progression.Features.GetFact(bp);
                                               return feature?.GetRank() > 1;
                                           });

            BlueprintAction.Register<BlueprintFeature>(">".localize(),
                                                       (bp, ch, n, index) => ch.Progression.Features.GetFact(bp)?.AddRank(),
                                                       (bp, ch, index) => {
                                                           var feature = ch.Progression.Features.GetFact(bp);
                                                           return feature != null && feature.GetRank() < feature.Blueprint.Ranks;
                                                       });
            // Paramaterized Feature
            BlueprintAction.Register<BlueprintParametrizedFeature>("Add".localize(),
                 (bp, ch, n, index) => {
                     int itemIndex;
                     if (Main.tabs[Main.Settings.selectedTab].action == SearchAndPick.OnGUI) {
                         itemIndex = SearchAndPick.ParamSelected[index];
                     } else {
                         itemIndex = BlueprintListUI.ParamSelected[index];
                     }
                     var value = bp.ParametrizedSelectionItems(itemIndex)?.Param;
                     ch?.Descriptor?.AddFact<UnitFact>(bp, null, value);
                 },
                (bp, ch, index) => {
                    int itemIndex;
                    if (Main.tabs[Main.Settings.selectedTab].action == SearchAndPick.OnGUI) {
                        itemIndex = SearchAndPick.ParamSelected[index];
                    } else {
                        itemIndex = BlueprintListUI.ParamSelected[index];
                    }
                    var value = bp.ParametrizedSelectionItems(itemIndex)?.Param;
                    var existing = ch?.Descriptor?.Unit?.Facts?.Get<Feature>(i => i.Blueprint == bp && i.Param == value);
                    return existing == null;
                });
            BlueprintAction.Register<BlueprintParametrizedFeature>("Remove".localize(),
                (bp, ch, n, index) => {
                    int itemIndex;
                    if (Main.tabs[Main.Settings.selectedTab].action == SearchAndPick.OnGUI) {
                        itemIndex = SearchAndPick.ParamSelected[index];
                    } else {
                        itemIndex = BlueprintListUI.ParamSelected[index];
                    }
                    var value = bp.ParametrizedSelectionItems(itemIndex)?.Param;
                    var fact = ch.Descriptor()?.Unit?.Facts?.Get<Feature>(i => i.Blueprint == bp && i.Param == value);
                    ch?.Progression?.Features?.RemoveFact(fact);
                },
                (bp, ch, index) => {
                    if ((bp.Items?.Count() ?? 0) == 0) return false;
                    int itemIndex;
                    if (Main.tabs[Main.Settings.selectedTab].action == SearchAndPick.OnGUI) {
                        itemIndex = SearchAndPick.ParamSelected[index];
                    } else {
                        itemIndex = BlueprintListUI.ParamSelected[index];
                    }
                    var value = bp.ParametrizedSelectionItems(itemIndex)?.Param;
                    var existing = ch?.Descriptor?.Unit?.Facts?.Get<Feature>(i => i.Blueprint == bp && i.Param == value);
                    return existing != null;
                });
            // Feature Selection
            BlueprintAction.Register<BlueprintFeatureSelection>("Add".localize(),
                 (bp, ch, n, index) => {
                     int itemIndex;
                     if (Main.tabs[Main.Settings.selectedTab].action == SearchAndPick.OnGUI) {
                         itemIndex = SearchAndPick.ParamSelected[index];
                     } else {
                         itemIndex = BlueprintListUI.ParamSelected[index];
                     }
                     var source = new FeatureSource();
                     ch?.Descriptor()?.Progression.Features.AddFeature(bp).SetSource(source, 1);
                     var value = bp.FeatureSelectionItems(itemIndex);
                     ch.AddFeatureSelection(bp, value, 1);
                 },
                (bp, ch, index) => {
                    var progression = ch?.Descriptor()?.Progression;
                    if (progression == null) return false;
                    if (progression.Features.HasFact(bp)) return false;
                    var selections = ch?.Descriptor?.Progression.Selections;
                    foreach (var selection in selections) {
                        if (selection.Key == bp) {
                            foreach (var keyValuePair in selection.Value.SelectionsByLevel.ToList()) {
                                foreach (var feat in keyValuePair.Value.ToList()) {
                                    return false;
                                }
                            }
                        }
                    }
                    return true;
                });
            BlueprintAction.Register<BlueprintFeatureSelection>("Rem. All".localize(),
                (bp, ch, n, index) => {
                    var progression = ch?.Descriptor?.Progression;
                    int itemIndex;
                    if (Main.tabs[Main.Settings.selectedTab].action == SearchAndPick.OnGUI) {
                        itemIndex = SearchAndPick.ParamSelected[index];
                    } else {
                        itemIndex = BlueprintListUI.ParamSelected[index];
                    }
                    var value = bp.FeatureSelectionItems(itemIndex);
                    //Feature fact = progression.Features.GetFact(bp);
                    var fact = ch.Descriptor()?.Unit?.Facts?.Get<Feature>(i => i.Blueprint == bp && i.Param == value);
                    var selections = ch?.Descriptor?.Progression.Selections;
                    foreach (var selection in selections) {
                        if (selection.Key == bp) {
                            foreach (var keyValuePair in selection.Value.SelectionsByLevel.ToList()) {
                                foreach (var feat in keyValuePair.Value.ToList()) {
                                    ch.RemoveFeatureSelection(bp, selection.Value, feat);
                                }
                            }
                        }
                    }
                    progression.Features.RemoveFact(bp);
                },
                (bp, ch, index) => {
                    var progression = ch?.Descriptor()?.Progression;
                    if (progression == null) return false;
                    if (progression.Features.HasFact(bp)) return true;
                    var selections = ch?.Descriptor?.Progression.Selections;
                    foreach (var selection in selections) {
                        if (selection.Key == bp) {
                            foreach (var keyValuePair in selection.Value.SelectionsByLevel.ToList()) {
                                foreach (var feat in keyValuePair.Value.ToList()) {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                });

            // Facts
            BlueprintAction.Register<BlueprintUnitFact>("Add".localize(),
                                                       (bp, ch, n, index) => ch.AddFact(bp),
                                                       (bp, ch, index) => !ch.Facts.List.Select(f => f.Blueprint).Contains(bp));

            BlueprintAction.Register<BlueprintUnitFact>("Remove".localize(),
                                                       (bp, ch, n, index) => ch.RemoveFact(bp),
                                                       (bp, ch, index) => ch.Facts.List.Select(f => f.Blueprint).Contains(bp));

            //BlueprintAction.Register<BlueprintArchetype>(
            //    "Add",
            //    (bp, ch, n, index) => ch.Progression.AddArchetype(ch.Progression.Classes.First().CharacterClass, bp),
            //    (bp, ch, index) => ch.Progression.CanAddArchetype(ch.Progression.Classes.First().CharacterClass, bp)
            //    );
            //BlueprintAction.Register<BlueprintArchetype>("Remove",
            //    (bp, ch, n, index) => ch.Progression.AddArchetype(ch.Progression.Classes.First().CharacterClass, bp),
            //    (bp, ch, index) => ch.Progression.Classes.First().Archetypes.Contains(bp)
            //    );

            // Spellbooks
            BlueprintAction.Register<BlueprintSpellbook>("Add".localize(),
                                                         (bp, ch, n, index) => ch.Descriptor().DemandSpellbook(bp),
                                                         (bp, ch, index) => ch.Descriptor().Spellbooks.All(sb => sb.Blueprint != bp));

            BlueprintAction.Register<BlueprintSpellbook>("Remove".localize(),
                                                         (bp, ch, n, index) => ch.Descriptor().DeleteSpellbook(bp),
                                                         (bp, ch, index) => ch.Descriptor().Spellbooks.Any(sb => sb.Blueprint == bp));

            BlueprintAction.Register<BlueprintSpellbook>(">".localize(),
                                                         (bp, ch, n, index) => {
                                                             try {
                                                                 var spellbook = ch.Descriptor().Spellbooks.FirstOrDefault(sb => sb.Blueprint == bp);

                                                                 if (spellbook.IsMythic) {
                                                                     spellbook.AddMythicLevel();
                                                                 } else {
                                                                     spellbook.AddBaseLevel();
                                                                 }
                                                             } catch (Exception e) { Mod.Error(e); }
                                                         },
                                                         (bp, ch, index) => ch.Descriptor().Spellbooks.Any(sb => sb.Blueprint == bp && sb.CasterLevel < bp.MaxSpellLevel));
            // Abilities
            BlueprintAction.Register<BlueprintAbility>("Add".localize(),
                                                       (bp, ch, n, index) => ch.AddAbility(bp),
                                                       (bp, ch, index) => ch.CanAddAbility(bp));

            BlueprintAction.Register<BlueprintAbility>("At Will".localize(),
                                                       (bp, ch, n, index) => ch.AddSpellAsAbility(bp),
                                                       (bp, ch, index) => ch.CanAddSpellAsAbility(bp));

            BlueprintAction.Register<BlueprintAbility>("Remove".localize(),
                                                       (bp, ch, n, index) => ch.RemoveAbility(bp),
                                                       (bp, ch, index) => ch.HasAbility(bp));

            // Buffs
            BlueprintAction.Register<BlueprintBuff>("Add".localize(),
                                                    (bp, ch, n, index) => GameHelper.ApplyBuff(ch, bp),
                                                    (bp, ch, index) => !ch.Descriptor().Buffs.HasFact(bp));

            BlueprintAction.Register<BlueprintBuff>("Remove".localize(),
                                                    (bp, ch, n, index) => ch.Descriptor().RemoveFact(bp),
                                                    (bp, ch, index) => ch.Descriptor().Buffs.HasFact(bp));

            BlueprintAction.Register<BlueprintBuff>("<".localize(),
                                                    (bp, ch, n, index) => ch.Descriptor().Buffs.GetFact(bp)?.RemoveRank(),
                                                    (bp, ch, index) => {
                                                        var buff = ch.Descriptor().Buffs.GetFact(bp);
                                                        return buff?.GetRank() > 1;
                                                    });

            BlueprintAction.Register<BlueprintBuff>(">".localize(),
                                                    (bp, ch, n, index) => ch.Descriptor().Buffs.GetFact(bp)?.AddRank(),
                                                    (bp, ch, index) => {
                                                        var buff = ch.Descriptor().Buffs.GetFact(bp);
                                                        return buff != null && buff?.GetRank() < buff.Blueprint.Ranks - 1;
                                                    });
            // Kingdom Bufs
            BlueprintAction.Register<BlueprintKingdomBuff>("Add".localize(),
                                                       (bp, ch, n, index) => KingdomState.Instance?.AddBuff(bp, null, null, 0),
                                                       (bp, ch, index) => (KingdomState.Instance != null) && !KingdomState.Instance.ActiveBuffs.HasFact(bp));

            BlueprintAction.Register<BlueprintKingdomBuff>("Remove".localize(),
                                                       (bp, ch, n, index) => KingdomState.Instance?.ActiveBuffs.RemoveFact(bp),
                                                       (bp, ch, index) => (KingdomState.Instance != null) && KingdomState.Instance.ActiveBuffs.HasFact(bp));

            // GlobalSpells
            BlueprintAction.Register<BlueprintGlobalMagicSpell>("Add".localize(),
                                                       (bp, ch, n, index) => Game.Instance.Player.GlobalMapSpellsManager.AddSpell(bp),
                                                       (bp, ch, index) => !Game.Instance.Player.GlobalMapSpellsManager.m_SpellBook.HasItem(x => x.BlueprintGuid == bp.AssetGuid));

            BlueprintAction.Register<BlueprintGlobalMagicSpell>("Remove".localize(),
                                                                (bp, ch, n, index) => Game.Instance.Player.GlobalMapSpellsManager.RemoveSpell(bp),
                                                                (bp, ch, index) => Game.Instance.Player.GlobalMapSpellsManager.m_SpellBook.HasItem(x => x.BlueprintGuid == bp.AssetGuid));
            // Ability Resources

            BlueprintAction.Register<BlueprintAbilityResource>("Add".localize(),
                (bp, ch, n, index) => ch.Resources.Add(bp, true),
                (bp, ch, index) => !ch.Resources.ContainsResource(bp));

            BlueprintAction.Register<BlueprintAbilityResource>("Remove".localize(),
                (bp, ch, n, index) => ch.Resources.Remove(bp),
                (bp, ch, index) => ch.Resources.ContainsResource(bp));

            // Spellbooks


            // BlueprintActivatableAbility
            BlueprintAction.Register<BlueprintActivatableAbility>("Add".localize(),
                                                                  (bp, ch, n, index) => ch.Descriptor().AddFact(bp),
                                                                  (bp, ch, index) => !ch.Descriptor().HasFact(bp));

            BlueprintAction.Register<BlueprintActivatableAbility>("Remove".localize(),
                                                                  (bp, ch, n, index) => ch.Descriptor().RemoveFact(bp),
                                                                  (bp, ch, index) => ch.Descriptor().HasFact(bp));
            // Teleport
            BlueprintAction.Register<BlueprintGlobalMap>("Teleport".localize(), (map, ch, n, index) => Teleport.To(map));
            BlueprintAction.Register<BlueprintGlobalMapPoint>("Teleport".localize(), (globalMapPoint, ch, n, index) => Teleport.To(globalMapPoint));

            // Army
            BlueprintAction.Register<BlueprintArmyPreset>("Add Friendly".localize(), (bp, ch, n, l) => {
                Actions.CreateArmy(bp, true);
            });
            BlueprintAction.Register<BlueprintArmyPreset>("Add Hostile".localize(), (bp, ch, n, l) => {
                Actions.CreateArmy(bp, false);
            });

            // ArmyGeneral
            BlueprintAction.Register<BlueprintLeaderSkill>("Add".localize(),
                (bp, ch, n, l) => Actions.AddSkillToLeader(bp),
                (bp, ch, index) => Actions.LeaderSelected(bp) && !Actions.LeaderHasSkill(bp));

            // ArmyGeneral
            BlueprintAction.Register<BlueprintLeaderSkill>("Remove".localize(),
                (bp, ch, n, l) => Actions.RemoveSkillFromLeader(bp),
                (bp, ch, index) => Actions.LeaderSelected(bp) && Actions.LeaderHasSkill(bp));
        }
    }
}