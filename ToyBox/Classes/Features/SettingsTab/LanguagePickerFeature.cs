using System.Globalization;
using ToyBox.Infrastructure.Localization;
using UnityEngine;

namespace ToyBox.Features.SettingsFeatures;
public partial class LanguagePickerFeature : Feature {
    [LocalizedString("ToyBox_Features_SettingsFeatures_LanguagePickerFeature_LanguagePickerText", "Language Picker")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_SettingsFeatures_LanguagePickerFeature_PickYourCurrentUiLocaleText", "Pick your current ui locale")]
    public override partial string Description { get; }
    private static CultureInfo? m_UiCulture;
    private static List<CultureInfo>? m_Cultures;
    [LocalizedString("ToyBox_Features_SettingsFeatures_LanguagePickerFeature_CurrentCultureText", "Current Language")]
    private static partial string CurrentText { get; }
    public override void OnGui() {
        if (m_Cultures == null || m_UiCulture == null) {
            if (Event.current.type != EventType.Repaint) {
                m_UiCulture = CultureInfo.GetCultureInfo(Settings.UILanguage);
                m_Cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(ci => ci.DisplayName).ToList();
            }
        } else {
            using (VerticalScope()) {
                using (HorizontalScope()) {
                    UI.Label(CurrentText.Cyan(), Width(275));
                    Space(25);
                    UI.Label($"{m_UiCulture.DisplayName}({m_UiCulture.Name})".Orange());
                    Space(25);
                }
                Space(15);
                Div.DrawDiv(0, 25);
                var tmp = m_Cultures.IndexOf(m_UiCulture);
                var tmp2 = GUILayout.SelectionGrid(tmp, m_Cultures.Select(c => $"{c.Name.Cyan().Bold()} {c.DisplayName.Orange()}").ToArray(), 6);
                if (tmp != tmp2) {
                    m_UiCulture = m_Cultures[tmp2];
                    Settings.UILanguage = m_UiCulture.Name;
                    LocalizationManager.Update();
                }
            }
        }
    }
}
