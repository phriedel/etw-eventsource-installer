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
    public class Program
    {
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
           
            var p = new OptionSet() {
                { "i|install", "Install manifest. Add the man file here.",
                    v => install = v != null },
                { "u|uninstall", "Uninstall manifest.",
                    v => uninstall = v != null },
                { "h|help",  "Show this message and exit.",
                    v => show_help = v != null },
                { "m|manifest=",  "Specify manifest file to install or uninstall.",
                    v => manifest = v },
                { "l|dll=",  "Specify dll file to install with your manifest.",
                    v => dll = v },
                { "s|source=",  "Specify source folder for dll and minfest file to install. Both files must be in the same source folder. Also needed for uninstall.",
                    v => source = v },
                { "d|destination=",  "Specify destination folder where to install the manifest and dll.",
                    v => destination = v },
            };
           
            if (show_help)
            {
                ShowHelp(p);
                return;
            }
            if (install && uninstall)
            {
                Console.Write("etw-installer: Please choose i|install or u|uninstall. You cannot choose both options together.");
            }
            else if (install)
            {
                if (String.IsNullOrWhiteSpace(manifest) ||
                    String.IsNullOrWhiteSpace(dll) ||
                    String.IsNullOrWhiteSpace(source) ||
                    String.IsNullOrWhiteSpace(destination))
                {
                    Console.Write("etw-installer: Installer needs m|manifest= + l|dll= + -s|source= + d|destination=");
                }
                else
                {
                    _installer.Install(manifest, dll, source, destination);
                }
            }
            else if (uninstall)
            {
                if (String.IsNullOrWhiteSpace(manifest) ||
                    String.IsNullOrWhiteSpace(source))
                {
                    Console.Write("etw-installer: Uninstaller needs m|manifest= + s|source=");
                }
                else
                {
                    _installer.Uninstall(Path.Combine(source, manifest));
                }
            }
            else
            {
                Console.Write("etw-installer: Please choose i|install or u|uninstall.");
            }                       
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
