using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.ServiceProcess;
using System.Diagnostics;
using YamlHandlers;

namespace RXInstanceManager
{
    public partial class MainWindow : Window
    {
        private static string _processLog { get; set; }

        #region Работа с Grid.

        private void LoadInstances()
        {
            LoadInstancesItems();
        }

        private void LoadInstances(Instance instance)
        {
            LoadInstancesItems(instance);
        }

        private void LoadInstancesItems()
        {
            GridInstances.ItemsSource = Instances.Get().OrderBy(x => x.Id).ThenBy(x => x.Status);
        }

        private void LoadInstancesItems(Instance instance)
        {
            LoadInstancesItems();
            CollectionViewSource.GetDefaultView(GridInstances.ItemsSource).Refresh();
            GridInstances.UpdateLayout();

            for (int i = 0; i < GridInstances.Items.Count; i++)
            {
                var item = GridInstances.Items[i] as Instance;
                if (item.Id == instance.Id)
                {
                    GridInstances.SelectedItem = item;
                    GridInstances.ScrollIntoView(item);
                    GridInstances.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    break;
                }
            }
        }

        #endregion

        #region Работа с визуальными эффектами.

        private void EditStyleChanging(TextBox textbox, string emptyValue)
        {
            textbox.Style = (string.IsNullOrWhiteSpace(textbox.Text) || textbox.Text == emptyValue) ?
                this.FindResource("EditTextBoxEmpty") as Style :
                this.FindResource("EditTextBoxEdit") as Style;
        }

        private void ActionButtonVisibleChanging(string status = null)
        {
            ButtonCopy.Visibility = Visibility.Collapsed;
            ButtonInstall.Visibility = Visibility.Collapsed;
            ButtonDelete.Visibility = Visibility.Collapsed;
            ButtonDDSStart.Visibility = Visibility.Collapsed;
            ButtonRXStart.Visibility = Visibility.Collapsed;

            StartContext.Visibility = Visibility.Collapsed;
            StopContext.Visibility = Visibility.Collapsed;
            RestartContext.Visibility = Visibility.Collapsed;

            switch (status)
            {
                case Constants.InstanceStatus.NeedInstall:
                    //ButtonCopy.Visibility = Visibility.Visible;
                    ButtonInstall.Visibility = Visibility.Visible;
                    ButtonDelete.Visibility = Visibility.Visible;
                    break;
                case Constants.InstanceStatus.Working:
                    //ButtonCopy.Visibility = Visibility.Visible;
                    ButtonDelete.Visibility = Visibility.Visible;
                    ButtonDDSStart.Visibility = Visibility.Visible;
                    ButtonRXStart.Visibility = Visibility.Visible;
                    StopContext.Visibility = Visibility.Visible;
                    RestartContext.Visibility = Visibility.Visible;
                    break;
                case Constants.InstanceStatus.Stopped:
                    //ButtonCopy.Visibility = Visibility.Visible;
                    ButtonDelete.Visibility = Visibility.Visible;
                    StartContext.Visibility = Visibility.Visible;
                    RestartContext.Visibility = Visibility.Visible;
                    break;
            }
        }

        #endregion

        #region Работа с процессами.

        private static void LaunchProcess(string fileName)
        {
            using (var build = new Process())
            {
                build.StartInfo.FileName = fileName;
                build.Start();
            }
        }

        private static void LaunchProcess(string fileName, string args)
        {
            using (var build = new Process())
            {
                build.StartInfo.FileName = fileName;
                build.StartInfo.Arguments = args;

                build.StartInfo.UseShellExecute = false;
                build.StartInfo.RedirectStandardOutput = true;
                build.StartInfo.RedirectStandardInput = true;
                build.StartInfo.RedirectStandardError = true;
                build.StartInfo.CreateNoWindow = false;
                build.ErrorDataReceived += build_OutputDataReceived;
                build.OutputDataReceived += build_OutputDataReceived;
                build.EnableRaisingEvents = true;
                build.Start();
                build.StandardInput.WriteLine(@"Chcp 1251");
                build.BeginOutputReadLine();
                build.BeginErrorReadLine();
                build.WaitForExit();
            }
        }

        private static void build_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
                _processLog = $"{_processLog}{Environment.NewLine}{e.Data}";
        }

        private static void InstallInstance()
        {
            if (_instance.Certificate != null)
                WriteCertificate(_instance.Certificate);

            var doPath = AppHelper.GetDoPath(_instance.InstancePath);
            if (File.Exists(_instance.InstancePath + "\\DevelopmentStudio.zip"))
                LaunchProcess(doPath, $"do components add_package {_instance.InstancePath}\\DevelopmentStudio.zip");
            if (File.Exists(_instance.InstancePath + "\\DeploymentTool.zip"))
                LaunchProcess(doPath, $"do components add_package {_instance.InstancePath}\\DeploymentTool.zip");
            if (File.Exists(_instance.InstancePath + "\\DirectumRX.zip"))
                LaunchProcess(doPath, $"do components add_package {_instance.InstancePath}\\DirectumRX.zip");

            LaunchProcess(doPath, "do iis configure");
            LaunchProcess(doPath, "do db up");
            LaunchProcess(doPath, "do all up");
            LaunchProcess(doPath, "do dds config_up");

            Dialogs.ShowInformationDialog(_processLog);
            _processLog = string.Empty;
        }

        private static void RestartInstance()
        {
            var doPath = AppHelper.GetDoPath(_instance.InstancePath);
            LaunchProcess(doPath, "do dds config_up");
            LaunchProcess(doPath, "do all up");

            Dialogs.ShowInformationDialog(_processLog);
            _processLog = string.Empty;
        }

        #endregion

        #region Работа с сертификатом.

        private static Certificate GetInstanceCertificate(Config config, string storagePath)
        {
            if (string.IsNullOrEmpty(storagePath) || !Directory.Exists(storagePath))
                return null;

            var instances = Instances.Get();
            if (instances.Any(x => x.StoragePath == storagePath && x.Certificate != null))
                return instances.First(x => x.StoragePath == storagePath).Certificate;

            var certificate = Certificates.Get().FirstOrDefault();
            if (certificate == null || string.IsNullOrEmpty(certificate.Body))
                certificate = CreateCertificate(config, storagePath);

            return certificate;
        }

        private static Certificate CreateCertificate(Config config, string storagePath)
        {
            var yamlParser = new YamlParser(config.Body);
            var certificatePath = yamlParser.SelectToken("$.common_config.DATA_PROTECTION_CERTIFICATE_FILE").ToString();
            var certificateBody = GetCertificateBody(storagePath, certificatePath);

            var certificate = new Certificate();
            certificate.Body = certificateBody;
            certificate.Path = certificatePath;
            certificate.Save();

            return certificate;
        }

        private static void WriteCertificate(Certificate certificate)
        {
            if (string.IsNullOrEmpty(_instance.Config.Body))
                return;

            var yamlParser = new YamlParser(_instance.Config.Body);
            var certificatePath = yamlParser.SelectToken("$.common_config.DATA_PROTECTION_CERTIFICATE_FILE").ToString();
            if (!string.IsNullOrEmpty(certificatePath))
            {
                certificatePath = certificatePath.Replace("{{ home_path }}", _instance.StoragePath);
                var dataProtectionPath = Path.GetDirectoryName(certificatePath);
                if (!Directory.Exists(dataProtectionPath))
                    Directory.CreateDirectory(dataProtectionPath);

                if (!string.IsNullOrEmpty(certificate.Body))
                    File.WriteAllText(certificatePath, certificate.Body);
            }
        }

        private static string GetCertificateBody(string storagePath, string certificatePath)
        {
            if (!string.IsNullOrEmpty(storagePath) && !string.IsNullOrEmpty(certificatePath))
            {
                certificatePath = certificatePath.Replace("{{ home_path }}", storagePath);
                if (File.Exists(certificatePath))
                    return File.ReadAllText(certificatePath);
            }

            return string.Empty;
        }

        #endregion

        #region Работа с ServiceRunner.

        private static string GetServiceStatus()
        {
            var serviceStatus = Constants.InstanceStatus.NeedInstall;

            using (var service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == _instance.ServiceName))
            {
                if (service != null)
                    serviceStatus = service.Status == ServiceControllerStatus.Running ?
                        Constants.InstanceStatus.Working :
                        Constants.InstanceStatus.Stopped;
            }

            return serviceStatus;
        }

        #endregion

        #region Работа с версиями.

        private static string GetInstanceVersion(string instancePath)
        {
            var versionsPath = AppHelper.GetVersionsPath(instancePath);
            if (!File.Exists(versionsPath))
                return Constants.NullVersion;

            var versions = File.ReadAllText(versionsPath);
            var versionsParser = new YamlParser(versions);
            return versionsParser.SelectToken("$.builds.platform_builds")["version"].ToString();
        }

        #endregion

        #region Работа с конфигом.

        private static Config GetInstanceConfig(string instancePath, string version)
        {
            if (string.IsNullOrEmpty(instancePath) || !Directory.Exists(instancePath))
                return null;

            if (string.IsNullOrEmpty(version) || version == Constants.NullVersion)
                return null;

            var instances = Instances.Get();
            if (instances.Any(x => x.InstancePath == instancePath && x.Config != null))
                return instances.First(x => x.InstancePath == instancePath).Config;

            var configYamlPath = AppHelper.GetConfigYamlPath(instancePath);
            if (!File.Exists(configYamlPath))
            {
                var similarConfig = Configs.GetLessOrEqualVersion(version).OrderByDescending(x => x.Version).FirstOrDefault();
                if (similarConfig == null)
                {
                    similarConfig = Configs.GetGreaterOrEqualVersion(version).OrderBy(x => x.Version).FirstOrDefault();
                    if (similarConfig == null)
                        return null;
                }

                File.WriteAllText(configYamlPath, similarConfig.Body);
                YamlWriter.ClearInstanceConfig(configYamlPath);
            }

            var config = new Config();
            config.Version = version;
            config.Path = configYamlPath;
            config.Save();

            return config;
        }

        private static void RebuildConfig()
        {
            var configYamlPath = AppHelper.GetConfigYamlPath(_instance.InstancePath);
            YamlWriter.RebuildVariablesLinks(configYamlPath, _instance.Config.Body);
            _instance.Config.Save();
        }

        #endregion

        #region Проверки перед выполнением действий.

        private static void ValidateDirectoryBeforeAddInstance(string instancePath)
        {
            var instances = Instances.Get();

            if (instances.Any(x => x.InstancePath == instancePath))
            {
                var code = instances.First(x => x.InstancePath == instancePath).Code;
                MessageBox.Show($"Выбранная папка уже является папкой экзампляра {code}");
                return;
            }

            var instanceDLPath = System.IO.Path.Combine(instancePath, "DirectumLauncher.exe");
            if (!File.Exists(instanceDLPath))
            {
                MessageBox.Show("Выбранная папка не является папкой экземпляра DirectumRX (DirectumLauncher не найден)");
                return;
            }
        }

        #endregion
    }
}
