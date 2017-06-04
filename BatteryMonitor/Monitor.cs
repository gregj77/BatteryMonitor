using System;
using System.Configuration;
using System.Net.Mail;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace BatteryMonitor
{
    public class Monitor
    {
        private readonly SerialDisposable _subscription = new SerialDisposable();
        private readonly ILogger _logger = LogManager.GetLogger("Monitor");

        public const int ProbeIntervalSeconds = 5;
        public const int NoiseThresholdSeconds = 10;

        public void Start()
        {
            _logger.Info("starting....");

            var collection = new CompositeDisposable();

            collection.Add(SubscribePowerStateChange());
            collection.Add(SubscribeStatusNotification());

            _subscription.Disposable = collection;

            _logger.Info("started!");
        }

        public void Stop()
        {
            _logger.Info("stopping...");
            _subscription.Disposable = Disposable.Empty;
        }

        private IDisposable SubscribePowerStateChange()
        {
            return Observable
                .Interval(TimeSpan.FromSeconds(ProbeIntervalSeconds), TaskPoolScheduler.Default)
                .Select(_ => GetCurrentPowerstatus())
                .DistinctUntilChanged()
                .Skip(1)
                .Throttle(TimeSpan.FromSeconds(NoiseThresholdSeconds), TaskPoolScheduler.Default)
                .Select(StatusToString)
                .Subscribe(s => SendEmail(s, ProvidePowerStateChangeBody));
        }

        private IDisposable SubscribeStatusNotification()
        {
            var now = DateTimeOffset.Now;
            var when = new DateTimeOffset(now.Year, now.Month, now.Day, 9, 0, 0, now.Offset);
            return Observable.Timer(when, TimeSpan.FromHours(24), TaskPoolScheduler.Default)
                .Select(_ => GetCurrentPowerstatus())
                .Select(StatusToString)
                .Subscribe(s => SendEmail(s, ProvidePowerStateMonitoringStarted));
        }

        private void SendEmail(string status, Action<MailMessage, string> fillMessageDetails)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _logger.Info("New battery status: {0}", status);
                    using (var client = new SmtpClient())
                    {
                        var to = ConfigurationManager.AppSettings["warningEmailTo"];
                        using (var msg = new MailMessage
                        {
                            From = new MailAddress("battery@monitor.pl"),
                            To = {to},
                            BodyEncoding = Encoding.Default,
                            IsBodyHtml = false,
                        })
                        {
                            fillMessageDetails(msg, status);
                            _logger.Info("Sending email message to {0}", to);
                            client.Send(msg);
                            _logger.Info("Email to {0} sent", to);
                        }
                    }
                }
                catch (Exception err)
                {
                    _logger.Error("Failed to send email: {0}<{1}>\n{2}", err.Message, err.GetType().FullName,
                        err.StackTrace);
                }
            });
        }

        private void ProvidePowerStateChangeBody(MailMessage msg, string custom)
        {
            msg.Subject = "Zmiana stanu zasilania - " + custom;
            msg.Body = string.Format("Aktualny stan zasilania: {0}\nCzas: {1}", custom, DateTimeOffset.Now.ToString("yyyy-MM-dd, HH:mm"));
            msg.Priority = MailPriority.High;            
        }

        private void ProvidePowerStateMonitoringStarted(MailMessage msg, string custom)
        {
            msg.Subject = "Aktualizacja stanu serwisu monitorującego zasilanie";
            msg.Body = string.Format("Aktualny stan zasilania: {0}\nCzas: {1}", custom, DateTimeOffset.Now.ToString("yyyy-MM-dd, HH:mm"));
            msg.Priority = MailPriority.Low;
        }

        private AcLineStatus GetCurrentPowerstatus()
        {
            _logger.Debug("Fetching power state...");
            try
            {
                var state = PowerState.GetPowerState();
                _logger.Debug("AC status is {0}", state.ACLineStatus);
                return state.ACLineStatus;
            }
            catch (Exception err)
            {
                _logger.Error("Failed to fetch the battery state... {0}<{1}>\n{2}", err.Message, err.GetType().FullName,
                    err.StackTrace);
                return AcLineStatus.Unknown;
            }
        }

        private static string StatusToString(AcLineStatus status)
        {
            switch (status)
            {
                case AcLineStatus.Online:
                    return "Z SIECI";

                case AcLineStatus.Offline:
                    return "Z BATERII";

                default:
                    return "NIEOKREŚLONE";
            }
        }
    }
}
