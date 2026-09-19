using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;

namespace ControlDeck;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int SystemParametersInfo(uint action, uint parameter, string imagePath, uint updateIniFile);

    private const uint SetDesktopWallpaper = 20;
    private const uint UpdateIniFile = 1;
    private const uint SendChange = 2;

    public MainWindow()
    {
        InitializeComponent();
        RefreshProcesses();
    }

    private void SetStatus(string message) => StatusText.Text = message;

    private void Overview_Click(object sender, RoutedEventArgs e)
    {
        PageTitle.Text = "Обзор";
        PageSubtitle.Text = "Быстрый доступ к повседневным действиям";
    }

    private void Wallpaper_Click(object sender, RoutedEventArgs e) => ChooseWallpaper_Click(sender, e);
    private void Library_Click(object sender, RoutedEventArgs e) => Steam_Click(sender, e);
    private void TaskManager_Click(object sender, RoutedEventArgs e)
    {
        PageTitle.Text = "Мониторинг";
        PageSubtitle.Text = $"Активных процессов: {Process.GetProcesses().Length}";
        StartProcess("taskmgr.exe");
        RefreshProcesses();
    }
    private void Tools_Click(object sender, RoutedEventArgs e) => Amd_Click(sender, e);

    private void ChooseWallpaper_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp", Title = "Выберите обои" };
        if (dialog.ShowDialog() != true) return;
        SystemParametersInfo(SetDesktopWallpaper, 0, dialog.FileName, UpdateIniFile | SendChange);
        WallpaperName.Text = Path.GetFileName(dialog.FileName);
        SetStatus("Обои рабочего стола обновлены");
    }

    private void VoiceInput_Click(object sender, RoutedEventArgs e)
    {
        StartProcess("ms-settings:privacy-speech");
        SetStatus("Включите русский язык и используйте Win + H для диктовки");
    }

    private void Steam_Click(object sender, RoutedEventArgs e) => StartProcess("steam://open/main");
    private void WallpaperEngine_Click(object sender, RoutedEventArgs e) => StartProcess("steam://rungameid/431960");
    private void Explorer_Click(object sender, RoutedEventArgs e) => StartProcess("explorer.exe");
    private void Settings_Click(object sender, RoutedEventArgs e) => StartProcess("ms-settings:");
    private void Amd_Click(object sender, RoutedEventArgs e)
    {
        var amdSoftware = @"C:\Program Files\AMD\CNext\CNext.exe";
        StartProcess(File.Exists(amdSoftware) ? amdSoftware : "ms-settings:appsfeatures");
    }

    private void RefreshProcesses_Click(object sender, RoutedEventArgs e) => RefreshProcesses();

    private void RefreshProcesses()
    {
        ProcessList.ItemsSource = Process.GetProcesses()
            .OrderBy(process => process.ProcessName)
            .Select(process => $"{process.ProcessName}  |  PID {process.Id}")
            .ToList();
    }

    private void NetworkProfiles_Click(object sender, RoutedEventArgs e)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ControlDeck", "Profiles");
        Directory.CreateDirectory(folder);
        StartProcess(folder);
        SetStatus("Открыта папка локальных сетевых профилей");
    }

    private void StartProcess(string target)
    {
        try
        {
            Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
            SetStatus($"Запущено: {target}");
        }
        catch (Exception exception)
        {
            SetStatus($"Не удалось запустить: {exception.Message}");
        }
    }
}