using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModKit.Utility.Extensions {
    public static class MiscExtensions {
        // Takes the last N objects of the source collection
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N) {
            return source.Skip(Math.Max(0, source.Count() - N));
        }
        // Creates readable collection content string
        public static string ToContentString(this IEnumerable enumerable) {
            return InternalToContentString(enumerable);
        }
        private static string InternalToContentString(object obj) {
            if (obj == null) {
                return "null";
            }

            if (obj is string str) {
                return $"\"{str}\"";
            }

            if (obj is IEnumerable enumerable && !(obj is IDictionary)) {
                var elements = new List<string>();

                foreach (var item in enumerable) {
                    elements.Add(InternalToContentString(item));
                }

                return "[" + string.Join(", ", elements) + "]";
            }

            if (obj is IDictionary dictionary) {
                var elements = new List<string>();

                foreach (DictionaryEntry entry in dictionary) {
                    elements.Add($"{InternalToContentString(entry.Key)}: {InternalToContentString(entry.Value)}");
                }

                return "{" + string.Join(", ", elements) + "}";
            }

            return obj.ToString();
        }
    }
}
