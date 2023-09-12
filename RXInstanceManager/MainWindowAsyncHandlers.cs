using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Threading.Tasks;

namespace RXInstanceManager
{
  public partial class MainWindow : Window
  {
    private async Task UpdateInstanceGridAsync()
    {
      while (true)
      {
        await Task.Delay(TimeSpan.FromSeconds(2));

        foreach (var instance in GridInstances.Items.Cast<Instance>())
        {
          var status = AppHandlers.GetServiceStatus(instance);
          if (status != instance.Status)
            LoadInstances(_instance.InstancePath);
        }
      }
    }

    private async Task UpdateInstanceDataAsync()
    {
      while (true)
      {
        await Task.Delay(TimeSpan.FromSeconds(2));

        foreach (var instance in GridInstances.Items.Cast<Instance>())
        {
          var configYamlPath = AppHelper.GetConfigYamlPath(instance.InstancePath);
          if (File.Exists(configYamlPath))
          {
            var changeTime = AppHelper.GetFileChangeTime(configYamlPath);
            if (instance.ConfigChanged == null ||  changeTime.MoreThanUpToSeconds(instance.ConfigChanged))
            {
              AppHandlers.UpdateInstanceData(instance);
              LoadInstances(_instance.InstancePath);
            }
          }
        }
      }
    }
  }
}
