using Calculator.Shared.Constants;
using Calculator.Shared.DataStructures;
using Calculator.Shared.Models.Theming;
using Calculator.Shared.PlatformServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calculator.Shared.Logic
{
    public class Settings
    {
        private static ISettingsService _settingsService;
        private static IThemingService _themingService;

        public static Settings Instance
        {
            get;
            private set;
        }

        public static void Initialize(
            ISettingsService settingsService,
            IThemingService themingService) =>
            Instance = new Settings(settingsService, themingService);

        private Settings(
            ISettingsService settingsService,
            IThemingService themingService)
        {
            _settingsService = settingsService;
            _themingService = themingService;
        }

        public int GetResultsHistoryLength() =>
            _settingsService.Get(Strings.HistoryLength, Numbers.HistoryLengthDefault);

        public void SetResultsHistoryLength(int length) =>
            _settingsService.Set(Strings.HistoryLength, length);

        public bool ContainsResultsHistory() =>
            _settingsService.Contains(Strings.ResultsHistory);

        public void ClearResultsHistory() =>
            _settingsService.Remove(Strings.ResultsHistory);

        public async Task ManageNewResultAsync(string result) =>
            await Task.Run(() => ManageNewResult(result)).ConfigureAwait(false);

        private void ManageNewResult(string result)
        {
            var resultsHistory = new CircularBuffer<string>(GetResultsHistoryLength(),
                ContainsResultsHistory() ? GetResultsHistory() : new List<string>());
            resultsHistory.Write(result);
            SetResultsHistory(new CircularBuffer<string>(GetResultsHistoryLength(), resultsHistory));
        }

        public async Task<List<string>> GetResultsHistoryAsync() =>
            await Task.Run(GetResultsHistory).ConfigureAwait(false);

        private List<string> GetResultsHistory() =>
            JsonConvert.DeserializeObject<List<string>>(_settingsService.Get(Strings.ResultsHistory, string.Empty));

        public async void SetResultsHistoryAsync(IEnumerable<string> resultsHistory) =>
            await Task.Run(() => SetResultsHistory(resultsHistory)).ConfigureAwait(false);

        private void SetResultsHistory(IEnumerable<string> resultsHistory) =>
            _settingsService.Set(Strings.ResultsHistory, JsonConvert.SerializeObject(resultsHistory));

        public Theme GetTheme() =>
            (Theme)Enum.Parse(typeof(Theme),
                _settingsService.Get(Strings.Theme, _themingService.GetDeviceDefaultTheme().ToString()));

        public void SetTheme(Theme theme) =>
            _settingsService.Set(Strings.Theme, theme.ToString());

        public bool ContainsTheme() =>
            _settingsService.Contains(Strings.Theme);

        public void ClearTheme() =>
            _settingsService.Remove(Strings.Theme);
    }
}
