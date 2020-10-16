using System;
using System.Diagnostics;
using System.Windows;

namespace AllocationRerouter
{
    class Program
    {
        [STAThread()]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowGUI();
            }

            else
            {
                /*var getNewPartsTask = Task.Run(() => XMLManager.GetPartLookup(@"C:\Users\mtadara1\Desktop\HAP mapping\New L550 L551.xml", false));
                var getOldPartsTask = Task.Run(() => XMLManager.GetPartLookup(@"C:\Users\mtadara1\Desktop\HAP mapping\Old L550 L551.xml", true));

                Task.WaitAll(new[] { getNewPartsTask, getOldPartsTask });

                var newParts = getNewPartsTask.Result;
                var oldParts = getOldPartsTask.Result;

                var flows = XMLManager.ReadFlows(@"C:\Users\mtadara1\Desktop\HAP mapping\PTA HAP Process.xml");

                var outputStream = File.Create(@"C:\Users\mtadara1\Desktop\HAP mapping\Mapping.xml");*/

                //XMLManager.Remap(flows, oldParts, newParts, outputStream);
            }
        }

        [DebuggerNonUserCode()]
        static void ShowGUI()
        {
            new Application().Run(new MainWindow());
        }
    }
}
