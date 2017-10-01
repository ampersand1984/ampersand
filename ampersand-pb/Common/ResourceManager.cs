using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ampersand_pb.Common
{
    public class ResourceManager
    {
        public static void AplicarTema()
        {
            Uri uriChartStyle = null;
            string mahappsStyle = "";

            switch (Properties.Settings.Default.Tema)
            {
                case "Dark":
                    {
                        uriChartStyle = new Uri("Resources/ChartDarkStyle.xaml", UriKind.Relative);
                        mahappsStyle = "BaseDark";
                    }
                    break;

                default:
                    {
                        uriChartStyle = new Uri("Resources/ChartLightStyle.xaml", UriKind.Relative);
                        mahappsStyle = "BaseLight";
                    }
                    break;
            }
            
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = uriChartStyle });
            ThemeManager.ChangeAppStyle(Application.Current,
                                    ThemeManager.GetAccent("Blue"),
                                    ThemeManager.GetAppTheme(mahappsStyle));
        }
    }
}
