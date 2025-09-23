using Kingmaker.Blueprints;
using ToyBox.Infrastructure.Inspector;
using UnityEngine;

namespace ToyBox.Infrastructure;
public static class BlueprintPicker<T> where T : SimpleBlueprint {
    private static string m_CurrentlyTyped = "";
    private static bool m_EnteredInvalidGuid = false;
    private static bool m_ShowBrowser = false;
    private static Browser<T>? m_Browser;
    private static WeakReference<T>? m_CurrentBlueprint;
    private static readonly Lazy<float> m_ButtonWidth = new(() => CalculateLargestLabelSize([SharedStrings.PickBlueprintText], GUI.skin.button));
    public static T? CurrentBlueprint {
        get {
            if (m_CurrentBlueprint is not null && m_CurrentBlueprint.TryGetTarget(out var bp)) {
                return bp;
            } else {
                return null;
            }
        }
    }
    public static bool OnPickerGUI() {
        bool didChange = false;
        using (VerticalScope()) {
            if (CurrentBlueprint != null) {
                UI.Label(SharedStrings.CurrentlySelectedBlueprintText + $" {BPHelper.GetTitle(CurrentBlueprint)}");
            }
            UI.DisclosureToggle(ref m_ShowBrowser, SharedStrings.ShowListOfBlueprintsText);
            if (m_ShowBrowser) {
                BPLoader.GetBlueprintsOfType<T>(bps => {
                    m_Browser = new(BPHelper.GetSortKey, BPHelper.GetSearchKey, bps, null);
                });
                m_Browser?.OnGUI(bp => {
                    Space(10);
                    using (VerticalScope()) {
                        using (HorizontalScope()) {
                            string title;
                            if (bp != CurrentBlueprint) {
                                UI.Button(SharedStrings.PickBlueprintText, () => {
                                    m_CurrentBlueprint = new(bp);
                                    didChange = true;
                                });
                                title = BPHelper.GetTitle(bp).Cyan().Bold();
                            } else {
                                Space(m_ButtonWidth.Value);
                                title = BPHelper.GetTitle(bp).Orange().Bold();
                            }
                            Space(17);
                            UI.Label(title, Width(300));
                            InspectorUI.InspectToggle(bp, "");
                            Space(17);
                            UI.Label(bp.GetType().Name.Grey());
                            Space(17);
                            var tmp = bp.AssetGuid.ToString();
                            UI.TextField(ref tmp, null, AutoWidth(), Width(300));
                            var desc = BPHelper.GetDescription(bp);
                            if (!string.IsNullOrWhiteSpace(desc)) {
                                UI.Label(desc!.Green());
                            }
                        }
                        InspectorUI.InspectIfExpanded(bp);
                    }
                });
            } else {
                using (HorizontalScope()) {
                    UI.Label(SharedStrings.EnterTargetBlueprintIdText, Width(200));
                    var before = m_CurrentlyTyped;
                    UI.TextField(ref m_CurrentlyTyped, null, Width(350));
                    if (before != m_CurrentlyTyped) {
                        m_EnteredInvalidGuid = false;
                    }
                    UI.Button(SharedStrings.PickBlueprintText, () => {
                        var maybeBP = ResourcesLibrary.TryGetBlueprint(BlueprintGuid.Parse(m_CurrentlyTyped)) as T;
                        if (maybeBP != null) {
                            m_CurrentBlueprint = new(maybeBP);
                            didChange = true;
                        } else {
                            m_EnteredInvalidGuid = true;
                        }
                    });
                    if (m_EnteredInvalidGuid) {
                        Space(20);
                        UI.Label(SharedStrings.NoBlueprintWithThatGuidFound.Yellow(), Width(300));
                    }
                }
            }
        }
        return didChange;
    }
}
