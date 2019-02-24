using System;
using System.Windows;
using FontAwesome.WPF;
using MarkdownMonster;
using MarkdownMonster.AddIns;

namespace ConsoleAddin
{
    public class ConsoleAddin : MarkdownMonster.AddIns.MarkdownMonsterAddin
    {
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();


            // Id - should match output folder name. REMOVE 'Addin' from the Id
            Id = "ConsoleAddin";

            // a descriptive name - shows up on labels and tooltips for components
            // REMOVE 'Addin' from the Name
            Name = "ConsoleAddin";


            // by passing in the add in you automatically
            // hook up OnExecute/OnExecuteConfiguration/OnCanExecute
            var menuItem = new AddInMenuItem(this)
            {
                Caption = Name,

                // if an icon is specified it shows on the toolbar
                // if not the add-in only shows in the add-ins menu
                FontawesomeIcon = FontAwesomeIcon.Bullhorn
            };

            // if you don't want to display config or main menu item clear handler
            //menuItem.ExecuteConfiguration = null;

            // Must add the menu to the collection to display menu and toolbar items            
            this.MenuItems.Add(menuItem);
        }

        public override void OnExecute(object sender)
        {
            MessageBox.Show("Hello from your ConsoleAddin Addin", "ConsoleAddin Addin",
                MessageBoxButton.OK, MessageBoxImage.Information);


            // *** Some things you can do:

            // // modify selected editor text
            //var text = GetSelection();
            //text = "<small>" + text + "</small>";
            //SetSelection(text);
            //RefreshPreview();

            // // open a new tab with a file
            //OpenTab(Path.Combine(mmApp.Configuration.CommonFolder, "ConsoleAddin.json"));

            // // run a process
            //var imageFile = GetSelection();  // assume image file is selected
            //if (!imageFile.Contains(":\\"))
            //    imageFile = Path.Combine(Path.GetDirectoryName(ActiveDocument.Filename), imageFile);
            //Process.Start("paint.exe", imageFile);
        }

        public override void OnExecuteConfiguration(object sender)
        {
            MessageBox.Show("Configuration for our sample Addin", "Markdown Addin Sample",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public override bool OnCanExecute(object sender)
        {
            return true;
        }

    }
}
