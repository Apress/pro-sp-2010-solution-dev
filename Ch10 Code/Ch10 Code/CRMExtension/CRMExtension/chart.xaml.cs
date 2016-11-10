using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CRMExtension
{
    /// <summary>
    /// Interaction logic for chart.xaml
    /// </summary>
    public partial class chart : UserControl
    {
        public chart()
        {
            InitializeComponent();
        }
        public void SetImage(string Url)
        {
            image1.Source = new BitmapImage(new Uri(Url));
        }
    }
}
