// borrowed shamelessly and enhanced from Bag of Tricks https://www.nexusmods.com/pathfinderkingmaker/mods/26, which is under the MIT License

using HarmonyLib;
using Kingmaker;
using Kingmaker.Assets.Controllers.GlobalMap;
using Kingmaker.Cheats;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Armies;
using Kingmaker.Armies.TacticalCombat.Parts;
using Kingmaker.Globalmap.State;
using Kingmaker.Globalmap.View;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using System;
using System.Linq;
using ModKit;
using UnityEngine;

namespace ToyBox.BagOfPatches {
    internal static class Movement {
        private static Settings Settings => Main.Settings;

        [HarmonyPatch(typeof(UnitEntityData), nameof(UnitEntityData.ModifiedSpeedMps), MethodType.Getter)]
        public static class UnitEntityData_CalculateSpeedModifier_Patch {
            private static void Postfix(UnitEntityData __instance, ref float __result) {
                if (Mathf.Approximately(Settings.partyMovementSpeedMultiplier, 1.0f) 
                    || __instance?.Descriptor?.IsPartyOrPet() != true
                    || __instance.Get<UnitPartTacticalCombat>()?.Faction != ArmyFaction.Crusaders)
                    return;
                
                __result *= Settings.partyMovementSpeedMultiplier;
            }
        }

        [HarmonyPatch(typeof(ClickGroundHandler), nameof(ClickGroundHandler.RunCommand))]
        public static class ClickGroundHandler_RunCommand_Patch {
            public static bool Prefix(UnitEntityData unit, ClickGroundHandler.CommandSettings settings) {
                
                if (!Main.Settings.toggleMoveSpeedAsOne) return true;
                if (unit.IsInCombat && Game.Instance.Player.IsTurnBasedModeOn()) return true;
                var partTacticalCombat = unit.Get<UnitPartTacticalCombat>();
                if (partTacticalCombat != null && partTacticalCombat.Faction != ArmyFaction.Crusaders) return true;

                var units = UnitEntityDataUtils.GetSelectedIncludingMounts(Game.Instance.UI.SelectionManager.SelectedUnits);
                var speedLimit = units.Select(u => u.ModifiedSpeedMps).Max();
                //Main.ModEntry.Logger.Log($"RunCommand - speedLimit: {speedLimit} selectedUnits: {string.Join(" ", units.Select(u => $"{u.CharacterName} {u.ModifiedSpeedMps}"))}");
                
                speedLimit *= Main.Settings.partyMovementSpeedMultiplier;
                var unitMoveTo = new UnitMoveTo(settings.Destination, 0.3f) {
                    MovementDelay = settings.Delay,
                    Orientation = settings.Orientation,
                    CreatedByPlayer = true
                };
                if (BuildModeUtility.IsDevelopment) {
                    if (CheatsAnimation.SpeedForce > 0f) {
                        unitMoveTo.OverrideSpeed = CheatsAnimation.SpeedForce;
                    }
                    unitMoveTo.MovementType = (UnitAnimationActionLocoMotion.WalkSpeedType)CheatsAnimation.MoveType.Get();
                }
                unitMoveTo.SpeedLimit = speedLimit;
                unitMoveTo.ApplySpeedLimitInCombat = settings.ApplySpeedLimitInCombat;
                unitMoveTo.OverrideSpeed = speedLimit * 1.5f;
                
                if (unit.RiderPart?.SaddledUnit == null)
                    unit.Commands.Run(unitMoveTo);
                else 
                    unit.RiderPart.SaddledUnit.Commands.Run(unitMoveTo);
                if (unit.Commands.Queue.FirstOrDefault(c => c is UnitMoveTo) == unitMoveTo || Game.Instance.IsPaused) {
                    ClickGroundHandler.ShowDestination(unit, unitMoveTo.Target, false);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(GlobalMapMovementController))]
        public static class GlobalMapMovementControllerPatch {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(GlobalMapMovementController), nameof(GlobalMapMovementController.GetRegionalModifier), [])]
            public static void MovementSpeed_GetRegionalModifier_Patch1(ref float __result) {
                var speedMultiplier = Mathf.Clamp(Settings.travelSpeedMultiplier, 0.1f, 100f);
                __result = speedMultiplier * __result;
            }
            
            [HarmonyPostfix]
            [HarmonyPatch(typeof(GlobalMapMovementController), nameof(GlobalMapMovementController.GetRegionalModifier), 
                          new[] { typeof(Vector3) })]
            public static void MovementSpeed_GetRegionalModifier_Patch2(ref float __result) {
                var speedMultiplier = Mathf.Clamp(Settings.travelSpeedMultiplier, 0.1f, 100f);
                __result = speedMultiplier * __result;
            }
        }

        /**
            GlobalMapState state,
            GlobalMapView view,
            IGlobalMapTraveler traveler,
            float visualStepDistance)
        */
        [HarmonyPatch(typeof(GlobalMapMovementUtility), nameof(GlobalMapMovementUtility.MoveAlongEdge), new[] {
            typeof(GlobalMapState), typeof(GlobalMapView), typeof(IGlobalMapTraveler), typeof(float)
            })]
        public static class GlobalMapMovementUtility_MoveAlongEdge_Patch {
            public static void Prefix(
                GlobalMapState state,
                GlobalMapView view,
                IGlobalMapTraveler traveler,
                ref float visualStepDistance) {
                // TODO - can we get rid of the other map movement multipliers and do them all here?
                if (traveler is GlobalMapArmyState armyState && armyState.Data.Faction == ArmyFaction.Crusaders) {
                    var speedMultiplier = Mathf.Clamp(Settings.travelSpeedMultiplier, 0.1f, 100f);
                    visualStepDistance = speedMultiplier * visualStepDistance;
                }
            }
        }

        [HarmonyPatch(typeof(GlobalMapArmyState), nameof(GlobalMapArmyState.SpendMovementPoints), new[] { typeof(float) })]
        public static class GlobalMapArmyState_SpendMovementPoints_Patch {
            public static void Prefix(GlobalMapArmyState __instance, ref float points) {
                if (__instance.Data.Faction == ArmyFaction.Crusaders) {
                    var speedMultiplier = Mathf.Clamp(Settings.travelSpeedMultiplier, 0.1f, 100f);
                    points /= speedMultiplier;
                }
            }
        }
    }
}
