// Copyright (c) Corey Hayward. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.Timers;

public class Main : IPlugin, IContextMenu, ISettingProvider
{
    private List<TimerPlus> _timers = [];
    private TimerResultService? _timerService;
    private PluginInitContext? _context;
    private Settings _settings = new();
    private SoundService? _soundService;
    private string? _pluginDirectory;

    public string Name => "Timer";

    public string Description => "Set and manage timers.";

    public static string PluginID => "0cd68088246649a38205c7641df39db0";

    public IEnumerable<PluginAdditionalOption> AdditionalOptions
    {
        get
        {
            return
            [
                new PluginAdditionalOption()
                {
                    Key = nameof(Settings.TimeSpanParserUncolonedDefault),
                    DisplayLabel = "Uncoloned Time Configuration",
                    DisplayDescription =
                        "Sets how the first number of the timer should be treated, when not using colons.",
                    ComboBoxValue = (int)_settings.TimeSpanParserUncolonedDefault,
                    PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Combobox,
                    ComboBoxItems = Settings.TimeSpanParserConfigurationOptions,
                },
                new PluginAdditionalOption()
                {
                    Key = nameof(Settings.TimeSpanParserColonedDefault),
                    DisplayLabel = "Coloned Time Configuration",
                    DisplayDescription =
                        "Sets how the first number of the timer should be treated, when using colons (i.e. 4:30).",
                    ComboBoxValue = (int)_settings.TimeSpanParserColonedDefault,
                    PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Combobox,
                    ComboBoxItems = Settings.TimeSpanParserConfigurationOptions,
                },
                new PluginAdditionalOption()
                {
                    Key = nameof(Settings.EnableAlarmSound),
                    DisplayLabel = "Enable Alarm Sound",
                    DisplayDescription =
                        "Enable or disable sound notifications when a timer expires.",
                    Value = _settings.EnableAlarmSound,
                    PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Checkbox,
                },
                new PluginAdditionalOption()
                {
                    Key = nameof(Settings.CustomSoundPath),
                    DisplayLabel = "Custom Sound Path",
                    DisplayDescription =
                        "Full path to a .wav file to use as alarm sound. Leave empty to use the built-in default sound.",
                    TextValue = _settings.CustomSoundPath,
                    PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
                },
                new PluginAdditionalOption()
                {
                    Key = nameof(Settings.AlarmDurationSeconds),
                    DisplayLabel = "Alarm Duration",
                    DisplayDescription = "How many seconds the alarm will play (1-30 seconds).",
                    NumberValue = _settings.AlarmDurationSeconds,
                    PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Numberbox,
                },
            ];
        }
    }

    public void Init(PluginInitContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
        _pluginDirectory = _context.CurrentPluginMetadata.PluginDirectory;
        _soundService = new(_pluginDirectory);
        _timerService = new(_context, _soundService, _settings);
    }

    public List<Result> Query(Query query) => _timerService!.GetQueryResult(query, _settings);

    public List<ContextMenuResult> LoadContextMenus(Result selectedResult) =>
        _timerService!.GetContextMenuResults(selectedResult);

    public System.Windows.Controls.Control CreateSettingPanel() =>
        throw new NotImplementedException();

    public void UpdateSettings(PowerLauncherPluginSettings settings)
    {
        if (settings?.AdditionalOptions is null)
        {
            return;
        }

        _settings.UpdateSettings(settings.AdditionalOptions);
    }
}
