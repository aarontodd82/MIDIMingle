using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MIDIMingle
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceProvider = BuildServiceProvider();
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            // Register your services here
            services.AddSingleton<IArduinoService, HandleArduinoCom>();
            services.AddSingleton<IMidiService>(_ => new MidiService("MIDIMingle"));
            services.AddSingleton<IButtonStateService, ButtonStateService>();
            services.AddSingleton<MainWindow>();

            return services.BuildServiceProvider();
        }
    }
}
