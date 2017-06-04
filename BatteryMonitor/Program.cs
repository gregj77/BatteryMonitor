using Topshelf;

namespace BatteryMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.New(x =>
            {
                x.Service<Monitor>(s =>
                {
                    s.ConstructUsing(_ => new Monitor());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.UseNLog();
                x.RunAsNetworkService();
                x.StartAutomatically();

                x.SetDescription("Monitor działania baterii");
                x.SetDisplayName("Batery Monitor");
                x.SetServiceName("Batery_Monitor");

                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(1);
                    r.RestartService(2);
                    r.RestartService(3);
                    r.SetResetPeriod(1);
                });
            })
            .Run();
        }
    }
}
