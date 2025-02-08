using System.Globalization;
using ToyBox.Infrastructure.Localization;
using ToyBox.Infrastructure.UI;
using UnityEngine;

namespace ToyBox.Features.SettingsFeature;
public class LanguagePickerFeature : Feature {
    [LocalizedString("Features.Settings.LanguagePickerFeature.Name")]
    private static string m_Name = "Language Picker";
    public override string Name => m_Name;
    [LocalizedString("Features.Settings.LanguagePickerFeature.Desc")]
    private static string m_Description = "Pick your current ui locale";
    public override string Description => m_Description;
    private static CultureInfo? m_UiCulture;
    private static List<CultureInfo>? m_Cultures;
    [LocalizedString("Features.Settings.LanguagePickerFeature.Current")]
    private static string m_CurrentText = "Current Culture";
    [LocalizedString("Features.Settings.LanguagePickerFeature.ExportCurrent")]
    private static string m_ExportCurrentText = "Export current locale to file";
    public override void OnGui() {
        if (m_Cultures == null || m_UiCulture == null) {
            if (Event.current.type != EventType.Repaint) {
                m_UiCulture = CultureInfo.GetCultureInfo(Settings.UILanguage);
                m_Cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(ci => ci.DisplayName).ToList();
            }
        } else {
            using (VerticalScope()) {
                using (HorizontalScope()) {
                    GUILayout.Label(m_CurrentText.Cyan(), GUILayout.Width(275));
                    GUILayout.Space(25);
                    GUILayout.Label($"{m_UiCulture.DisplayName}({m_UiCulture.Name})".Orange());
                    GUILayout.Space(25);
                    if (GUILayout.Button(m_ExportCurrentText.Cyan())) {
                        LocalizationManager.Export(Settings.UILanguage);
                    }
                    GUILayout.Space(25);
                }
                GUILayout.Space(15);
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
