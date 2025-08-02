﻿// Copyright < 2021 > Narria (github user Cabarius) - License: MIT

using Kingmaker;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Cheats;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.View;
using ModKit;
using System;
using System.Collections.Generic;
using System.Linq;
using ToyBox.BagOfPatches;
using ToyBox.classes.MainUI;
using UnityEngine;
using UnityModManagerNet;
using static ModKit.UI;
using Kingmaker.Kingdom;
using static ToyBox.BagOfPatches.Romance;
using Kingmaker.Designers;
using Kingmaker.Blueprints.Root;
namespace ToyBox {
    public static class BagOfTricks {
        public static Settings Settings => Main.Settings;

        // cheats combat
        private const string? RestAll = "Rest All";
        private const string? RestSelected = "Rest Selected";
        private const string? Empowered = "Empowered";
        private const string? FullBuffPlease = "Common Buffs";
        private const string? GoddesBuffs = "Buff Like A Goddess";
        private const string? RemoveBuffs = "Remove Buffs";
        private const string? RemoveDeathsDoor = "Remove Deaths Door";
        private const string? KillAllEnemies = "Kill All Enemies";
        //private const string SummonZoo = "Summon Zoo"
        private const string? LobotomizeAllEnemies = "Lobotomize Enemies";
        private const string? ToggleMurderHobo = "Toggle Murder Hobo";

        // cheats common
        private const string? TeleportPartyToYou = "Teleport Party To You";
        private const string? GoToGlobalMap = "Go To Global Map";
        private const string? RerollPerception = "Reroll Perception";
        private const string? RerollInteractionSkillChecks = "Reset Interactables";
        private const string? ChangeParty = "Change Party";
        private const string? ChangWeather = "Change Weather";

        // other
        private const string? TimeScaleMultToggle = "Main/Alt Timescale";
        private const string? PreviewDialogResults = "Preview Results";
        private const string? CopyUnit = "Copy Unit";
        private const string? PasteUnit = "Paste Unit";
        private static BlueprintUnit UnitToCopy;

        //For buffs exceptions
        private static bool showBuffDurationExceptions = false;
        private static bool showDLC6RomanceOverrideMenu = false;

        public static void OnLoad() {
            // Combat
            KeyBindings.RegisterAction(RestAll, () => CheatsCombat.RestAll());
            KeyBindings.RegisterAction(RestSelected, () => Actions.RestSelected());
            KeyBindings.RegisterAction(Empowered, () => CheatsCombat.Empowered(""));
            KeyBindings.RegisterAction(FullBuffPlease, () => CheatsCombat.FullBuffPlease(""));
            KeyBindings.RegisterAction(GoddesBuffs, () => CheatsCombat.Iddqd(""));
            KeyBindings.RegisterAction(RemoveBuffs, () => Actions.RemoveAllBuffs());
            KeyBindings.RegisterAction(RemoveDeathsDoor, () => CheatsCombat.DetachDebuff());
            KeyBindings.RegisterAction(KillAllEnemies, () => Actions.KillAll());
            //KeyBindings.RegisterAction(SummonZoo, () => CheatsCombat.SpawnInspectedEnemiesUnderCursor(""));
            KeyBindings.RegisterAction(LobotomizeAllEnemies, () => Actions.LobotomizeAllEnemies());
            // Common
            KeyBindings.RegisterAction(TeleportPartyToYou, () => Teleport.TeleportPartyToPlayer());
            KeyBindings.RegisterAction(GoToGlobalMap, () => Teleport.TeleportToGlobalMap());
            KeyBindings.RegisterAction(RerollPerception, () => Actions.RunPerceptionTriggers());
            KeyBindings.RegisterAction(RerollInteractionSkillChecks, () => Actions.RerollInteractionSkillChecks());
            KeyBindings.RegisterAction(ChangeParty, () => { Actions.ChangeParty(); });
            KeyBindings.RegisterAction(ChangWeather, () => CheatsCommon.ChangeWeather(""));
            // Other
            KeyBindings.RegisterAction(TimeScaleMultToggle,
                                       () => {
                                           Settings.useAlternateTimeScaleMultiplier = !Settings.useAlternateTimeScaleMultiplier;
                                           Actions.ApplyTimeScale();
                                       },
                                       title => ToggleTranscriptForState(title, Settings.useAlternateTimeScaleMultiplier)
                );
            KeyBindings.RegisterAction(PreviewDialogResults, () => {
                Settings.previewDialogResults = !Settings.previewDialogResults;
                var dialogController = Game.Instance.DialogController;
            });
            KeyBindings.RegisterAction(CopyUnit,
                                       () => {
                                           var characterList = GameHelper.GetTargetsAround(Utils.PointerPosition(), 10, false, false).ToList();
                                           if (characterList?.Count > 0) {
                                               if (settings.toggleOnlyCopyEnemy) {
                                                   characterList.RemoveAll(ch => !ch.IsPlayersEnemy);
                                               }
                                               characterList = characterList.OrderBy((ch) => ch.DistanceTo(Utils.PointerPosition())).ToList();
                                               UnitToCopy = characterList.First().Blueprint;
                                           }
                                       });
            KeyBindings.RegisterAction(PasteUnit,
                                       () => {
                                           if (UnitToCopy) {
                                               var unit = Game.Instance.EntityCreator.SpawnUnit(UnitToCopy, Utils.PointerPosition(), Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
                                               unit.LookAt(Shodan.MainCharacter.Position);
                                               if (settings.togglePastedUnitJoinFight) {
                                                   unit.CombatState.JoinCombat(false);
                                               }
                                               if (settings.togglePastedAreAlwaysEnemy) {
                                                   unit.AttackFactions.Add(BlueprintRoot.Instance.PlayerFaction);
                                               }
                                           }
                                       });
            KeyBindings.RegisterAction(ToggleMurderHobo,
                                       () => Settings.togglekillOnEngage = !Settings.togglekillOnEngage,
                                       title => ToggleTranscriptForState(title, Settings.togglekillOnEngage)
                                       );
        }
        public static void ResetGUI() { }

        public static void OnGUI() {
#if BUILD_CRUI
            ActionButton("Demo crUI", () => ModKit.crUI.Demo());
#endif
            if (Main.IsInGame) {
                using (HorizontalScope()) {
                    Space(25);
                    Label("increment".localize().Cyan(), AutoWidth());
                    IntTextField(ref Settings.increment, null, Width(150));
                }
                var increment = Settings.increment;
                var mainChar = Game.Instance.Player.MainCharacter.Value;
                var kingdom = KingdomState.Instance;
                HStack("Resources".localize(),
                       1,
                       () => {
                           var money = Game.Instance.Player.Money;
                           Label("Gold".localize().Cyan(), Width(150));
                           Label(money.ToString().Orange().Bold(), Width(200));
                           ActionButton("Gain ".localize() + $"{increment}", () => Game.Instance.Player.GainMoney(increment), AutoWidth());
                           ActionButton("Lose ".localize() + $"{increment}",
                                        () => {
                                            var loss = Math.Min(money, increment);
                                            Game.Instance.Player.GainMoney(-loss);
                                        },
                                        AutoWidth());
                       },
                       () => {
                           var exp = mainChar.Progression.Experience;
                           Label("Experience".localize().Cyan(), Width(150));
                           Label(exp.ToString().Orange().Bold(), Width(200));
                           ActionButton("Gain ".localize() + $"{increment}", () => { Game.Instance.Player.GainPartyExperience(increment); }, AutoWidth());
                       },
                    () => {
                        var corruption = Game.Instance.Player.Corruption;
                        Label("Corruption".localize().Cyan(), Width(150));
                        Label(corruption.CurrentValue.ToString().Orange().Bold(), Width(200));
                        ActionButton($"Clear".localize(), () => corruption.Clear(), AutoWidth());
                        25.space();
                        Toggle("Disable Corruption".localize(), ref Settings.toggleDisableCorruption);
                    },
                       () => { }
                    );
                Div(0, 25);
            }
            HStack("Combat".localize(),
                   2,
                   () => BindableActionButton(RestAll, true),
                   () => BindableActionButton(RestSelected, true),
                   () => BindableActionButton(FullBuffPlease, true),
                   () => BindableActionButton(Empowered, true),
                   () => BindableActionButton(GoddesBuffs, true),
                   () => BindableActionButton(RemoveBuffs, true),
                   () => BindableActionButton(RemoveDeathsDoor, true),
                   () => BindableActionButton(KillAllEnemies, true),
                   //() => UI.BindableActionButton(SummonZoo),
                   () => BindableActionButton(LobotomizeAllEnemies, true),
                   () => { },
                   () => {
                       using (VerticalScope()) {
                           using (HorizontalScope()) {
                               using (VerticalScope(220.width())) {
                                   using (HorizontalScope()) {
                                       Toggle(("Be a " + "Murder".Red().Bold() + " Hobo".Orange()).localize(), ref Settings.togglekillOnEngage, 222.width());
                                       KeyBindPicker(ToggleMurderHobo, "", 50);
                                   }
                               }
                               158.space();
                               Label(("If ticked, this will " + "MURDER".Red().Bold() + " all who dare to engage you!".Green()).localize(), AutoWidth());
                           }
                           using (HorizontalScope()) {
                               if (Toggle("Log ToyBox Keyboard Commands In Game".localize(), ref Mod.ModKitSettings.toggleKeyBindingsOutputToTranscript, 450.width()))
                                   ModKitSettings.Save();
                               50.space();
                               HelpLabel("When ticked this shows ToyBox commands in the combat log which is helpful for you to know when you used the shortcut".localize());
                           }
                       }
                   }
                );
            Div(0, 25);
            HStack("Teleport".localize(),
                   2,
                   () => BindableActionButton(TeleportPartyToYou, true),
                   () => {
                       Toggle("Enable Teleport Keys".localize(), ref Settings.toggleTeleportKeysEnabled);
                       Space(100);
                       if (Settings.toggleTeleportKeysEnabled) {
                           using (VerticalScope()) {
                               KeyBindPicker("TeleportMain", "Main Character".localize(), 0, 200);
                               KeyBindPicker("TeleportSelected", "Selected Chars".localize(), 0, 200);
                               KeyBindPicker("TeleportParty", "Whole Party".localize(), 0, 200);
                           }
                       }
                       Space(25);
                       Label("You can enable hot keys to teleport members of your party to your mouse cursor on Area or the Global Map".localize().Green());
                   });
            Div(0, 25);
            HStack("Unit Copying".localize(),
                   2,
                   () => {
                       using (VerticalScope()) {
                           KeyBindPicker(CopyUnit, CopyUnit.localize(), 0, 200);
                           KeyBindPicker(PasteUnit, PasteUnit.localize(), 0, 200);
                           Toggle("Try to make spawned unit join fight".localize(), ref Settings.togglePastedUnitJoinFight);
                           Toggle("Only select enemies for copying".localize(), ref Settings.toggleOnlyCopyEnemy);
                           Toggle("Make copied unit part of enemy faction".localize(), ref Settings.togglePastedAreAlwaysEnemy);
                       }
                   });
            Div(0, 25);
            HStack("Common".localize(),
                   2,
                   () => BindableActionButton(GoToGlobalMap, true),
                   () => {
                       BindableActionButton(ChangeParty, true);
                       Space(-75);
                       HelpLabel("Change the party without advancing time (good to bind)".localize());
                   },
                   () => BindableActionButton(RerollPerception, true),
                   () => {
                       BindableActionButton(RerollInteractionSkillChecks, true);
                       Space(-75);
                       Label("This resets all the skill check rolls for all interactable objects in the area".localize().Green());
                   },
            () => {
                NonBindableActionButton("Set Perception to 40".localize(), () => {
                    CheatsCommon.StatPerception();
                    Actions.RunPerceptionTriggers();
                });
            },
                   () => BindableActionButton(ChangWeather, true),
                   () => NonBindableActionButton("Give All Items".localize(), () => CheatsUnlock.CreateAllItems("")),
                   () => NonBindableActionButton("Identify All".localize(), () => Actions.IdentifyAll()),
                   () => { }
                );
            Div(0, 25);
            HStack("Preview".localize(),
                   0,
                   () => {
                       Toggle("Dialog Results".localize(), ref Settings.previewDialogResults);
                       25.space();
                       Toggle("Dialog Conditions".localize(), ref Settings.previewDialogConditions);
                       25.space();
                       Toggle("Dialog Alignment".localize(), ref Settings.previewAlignmentRestrictedDialog);
                       25.space();
                       Toggle("Random Encounters".localize(), ref Settings.previewRandomEncounters);
                       25.space();
                       Toggle("Events".localize(), ref Settings.previewEventResults);
                       25.space();
                       Toggle("Decrees".localize(), ref Settings.previewDecreeResults);
                       25.space();
                       Toggle("Relic Info".localize(), ref Settings.previewRelicResults);
                       25.space();
                       BindableActionButton(PreviewDialogResults, true);
                   });
            Div(0, 25);
            HStack("Dialog".localize(),
                   1,
                   () => {
                       Toggle(("♥♥ ".Red() + "Love is Free".Bold() + " ♥♥".Red()).localize(), ref Settings.toggleAllowAnyGenderRomance, 300.width());
                       25.space();
                       Label(("Allow ".Green() + "any gender".Color(RGBA.purple) + " " + "for any ".Green() + "R".Color(RGBA.red) + "o".Orange() + "m".Yellow() + "a".Green() + "n".Cyan() + "c".Color(RGBA.blue) + "e".Color(RGBA.purple)).localize());
                   },
                   () => {
                       Toggle("Jealousy Begone!".localize().Bold(), ref Settings.toggleMultipleRomance, 300.width());
                       25.space();
                       Label(("Allow ".Green() + "multiple".Color(RGBA.purple) + " romances at the same time".Green()).localize());
                   },
                   () => {
                       if (Settings.toggleMultipleRomance) {
                           25.space();
                           DisclosureToggle("Show End of DLC6 romance picker. You should use this if you have multiple romances just before the end of DLC6.".localize(), ref showDLC6RomanceOverrideMenu);
                           if (showDLC6RomanceOverrideMenu) {
                               using (VerticalScope()) {
                                   Label("");
                                   EnumerablePicker("Which cutscene should play at the end of DLC6?", ref Settings.pickedDLC6Override, (DLC6RomanceOverride[])Enum.GetValues(typeof(DLC6RomanceOverride)), 3);
                               }
                           }
                       }
                   },
                   () => {
                       Toggle("Friendship is Magic".localize().Bold(), ref Settings.toggleFriendshipIsMagic, 300.width());
                       25.space();
                       Label(("Experimental".Orange() + " your friends forgive even your most vile choices.").localize().Green());
                   },
                   () => {
                       Toggle("Disallow Companions Leaving Party".localize(), ref Settings.toggleBlockUnrecruit, 300.width());
                       200.space();
                       Label(("Warning: ".Color(RGBA.red) + " Only use when Friendship is Magic doesn't work, and then turn off immediately after. Can otherwise break your save").localize().Orange());
                   },
                   () => {
                       Toggle("Previously Chosen Dialog Is Smaller ".localize(), ref Settings.toggleMakePreviousAnswersMoreClear, 300.width());
                       200.space();
                       Label("Draws dialog choices that you have previously selected in smaller type".localize().Green());
                   },
                   () => {
                       Toggle("Expand Dialog To Include Remote Companions".localize().Bold(), ref Settings.toggleRemoteCompanionDialog, 300.width());
                       200.space();
                       Label(("Experimental".Orange() + " Allow remote companions to make comments on dialog you are having.").localize().Green());
                   },
                   () => {
                       if (Settings.toggleRemoteCompanionDialog) {
                           50.space();
                           Toggle("Include Former Companions".localize(), ref Settings.toggleExCompanionDialog, 300.width());
                           150.space();
                           Label("This also includes companions who left the party such as Wenduag if you picked Lann".localize().Green());
                       }
                   },
                   () => {
                       using (VerticalScope(300.width())) {
                           Toggle("Expand Answers For Conditional Responses".localize(), ref Settings.toggleShowAnswersForEachConditionalResponse, 300.width());
                           if (Settings.toggleShowAnswersForEachConditionalResponse) {
                               using (HorizontalScope()) {
                                   50.space();
                                   Toggle("Show Unavailable Responses".localize(), ref Settings.toggleShowAllAnswersForEachConditionalResponse, 250.width());
                               }
                           }
                       }
                       200.space();
                       Label("Some responses such as comments about your mythic powers will always choose the first one by default. This will show a copy of the answer and the condition for each possible response that an NPC might make to you based on".localize().Green());
                   },
#if DEBUG
                   () => {
                       Toggle("Randomize NPC Responses To Dialog Choices".localize(), ref Settings.toggleRandomizeCueSelections, 300.width());
                       200.space();
                       Label(("Some responses such as comments about your mythic powers will always choose the first one by default. This allows the game to mix things up a bit".Green() + "\nWarning:".Yellow().Bold() + " this will introduce randomness to NPC responses to you in general and may lead to surprising or even wild outcomes".Orange()).localize());
                   },
#endif
                   () => Toggle("Disable Dialog Restrictions (Alignment)".localize(), ref Settings.toggleDialogRestrictions),
                   () => Toggle("Disable Dialog Restrictions (Mythic Path)".localize(), ref Settings.toggleDialogRestrictionsMythic),
                   () => Toggle("Disable Dialog Restrictions (Racial)".localize(), ref Settings.toggleDialogRestrictionsRace),
                   //() => Toggle("Disable Dialog Restrictions (Class)".localize(), ref Settings.toggleDialogRestrictionsClass),
                   () => Toggle("Ignore Event Solution Restrictions".localize(), ref Settings.toggleIgnoreEventSolutionRestrictions),
#if DEBUG
                   () => Toggle("Disable Dialog Restrictions (Everything, Experimental)".localize(), ref Settings.toggleDialogRestrictionsEverything),
#endif
                   () => { }
                );
            Div(0, 25);
            HStack("Quality of Life".localize(),
                   1,
                   () => {
                       Toggle("Allow Achievements While Using Mods".localize(), ref Settings.toggleAllowAchievementsDuringModdedGame, 500.width());
                       Label("This is intended for you to be able to enjoy the game while using mods that enhance your quality of life.  Please be mindful of the player community and avoid using this mod to trivialize earning prestige achievements like Sadistic Gamer. The author is in discussion with Owlcat about reducing the scope of achievement blocking to just these. Let's show them that we as players can mod and cheat responsibly.".localize().Orange());
                   },
                   () => {
                       Toggle("Skip Splash Screen".localize(), ref Settings.toggleSkipSplashScreen, 500.width());
                       Label("This skips the splash screen that appears when the game starts. Helpful if you need to frequently restart the game".localize());
                   },
                   () => {
                       Toggle("Enhanced Map View".localize(), ref Settings.toggleZoomableLocalMaps, 500.width());
                       HelpLabel("Makes mouse zoom works for the local map (cities, dungeons, etc). Game restart required if you turn it off".localize());
                   },
                   () => {
                       Toggle("Click On Equip Slots To Filter Inventory".localize(), ref Settings.togglEquipSlotInventoryFiltering, 500.width());
                       HelpLabel($"If you tick this you can click on equipment slots to filter the inventory for items that fit in it.\nFor more {"Enhanced Inventory".Orange()} and {"Spellbook".Orange()} check out the {"Loot & Spellbook Tab".Orange().Bold()}".localize());
                   },
                   () => {
                       Toggle("Enhanced Load/Save".localize(), ref Settings.toggleEnhancedLoadSave, 500.width());
                       HelpLabel("Adds a search field to Load/Save screen (in game only)".localize());
                   },
                   () => Toggle("Object Highlight Toggle Mode".localize(), ref Settings.highlightObjectsToggle),
                   () => {
                       if (Settings.highlightObjectsToggle) {
                           50.space();
                           Toggle("Hide Name Overtips".localize(), ref Settings.highlightObjectsToggleHideNameOvertip);
                       }
                   },
                   () => {
                       Toggle("Combat Log divider line".localize(), ref settings.toggledividerlineinlog, 500.width());
                       HelpLabel("On round end, outputs a divider line in the combat log".localize());
                   },
                   () => {
                       if (Settings.highlightObjectsToggle && Settings.highlightObjectsToggleHideNameOvertip) {
                           50.space();
                           Slider("Fade Delay".localize(), ref Settings.highlightObjectsToggleHideNameOvertipDelay, 0f, 10f, 3f, options: 200.width());
                       }
                   },
                   () => {
                       Toggle("Mark Interesting NPCs".localize(), ref Settings.toggleShowInterestingNPCsOnLocalMap, 500.width());
                       HelpLabel("This will change the color of NPC names on the highlike makers and change the color map markers to indicate that they have interesting or conditional interactions".localize());
                   },
                   () => Toggle("Highlight Copyable Scrolls".localize(), ref Settings.toggleHighlightCopyableScrolls),
                   () => {
                       Toggle("Auto load Last Save on launch".localize(), ref Settings.toggleAutomaticallyLoadLastSave, 500.width());
                       HelpLabel("Hold down shift during launch to bypass".localize());
                   },
                   () => Toggle("Make game continue to play music on lost focus".localize(), ref Settings.toggleContinueAudioOnLostFocus),
                   () => Toggle(("Game Over Fix For " + "LEEEROOOOOOOYYY JEEEENKINS!!!".Color(RGBA.maroon) + " omg he just ran in!").localize(), ref Settings.toggleGameOverFixLeeerrroooooyJenkins),
                   () => {
                       503.space();
                       HelpLabel("Prevents dumb companions (that's you Greybor) from wiping the party by running running into the dragon room and dying...".localize());
                   },
                   () => Toggle("Make Spell/Ability/Item Pop-Ups Wider ".localize(), ref Settings.toggleWidenActionBarGroups),
                   () => {
                       if (Toggle("Show Acronyms in Spell/Ability/Item Pop-Ups".localize(), ref Settings.toggleShowAcronymsInSpellAndActionSlots)) {
                           Main.SetNeedsResetGameUI();
                       }
                   },
                   () => {
                       Toggle("Icky Stuff Begone!!!".localize(), ref Settings.toggleReplaceModelMenu, (Settings.toggleReplaceModelMenu ? 248 : 499).width());
                       if (Settings.toggleReplaceModelMenu) {
                           using (VerticalScope(Width(247))) {
                               Toggle("Spiders Begone!".localize(), ref Settings.toggleSpiderBegone);
                               Toggle("Vescavors Begone!".localize(), ref Settings.toggleVescavorsBegone);
                               Toggle("Retrievers Begone!".localize(), ref Settings.toggleRetrieversBegone);
                               Toggle("Deraknis Begone!".localize(), ref Settings.toggleDeraknisBegone);
                               Toggle("Deskari Begone!".localize(), ref Settings.toggleDeskariBegone);
                               Toggle("Locust Begone!".localize(), ref Settings.toggleLocustBegone);
                           }
                       }
                       Label("Some players find spiders and other swarms icky. This replaces them with something more pleasant".localize().Green());
                   },
                   () => Toggle("Make tutorials not appear if disabled in settings".localize(), ref Settings.toggleForceTutorialsToHonorSettings),
                   () => {
                       if (Settings.toggleForceTutorialsToHonorSettings) {
                           Space(25);
                           Toggle("Disable tutorials forcefully".localize(), ref Settings.toggleForceDisableTutorials);
                       }
                   },
                   () => Toggle("Automatically skip all skippable cutscenes".localize(), ref Settings.toggleSkipSkippableCutscenes),
                   () => Toggle("Refill consumables in belt slots if in inventory".localize(), ref Settings.togglAutoEquipConsumables),
                   () => {
                       var modifier = KeyBindings.GetBinding("InventoryUseModifier");
                       var modifierText = modifier.Key == KeyCode.None ? "Modifer" : modifier.ToString();
                       Toggle("Allow ".localize() + $"{modifierText}".Cyan() + (" + Click".Cyan() + " To Use Items In Inventory").localize(), ref Settings.toggleShiftClickToUseInventorySlot, 470.width());
                       if (Settings.toggleShiftClickToUseInventorySlot) {
                           ModifierPicker("InventoryUseModifier", "", 0);
                       }
                   },
                   () => {
                       var modifier = KeyBindings.GetBinding("ClickToTransferModifier");
                       var modifierText = modifier.Key == KeyCode.None ? "Modifer" : modifier.ToString();
                       Toggle("Allow ".localize() + $"{modifierText}".Cyan() + (" + Click".Cyan() + " To Transfer Entire Stack").localize(), ref Settings.toggleShiftClickToFastTransfer, 470.width());
                       if (Settings.toggleShiftClickToFastTransfer) {
                           ModifierPicker("ClickToTransferModifier", "", 0);
                       }
                   },
                   () => Toggle("Respec Refund Scrolls".localize(), ref Settings.toggleRespecRefundScrolls),
                   () => {
                       Toggle("Make Puzzle Symbols More Clear".localize(), ref Settings.togglePuzzleRelief);
                       25.space();
                       HelpLabel(("ToyBox Archeologists can tag confusing puzzle pieces with green numbers in the game world and for inventory tool tips it will show text like this: " + "[PuzzlePiece Green3x1]".Yellow().Bold() + "\nNOTE: ".Orange().Bold() + "Needs game restart to take efect".Orange()).localize());
                   },
                () => {
                    ActionButton("Clear Action Bar".localize(), () => Actions.ClearActionBar());
                    50.space();
                    Label("Make sure you have auto-fill turned off in settings or else this will just reset to default".localize().Green());
                },
                   () => ActionButton("Fix Incorrect Main Character".localize(),
                                      () => {
                                          var probablyPlayer = Game.Instance.Player?.Party?
                                                                   .Where(x => !x.IsCustomCompanion())
                                                                   .Where(x => !x.IsStoryCompanion())
                                                                   .ToList();
                                          if (probablyPlayer is { Count: 1 }) {
                                              var newMainCharacter = probablyPlayer.First();
                                              Mod.Warn($"Promoting {newMainCharacter.CharacterName} to main character!");
                                              if (Game.Instance != null) Game.Instance.Player.MainCharacter = newMainCharacter;
                                          }
                                      },
                                      AutoWidth()),
                   () => {
                       Toggle("Enable Loading with Blueprint Errors".localize().Color(RGBA.maroon), ref Settings.enableLoadWithMissingBlueprints);
                       25.space();
                       Label($"This {"incredibly dangerous".Bold()} setting overrides the default behavior of failing to load saves depending on missing blueprint mods. This desperate action can potentially enable you to recover your saved game, though you'll have to respec at minimum.".localize().Orange());
                   },
                   () => {
                       if (Settings.enableLoadWithMissingBlueprints) {
                           Label("To permanently remove these modded blueprint dependencies, load the damaged saved game, change areas, and then save the game. You can then respec any characters that were impacted.".localize().Orange());
                       }
                   },
                   () => {
                       using (VerticalScope()) {
                           Div(0, 25, 1280);
                           var useAlt = Settings.useAlternateTimeScaleMultiplier;
                           var mainTimeScaleTitle = "Game Time Scale".localize();
                           if (useAlt) mainTimeScaleTitle = mainTimeScaleTitle.Grey();
                           var altTimeScaleTitle = "Alternate Time Scale".localize();
                           if (!useAlt) altTimeScaleTitle = altTimeScaleTitle.Grey();
                           using (HorizontalScope()) {
                               LogSlider(mainTimeScaleTitle, ref Settings.timeScaleMultiplier, 0f, 20, 1, 1, "", Width(450));
                               Space(25);
                               Label("Speeds up or slows down the entire game (movement, animation, everything)".localize().Green());
                           }
                           using (HorizontalScope()) {
                               LogSlider(altTimeScaleTitle, ref Settings.alternateTimeScaleMultiplier, 0f, 20, 5, 1, "", Width(450));
                           }
                           using (HorizontalScope()) {
                               BindableActionButton(TimeScaleMultToggle, true);
                               Space(-95);
                               Label("Bindable hot key to swap between main and alternate time scale multipliers".localize().Green());
                           }
                           Div(0, 25, 1280);
                           Actions.ApplyTimeScale();
                       }
                   },
                   () => Slider("Turn Based Combat Delay".localize(), ref Settings.turnBasedCombatStartDelay, 0f, 4f, 4f, 1, "", Width(450)),
                   () => {
                       using (VerticalScope()) {

                           using (HorizontalScope()) {
                               using (VerticalScope()) {
                                   Div(0, 25, 1280);
                                   if (Toggle("Enable Brutal Unfair Difficulty".localize(), ref Settings.toggleBrutalUnfair)) {
                                       EventBus.RaiseEvent<IDifficultyChangedClassHandler>((Action<IDifficultyChangedClassHandler>)(h => {
                                           h.HandleDifficultyChanged();
                                           Main.SetNeedsResetGameUI();
                                       }));
                                   }
                                   Space(15);
                                   Label("This allows you to play with the originally released Unfair difficulty. ".localize().Green() + ("Note:".Orange().Bold() + "This Unfair difficulty was bugged and applied the intended difficulty modifers twice. ToyBox allows you to keep playing at this Brutal difficulty level and beyond.  Use the slider below to select your desired Brutality Level".Green()).localize(), Width(1200));
                                   Space(15);
                                   using (HorizontalScope()) {
                                       if (Slider("Brutality Level".localize(), ref Settings.brutalDifficultyMultiplier, 1f, 8f, 2f, 1, "", Width(450))) {
                                           EventBus.RaiseEvent<IDifficultyChangedClassHandler>((Action<IDifficultyChangedClassHandler>)(h => {
                                               h.HandleDifficultyChanged();
                                               Main.SetNeedsResetGameUI();
                                           }));
                                       }
                                       Space(25);
                                       var brutaltiy = Settings.brutalDifficultyMultiplier;
                                       string? label;
                                       var suffix = Math.Abs(brutaltiy - Math.Floor(brutaltiy)) <= float.Epsilon ? "" : "+";
                                       switch (brutaltiy) {
                                           case float level when level < 2.0:
                                               label = ("Unfair".localize() + suffix).Rarity(RarityType.Common);
                                               break;
                                           case float level when level < 3.0:
                                               label = "Brutal".localize() + suffix;
                                               break;
                                           default:
                                               var rarity = (RarityType)brutaltiy;
                                               label = $"{rarity}{suffix}".Rarity(rarity);
                                               break;
                                       }
                                       using (VerticalScope(AutoWidth())) {
                                           Space(UnityModManager.UI.Scale(3));
                                           Label(label.localize().Bold(), largeStyle, AutoWidth());
                                       }
                                   }
                                   Space(-10);
                               }
                           }
                       }
                   },
                   () => { }
                );

            Div(0, 25);
            EnhancedCamera.OnGUI();
            Div(0, 25);
            HStack("Alignment".localize(), 1,
                   () => { Toggle("Fix Alignment Shifts".localize(), ref Settings.toggleAlignmentFix); Space(119); Label("Makes alignment shifts towards pure good/evil/lawful/chaotic only shift on those axes".localize().Green()); },
                   () => { Toggle("Prevent Alignment Changes".localize(), ref Settings.togglePreventAlignmentChanges); Space(25); Label("See Party Editor for more fine grained alignment locking per character".localize().Green()); },
                   () => { }
                );
            Div(0, 25);
            HStack("Cheats".localize(), 1,
                   () => {
                       Toggle("Prevent Traps from triggering".localize(), ref Settings.disableTraps, 500.width());
                       Label("Enterint a Trap Zone while having Traps disabled will prevent that Trap from triggering even if you deactivate this option in the future".localize().Green());
                   },
                   () => Toggle("Prevent Locks from jamming".localize(), ref Settings.togglelockjam),
                   () => Toggle("Unlimited Stacking of Modifiers (Stat/AC/Hit/Damage/Etc)".localize(), ref Settings.toggleUnlimitedStatModifierStacking),
                   () => {
                       using (HorizontalScope()) {
                           ToggleCallback("Highlight Hidden Objects".localize(), ref Settings.highlightHiddenObjects, Actions.UpdateHighlights);
                           if (Settings.highlightHiddenObjects) {
                               Space(100);
                               ToggleCallback("In Fog Of War ".localize(), ref Settings.highlightHiddenObjectsInFog, Actions.UpdateHighlights);
                           }
                       }
                   },
                   () => Toggle("Infinite Abilities".localize(), ref Settings.toggleInfiniteAbilities),
                   () => Toggle("Infinite Spell Casts".localize(), ref Settings.toggleInfiniteSpellCasts),
                () => Toggle("No Material Components".localize(), ref Settings.toggleMaterialComponent),
                () => Toggle("Disable Party Negative Levels".localize(), ref Settings.togglePartyNegativeLevelImmunity),
                () => Toggle("Disable Party Ability Damage".localize(), ref Settings.togglePartyAbilityDamageImmunity),
                () => Toggle("Disable Attacks of Opportunity".localize(), ref Settings.toggleAttacksofOpportunity),
                () => Toggle("Unlimited Actions During Turn".localize(), ref Settings.toggleUnlimitedActionsPerTurn),
                () => Toggle("Infinite Charges On Items".localize(), ref Settings.toggleInfiniteItems),
                () => Toggle("Instant Cooldown".localize(), ref Settings.toggleInstantCooldown),
                () => Toggle("Instant Global Crusade Spells Cooldown".localize(), ref Settings.toggleInstantCrusadeSpellsCooldown),
                () => Toggle("Spontaneous Caster Scroll Copy".localize(), ref Settings.toggleSpontaneousCopyScrolls),
                () => Toggle("Ignore Equipment Restrictions".localize(), ref Settings.toggleEquipmentRestrictions),
                () => Toggle("Disable Armor Max Dexterity".localize(), ref Settings.toggleIgnoreMaxDexterity),
                () => Toggle("Disable Armor Speed Reduction".localize(), ref Settings.toggleIgnoreSpeedReduction),
                () => Toggle("Disable Armor & Shield Arcane Spell Failure".localize(), ref Settings.toggleIgnoreSpellFailure),
                () => Toggle("Disable Armor & Shield Checks Penalty".localize(), ref Settings.toggleIgnoreArmorChecksPenalty),
                () => Toggle("No Friendly Fire On AOEs".localize(), ref Settings.toggleNoFriendlyFireForAOE),
                () => Toggle("Free Meta-Magic".localize(), ref Settings.toggleMetamagicIsFree),
                () => Toggle("No Fog Of War".localize(), ref Settings.toggleNoFogOfWar),
                () => Toggle("Restore Spells & Skills After Combat".localize(), ref Settings.toggleRestoreSpellsAbilitiesAfterCombat),
                () => Toggle("Restore Just Spells After Combat".localize(), ref Settings.toggleRestoreSpellAfterCombat),
                () => Toggle("Restore Just Abilities After Combat".localize(), ref Settings.toggleRestoreAbilitiesAfterCombat),
                () => Toggle("Recharge Items After Combat".localize(), ref settings.toggleRechargeItemsAfterCombat),
                //() => UI.Toggle("Access Remote Characters", ref settings.toggleAccessRemoteCharacters,0),
                //() => UI.Toggle("Show Pet Portraits", ref settings.toggleShowAllPartyPortraits,0),
                () => Toggle("Instant Rest After Combat".localize(), ref Settings.toggleInstantRestAfterCombat),
                () => Toggle("Full Heal After Combat".localize(), ref Settings.toggleFullHealAfterCombat),
                () => Toggle("Instant change party members".localize(), ref Settings.toggleInstantChangeParty),
                () => ToggleCallback("Equipment No Weight".localize(), ref Settings.toggleEquipmentNoWeight, BagOfPatches.Tweaks.NoWeight_Patch1.Refresh),
                () => Toggle("Allow Equipment Change During Combat".localize(), ref Settings.toggleEquipItemsDuringCombat),
                () => Toggle("Allow Item Use From Inventory During Combat".localize(), ref Settings.toggleUseItemsDuringCombat),
                () => Toggle("Ignore Alignment Requirements for Abilities".localize(), ref Settings.toggleIgnoreAbilityAlignmentRestriction),
                () => Toggle("Ignore all Requirements for Abilities".localize(), ref Settings.toggleIgnoreAbilityAnyRestriction),
                () => Toggle("Ignore Pet Sizes For Mounting".localize(), ref Settings.toggleMakePetsRidable),
                () => Toggle("Ride Any Unit As Your Mount".localize(), ref Settings.toggleRideAnything),
                () => { }
                );
            Div(153, 25);
            HStack("", 1,
                () => EnumGrid("Disable Attacks Of Opportunity".localize(), ref Settings.noAttacksOfOpportunitySelection, AutoWidth()),
                    () => EnumGrid("Can Move Through".localize(), ref Settings.allowMovementThroughSelection, AutoWidth()),
                    () => {
                        Space(328); Label("This allows characters you control to move through the selected category of units during combat".localize().Green(), AutoWidth());
                    }
#if false
                () => { UI.Slider("Collision Radius Multiplier", ref settings.collisionRadiusMultiplier, 0f, 2f, 1f, 1, "", UI.AutoWidth()); },
#endif
                );
            HStack("Class Specific".localize(), 1,
                        () => Slider("Kineticist: Burn Reduction".localize(), ref Settings.kineticistBurnReduction, 0, 30, 0, "", AutoWidth()),
                        () => Slider("Arcanist: Spell Slot Multiplier".localize(), ref Settings.arcanistSpellslotMultiplier, 0.5f, 10f,
                                1f, 1, "", AutoWidth()),
                        () => Slider("Enduring Spells time needed for extension".localize(), ref Settings.enduringSpellsTimeThreshold,
                                0f, 120f, 60f, 2, "min".localize(), AutoWidth()),
                        () => Slider("Greater Enduring Spells time needed for extension".localize(), ref Settings.greaterEnduringSpellsTimeThreshold,
                                0f, 120f, 5f, 2, "min".localize(), AutoWidth()),
                        () => {
                            Space(25);
                            Label("Please rest after adjusting to recalculate your spell slots.".localize().Green());
                        },
                        () => Toggle("Witch/Shaman: Cackling/Shanting Extends Hexes By 10 Min (Out Of Combat)".localize(), ref Settings.toggleExtendHexes),
                        () => Toggle("Allow Simultaneous Activatable Abilities (Like Judgements)".localize(), ref Settings.toggleAllowAllActivatable),
                        () => Toggle("Kineticist: Allow Gather Power Without Hands".localize(), ref Settings.toggleKineticistGatherPower),
                        () => Toggle("Barbarian: Auto Start Rage When Entering Combat".localize(), ref Settings.toggleEnterCombatAutoRage),
                        () => Toggle("Demon: Auto Start Rage When Entering Combat".localize(), ref Settings.toggleEnterCombatAutoRageDemon),
                        () => Toggle("Magus: Always Allow Spell Combat".localize(), ref Settings.toggleAlwaysAllowSpellCombat),
                        () => { }
                        );
            Div(0, 25);
            HStack("Experience Multipliers".localize(), 1,
                () => LogSlider("All Experience".localize(), ref Settings.experienceMultiplier, 0f, 100f, 1, 1, "", AutoWidth()),
                () => {
                    using (HorizontalScope()) {
                        Toggle("Override for Combat".localize(), ref Settings.useCombatExpSlider, Width(275));
                        if (Settings.useCombatExpSlider) {
                            Space(10);
                            LogSliderCustomLabelWidth("", ref Settings.experienceMultiplierCombat, 0f, 100f, 1, 1, "", 12, AutoWidth());
                        }
                    }
                },
                () => {
                    using (HorizontalScope()) {
                        Toggle("Override for Quests".localize(), ref Settings.useQuestsExpSlider, Width(275));
                        if (Settings.useQuestsExpSlider) {
                            Space(10);
                            LogSliderCustomLabelWidth("", ref Settings.experienceMultiplierQuests, 0f, 100f, 1, 1, "", 12, AutoWidth());
                        }
                    }
                },
                () => {
                    using (HorizontalScope()) {
                        Toggle("Override for Skill Checks".localize(), ref Settings.useSkillChecksExpSlider, Width(275));
                        if (Settings.useSkillChecksExpSlider) {
                            Space(10);
                            LogSliderCustomLabelWidth("", ref Settings.experienceMultiplierSkillChecks, 0f, 100f, 1, 1, "", 12, AutoWidth());
                        }
                    }
                },
                () => {
                    using (HorizontalScope()) {
                        Toggle("Override for Traps".localize(), ref Settings.useTrapsExpSlider, Width(275));
                        if (Settings.useTrapsExpSlider) {
                            Space(10);
                            LogSliderCustomLabelWidth("", ref Settings.experienceMultiplierTraps, 0f, 100f, 1, 1, "", 12, AutoWidth());
                        }
                    }
                }
                );
            Div(0, 25);
            HStack("Other Multipliers".localize(), 1,
                () => {
                    LogSlider("Vision Range".localize(), ref Settings.fowMultiplier, 0f, 100f, 1, 1, "", AutoWidth());
                    List<UnitEntityData> units = Game.Instance?.Player?.m_PartyAndPets;
                    if (units != null) {
                        foreach (var unit in units) {
                            FogOfWarController.VisionRadiusMultiplier = Settings.fowMultiplier;
                            // TODO: do we need this for RT?
                            FogOfWarRevealerSettings revealer = unit.View?.FogOfWarRevealer;
                            if (revealer != null) {
                                if (Settings.fowMultiplier == 1) {
                                    revealer.DefaultRadius = true;
                                    revealer.UseDefaultFowBorder = true;
                                    revealer.Radius = 1.0f;
                                } else {
                                    revealer.DefaultRadius = false;
                                    revealer.UseDefaultFowBorder = false;
                                    // TODO: is this right?
                                    revealer.Radius = Settings.fowMultiplier;
                                }
                            }
                        }
                    }
                },
                () => LogSlider("Money Earned".localize(), ref Settings.moneyMultiplier, 0f, 20, 1, 1, "", AutoWidth()),
                () => LogSlider("Vendor Sell Price".localize(), ref Settings.vendorSellPriceMultiplier, 0f, 20, 1, 1, "", AutoWidth()),
                () => LogSlider("Vendor Buy Price".localize(), ref Settings.vendorBuyPriceMultiplier, 0f, 20, 1, 1, "", AutoWidth()),
                () => Slider("Increase Carry Capacity".localize(), ref Settings.encumberanceMultiplier, 1, 100, 1, "", AutoWidth()),
                () => Slider("Increase Carry Capacity (Party Only)".localize(), ref Settings.encumberanceMultiplierPartyOnly, 1, 100, 1, "", AutoWidth()),
                () => LogSlider("Spontaneous Spells Per Day".localize(), ref Settings.spellsPerDayMultiplier, 0f, 20, 1, 1, "", AutoWidth()),
                () => LogSlider("Prepared Spellslots".localize(), ref Settings.memorizedSpellsMultiplier, 0f, 20, 1, 1, "", AutoWidth()),
                () => {
                    LogSlider("Movement Speed".localize(), ref Settings.partyMovementSpeedMultiplier, 0f, 20, 1, 1, "", Width(600));
                    Space(25);
                    Toggle("Whole Team Moves Same Speed".localize(), ref Settings.toggleMoveSpeedAsOne);
                    Space(25);
                    Label("Adjusts the movement speed of your party in area maps".localize().Green());
                },
                () => {
                    LogSlider("Travel Speed".localize(), ref Settings.travelSpeedMultiplier, 0f, 20, 1, 1, "", Width(600));
                    Space(25);
                    Label("Adjusts the movement speed of your party on world maps".localize().Green());
                },
                () => {
                    LogSlider("Companion Cost".localize(), ref Settings.companionCostMultiplier, 0, 20, 1, 1, "", Width(600));
                    Space(25);
                    Label("Adjusts costs of hiring mercenaries at the Pathfinder vendor".localize().Green());

                },
                () => LogSlider("Enemy HP Multiplier".localize(), ref Settings.enemyBaseHitPointsMultiplier, 0.1f, 20, 1, 1, "", AutoWidth()),
                () => LogSlider("Buff Duration".localize(), ref Settings.buffDurationMultiplierValue, 0f, 9999, 1, 1, "", AutoWidth()),
                () => DisclosureToggle("Exceptions to Buff Duration Multiplier (Advanced; will cause blueprints to load)".localize(), ref showBuffDurationExceptions),
                () => {
                    if (!showBuffDurationExceptions) return;

                    BuffExclusionEditor.OnGUI();
                },
                () => { }
                );
            Div(0, 25);
            DiceRollsGUI.OnGUI();
            Div(0, 25);
            HStack("Summons".localize(), 1,
                () => Toggle("Make Controllable".localize(), ref Settings.toggleMakeSummmonsControllable),
                () => {
                    using (VerticalScope()) {
                        Div(0, 25);
                        using (HorizontalScope()) {
                            Label("Primary".localize().Orange(), AutoWidth()); Space(215); Label("good for party".localize().Green());
                        }
                        Space(25);
                        EnumGrid("Modify Summons For".localize(), ref Settings.summonTweakTarget1, AutoWidth());
                        LogSlider("Duration Multiplier".localize(), ref Settings.summonDurationMultiplier1, 0f, 20, 1, 2, "", AutoWidth());
                        Slider("Level Increase/Decrease".localize(), ref Settings.summonLevelModifier1, -20f, +20f, 0f, 0, "", AutoWidth());
                        Div(0, 25);
                        using (HorizontalScope()) {
                            Label("Secondary".localize().Orange(), AutoWidth()); Space(215); Label("good for larger group or to reduce enemies".localize().Green());
                        }
                        Space(25);
                        EnumGrid("Modify Summons For".localize(), ref Settings.summonTweakTarget2, AutoWidth());
                        LogSlider("Duration Multiplier".localize(), ref Settings.summonDurationMultiplier2, 0f, 20, 1, 2, "", AutoWidth());
                        Slider("Level Increase/Decrease".localize(), ref Settings.summonLevelModifier2, -20f, +20f, 0f, 0, "", AutoWidth());
                    }
                },
                () => { }
             );
        }
    }
}
