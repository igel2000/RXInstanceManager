using System;
using System.Linq;
using System.ServiceProcess;

namespace RXInstanceManager
{
    public static class AppHandlers
    {
        #region Работа с ServiceRunner.

        public static string GetServiceStatus(Instance instance)
        {
            var serviceStatus = Constants.InstanceStatus.NeedInstall;

            using (var service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == instance.ServiceName))
            {
                if (service != null)
                    serviceStatus = service.Status == ServiceControllerStatus.Running ?
                        Constants.InstanceStatus.Working :
                        Constants.InstanceStatus.Stopped;
            }

            return serviceStatus;
        }

        #endregion
    }
}
