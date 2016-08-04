using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace BJUTDUHelper.Service
{
    public class SettingService
    {
        public static void SetThemeColor(Color color)
        {
            var themeColor = Application.Current.Resources["BJUTDUHelperBaseBackgroundThemeColor"] as SolidColorBrush;
            if (themeColor != null && color != null)
            {
                themeColor.Color = color;
                var view = ApplicationView.GetForCurrentView();
                view.TitleBar.ButtonBackgroundColor = color;
                view.TitleBar.BackgroundColor = color;

                if ("Windows.Mobile" == Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily)
                {
                    StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                    statusBar.BackgroundOpacity = 1;
                    statusBar.BackgroundColor = color;
                }

            }
        }
    }
}
