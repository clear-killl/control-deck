using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Windows;
using System.Linq;

namespace ControlDeck;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(uint action, uint parameter, string imagePath, uint updateIniFile);

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
        PageSubtitle.Text = $"Активных процессов: {GetProcessCount()}";
        StartProcess("taskmgr.exe");
        RefreshProcesses();
    }
    private void Tools_Click(object sender, RoutedEventArgs e) => Amd_Click(sender, e);

    private void ChooseWallpaper_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp", Title = "Выберите обои" };
        if (dialog.ShowDialog() != true) return;
        if (!SystemParametersInfo(SetDesktopWallpaper, 0, dialog.FileName, UpdateIniFile | SendChange))
        {
            SetStatus($"Не удалось установить обои (код Windows {Marshal.GetLastWin32Error()})");
            return;
        }

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
        var rows = new List<string>();
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                rows.Add($"{process.ProcessName}  |  PID {process.Id}");
            }
            catch (InvalidOperationException)
            {
                // The process can exit while the snapshot is being read.
            }
            catch (Win32Exception)
            {
                // Access to a protected process can be denied by Windows.
            }
            finally
            {
                process.Dispose();
            }
        }

        ProcessList.ItemsSource = rows.OrderBy(row => row, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static int GetProcessCount()
    {
        var processes = Process.GetProcesses();
        try
        {
            return processes.Length;
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
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
            using var process = Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
            if (process is null)
            {
                SetStatus($"Windows не запустила: {target}");
                return;
            }

            SetStatus($"Запущено: {target}");
        }
        catch (Exception exception)
        {
            SetStatus($"Не удалось запустить: {exception.Message}");
        }
    }
}