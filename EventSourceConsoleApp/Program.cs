using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventSourceInstallerLib;
using Mono.Options;

namespace EventSourceConsoleApp
{
    class Program
    {
        static int verbosity;

        static void Main(string[] args)
        {
            EventSourceInstaller _installer = new EventSourceInstaller();

            bool show_help = false;
            bool install = false;
            bool uninstall = false;
            string manifest = String.Empty;
            string dll = String.Empty;
            string source = String.Empty;
            string destination = String.Empty;

            List<string> names = new List<string>();            

            var p = new OptionSet() {
                { "i|install=", "Install manifest. Add the man file here.",
                  v => install = v != null },
                { "u|uninstall=", "Uninstall manifest.",
                  v => uninstall = v != null },
                { "h|help",  "Show this message and exit.",
                  v => show_help = v != null },                
                { "m|manifest",  "Specify manifest file to install or uninstall.",
                  v => manifest = v },
                { "l|dll",  "Specify dll file to install with your manifest.",
                  v => dll = v },
                { "s|source",  "Specify source folder for dll and minfest file to install. Both files must be in the same source folder. Also needed for uninstall.",
                  v => source = v },
                { "d|destination",  "Specify destination folder where to install the manifest and dll.",
                  v => destination = v },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("etw-installer: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `etw-installer --help' for more information.");
                return;
            }

            if (show_help)
            {
                ShowHelp(p);
                return;
            }

            //_installer.Install(bundle.Manifest, bundle.Dll, bundle.SourcePath, InstallationPathTextBox.Text);
            //_installer.Uninstall(Path.Combine(bundle.SourcePath, bundle.Manifest));
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage for install: etw-installer -i -m [manifest file name]  -l [dll file name] -s [source folder] -d [installation path]");
            Console.WriteLine("Usage for uninstall: etw-installer -u -m [manifest file name]  -s [source folder]");
            Console.WriteLine("etw-installer can install or uninstall etw custom event sources.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
