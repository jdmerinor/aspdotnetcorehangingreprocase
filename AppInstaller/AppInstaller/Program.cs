using AppInstaller;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Web.Administration;
using System.Globalization;

//ResetAppPools();
using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<InstallerService>();
    })
    .ConfigureLogging((ILoggingBuilder logging) =>
    {
        logging.ClearProviders()
        .AddConsole()
        .AddFile("installation.log");
    })
    .Build();

await host.RunAsync();

class InstallerService : BackgroundService
{
    private readonly ILogger<InstallerService> logger;

    public InstallerService(ILogger<InstallerService> logger)
    {
        this.logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken token)
    {
        Console.ReadLine();
        StartInstallations();

        return Task.CompletedTask;
    }

    private static void StartInstallations()
    {
        using var serverManager = new ServerManager();

        for (var i = 1; i <= 100; i++)
        {
            var installSiteItem = new InstallSiteItem("UIServer", "www" + i, "UIServer", "UI server for the weather");
            var installSiteItemSlow = new InstallSiteItem("Slowserver", "www" + i, "SlowServer", "Slowserver for the weather");
            AddIISWebSite(serverManager, installSiteItem);
            AddIISWebSite(serverManager, installSiteItemSlow);
        }
        //serverManager.ApplicationPools.Clear();
        //serverManager.Sites.Clear();

        serverManager.CommitChanges();
    }

    static void AddIISWebSite(ServerManager serverManager, InstallSiteItem installSiteItem)
    {
        var installationPath = @"C:\Users\Administrator\Desktop\Apps";
        var site = serverManager.Sites.SingleOrDefault(s => s.Name.Equals(installSiteItem.Site, StringComparison.OrdinalIgnoreCase));
        if (site == null)
        {
            var sitePhysicalPath = installSiteItem.ApplicationPath == "/" ? Path.Combine(installationPath, installSiteItem.FolderName) : installationPath;
            site = serverManager.Sites.Add(installSiteItem.Site, "http", string.Format(CultureInfo.InvariantCulture, "*:80:{0}", installSiteItem.Site), sitePhysicalPath);
            site.Bindings.Add(string.Format(CultureInfo.InvariantCulture, "*:443:{0}", installSiteItem.Site), "https");
        }

        var applicationPhysicalPath = Path.Combine(installationPath, installSiteItem.FolderName);
        var application = site.Applications.SingleOrDefault(app => app.Path == installSiteItem.ApplicationPath);

        if (application == null)
        {
            application = site.Applications.Add(installSiteItem.ApplicationPath, applicationPhysicalPath);
        }
        else if (application.VirtualDirectories != null && application.VirtualDirectories["/"] != null && application.VirtualDirectories["/"].PhysicalPath != null && !application.VirtualDirectories["/"].PhysicalPath.Equals(applicationPhysicalPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Application already exists at '" + installSiteItem.WebAddress + "'.");
        }
        else
        {
            throw new InvalidOperationException("Trying to create duplicated application");
        }

        var appPoolName = installSiteItem.WebAddress.Replace('/', '_');

        if (serverManager.ApplicationPools.Any(ap => ap.Name == appPoolName))
        {
            throw new InvalidOperationException("Application Pool with name '" + appPoolName + "' already exists");
        }

        var appPool = serverManager.ApplicationPools.Add(appPoolName);
        appPool.ProcessModel.LoadUserProfile = false;
        appPool.Recycling.PeriodicRestart.Time = TimeSpan.Zero;
        appPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
        appPool.StartMode = StartMode.AlwaysRunning;
        appPool.ProcessModel.IdleTimeout = TimeSpan.Zero;
        appPool.Enable32BitAppOnWin64 = false;
        appPool.ManagedRuntimeVersion = string.Empty;

        application.ApplicationPoolName = appPool.Name;
        application["preloadEnabled"] = true;
    }
}
