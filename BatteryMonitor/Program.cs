using Topshelf;

namespace BatteryMonitor
{
    class Program
    {
        static void Main(string[] args)
        {/*
            HostFactory.New(x =>
            {
                x.Service<Monitor>(s =>
                {
                    s.ConstructUsing(_ => new Monitor());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.StartAutomatically();
                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(1);
                    r.RestartService(2);
                    r.RestartService(3);
                    r.SetResetPeriod(1);
                });
                x.RunAsLocalService();
            });*/
            HostFactory.Run(x =>
            {
                x.Service<Monitor>(s =>
                {
                    s.ConstructUsing(_ => new Monitor());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.UseNLog();
                x.RunAsLocalSystem();
                x.SetDescription("Monitor działania baterii");
                x.SetDisplayName("Monitor Baterii");
                x.SetServiceName("Monitor_Baterii");
                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(1);
                    r.RestartService(1);
                    r.RestartService(1);
                    r.SetResetPeriod(1);
                });
            });
        }
    }
}
