using System.Text;
using System.Reflection;
using Gtk;
using Adw;

namespace BlueprintSharp
{
    // StringBuilder Extensions
    public static class Extensions
    {
        public static string GetText(this StringBuilder sb, bool reset = true) {
            string result = sb.ToString();
            if (reset) { 
                sb.Length = 0;
            }
            return result;
        }

        public static void SetText(this StringBuilder sb, string str) {
            sb.Length = 0;
            sb.Append(str);
        }

        public static void Reset(this StringBuilder sb) {
            sb.Length = 0;
        }

        public static char GetLast(this StringBuilder sb, bool NoWhiteSpace = true) {
            if (sb.Length == 0) {
                throw new IndexOutOfRangeException("StringBuilder is empty.");
            }

            if (NoWhiteSpace) {
                for (int i = sb.Length - 1; i >= 0; i--) {
                    char c = sb[i];

                    if (!char.IsWhiteSpace(c)) {
                        return c;
                    }
                }
                throw new InvalidOperationException("No non-whitespace character found.");
            } else {
                return sb[sb.Length - 1];
            }
        }

        public static void SetLast(this StringBuilder sb, char c) {
            if (sb.Length > 0) {
                sb[sb.Length - 1] = c;
            } else {
                throw new IndexOutOfRangeException("StringBuilder is empty.");
            }
        }
    }

    // Gtk.Widget Extensions
    public static class WidgetExtensions
    {
        public static void SetProperty(this Widget widget, string propertyName, object value)
        {
            if (widget == null) throw new ArgumentNullException(nameof(widget));
            
            var _type = widget.GetType();
            var p_info = _type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            
            if (p_info != null && p_info.CanWrite)
            {
                object convertedValue = Convert.ChangeType(value, p_info.PropertyType);
                p_info.SetValue(widget, convertedValue);
            }
            else
            {
                throw new ArgumentException($"Property '{propertyName}' not found or cannot be written.");
            }
        }

        public static void Invoke(this Widget widget, string methodName, params object[] values)
        {
            if (widget == null) throw new ArgumentNullException(nameof(widget));
            
            var widgetType = widget.GetType();
            var methodInfo = widgetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            
            if (methodInfo == null)
            {
                throw new ArgumentException($"Method '{methodName}' not found on widget of type '{widgetType.Name}'.");
            }

            var methodParameters = methodInfo.GetParameters();
            
            if (values.Length != methodParameters.Length)
            {
                throw new ArgumentException($"Method '{methodName}' expects {methodParameters.Length} parameters, but {values.Length} were provided.");
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != null && !methodParameters[i].ParameterType.IsAssignableFrom(values[i].GetType()))
                {
                    throw new ArgumentException($"Parameter {i + 1} is of the wrong type. Expected '{methodParameters[i].ParameterType.Name}', but received '{values[i].GetType().Name}'.");
                }
            }

            methodInfo.Invoke(widget, values);
        }
    }

    // Dictionary Extensions for widget tree
    public static class DictionaryExtensions
    {
        public static void Push(this Dictionary<int, List<Widget>> d, Widget w, int bracket_index) {
            if (w == null) {
                throw new ArgumentNullException(nameof(w), "Widget cannot be null.");
            }

            if (!d.ContainsKey(bracket_index)) {
                d[bracket_index] = new List<Widget>();
            }

            d[bracket_index].Add(w);
        }

        public static void CreateIndex(this Dictionary<int, List<Widget>> d, int bracket_index) {
            if (!d.ContainsKey(bracket_index)) {
                d[bracket_index] = new List<Widget>();
            } else {
                throw new ArgumentException($"Bracket index `{bracket_index}` already exists.");
            }
        }

        public static bool IsEmpty(this Dictionary<int, List<Widget>> d) => d.Count == 0;
        
        public static bool HasIndex(this Dictionary<int, List<Widget>> d, int bracket_index) => d.ContainsKey(bracket_index);
        
        public static int CountForIndex(this Dictionary<int, List<Widget>> d, int bracket_index) {
            try {
            return d[bracket_index].Count();
            } catch {
            return 0;
            }
        }
    }
}
