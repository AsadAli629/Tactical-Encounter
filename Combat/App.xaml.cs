using System.Configuration;
using System.Data;
using System.Windows;

namespace Combat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Very important: prevent WPF from using system light theme colors
            Application.Current.Resources.MergedDictionaries.Add(
                new ResourceDictionary { Source = new Uri("pack://application:,,,/PresentationFramework.Aero2;component/themes/Aero2.NormalColor.xaml") }
            );

            // OR — completely disable theme dictionaries (more aggressive)
            // Application.Current.Resources.MergedDictionaries.Clear();

            base.OnStartup(e);
        }

    }
}

    

