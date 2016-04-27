using System;
using EventSourceInstallerLib;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            EventSourceInstaller e = new EventSourceInstaller();

            e.Install(
                @"EventSourceExample.Company-MyProject-WebApp.etwManifest.man",
                @"EventSourceExample.Company-MyProject-WebApp.etwManifest.dll",
                @"C:\src\EventSource Tests\EventSourceExample\bin\Debug\",
                @"C:\CustomEventSources\Test"
            );

            //e.Uninstall(@"C:\CustomEventSources\Test\EventSourceExample.Company-MyProject-WebApp.etwManifest.man");

            Console.ReadLine();
        }
    }
}
