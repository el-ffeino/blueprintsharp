using System;
using System.Text;

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

namespace BlueprintSharp
{
    // Different types of reading modes for the BlueprintReader
    internal class ReadingModes
    {
        internal bool Nested { get; set; } = false;
        internal bool Property { get; set; } = false;
        internal bool String { get; set; } = false;
        internal bool Translatable { get; set; } = false;
        internal bool Styles { get; set; } = false;
        internal char Delimiter { get; set; } = '"';
    }
}
