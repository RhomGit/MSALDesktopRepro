using B2i.MSALInterop; 
using System.Windows; 

namespace MSALTesting
{ 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new Auth_VM();
        }

        Auth auth; 

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            var vm = (Auth_VM)this.DataContext;
            auth = new Auth(vm, Auth.AppPlatform.DesktopClient, vm.clientId, null);
        }

        private async void btn2_Click(object sender, RoutedEventArgs e)
        {
            // silent
            var vm = (Auth_VM)this.DataContext;
            await auth.Connect(vm, true, vm.previousSignInName);
        }

        private async void btn3_Click(object sender, RoutedEventArgs e)
        {
            // interactive
            var vm = (Auth_VM)this.DataContext;
            await auth.Connect(vm, false, vm.previousSignInName);
        }

        private async void btn4_Click(object sender, RoutedEventArgs e)
        {
            var vm = (Auth_VM)this.DataContext;
            await auth.EditProfile(vm);
        }

        private async void btn5_Click(object sender, RoutedEventArgs e)
        {
            var vm = (Auth_VM)this.DataContext;
            await auth.ResetPassword(vm);
        }
    }
}
