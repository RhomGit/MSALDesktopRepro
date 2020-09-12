using System.Threading.Tasks;
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



        private async void btn1_Click(object sender, RoutedEventArgs e)
        {
            var vm = (Auth_VM)this.DataContext;
            await vm.CreateAuth();
            MessageBox.Show("Auth created");
        }

        private async void btn2_Click(object sender, RoutedEventArgs e)
        {
            // silent
            var vm = (Auth_VM)this.DataContext;
            try
            {
                await vm.GetAuthResult(true); 
            }
            catch (System.Exception)
            {
                // do nothing, we are silent
                MessageBox.Show("Silent fail.");
            }
        }

        private async void btn3_Click(object sender, RoutedEventArgs e)
        {
            // interactive
            var vm = (Auth_VM)this.DataContext;
            try
            {
                await vm.GetAuthResult(false);
            }
            catch (System.Exception)
            {
                // do nothing, we are silent
                MessageBox.Show("Silent fail.");
            }
        }

        private async void btn4_Click(object sender, RoutedEventArgs e)
        {
            var vm = (Auth_VM)this.DataContext;
            await vm.auth.EditProfile(vm);
        }

        private async void btn5_Click(object sender, RoutedEventArgs e)
        {
            var vm = (Auth_VM)this.DataContext;
            await vm.auth.ResetPassword(vm);
        }

        private async void btn6_Click(object sender, RoutedEventArgs e)
        {
            var vm = (Auth_VM)this.DataContext;
            if (vm is null || vm.auth is null)
            {
                MessageBox.Show("Auth is null, no need to sign out");
                return;
            }
            await vm.auth.SignOut();
            MessageBox.Show("Signed out");
        }
    }
}
