using System.Windows;

namespace PracticaVideojuegos
{
    public partial class LoginWindow : Window
    {
        // Almacén de credenciales en memoria: usuario, contraseña
        private readonly string[,] _users = new string[,] {
            { "user1", "pass1" },
            { "admin", "1234" },
            { "user", "user" } // credencial añadida según petición
        };

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtUsuario.Focus();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var user = TxtUsuario.Text?.Trim();
            var pass = PwdPassword.Password ?? string.Empty;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                TxtMensaje.Text = "Rellena usuario y contraseña.";
                return;
            }

            if (ValidateCredentials(user, pass))
            {
                // Iniciar MainWindow y pasar el usuario para mensaje de bienvenida
                var main = new MainWindow(user);
                main.Show();
                this.Close();
            }
            else
            {
                TxtMensaje.Text = "Usuario o contraseña incorrectos.";
            }
        }

        private bool ValidateCredentials(string user, string pass)
        {
            for (int i = 0; i < _users.GetLength(0); i++)
            {
                if (_users[i, 0] == user && _users[i, 1] == pass)
                    return true;
            }
            return false;
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
