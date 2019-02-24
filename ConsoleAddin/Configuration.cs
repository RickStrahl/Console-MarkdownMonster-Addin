using MarkdownMonster.AddIns;

namespace ConsoleAddin
{
    public class ConsoleAddinConfiguration : BaseAddinConfiguration<ConsoleAddinConfiguration>
    {
        public ConsoleAddinConfiguration()
        {
            // uses this file for storing settings in `%appdata%\Markdown Monster`
            // to persist settings call `ConsoleAddinConfiguration.Current.Write()`
            // at any time or when the addin is shut down
            ConfigurationFilename = "ConsoleAddin.json";
        }

        // Add properties for any configuration setting you want to persist and reload
        // you can access this object as 
        //     ConsoleAddinConfiguration.Current.PropertyName
    }
}