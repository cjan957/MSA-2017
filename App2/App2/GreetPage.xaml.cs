using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;
using Tabs.Model;

namespace App2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GreetPage : TabbedPage
    {
        public GreetPage()
        {
            InitializeComponent();
        }
    }


        /*
private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
{
label.Text = String.Format("Value is {0:F2}", e.NewValue);
}
*/
    }
