using System;
using System.Collections.Generic;
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

        // Control de intentos fallidos y bloqueo temporal por usuario
        private readonly Dictionary<string, int> _failedAttempts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DateTime> _lockedUntil = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        // Duración del bloqueo para pruebas (ajustable)
        private readonly TimeSpan _lockDuration = TimeSpan.FromMinutes(1);

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

            // Buscar si el usuario existe y obtener su índice
            int userIndex = -1;
            for (int i = 0; i < _users.GetLength(0); i++)
            {
                if (string.Equals(_users[i, 0], user, StringComparison.OrdinalIgnoreCase))
                {
                    userIndex = i;
                    break;
                }
            }

            if (userIndex == -1)
            {
                // Usuario no existe
                TxtMensaje.Text = "El usuario introducido no existe";
                return;
            }

            // Comprueba si el usuario está bloqueado temporalmente
            if (_lockedUntil.TryGetValue(user, out var until) && DateTime.Now < until)
            {
                var remaining = until - DateTime.Now;
                TxtMensaje.Text = $"Usuario bloqueado temporalmente. Inténtalo de nuevo en {remaining.Seconds} s.";
                return;
            }

            // Comprobación de contraseña
            if (string.Equals(_users[userIndex, 1], pass))
            {
                // Login correcto: resetear intentos y abrir MainWindow
                if (_failedAttempts.ContainsKey(user))
                    _failedAttempts.Remove(user);
                if (_lockedUntil.ContainsKey(user))
                    _lockedUntil.Remove(user);

                var main = new MainWindow(user);
                main.Show();
                this.Close();
                return;
            }

            // Contraseña incorrecta: incrementar contador
            int attempts = 0;
            _failedAttempts.TryGetValue(user, out attempts);
            attempts++;
            _failedAttempts[user] = attempts;

            if (attempts >= 3)
            {
                _lockedUntil[user] = DateTime.Now.Add(_lockDuration);
                TxtMensaje.Text = "Usuario bloqueado temporalmente debido a varios intentos fallidos.";
            }
            else
            {
                TxtMensaje.Text = "La contraseña introducida no es correcta";
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
