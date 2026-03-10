using Gtk;
using Adw;
using BlueprintSharp;
using Builder = BlueprintSharp.Builder;

var application = Gtk.Application.New("com.test.app", Gio.ApplicationFlags.FlagsNone);
application.OnActivate += (sender, args) =>
{
    var window = Gtk.ApplicationWindow.New((Gtk.Application) sender);
    window.Title = "BlueprintSharp Test";
    window.SetDefaultSize(300, 300);

    // Blueprint# Example
    var btn = Builder.Build("button.blp");

    // Add the button we just created to the window and show the application
    window.Child = btn;
    window.Show();
};
return application.RunWithSynchronizationContext(null);