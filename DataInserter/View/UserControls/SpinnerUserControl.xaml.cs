using DataInserter.ViewModel.Commands;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataInserter.View.UserControls
{
    /// <summary>
    /// Interaction logic for SpinnerUserControl.xaml
    /// </summary>
    public partial class SpinnerUserControl : UserControl
    {
        public SpinnerUserControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty RandomAmountProperty = DependencyProperty.Register("RandomAmount", typeof(string), typeof(SpinnerUserControl), new PropertyMetadata("test", OnCustomerChangedCallBack));

        private static void OnCustomerChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SpinnerUserControl c = sender as SpinnerUserControl;
            if (c != null)
            {
                c.tb_main.Text = e.NewValue?.ToString();
            }
        }

        public ICommand RandomInsertsUPCommand { get { return new RelayCommand(() => RandomAmount = (Convert.ToInt32(RandomAmount) + 1).ToString(), () => true); } }
        public ICommand RandomInsertsDOWNCommand { get { return new RelayCommand(() => RandomAmount = (Convert.ToInt32(RandomAmount) - 1).ToString(), () => true); } }

        public string RandomAmount
        {
            get { return (string)GetValue(RandomAmountProperty); }
            set { SetValue(RandomAmountProperty, value); }
        }
    }
}
