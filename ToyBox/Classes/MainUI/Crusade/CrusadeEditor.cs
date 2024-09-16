using Kingmaker;
using Kingmaker.Armies;
using Kingmaker.Armies.State;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Globalmap.State;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using ModKit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ToyBox.classes.MainUI {
    public static class CrusadeEditor {
        public static void ResetGUI() { }
        public static Settings settings => Main.Settings;

        public static void OnGUI() {
            if (Game.Instance?.Player == null) return;
            var ks = KingdomState.Instance;
            if (ks == null) {
                UI.Label("You must unlock the crusade before you can access these toys.".localize().Yellow().Bold());
                return;
            }
            var moraleState = ks.MoraleState;
            UI.HStack("Morale".localize(), 1,
                () => {
                    UI.Toggle("Flags always green".localize(), ref settings.toggleCrusadeFlagsStayGreen);
                    KingdomCheats.AddMorale();
                },
                () => {
                    var value = moraleState.CurrentValue;
                    UI.Slider("Morale".localize(), ref value, moraleState.MinValue, moraleState.MaxValue, 1, "", UI.AutoWidth());
                    moraleState.CurrentValue = value;
                },
                () => {
                    var value = moraleState.MaxValue;
                    UI.Slider("Max Morale".localize(), ref value, -200, 200, 20, "", UI.AutoWidth());
                    moraleState.MaxValue = value;
                },
                () => {
                    var value = moraleState.MinValue;
                    UI.Slider("Min Morale".localize(), ref value, -200, 200, -100, "", UI.AutoWidth());
                    moraleState.MinValue = value;
                },
                () => { }
            );
            UI.Div(0, 25);
            UI.HStack("Kingdom".localize(), 1,
                () => {
                    UI.Label("increment".localize().Cyan(), UI.Width(325));
                    var increment = UI.IntTextField(ref settings.increment, null, UI.Width(150));
                    UI.Space(25);
                    UI.Label("Experimental".localize().Orange().Bold());
                },
                () => {
                    using (UI.VerticalScope()) {
                        UI.Space(15);
                        UI.Div(0, 0, 800);
                        using (UI.HorizontalScope()) {
                            UI.Label("Kingdom Stat".localize(), UI.Width(325));
                            UI.Label("Rank".localize(), UI.Width(150));
                            UI.Label("Experience".localize(), UI.Width(150));
                            UI.Label("Next Rank".localize(), UI.Width(150));
                        }
                        foreach (var kingdomStat in ks.Stats) {
                            var conditions = KingdomRoot.Instance.RankUps.Conditions[kingdomStat.Type];
                            using (UI.HorizontalScope()) {
                                var rank = kingdomStat.Rank;
                                var exp = kingdomStat.Value.ToString().Orange();
                                var required = conditions.GetRequiredStatValue(kingdomStat.Rank + 1).ToString().Cyan();
                                UI.ValueAdjuster(kingdomStat.Type.ToString().localize(), () => kingdomStat.Rank, v => kingdomStat.Rank = v, 1, 0, 8);
                                UI.Space(42);
                                UI.Label(exp, UI.Width(150));
                                UI.Label(required, UI.Width(150));
                                UI.Space(10);
                                UI.ActionButton("Gain ".localize() + $"{settings.increment}", () => {
                                    kingdomStat.Value += settings.increment;
                                }, UI.AutoWidth());
                                UI.ActionButton("Lose ".localize() + $"{settings.increment}", () => {
                                    kingdomStat.Value -= settings.increment;
                                }, UI.AutoWidth());
                            }
                        }
                        UI.Div(0, 0, 800);
                        UI.DescriptiveLabel("Cost Modifiers".localize(), ("The following modifiers all work on ".Green() + "cost = cost (1 + modifier) ".Yellow() + "so a value of ".Green() + "-1".Yellow() + " means the cost is free, ".Green() + "0".Yellow() + " is normal cost and ".Green() + "2".Yellow() + " increases it 3x".Green()).localize());
                        UI.Slider("Claim Cost Modifier".localize(), () => ks.ClaimCostModifier, v => ks.ClaimCostModifier = v, -1, 2, 0, 1);
                        UI.Slider("Claim Time Modifier".localize(), () => ks.ClaimTimeModifier, v => ks.ClaimTimeModifier = v, -1, 2, 0, 1);
                        UI.Slider("Rankup Time Modifer".localize(), () => ks.RankupTimeModifier, v => ks.RankupTimeModifier = v, -1, 2, 0, 1);
                        UI.Slider("Build Time Modifier".localize(), ref settings.kingdomBuildingTimeModifier, -1, 2, 0, 1);
                        UI.Div(0, 0, 800);
                        UI.DescriptiveLabel("Random Encounters".localize(), ("The following modifiers all work on ".Green() + "chance = chance (1 + modifier) ".Yellow() + "so a value of ".Green() + "-1".Yellow() + " means the chance is 0, ".Green() + "0".Yellow() + " is chance cost and ".Green() + "2".Yellow() + " increases it 3x".Green()).localize());
                        UI.Slider("% Chance (Unclaimed)".localize(), () => ks.REModifierUnclaimed, v => ks.REModifierUnclaimed = v, -1f, 2f, 0f, 1);
                        UI.Slider("% Chance (Claimed)".localize(), () => ks.REModifierClaimed, v => ks.REModifierClaimed = v, -1, 2, -0.5f, 1);
                        UI.Slider("% Chance (Upgraded)".localize(), () => ks.REModifierUpgraded, v => ks.REModifierUnclaimed = v, -1f, 2f, -1f, 1);
                        UI.Div(0, 0, 800);
                        UI.ValueAdjuster("Confidence (Royal Court)".localize(), () => ks.RoyalCourtConfidence, v => ks.RoyalCourtConfidence = v, 1, 0, int.MaxValue);
                        UI.ValueAdjuster("Confidence (Nobles)".localize(), () => ks.NobilityConfidence, v => ks.NobilityConfidence = v, 1, 0, int.MaxValue);
                        UI.ValueAdjuster("Victories This Week".localize(), () => ks.VictoriesThisWeek, v => ks.VictoriesThisWeek = v, 1, 0, int.MaxValue);
                        UI.EnumGrid("Unrest".localize(), () => ks.Unrest, (u) => ks.Unrest = u);
                        UI.AlignmentGrid("Alignment".localize(), ks.Alignment, (a) => ks.Alignment = a, UI.Width(325));
                    }
                },
                () => {
                    using (UI.VerticalScope()) {
                        UI.Space(15);
                        UI.Div(0, 0, 800);
                        UI.Label("Kingdom Finances".localize());
                    }
                },
                () => {
                    UI.Label("Finances".localize().Cyan(), UI.Width(325));
                    UI.Label(ks.Resources.Finances.ToString().Orange().Bold(), UI.Width(100));
                    UI.ActionButton("Gain ".localize() + $"{settings.increment}", () => {
                        ks.Resources += KingdomResourcesAmount.FromFinances(settings.increment);
                    }, UI.AutoWidth());
                    UI.ActionButton("Lose ".localize() + $"{settings.increment}", () => {
                        ks.Resources -= KingdomResourcesAmount.FromFinances(settings.increment);
                    }, UI.AutoWidth());
                },
                () => {
                    UI.Label("Materials".localize().Cyan(), UI.Width(325));
                    UI.Label(ks.Resources.Materials.ToString().Orange().Bold(), UI.Width(100));
                    UI.ActionButton("Gain ".localize() + $"{settings.increment}", () => {
                        ks.Resources += KingdomResourcesAmount.FromMaterials(settings.increment);
                    }, UI.AutoWidth());
                    UI.ActionButton("Lose ".localize() + $"{settings.increment}", () => {
                        ks.Resources -= KingdomResourcesAmount.FromMaterials(settings.increment);
                    }, UI.AutoWidth());
                },
                () => {
                    UI.Label("Favors".localize().Cyan(), UI.Width(325));
                    UI.Label(ks.Resources.Favors.ToString().Orange().Bold(), UI.Width(100));
                    UI.ActionButton("Gain ".localize() + $"{settings.increment}", () => {
                        ks.Resources += KingdomResourcesAmount.FromFavors(settings.increment);
                    }, UI.AutoWidth());
                    UI.ActionButton("Lose ".localize() + $"{settings.increment}", () => {
                        ks.Resources -= KingdomResourcesAmount.FromFavors(settings.increment);
                    }, UI.AutoWidth());
                },
                () => {
                    using (UI.VerticalScope()) {
                        UI.Space(15);
                        UI.Div(0, 0, 800);
                    }
                },
                () => UI.Toggle("Instant Events".localize(), ref settings.toggleInstantEvent),
                () => UI.Toggle("Ignore Event Solution Restrictions".localize(), ref settings.toggleIgnoreEventSolutionRestrictions),
                () => {

                    UI.Slider("Crusade card resolution time multiplier".localize(), ref settings.kingdomTaskResolutionLengthMultiplier, -1, 2, 0, 2, "", UI.Width(400));
                    UI.Space(25);
                    UI.Label("Multiplies crusade card resolution time by (1 + modifier). -1 will make things as fast as possible (minimum 1 day to avoid possible bugs)".localize().Green());
                },
            () => {
                UI.Slider("Build Time Modifier".localize(), ref settings.kingdomBuildingTimeModifier, -1, 2, 0, 2, "", UI.Width(400));
                var instance = KingdomState.Instance;
                if (instance != null) {
                    instance.BuildingTimeModifier = settings.kingdomBuildingTimeModifier;
                }
                UI.Space(25);
                UI.Label("Multiplies build time by (1 + modifier). -1 will make new buildings instant.".localize().Green());
            },
                () => {
                    var startDate = Game.Instance.BlueprintRoot.Calendar.GetStartDate();
                    var currentDate = KingdomState.Instance.Date;
                    var dateText = Game.Instance.BlueprintRoot.Calendar.GetDateText(currentDate - startDate, GameDateFormat.Full, true);
                    using (UI.VerticalScope()) {
                        UI.Space(15);
                        UI.Div(0, 0, 800);
                        using (UI.HorizontalScope()) {
                            UI.Label("Date".localize().Cyan(), UI.Width(325));
                            UI.Label(dateText.Orange().Bold(), UI.AutoWidth());
                            UI.ActionButton("+1 " + "Day".localize(), () => { Actions.KingdomTimelineAdvanceDays(1); }, UI.Width(150));
                            UI.ActionButton("+1 " + "Month".localize(), () => {
                                Actions.KingdomTimelineAdvanceDays(KingdomState.Instance.DaysTillNextMonth);
                            }, UI.Width(150));
                        }
                        UI.ValueAdjuster("Current Day".localize(), () => ks.CurrentDay, v => ks.CurrentDay = v, 1, 0, int.MaxValue);
                        UI.ValueAdjuster("Current Turn".localize(), () => ks.CurrentTurn, v => ks.CurrentTurn = v, 1, 0, int.MaxValue);
                    }
                },
               () => { }
            );
            25.space();
            UI.Div();
            SettlementsEditor.OnGUI();
        }
    }
}
