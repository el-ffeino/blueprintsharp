# About

Blueprint# is a library meant to implement Blueprint code directly into C# gtk/libadwaita apps.  
With simplicity in mind, it's as easy as writing a single line of code to build your C# application UI with Blueprint.  

It would allow you to use [Blueprint](https://github.com/GNOME/blueprint-compiler) markup language directly inside your applcation without having to *compile* it into the traditional XML `.ui` format.

## Example
`Button.blp`:  
*This can either be an absolute path or a resource within the application.*
```
Button regular {
  name: "regular";
  label: _("Regular");
  margin-end: 40;
}
```
`Program.cs`:
```
var MyButton = Builder.Build("Button.blp");

/* Alternatively, you may also declare it as such: */
var MyButton = Builder.Build("Button.blp") as Gtk.Button;

MyButton.Label = "Hello there!";
```

## Try it yourself
- Clone the project
- Inside Blueprint# directory run `dotnet restore && dotnet build`
- Create a new project with `dotnet new console`
- Inside your `.csproj` reference Blueprint# and GirCore bindings for Adwaita & GTK*
- Refer to the example above

You might wanna check `test/` directory for a complete example.  
\**Version 0.6.0 of GirCore is the last supported one*

## Motivation
Project was originally meant to be an IDE to make the whole process of writing Libadwaita applications in C# as easy as possible, aiming for the low cortisol levels of writing UI applications that HTML/CSS/JS stack provides.  
However, taking on the Chromium cartel is one thing, while keeping up with other Linux software devs who introduce compatiblity breaking updates to projects that depend on it for no reason is another thing.  

Besides the IDE with live preview, something like this **would** have been a valid GTK C# application without all the extra lines and nesting that it currently requires (see `test/`):
```
// Run the app with a single line of code
var Window = Initialize("com.test.app", "ui.blp");

// Retrieve custom element from the root element
var AboutTab = Window.GetByName("MyAboutTab");

// Fill the AboutTab with Blueprint contents from another file
AboutTab.SetContent(Builder.Build("about.blp")); 
```

[Latest update]  
Version 0.6.2 of GirCore broke dynamic instancing - 0.6.0 preview works  
Library is fully functional, with the exception of StringList { strings[] };  
Which I won't be implementing as I will no longer work on this project since I don't have the time to write my own bindings

[Important]  
Project in it's current state is more of a conceptual idea.  
It can be further improved, this function would be convenient:  
`Widget.GetChild(int parent_index, int child_at_ypos = 0);`  
