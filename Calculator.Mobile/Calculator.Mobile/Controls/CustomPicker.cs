﻿using Calculator.Shared.Localization;
using System;
using System.Linq;
using Xamarin.Forms;

namespace Calculator.Mobile.Controls
{
    internal class CustomPicker : Picker
    {
        private bool _customPickerIsShowing;

        public CustomPicker() =>
            Focused += CustomPicker_Focused;

        ~CustomPicker() =>
            Focused -= CustomPicker_Focused;

        private void CustomPicker_Focused(object sender, FocusEventArgs args)
        {
            if (Device.RuntimePlatform != Device.Android)
                return;

            var customPicker = (Picker)sender;
            customPicker.Unfocus();

            Device.BeginInvokeOnMainThread(async () =>
            {
                if (_customPickerIsShowing)
                    return;

                _customPickerIsShowing = true;

                var customPickerItems = customPicker.Items.ToArray();

                var selectedItem = await Application.Current.MainPage.DisplayActionSheet(
                    customPicker.Title,
                    LocalizedStrings.Cancel,
                    null,
                    customPickerItems);

                if (customPickerItems.Contains(selectedItem))
                    customPicker.SelectedIndex = Array.IndexOf(customPickerItems, selectedItem);

                _customPickerIsShowing = false;
            });
        }
    }
}
