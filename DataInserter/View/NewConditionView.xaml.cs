using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DataInserter.View
{
    /// <summary>
    /// Interaction logic for NewConditionView.xaml
    /// </summary>
    public partial class NewConditionView : Window
    {
        public NewConditionView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LevelCombobox.ItemsSource = Enum.GetValues(typeof(Model.NodeLevel));
            FieldCombobox.ItemsSource = Enum.GetValues(typeof(Model.XmlNodes));
        }
    }

}
