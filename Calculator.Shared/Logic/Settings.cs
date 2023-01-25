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
        private readonly ISettingsService _settingsService;
        private readonly IThemingService _themingService;

        public Settings(
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

        public Task ManageNewResultAsync(string result) =>
            Task.Run(() => ManageNewResult(result));

        private void ManageNewResult(string result)
        {
            var resultsHistory = new CircularBuffer<string>(GetResultsHistoryLength(),
                ContainsResultsHistory() ? GetResultsHistory() : new List<string>());
            resultsHistory.Write(result);
            SetResultsHistory(new CircularBuffer<string>(GetResultsHistoryLength(), resultsHistory));
        }

        public Task<List<string>> GetResultsHistoryAsync() =>
            Task.Run(GetResultsHistory);

        private List<string> GetResultsHistory() =>
            JsonConvert.DeserializeObject<List<string>>(_settingsService.Get(Strings.ResultsHistory, string.Empty));

        public Task SetResultsHistoryAsync(IEnumerable<string> resultsHistory) =>
            Task.Run(() => SetResultsHistory(resultsHistory));

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
