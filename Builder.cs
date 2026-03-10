using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using Gtk;
using Adw;

/* 

    [ LATEST UPDATE - READ ]
    Version 0.6.2 of GirCore broke dynamic instancing - 0.6.0 preview works
    Library is fully functional, with the exception of StringList { strings[] };
    Which I won't be implementing as I will no longer work on this project.
    
    The initial idea was creating an easy-to-use IDE for GTK/Adwaita that updates the UI as you write it.
    Think of it like an HTML live server - but for GTK/Adwaita.

    [ About ]
    Blueprint# is a library meant to implement Blueprint code directly into C# gtk/libadwaita apps.
    With simplicity in mind, it's as easy as writing a single line of code to build your C# application UI with Blueprint.

    [ Important ]
    Project in it's current state is more of a conceptual idea.

    It can be further improved, this function would be convenient:
    Widget.GetChild(int parent_index, int child_at_ypos = 0);
    
    I made the `Builder` class static to make it as easy to use as possible.
    Changing it to a normal class could open the door to implementing a lot more features.
    Perhaps both could be done and the user can choose in some way.

*/

namespace BlueprintSharp;
public static class Builder
{
    // Characters that trigger different actions when reading the blueprint
    /* Have to add [ ] here to read styles I'm pretty sure */
    private static char[] Triggers = { ';', ':', '{', '}', '[', ']' };

    // Create a temporary tree to hold all the widgets so you can easily add each element to it's parent
    // Once you're done with Tree, you can set it to null to free memory
    private static Dictionary<int, List<Widget>> Tree = new Dictionary<int, List<Widget>>();

    // Create a dictionary to store the widgets themselves in order to be able to retrieve them later
    private static Dictionary<string, Widget> Widgets = new Dictionary<string, Widget>();

    // Create a temporary instance
    private static Widget Instance = null!;
    private static String[] Property = new String[2];

    // Create a temporary nested instance
    private static Widget Nested_Instance = null!;
    private static String[] Nested_Property = new String[2];

    // Declare a new ReadingModes class to keep track within the Blueprint reader
    private static ReadingModes Reading = new ReadingModes();

    public static Widget Build(string blueprint_file)
    {
        // Get the blueprint from the passed argument
        // The argument can either be a direct path or a name of the resource bundled within the app
        string blueprint_content = GetContent(blueprint_file);

        StringReader reader = new StringReader(blueprint_content);
        StringBuilder sb = new StringBuilder();

        int current_char;
        int bracket_index = -1;

        // Read the Blueprint
        while ((current_char = reader.Read()) != -1) {
            char c = (char)current_char;

            if (Reading.String && Triggers.Contains(c)) {
                sb.Append(c);
                continue;
            }

            switch (c) {
                // Blueprint string declaration handler
                case '"':
                case '\'':
                {
                    bool IsClosing = (Reading.Delimiter == c);

                    // If the character isn't the same as the one that opened the string
                    // Just append the StringBuilder class and keep on reading
                    if (!IsClosing) {
                        sb.Append(c);
                        break;
                    }

                    // In case the `Reading.String` mode is active and it came across a string closing character
                    // Check if there's an escape character before it to know whether to close it or append the StringBuilder
                    // Otherwise, check if the declared string is translatable and handle the string reading mode
                    if (Reading.String && IsClosing) {
                        if (sb.GetLast() is '\\') {
                            sb.SetLast(c);
                        } else {
                            Reading.String = false;
                        }
                    } else {
                        if (sb.ToString().EndsWith("_(")) {
                            Reading.Translatable = true;
                        }
                        Reading.Delimiter = c;
                        Reading.String = true;
                    }
                }
                break;

                // Widget property end handler
                case ';':
                {
                    string property_value = sb.GetText(false).Trim();
                    
                    // Set nested widget as the property
                    if (sb.GetLast() is '}') {
                        Reading.Nested = false;
                        Instance.SetProperty(Property[0], Nested_Instance);
                        Property = new String[2];
                        Nested_Property = new String[2];
                        break;
                    }

                    if (Reading.Translatable) {
                        ConvertTranslatable(ref property_value);
                        Reading.Translatable = false;
                    }
                    
                    // Handle the property
                    if (Reading.Nested) 
                        Console.WriteLine($"Handling property: {Nested_Property[0]}: {property_value}");
                    else
                        Console.WriteLine($"Handling property: {Property[0]}: {property_value}");
                    HandleProperty(property_value);

                    sb.Reset();
                    Reading.Property = false;
                }
                break;

                // Blueprint widget as property handler
                case ':':
                {
                    Reading.Property = true;
                    string Name = sb.GetText().Trim();
                    ConvertProperty(ref Name);

                    if (Reading.Nested) {
                        Nested_Property[0] = Name;
                    } else {
                        Property[0] = Name;
                    }
                }
                break;

                // Blueprint widget declaration
                case '{':
                {
                    bracket_index++;
                    if (!Tree.HasIndex(bracket_index)) {
                        Tree.CreateIndex(bracket_index);
                    }

                    string Role = string.Empty;
                    string Type = string.Empty;
                    string Name = string.Empty;

                    string widget_info = Clean(sb.GetText());
                    int[] parent_pos = { bracket_index - 1, Tree.CountForIndex(bracket_index - 1) - 1 };
                    int[] widget_pos = { bracket_index, Tree.CountForIndex(bracket_index) };

                    string[] Parsed = widget_info.Split(" ");
                    if (Parsed.Length < 1) {
                        throw new ArgumentException("Widget info must contain at least one word.", nameof(Type));
                    }

                    // Parse widget's role, type and name
                    if (Parsed[0].StartsWith("[") && Parsed[0].EndsWith("]")) {
                        if (Parsed.Length < 2) {
                            throw new ArgumentException($"Widget info {widget_info} must contain a class type.", nameof(Type));
                        }
                        Role = FormatRole(Parsed[0]);
                        Type = Parsed[1];
                        if (Parsed.Length == 3) {
                            Name = Parsed[2];
                        }
                    } else {
                        Type = Parsed[0];
                        if (Parsed.Length == 2) {
                            Name = Parsed[1];
                        }
                    }

                    Console.WriteLine($"New element: {Role} | {Type} | {Name}"); // debug

                    // Create a new nested instance if reader is reading 
                    if (Reading.Property) {
                        Reading.Nested = true;
                        Nested_Instance = null!;
                        Nested_Instance = InstanceFromString(Type);
                    } else {
                        Instance = null!;
                        Instance = InstanceFromString(Type);
                        Tree.Push(Instance, bracket_index);

                        if (!string.IsNullOrEmpty(Name)) {
                            Widgets[Name] = Instance;
                        }

                        if (!string.IsNullOrEmpty(Role)) {
                            Widget Parent = Tree[parent_pos[0]][parent_pos[1]];
                            Parent.Invoke(Role, Instance);
                            break;
                        }

                        if (bracket_index > 0) {
                            Widget Parent = Tree[parent_pos[0]][parent_pos[1]];
                            Instance.Invoke("SetParent", Parent);
                        }
                    }
                }
                break;

                // Closing of a widget (including nested ones)
                case '}':
                    bracket_index--;
                break;

                // Make sure reader came across CSS classes
                case '[':
                {
                    if (sb.ToString().Trim() == "styles") {
                        sb.Reset();
                        Reading.Styles = true;
                    } else {
                        sb.Append(c);
                    }
                }
                break;

                // Apply CSS classes to the widget
                case ']':
                {
                    if (Reading.Styles) {
                        string array_str = Clean(sb.GetText());
                        string[] Styles = StylesToArray(array_str);
                        Reading.Styles = false;
                        if (Reading.Nested) {
                            Nested_Instance.SetProperty("CssClasses", Styles);
                        } else {
                            Instance.SetProperty("CssClasses", Styles);
                        }
                    } else {
                        sb.Append(c);
                    }
                }
                break;

                // Just append the StringBuilder class by default
                default:
                    sb.Append(c);
                break;
            }
        }

        return Tree[0][0];
    }

    // Apply property to widget instance
    // Application of Nested.Widget is still handled through Widget.SetProperty function
    private static void HandleProperty(string property_value)
    {
        Widget Target_Instance = Reading.Nested ? Nested_Instance : Instance;
        string Target_Property = Reading.Nested ? Nested_Property[0] : Property[0];
        PropertyInfo? Property_Info = Target_Instance.GetType().GetProperty(Target_Property);

        if (Property_Info != null && Property_Info.CanWrite)
            Property_Info.SetValue(Target_Instance, Convert.ChangeType(property_value, Property_Info.PropertyType));
        else
        {
            string Method_Name = $"Set{Target_Property}";
            MethodInfo? Method_Info = Target_Instance.GetType().GetMethod(Method_Name);

            if (Method_Info == null)
                throw new ArgumentException($"No {Target_Property} or {Method_Name} found for {Target_Instance.GetType().Name}");
            
            Type Param_Type = Method_Info.GetParameters()[0].ParameterType;
            object? Converted_Value;

            if (Param_Type.IsEnum)
                if (Enum.TryParse(Param_Type, property_value.Trim(), true, out object? enum_value))
                    Converted_Value = enum_value;
                else
                    throw new ArgumentException($"Invalid enum value '{property_value}' for {Param_Type.Name}");
            else
                Converted_Value = Convert.ChangeType(property_value, Param_Type);

            Method_Info.Invoke(Target_Instance, new[] { Converted_Value });
        }
    }

    // Reflection function to create a Gtk.Widget instance from a string
    public static Widget InstanceFromString(string className)
    {
        string AssemblyName = "Gtk-4.0";
        if (className.StartsWith("Adw")) {
            AssemblyName = "Adw-1";
        } else {
            if (!className.StartsWith("Gtk")) {
                className = "Gtk." + className;
            }
        }
        className += $", {AssemblyName}";
        
        var type = Type.GetType(className);
        if (type == null) {
            throw new TypeLoadException($"Type {className} not found.");
        }

        var instance = Activator.CreateInstance(type) as Widget;
        if (instance == null) {
            throw new InvalidOperationException($"An instance of type '{type.FullName}' could not be created.");
        }

        return instance;
    }

    // Retrieve a widget by it's Blueprint name
    public static Widget GetByName(string name)
    {
        if (Widgets.ContainsKey(name)) {
            return Widgets[name];
        }
        throw new KeyNotFoundException($"No widget found with the name '{name}'.");
    }

    // Converts the property name into the format that Gtk and Adw libraries use
    // example-property-name -> ExamplePropertyName
    private static void ConvertProperty(ref string property)
    {
        string[] parts = property.Split("-");
        property = string.Empty;
        foreach(string p in parts) {
            property += char.ToUpper(p[0]) + p.Substring(1);
        }
    }

    // Converts a translatable string into a normal one
    // +15 knowledge points for using a reference instead of returning a string
    private static void ConvertTranslatable(ref string str)
    {
        if (str.StartsWith("_(") && str.EndsWith(")")) {
            str = str.Substring(2, str.Length - 3);
        }
    }

    // Get text content from the provided Blueprint file
    // Uses absolute path, then checks in the assembly's resources if it doesn't exist
    private static string GetContent(string blueprint_file)
    {
        if (File.Exists(blueprint_file))
        {
            return Format(File.ReadAllText(blueprint_file));
        }
        else
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream? stream = assembly.GetManifestResourceStream(blueprint_file);
            if (stream != null)
            {
                using StreamReader reader = new StreamReader(stream);
                return Format(reader.ReadToEnd());
            }
            else
            {
                throw new InvalidOperationException($"Path or resource '{blueprint_file}' not found.");
            }
        }
    }

    // Format provided Blueprint file into a readable version
    private static string Format(string blueprint)
    {
        // Remove imports
        string pattern = @"^\s*using\s.*?;$";
        blueprint = Regex.Replace(blueprint, pattern, string.Empty, RegexOptions.Multiline);

        // Remove empty new lines
        pattern = @"^\s*$[\r\n]*";
        return Regex.Replace(blueprint, pattern, string.Empty, RegexOptions.Multiline);
    }

    // Convert string into a cleaner version
    private static string Clean(string str) 
    {
        return Regex.Replace(str.Trim(), @"\s+", " ");
    }

    // Convert role to a format compatible with Gtk and Adwaita libs
    private static string FormatRole(string str)
    {
        str = str.TrimStart('[').TrimEnd(']');

        /* [ ! ] Might be missing some, feel free to add */
        switch (str)
        {
            case "prefix":
            case "suffix":
                str = "Add" + char.ToUpper(str[0]) + str.Substring(1);
            break;

            case "start":
            case "end":
                str = "Pack" + char.ToUpper(str[0]) + str.Substring(1);
            break;

            case "titlebar":
                str = "SetTitlebar";
            break;
        }

        return str;
    }

    private static string[] StylesToArray(string str)
    {
        str = str.TrimEnd(',');
        string[] array = str.Split(',').Select(s => s.Trim()).ToArray();
        return array;
    }
}