using System;
using System.Diagnostics;
using System.IO;

namespace EventSourceInstallerLib
{
    public delegate void StatusMessageEventHandler(object sender, EventArgs e);

    public class EventSourceInstaller
    {
        #region Member

        public event StatusMessageEventHandler _newStatuMessageEvent;
        private TextWriter _out = Console.Out;

        #endregion

        #region Install

        public void Install(string manFile, string dllFile, string sourceFolder, string destinationFolder)
        {
            var sourceManFile = Path.Combine(sourceFolder, Path.GetFileName(manFile));
            var sourceDllFile = Path.Combine(sourceFolder, Path.GetFileName(dllFile));
            var destinationManFile = Path.Combine(destinationFolder, Path.GetFileName(manFile));
            var destinationDllFile = Path.Combine(destinationFolder, Path.GetFileName(dllFile));

            // To avoid installation issues we make sure there is no previous installation for this EventSource
            Uninstall(sourceManFile);

            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            destinationFolder = AlignFolderName(destinationFolder);
            sourceFolder = AlignFolderName(sourceFolder);

            // If destination and source is not identical we do not need to copy the files
            if (destinationFolder != sourceFolder)
            {
                File.Copy(sourceManFile, destinationManFile, true);
                File.Copy(sourceDllFile, destinationDllFile, true);
            }

            var commandArgs = string.Format("im \"{0}\" /rf:\"{1}\" /mf:\"{1}\"",
                destinationManFile,
                destinationDllFile
            );

            ExecuteWevtutil(manFile, commandArgs);
        }

        private static string AlignFolderName(string destinationFolder)
        {
            if (destinationFolder.EndsWith("\\"))
            {
                destinationFolder = destinationFolder.Remove(destinationFolder.Length - 1);
            }

            return destinationFolder;
        }

        #endregion

        #region Uninstall

        public void Uninstall(string manFile)
        {
            var commandArgs = string.Format("um \"{0}\"", manFile);

            ExecuteWevtutil(manFile, commandArgs);
        }

        #endregion

        #region Utilities

        private void ExecuteWevtutil(string manFile, string commandArgs)
        {
            // The 'RunAs' indicates it needs to be elevated.  
            var process = Process.Start(new ProcessStartInfo(@"C:\Windows\System32\wevtutil.exe", commandArgs)
            {
                //Process will be started as admin
                Verb = "runAs",
                //Do not show the shell window
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = false,
                RedirectStandardInput = false,

            });

            string error = process.StandardError.ReadToEnd();

            _out.WriteLine(String.Format("wevtutil.exe {0}", commandArgs));


            if (!String.IsNullOrEmpty(error))
            {
                _out.WriteLine(error);
                OnNewStatusMessage(Path.GetFileNameWithoutExtension(manFile), error, EventArgs.Empty);
            }
            else
            {
                var command = commandArgs.Substring(0, 2)
                    .Replace("im", "Install manifest")
                    .Replace("um", "Uninstall manifest");
                var message = String.Format("{0} successful.", command);

                OnNewStatusMessage(Path.GetFileNameWithoutExtension(manFile), message, EventArgs.Empty);
            }

            process.WaitForExit();
        }

        protected virtual void OnNewStatusMessage(string manifestKey, string errorMessage, EventArgs e)
        {
            if (_newStatuMessageEvent != null)
            {
                _newStatuMessageEvent(new StatusEventMessage(manifestKey, errorMessage), e);
            }
        }

        #endregion
    }
}