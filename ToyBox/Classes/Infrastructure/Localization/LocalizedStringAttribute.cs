using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyBox;
[AttributeUsage(AttributeTargets.Field)]
public class LocalizedStringAttribute : Attribute {
    public string Key { get; }
    public LocalizedStringAttribute(string key) {
        Key = key;
    }
}
