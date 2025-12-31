using System;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Community.PowerToys.Run.Plugin.Timers;

/// <summary>
/// Service for managing toast notifications with action buttons.
/// </summary>
public class ToastNotificationService
{
    private const string AppId = "PowerToys Run: Timers";
    private readonly SoundService _soundService;
    private ToastNotifier? _notifier;

    public ToastNotificationService(SoundService soundService)
    {
        _soundService = soundService;
        try
        {
            _notifier = ToastNotificationManager.CreateToastNotifier(AppId);
        }
        catch
        {
            _notifier = null;
        }
    }

    public void ShowTimerNotification(string title, string message, Action onDismiss)
    {
        if (_notifier == null)
        {
            return;
        }

        try
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(
                ToastTemplateType.ToastText02
            );

            var textNodes = toastXml.GetElementsByTagName("text");
            textNodes[0].AppendChild(toastXml.CreateTextNode(title));
            textNodes[1].AppendChild(toastXml.CreateTextNode(message));

            var toastNode = toastXml.SelectSingleNode("/toast");
            var actionsNode = toastXml.CreateElement("actions");

            var actionNode = toastXml.CreateElement("action");
            actionNode.SetAttribute("content", "Stop Alarm");
            actionNode.SetAttribute("arguments", "dismiss");
            actionNode.SetAttribute("activationType", "background");

            actionsNode.AppendChild(actionNode);
            toastNode?.AppendChild(actionsNode);

            var toast = new ToastNotification(toastXml);

            toast.Dismissed += (sender, args) =>
            {
                onDismiss?.Invoke();
            };

            toast.Activated += (sender, args) =>
            {
                onDismiss?.Invoke();
            };

            _notifier.Show(toast);
        }
        catch { }
    }
}
