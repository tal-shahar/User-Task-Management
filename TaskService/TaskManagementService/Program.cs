using System;
using System.ServiceProcess;
using System.Threading;

namespace TaskManagementService
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new TaskReminderService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

