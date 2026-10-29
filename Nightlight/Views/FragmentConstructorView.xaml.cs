using Nightlight.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Nightlight.Views
{
    public partial class FragmentConstructorView : UserControl
    {
        public FragmentConstructorView()
        {
            InitializeComponent();
            
        }

        //private void FragmentConstructorView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var oldVm = e.OldValue as FragmentConstructorViewModel;
        //    if (oldVm != null)
        //        oldVm.ContractRequested -= ShowContractWindow;

        //    var newVm = e.NewValue as FragmentConstructorViewModel;
        //    if (newVm != null)
        //        newVm.ContractRequested += ShowContractWindow;
        //}

        private void ShowContractWindow(ContractViewModel contract)
        {
            var window = new ContractWindow(contract);
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }
    }
}