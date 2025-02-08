using UnityEngine;

namespace ToyBox.Infrastructure.UI;
public static class StringExtensions {
    public static string Size(this string s, int size) => $"<size={size}>{s}</size>";
    public static string Bold(this string s) => $"<b>{s}</b>";
    public static string Color(this string s, string color) => $"<color={color}>{s}</color>";
    public static string Color(this string str, Color color) => $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{str}</color>";
    public static string White(this string s) => s.Color("white");
    public static string Grey(this string s) => s.Color("#A0A0A0FF");
    public static string DarkGrey(this string s) => s.Color("#505050FF");
    public static string Red(this string s) => s.Color("#C04040E0");
    public static string Green(this string s) => s.Color("#00ff00ff");
    public static string Blue(this string s) => s.Color("blue");
    public static string Cyan(this string s) => s.Color("cyan");
    public static string Magenta(this string s) => s.Color("magenta");
    public static string Yellow(this string s) => s.Color("yellow");
    public static string Orange(this string s) => s.Color("orange");
    public static string SizePercent(this string s, int percent) => $"<size={percent}%>{s}</size>";
}
