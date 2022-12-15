using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Forms;



namespace RXInstanceManager
{

  /// <summary>
  /// Логика взаимодействия для MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
    {
        internal static Instance _instance;

    #region WPFToTray
    // https://possemeeg.wordpress.com/2007/09/06/minimize-to-tray-icon-in-wpf/
    private System.Windows.Forms.NotifyIcon m_notifyIcon;

    public void Window1()
    {     // initialise code here
      //m_notifyIcon = new System.Windows.Forms.NotifyIcon();
      //m_notifyIcon.BalloonTipText = "The app has been minimised. Click the tray icon to show.";
      //m_notifyIcon.BalloonTipTitle = "The App";
      //m_notifyIcon.Text = "The App";
      //m_notifyIcon.Icon = new System.Drawing.Icon("TheAppIcon.ico");
      //m_notifyIcon.Click += new EventHandler(m_notifyIcon_Click);
    }

    void OnClose(object sender, System.ComponentModel.CancelEventArgs args)
    {
      m_notifyIcon.Dispose();
      m_notifyIcon = null;
    }

    private WindowState m_storedWindowState = WindowState.Normal;
    void OnStateChanged(object sender, EventArgs args)
    {
      if (WindowState == WindowState.Minimized)
      {
        Hide();
        if (m_notifyIcon != null)
          m_notifyIcon.ShowBalloonTip(2000);
      }
      else
        m_storedWindowState = WindowState;
    }
    void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
    {
      CheckTrayIcon();
    }

    void m_notifyIcon_Click(object sender, EventArgs e)
    {
      Show();
      WindowState = m_storedWindowState;
    }
    void CheckTrayIcon()
    {
      ShowTrayIcon(!IsVisible);
    }

    void ShowTrayIcon(bool show)
    {
      if (m_notifyIcon != null)
        m_notifyIcon.Visible = show;
    }
    #endregion

    public MainWindow()
        {
            InitializeComponent();

            if (!Directory.Exists(Constants.LogPath))
                Directory.CreateDirectory(Constants.LogPath);

            DBInitializer.Initialize();
            Instances.Create();
            Configs.Create();

            ActionButtonVisibleChanging();
            LoadInstances();
            StartAsyncHandlers();

      m_notifyIcon = new System.Windows.Forms.NotifyIcon();
      m_notifyIcon.BalloonTipText = "The app has been minimised. Click the tray icon to show.";
      m_notifyIcon.BalloonTipTitle = "The App";
      m_notifyIcon.Text = "The App";
      m_notifyIcon.Icon = new System.Drawing.Icon("App.ico");
      m_notifyIcon.Click += new EventHandler(m_notifyIcon_Click);
    }

        private void GridInstances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var instance = GridInstances.SelectedItem as Instance;
            if (instance != null)
            {
                _instance = instance;
                ActionButtonVisibleChanging(instance.Status);
            }
        }

    #region ActionHandlers

    private void ButtonStart_Click(object sender, RoutedEventArgs e)
    {
      AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);

      if (_instance == null || _instance.Status != Constants.InstanceStatus.NeedInstall)
        return;

      try
      {
        var serviceStatus = AppHandlers.GetServiceStatus(_instance);
        if (serviceStatus == Constants.InstanceStatus.NeedInstall)
          AppHandlers.LaunchProcess(AppHelper.GetDoPath(_instance.InstancePath), "all up", true, true);
      }
      catch (Exception ex)
      {
        AppHandlers.ErrorHandler(_instance, ex);
      }
    }

    private void ButtonStop_Click(object sender, RoutedEventArgs e)
    {
      AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);

      if (_instance == null || _instance.Status != Constants.InstanceStatus.Working)
        return;

      try
      {
        var serviceStatus = AppHandlers.GetServiceStatus(_instance);
        if (serviceStatus == Constants.InstanceStatus.Working)
          AppHandlers.LaunchProcess(AppHelper.GetDoPath(_instance.InstancePath), "all down", true, true);
      }
      catch (Exception ex)
      {
        AppHandlers.ErrorHandler(_instance, ex);
      }
    }

    private void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
      AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);

      var instancePath = Dialogs.ShowEnterValueDialog("Укажите путь до инстанса");
      if (instancePath == null)
        return;

      var isValid = ValidateBeforeAddInstance(instancePath);
      if (!isValid)
        return;

      Instance instance;
      try
      {
        var config = AppHandlers.GetInstanceConfig(instancePath);
        var yamlValues = YamlSimple.Parser.Parse(config.Body);

        var instanceCode = yamlValues.GetConfigStringValue("variables.instance_name");
        if (string.IsNullOrEmpty(instanceCode))
        {
          instanceCode = Dialogs.ShowEnterValueDialog("Укажите код системы");
          if (string.IsNullOrEmpty(instanceCode))
            return;

          if (!AppHelper.ValidateInputCode(instanceCode))
          {
            System.Windows.MessageBox.Show("Код должен быть более от 4 до 10 символов английского алфавита в нижнем регистре и цифр");
            return;
          }
        }

        instance = Instances.Get().FirstOrDefault(x => x.Code == instanceCode);
        if (instance != null)
        {
          System.Windows.MessageBox.Show($"Экземпляр DirectumRX с кодом \"{instanceCode}\" уже добавлен");
          LoadInstances(instance);
          return;
        }

        AppHandlers.SetConfigStringValue(config, "variables.instance_name", instanceCode);

        instance = new Instance();
        instance.Code = instanceCode;
        instance.InstancePath = instancePath;
        instance.ServiceName = $"{Constants.Service}_{instanceCode}";
        instance.Status = Constants.InstanceStatus.NeedInstall;
        instance.Config = config;
        AppHandlers.UpdateInstanceData(instance);

        if (config.Instance == null)
        {
          config.Instance = instance;
          config.Save();
        }

        _instance = instance;
      }
      catch (Exception ex)
      {
        AppHandlers.ErrorHandler(null, ex);
      }

      LoadInstances(_instance);
    }

    private void ButtonInstall_Click(object sender, RoutedEventArgs e)
    {
      AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);

      if (_instance == null || _instance.Status != Constants.InstanceStatus.NeedInstall)
        return;

      var config = AppHandlers.GetInstanceConfig(_instance.InstancePath);
      var yamlValues = YamlSimple.Parser.Parse(config.Body);

      var isValid = ValidateBeforeInstallInstance(yamlValues);
      if (!isValid)
        return;

      try
      {
        var serviceStatus = AppHandlers.GetServiceStatus(_instance);
        if (serviceStatus == Constants.InstanceStatus.NeedInstall)
          AppHandlers.LaunchProcess(AppHelper.GetDirectumLauncherPath(_instance.InstancePath));
      }
      catch (Exception ex)
      {
        AppHandlers.ErrorHandler(_instance, ex);
      }
    }

    private void ButtonCopy_Click(object sender, RoutedEventArgs e)
    {
      AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);

      try
      {

      }
      catch (Exception ex)
      {
        AppHandlers.ErrorHandler(_instance, ex);
      }
    }

    private void ButtonDelete_Click(object sender, RoutedEventArgs e)
    {
      AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);

      try
      {
        var acceptResult = System.Windows.MessageBox.Show($"Подтвердите удаление экземпляра \"{_instance.Code}\"",
                                           "Подтверждение удаления", MessageBoxButton.YesNo);

        if (acceptResult != MessageBoxResult.Yes)
          return;

        //if (Instances.Get().Count() == 1)
        //{
        //    acceptResult = MessageBox.Show("Вы удаляете последний экземпляр системы. Продолжить?",
        //                                   "Подтверждение удаления", MessageBoxButton.YesNo);
        //
        //    if (acceptResult != MessageBoxResult.Yes)
        //        return;
        //}

        //var serviceStatus = AppHandlers.GetServiceStatus(_instance);
        //if (serviceStatus != Constants.InstanceStatus.NeedInstall)
        //AppHandlers.ExecuteDoCommands(_instance.InstancePath, "do all down");

        //var removeFolderCommands = new List<string>();
        //if (!string.IsNullOrEmpty(_instance.StoragePath) && Directory.Exists(_instance.StoragePath))
        //    removeFolderCommands.Add("rmdir /s /q \"" + _instance.StoragePath + "\"");
        //if (!string.IsNullOrEmpty(_instance.SourcesPath) && Directory.Exists(_instance.SourcesPath))
        //    removeFolderCommands.Add("rmdir /s /q \"" + _instance.SourcesPath + "\"");
        //if (!string.IsNullOrEmpty(_instance.InstancePath) && Directory.Exists(_instance.InstancePath))
        //    removeFolderCommands.Add("rmdir /s /q \"" + _instance.InstancePath + "\"");
        //if (Directory.Exists(@"C:\inetpub\DirectumRX Web Site_" + _instance.Code))
        //    removeFolderCommands.Add("rmdir /s /q \"C:\\inetpub\\DirectumRX Web Site_" + _instance.Code + "\"");

        //if (removeFolderCommands.Any())
        //    AppHandlers.ExecuteCmdCommands(true, false, removeFolderCommands.ToArray());

        if (_instance.Config != null)
          Configs.Delete(_instance.Config);

        Instances.Delete(_instance);

        LoadInstances();
        ActionButtonVisibleChanging();
      }
      catch (Exception ex)
      {
        AppHandlers.ErrorHandler(_instance, ex);
      }
    }

    private void ButtonDDSStart_Click(object sender, RoutedEventArgs e)
    {
      AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);

      try
      {
        if (string.IsNullOrEmpty(_instance.StoragePath))
        {
          System.Windows.MessageBox.Show("Не указана папка исходников");
          return;
        }

        AppHandlers.LaunchProcess(AppHelper.GetDDSPath(_instance.InstancePath), true);
      }
      catch (Exception ex)
      {
        AppHandlers.ErrorHandler(_instance, ex);
      }
    }

    private void ButtonRXStart_Click(object sender, RoutedEventArgs e)
    {
      AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);

      try
      {
        if (!string.IsNullOrEmpty(_instance.URL))
          AppHandlers.LaunchProcess(_instance.URL);
      }
      catch (Exception ex)
      {
        AppHandlers.ErrorHandler(_instance, ex);
      }
    }

    private void ButtonInstruction_Click(object sender, RoutedEventArgs e)
    {
      AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);

      try
      {
        if (File.Exists("readme.txt"))
          Dialogs.ShowFileContentDialog("readme.txt");
      }
      catch (Exception ex)
      {
        AppHandlers.ErrorHandler(_instance, ex);
      }
    }

    #endregion


    #region ContextHandlers

    private void ConfigContext_Click(object sender, RoutedEventArgs e)
        {
            AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);



      try
            {
        var configYamlPath = AppHelper.GetConfigYamlPath(_instance.InstancePath);
        if (File.Exists(configYamlPath))
          AppHandlers.LaunchProcess(AppHelper.GetConfigYamlPath(_instance.InstancePath));
                else
          System.Windows.MessageBox.Show("Конфигурационный файл не найден");
      }
            catch (Exception ex)
            {
                AppHandlers.ErrorHandler(_instance, ex);
            }
        }

    private void ProjectConfigContext_Click(object sender, RoutedEventArgs e)
    {
      AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);

      try
      {
        var configYamlPath = _instance.ProjectConfigPath;
        if (File.Exists(configYamlPath))
          AppHandlers.LaunchProcess(configYamlPath);
        else
          System.Windows.MessageBox.Show(string.Format("Конфигурационный файл не найден {0}", configYamlPath));
      }
      catch (Exception ex)
      {
        AppHandlers.ErrorHandler(_instance, ex);
      }
    }

    private void ChangeProject_Click(object sender, RoutedEventArgs e)
    {
      AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);
      using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
      {
        var filter = string.Format("configs for {0}|{0}_*.yml;{0}_*.yaml|YAML-файлы|*.yml;*.yaml|All files (*.*)|*.*", _instance.Code);
        openFileDialog.InitialDirectory = Path.GetDirectoryName(_instance.ProjectConfigPath);
        openFileDialog.Filter = filter;
        openFileDialog.FilterIndex = 1;
        openFileDialog.RestoreDirectory = true;

        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
          var config_filename = openFileDialog.FileName;
          try
          {
              AppHandlers.LaunchProcess(AppHelper.GetDoPath(_instance.InstancePath), string.Format("map set {0} -rundds=False -need_pause", config_filename), true, true);
          }
          catch (Exception ex)
          {
            AppHandlers.ErrorHandler(_instance, ex);
          }

        }
      }
    }

    private void CreateProject_Click(object sender, RoutedEventArgs e)
    {
      AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);
      using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
      {
        var filter = string.Format("configs for {0}|{0}_*.yml;{0}_*.yaml|YAML-файлы|*.yml;*.yaml|All files (*.*)|*.*", _instance.Code);
        openFileDialog.InitialDirectory = string.IsNullOrEmpty(_instance.ProjectConfigPath) ? "C:\\" : Path.GetDirectoryName(_instance.ProjectConfigPath);
        openFileDialog.Filter = filter;
        openFileDialog.FilterIndex = 1;
        openFileDialog.RestoreDirectory = true;

        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
          var config_filename = openFileDialog.FileName;
          try
          {            
            AppHandlers.LaunchProcess(AppHelper.GetDoPath(_instance.InstancePath), string.Format("map create_project {0} -rundds=False -need_pause", config_filename), true, true);
          }
          catch (Exception ex)
          {
            AppHandlers.ErrorHandler(_instance, ex);
          }

        }
      }
    }
    private void UpdateConfig_Click(object sender, RoutedEventArgs e)
    {
      AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);
      using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
      {
        var filter = string.Format("configs for {0}|{0}_*.yml;{0}_*.yaml|YAML-файлы|*.yml;*.yaml|All files (*.*)|*.*", _instance.Code);
        openFileDialog.InitialDirectory =  string.IsNullOrEmpty(_instance.ProjectConfigPath) ? "C:\\" : Path.GetDirectoryName(_instance.ProjectConfigPath);
        openFileDialog.Filter = filter;
        openFileDialog.FilterIndex = 1;
        openFileDialog.RestoreDirectory = true;

        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
          var config_filename = openFileDialog.FileName;
          try
          {
            
              AppHandlers.LaunchProcess(AppHelper.GetDoPath(_instance.InstancePath), string.Format("map update_config {0} -rundds=False -need_pause", config_filename), true, true);
          }
          catch (Exception ex)
          {
            AppHandlers.ErrorHandler(_instance, ex);
          }

        }
      }
    }

    private void CmdContext_Click(object sender, RoutedEventArgs e)
        {
            AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);

            if (_instance == null)
                return;
            try
            {
                AppHandlers.ExecuteCmdCommand($"cd {_instance.InstancePath}", false);
            }
            catch (Exception ex)
            {
                AppHandlers.ErrorHandler(_instance, ex);
            }
        }

        private void CmdAdminContext_Click(object sender, RoutedEventArgs e)
        {
            AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);

            if (_instance == null)
                return;

            try
            {
                AppHandlers.ExecuteCmdCommand($"cd {_instance.InstancePath}", true);
            }
            catch (Exception ex)
            {
                AppHandlers.ErrorHandler(_instance, ex);
            }
        }

        private void InfoContext_Click(object sender, RoutedEventArgs e)
        {
            AppHandlers.InfoHandler(_instance, MethodBase.GetCurrentMethod().Name);

            if (_instance == null)
                return;

            Dialogs.ShowInformationDialog(_instance.ToString());
        }

        #endregion

        private void StartAsyncHandlers()
        {
#pragma warning disable CS4014
            // Так как этот вызов не ожидается, выполнение существующего метода продолжается до тех пор, пока вызов не будет завершен
            UpdateInstanceGridAsync();
            UpdateInstanceDataAsync();
#pragma warning restore CS4014
        }

        private void HiddenButton_Click(object sender, RoutedEventArgs e)
        {

      if (_instance == null)
        return;

      try
      {
         AppHandlers.LaunchProcess(AppHelper.GetDoPath(_instance.InstancePath), "map current -need_pause", true, true);
      }
      catch (Exception ex)
      {
        AppHandlers.ErrorHandler(_instance, ex);
      }

    }


  }
}
