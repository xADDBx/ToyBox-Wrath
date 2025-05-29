namespace ToyBox.Infrastructure.Enums;
public enum LogLevel {
    Error,
    Warning,
    Info,
    Debug,
    Trace
}
public static partial class LogLevel_Localizer {
    public static string GetLocalized(this LogLevel type) {
        return type switch {
            LogLevel.Error => ErrorText,
            LogLevel.Warning => WarningText,
            LogLevel.Info => InfoText,
            LogLevel.Debug => DebugText,
            LogLevel.Trace => TraceText,
            _ => "!!Error Unknown LogLevel!!",
        };
    }

    [LocalizedString("ToyBox_Infrastructure_Enums_LogLevel_Localizer_ErrorText", "Error")]
    private static partial string ErrorText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_LogLevel_Localizer_WarningText", "Warning")]
    private static partial string WarningText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_LogLevel_Localizer_InfoText", "Info")]
    private static partial string InfoText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_LogLevel_Localizer_DebugText", "Debug")]
    private static partial string DebugText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_LogLevel_Localizer_TraceText", "Trace")]
    private static partial string TraceText { get; }
}
