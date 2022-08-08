using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.ServiceProcess;
using System.Diagnostics;
using Microsoft.Win32;
using SQLQueryGen;
using YamlHandlers;

namespace RXInstanceManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static Instance _instance;

        public MainWindow()
        {
            InitializeComponent();

            if (!Directory.Exists(Constants.LogPath))
                Directory.CreateDirectory(Constants.LogPath);

            DBInitializer.Initialize();
            Instances.Create();
            Configs.Create();
            Certificates.Create();

            PanelOperation.Visibility = Visibility.Hidden;

            EditCode.Text = Constants.EditEmptyValue.Code;
            EditName.Text = Constants.EditEmptyValue.Name;
            EditDBName.Text = Constants.EditEmptyValue.DBName;
            EditURL.Text = Constants.EditEmptyValue.URL;
            EditHttpPort.Text = Constants.EditEmptyValue.HttpPort;
            EditServiceName.Text = Constants.EditEmptyValue.Service;
            EditStoragePath.Text = Constants.EditEmptyValue.StoragePath;
            EditSourcesPath.Text = Constants.EditEmptyValue.SourcesPath;

            ActionButtonVisibleChanging();
            LoadInstances();
        }

        private void GridInstances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var instance = GridInstances.SelectedItem as Instance;
            if (instance != null)
            {
                EditCode.Text = instance.Code;
                EditName.Text = !string.IsNullOrWhiteSpace(instance.Name) ? instance.Name : Constants.EditEmptyValue.Name;
                EditURL.Text = instance.URL;
                EditHttpPort.Text = instance.Port > 0 ? instance.Port.ToString() : Constants.EditEmptyValue.HttpPort;
                EditDBName.Text = !string.IsNullOrWhiteSpace(instance.DBName) ? instance.DBName : Constants.EditEmptyValue.DBName;
                EditServiceName.Text = !string.IsNullOrWhiteSpace(instance.ServiceName) ? instance.ServiceName : Constants.EditEmptyValue.Service;
                EditStoragePath.Text = !string.IsNullOrWhiteSpace(instance.StoragePath) ? instance.StoragePath : Constants.EditEmptyValue.StoragePath;
                EditSourcesPath.Text = !string.IsNullOrWhiteSpace(instance.SourcesPath) ? instance.SourcesPath : Constants.EditEmptyValue.SourcesPath;

                _instance = instance;
                EditInstanceCode.Text = $"Код:{instance.Code}";

                ActionButtonVisibleChanging(instance.Status);
            }
        }

        #region EditHandlers

        #region EditCode

        private void EditCode_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!EditCode.IsReadOnly && !string.IsNullOrWhiteSpace(EditCode.Text) && EditCode.Text.Contains(Constants.EditEmptyValue.Code))
                EditCode.Text = string.Empty;
        }

        private void EditCode_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EditCode.Text))
                EditCode.Text = Constants.EditEmptyValue.Code;
        }

        private void EditCode_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!PanelOperation.IsVisible && !EditCode.IsReadOnly)
                PanelOperation.Visibility = Visibility.Visible;
        }

        private void EditCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditStyleChanging(EditCode, Constants.EditEmptyValue.Code);
        }

        #endregion

        #region EditName

        private void EditName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!EditName.IsReadOnly && !string.IsNullOrWhiteSpace(EditName.Text) && EditName.Text.Contains(Constants.EditEmptyValue.Name))
                EditName.Text = string.Empty;
        }

        private void EditName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EditName.Text))
                EditName.Text = Constants.EditEmptyValue.Name;
        }

        private void EditName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!PanelOperation.IsVisible && !EditName.IsReadOnly)
                PanelOperation.Visibility = Visibility.Visible;
        }

        private void EditName_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditStyleChanging(EditName, Constants.EditEmptyValue.Name);
        }

        #endregion

        #region EditDBName

        private void EditDBName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!EditDBName.IsReadOnly && !string.IsNullOrWhiteSpace(EditDBName.Text) && EditDBName.Text.Contains(Constants.EditEmptyValue.DBName))
                EditDBName.Text = string.Empty;
        }

        private void EditDBName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EditDBName.Text))
                EditDBName.Text = Constants.EditEmptyValue.DBName;
        }

        private void EditDBName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!PanelOperation.IsVisible && !EditDBName.IsReadOnly)
                PanelOperation.Visibility = Visibility.Visible;
        }

        private void EditDBName_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditStyleChanging(EditDBName, Constants.EditEmptyValue.DBName);
        }

        #endregion

        #region EditHttpPort

        private void EditHttpPort_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!EditHttpPort.IsReadOnly && !string.IsNullOrWhiteSpace(EditHttpPort.Text) && EditHttpPort.Text.Contains(Constants.EditEmptyValue.HttpPort))
                EditHttpPort.Text = string.Empty;
        }

        private void EditHttpPort_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EditHttpPort.Text))
                EditHttpPort.Text = Constants.EditEmptyValue.HttpPort;
        }

        private void EditHttpPort_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!PanelOperation.IsVisible && !EditHttpPort.IsReadOnly)
                PanelOperation.Visibility = Visibility.Visible;
        }

        private void EditHttpPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditStyleChanging(EditHttpPort, Constants.EditEmptyValue.HttpPort);
        }

        #endregion

        #region EditURL

        private void ButtonURL_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(EditURL.Text))
                Process.Start(EditURL.Text);
        }

        #endregion

        #region EditStoragePath

        private void ButtonStoragePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = Dialogs.ShowSelectFolderDialog(EditStoragePath.Text);
            if (dialog != null && !string.IsNullOrEmpty(dialog.FileName))
            {
                EditStoragePath.Text = dialog.FileName;
                PanelOperation.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region EditSourcesPath

        private void ButtonSourcesPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = Dialogs.ShowSelectFolderDialog(EditSourcesPath.Text);
            if (dialog != null && !string.IsNullOrEmpty(dialog.FileName))
            {
                EditSourcesPath.Text = dialog.FileName;
                PanelOperation.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #endregion

        #region OperationHandlers

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(EditCode.Text) || EditCode.Text == Constants.EditEmptyValue.Code)
            {
                MessageBox.Show("Не заполнен обязательный параметр \"Код\"");
                PanelOperation.Visibility = Visibility.Hidden;
            }

            if (!AppHelper.ValidateInputCode(EditCode.Text))
            {
                MessageBox.Show("Код должен быть более от 4 до 10 символов английского алфавита в нижнем регистре и цифр");
                PanelOperation.Visibility = Visibility.Hidden;
            }

            if (string.IsNullOrEmpty(EditDBName.Text) || EditDBName.Text == Constants.EditEmptyValue.DBName)
            {
                MessageBox.Show("Не заполнен обязательный параметр \"Имя БД\"");
                PanelOperation.Visibility = Visibility.Hidden;
            }

            if (!AppHelper.ValidateInputDBName(EditDBName.Text))
            {
                MessageBox.Show("Имя БД должно состоять из символов английского алфавита и цифр");
                PanelOperation.Visibility = Visibility.Hidden;
            }

            if (string.IsNullOrEmpty(EditHttpPort.Text) || EditHttpPort.Text == Constants.EditEmptyValue.HttpPort)
            {
                MessageBox.Show("Не заполнен обязательный параметр \"Http порт\"");
                PanelOperation.Visibility = Visibility.Hidden;
            }

            if (!AppHelper.ValidateInputPort(EditHttpPort.Text))
            {
                MessageBox.Show("Некорректно указан порт");
                PanelOperation.Visibility = Visibility.Hidden;
            }

            if (!PanelOperation.IsVisible)
                return;

            _instance.Code = EditCode.Text != Constants.EditEmptyValue.Code ? EditCode.Text : string.Empty;
            _instance.Name = EditName.Text != Constants.EditEmptyValue.Name ? EditName.Text : string.Empty;
            _instance.DBName = EditDBName.Text != Constants.EditEmptyValue.DBName ? EditDBName.Text : string.Empty;
            _instance.Port = int.Parse(EditHttpPort.Text);
            _instance.URL = AppHelper.GetClientURL(Constants.Protocol, Constants.Host, _instance.Port);
            _instance.StoragePath = EditStoragePath.Text != Constants.EditEmptyValue.InstancePath ? EditStoragePath.Text : string.Empty;
            _instance.SourcesPath = EditSourcesPath.Text != Constants.EditEmptyValue.SourcesPath ? EditSourcesPath.Text : string.Empty;
            _instance.Save();

            _instance.Config.Save();
            UpdateConfigVariables();

            PanelOperation.Visibility = Visibility.Hidden;

            LoadInstances(_instance);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            PanelOperation.Visibility = Visibility.Hidden;

            LoadInstances(_instance);
        }

        #endregion

        #region ActionHandlers

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = Dialogs.ShowSelectFolderDialog();
            if (dialog == null)
                return;

            var instancePath = dialog.FileName;
            ValidateDirectoryBeforeAddInstance(instancePath);

            var version = GetInstanceVersion(instancePath);
            var config = GetInstanceConfig(instancePath, version);
            if (config == null)
            {
                MessageBox.Show("Не найден эталонный конфиг. Должен быть установлен и запущен хотя бы один экземпляр");
                return;
            }

            var yamlParser = new YamlParser(config.Body);
            var variablesNode = yamlParser.SelectToken("$.variables");

            var storagePath = variablesNode.AllNodes.Any(x => x.ToString() == "home_path") ? variablesNode["home_path"].ToString() : string.Empty;
            var certificate = GetInstanceCertificate(config, storagePath);

            var instanceCode = string.Empty;
            if (variablesNode.AllNodes.Any(x => x.ToString() == "instance_name"))
                instanceCode = variablesNode["instance_name"].ToString();

            if (string.IsNullOrEmpty(instanceCode))
            {
                instanceCode = Dialogs.ShowEnterValueDialog("Укажите код системы");
                if (string.IsNullOrEmpty(instanceCode))
                    return;

                if (!AppHelper.ValidateInputCode(instanceCode))
                {
                    MessageBox.Show("Код должен быть более от 4 до 10 символов английского алфавита в нижнем регистре и цифр");
                    return;
                }
            }

            var instance = Instances.Get().FirstOrDefault(x => x.Code == instanceCode);
            if (instance != null)
            {
                MessageBox.Show($"Экземпляр DirectumRX с кодом \"{instanceCode}\" уже добавлен");
                LoadInstances(instance);
                return;
            }

            var dbName = variablesNode.AllNodes.Any(x => x.ToString() == "database") ? variablesNode["database"].ToString() :
              AppHelper.GetDBNameFromConnectionString("mssql", yamlParser.SelectToken("$.common_config.CONNECTION_STRING").ToString());

            var httpPort = 0;
            if (!string.IsNullOrWhiteSpace(variablesNode["http_port"].ToString()))
                int.TryParse(variablesNode["http_port"].ToString(), out httpPort);

            try
            {
                instance = new Instance();
                instance.Code = instanceCode;
                instance.Name = variablesNode.AllNodes.Any(x => x.ToString() == "purpose") ? variablesNode["purpose"].ToString() : string.Empty;
                instance.Port = httpPort;
                instance.URL = AppHelper.GetClientURL(variablesNode["protocol"].ToString(), variablesNode["host_fqdn"].ToString(), instance.Port);
                instance.DBName = dbName;
                instance.InstancePath = instancePath;
                instance.StoragePath = storagePath;
                instance.SourcesPath = variablesNode.AllNodes.Any(x => x.ToString() == "home_path_src") ? variablesNode["home_path_src"].ToString() : string.Empty;
                instance.PlatformVersion = version;
                instance.SolutionVersion = Constants.NullVersion;
                instance.ServiceName = $"{Constants.Service}_{instanceCode}";
                instance.Status = Constants.InstanceStatus.NeedInstall;
                instance.Config = config;
                instance.Certificate = certificate;
                instance.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                return;
            }

            _instance = instance;
            UpdateConfigVariables();
            LoadInstances(_instance);
        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e)
        {
            if (_instance == null || _instance.Status != Constants.InstanceStatus.NeedInstall)
                return;

            var instances = Instances.Get();

            if (instances.Count(x => x.Code == _instance.Code) > 1)
            {
                MessageBox.Show("С указанным кодом найдено несколько экземпляров");
                return;
            }

            if (instances.Any(x => x.Code != _instance.Code && x.Port == _instance.Port))
            {
                var code = instances.FirstOrDefault(x => x.Code != _instance.Code && x.Port == _instance.Port);
                MessageBox.Show($"Указанный порт уже используется экземпляром \"{code}\"");
                return;
            }

            if (instances.Any(x => x.Code != _instance.Code && x.DBName == _instance.DBName))
            {
                var code = instances.FirstOrDefault(x => x.Code != _instance.Code && x.DBName == _instance.DBName);
                MessageBox.Show($"Указанная база данных уже используется экземпляром \"{code}\"");
                return;
            }

            if (instances.Any(x => x.Code != _instance.Code && x.StoragePath == _instance.StoragePath))
            {
                var code = instances.FirstOrDefault(x => x.Code != _instance.Code && x.StoragePath == _instance.StoragePath);
                MessageBox.Show($"Указанная папка хранилища используется экземпляром \"{code}\"");
                return;
            }

            if (instances.Any(x => x.Code != _instance.Code && x.SourcesPath == _instance.SourcesPath))
            {
                var code = instances.FirstOrDefault(x => x.Code != _instance.Code && x.SourcesPath == _instance.SourcesPath);
                MessageBox.Show($"Указанная папка исходников используется экземпляром \"{code}\"");
                return;
            }

            var services = instances.FindAll(x => x.Code != _instance.Code).Select(x => x.ServiceName).Distinct();
            if (!services.Any(x => x == _instance.ServiceName))
            {
                RebuildConfig();

                var serviceStatus = AppHandlers.GetServiceStatus(_instance);
                if (serviceStatus == Constants.InstanceStatus.NeedInstall)
                {
                    try
                    {
                        InstallInstance();
                        serviceStatus = Constants.InstanceStatus.Working;
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                    catch (Exception ex)
                    {
                        Dialogs.ShowInformationDialog(ex.Message + Environment.NewLine + ex.StackTrace);
                    }
                }

                _instance.Status = serviceStatus;
                _instance.Save();

                LoadInstances(_instance);
            }
        }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var acceptResult = MessageBox.Show($"Подтвердите удаление экземпляра \"{_instance.Code}\"",
                                               "Подтверждение удаления", MessageBoxButton.YesNo);

            if (acceptResult != MessageBoxResult.Yes)
                return;

            if (Instances.Get().Count() == 1)
            {
                acceptResult = MessageBox.Show("Вы удаляете последний экземпляр системы. Продолжить?",
                                               "Подтверждение удаления", MessageBoxButton.YesNo);

                if (acceptResult != MessageBoxResult.Yes)
                    return;
            }

            try
            {
                var serviceStatus = AppHandlers.GetServiceStatus(_instance);
                if (serviceStatus != Constants.InstanceStatus.NeedInstall)
                {
                    if (serviceStatus == Constants.InstanceStatus.Working)
                    {
                        var service = new ServiceController(_instance.ServiceName);
                        service.Stop();

                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }

                    // HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\<имя сервиса>
                    using (var regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services", true))
                    {
                        if (regKey != null && regKey.GetSubKeyNames().Any(x => x == _instance.ServiceName))
                            regKey.DeleteSubKey(_instance.ServiceName, true);
                    }

                    MessageBox.Show("Была удалена служба Windows. Необходимо перезагрузить компьютер.");
                }

                if (!string.IsNullOrEmpty(_instance.StoragePath) && Directory.Exists(_instance.StoragePath))
                {
                    var folder = new DirectoryInfo(_instance.StoragePath);
                    folder.Attributes &= ~FileAttributes.ReadOnly;

                    Directory.Delete(_instance.StoragePath, true);
                    Directory.CreateDirectory(_instance.StoragePath);
                }

                if (!string.IsNullOrEmpty(_instance.SourcesPath) && Directory.Exists(_instance.SourcesPath))
                {
                    var folder = new DirectoryInfo(_instance.SourcesPath);
                    folder.Attributes &= ~FileAttributes.ReadOnly;

                    Directory.Delete(_instance.SourcesPath, true);
                    Directory.CreateDirectory(_instance.SourcesPath);
                }

                Directory.Delete(_instance.InstancePath, true);
                Directory.CreateDirectory(_instance.InstancePath);

                if (_instance.Certificate != null)
                    Certificates.Delete(_instance.Certificate);
                if (_instance.Config != null)
                    Configs.Delete(_instance.Config);
                Instances.Delete(_instance);
            }
            catch (Exception ex)
            {
                Dialogs.ShowInformationDialog(ex.Message + Environment.NewLine + ex.StackTrace);
            }

            LoadInstances();
            ActionButtonVisibleChanging();
        }

        private void ButtonDDSStart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_instance.StoragePath))
            {
                MessageBox.Show("Не указана папка исходников");
                return;
            }

            LaunchProcess(AppHelper.GetDDSPath(_instance.InstancePath));
        }

        private void ButtonRXStart_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(EditURL.Text))
                Process.Start(EditURL.Text);
        }

        private void ButtonInstruction_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("readme.txt"))
                Dialogs.ShowFileContentDialog("readme.txt");
        }

        #endregion

        #region ContextHandlers

        private void StartContext_Click(object sender, RoutedEventArgs e)
        {
            var service = new ServiceController(_instance.ServiceName);
            service.Start();

            _instance.Status = Constants.InstanceStatus.Working;
            _instance.Save();

            LoadInstances(_instance);
        }

        private void StopContext_Click(object sender, RoutedEventArgs e)
        {
            var service = new ServiceController(_instance.ServiceName);
            service.Stop();

            _instance.Status = Constants.InstanceStatus.Stopped;
            _instance.Save();

            LoadInstances(_instance);
        }

        private void ConfigContext_Click(object sender, RoutedEventArgs e)
        {
            var config = AppHelper.GetConfigYamlPath(_instance.InstancePath);
            if (File.Exists(config))
            {
                var process = new Process();
                process.StartInfo.FileName = AppHelper.GetConfigYamlPath(_instance.InstancePath);
                process.Start();
            }
            else
                MessageBox.Show("Конфигурационный файл не найден");
        }

        private void RestartContext_Click(object sender, RoutedEventArgs e)
        {
            RestartInstance();

            _instance.Config.Save();
            _instance.Status = Constants.InstanceStatus.Working;
            _instance.Save();

            LoadInstances(_instance);
        }

        #endregion
    }
}
