using System;
using System.Collections.Generic;
using System.Windows;
using MySql.Data.MySqlClient;

namespace PracticaVideojuegos
{
    public partial class LoginWindow : Window
    {
        // Control de intentos fallidos y bloqueo temporal por usuario
        private readonly Dictionary<string, int> _failedAttempts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DateTime> _lockedUntil = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        // Duración del bloqueo para pruebas (ajustable)
        private readonly TimeSpan _lockDuration = TimeSpan.FromMinutes(1);
        private readonly DatabaseHelper _database = new DatabaseHelper();

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
                MessageBox.Show(
                    "Introduzca datos",
                    "Error de inicio de sesión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                TxtMensaje.Text = "Introduzca datos";
                return;
            }

            if (IsSqlInjectionAttempt(user) || IsSqlInjectionAttempt(pass))
            {
                ShowMessage("No se permite el acceso", "Acceso denegado", MessageBoxImage.Warning);
                return;
            }

            // Comprueba si el usuario está bloqueado temporalmente
            if (_lockedUntil.TryGetValue(user, out var until) && DateTime.Now < until)
            {
                var remaining = until - DateTime.Now;
                TxtMensaje.Text = $"Usuario bloqueado temporalmente. Inténtalo de nuevo en {(int)remaining.TotalSeconds} s.";
                return;
            }

            try
            {
                if (_database.ValidateCredentials(user, pass))
                {
                    // Login correcto: resetear intentos y abrir MainWindow
                    _failedAttempts.Remove(user);
                    _lockedUntil.Remove(user);

                    var main = new MainWindow(user);
                    main.Show();
                    Close();
                    return;
                }

                RegisterFailedAttempt(user);
            }
            catch (MySqlException)
            {
                ShowMessage(
                    "No se ha podido conectar con la base de datos. Comprueba que MySQL está iniciado y la configuración de conexión es correcta.",
                    "Error de conexión",
                    MessageBoxImage.Error);
            }
        }

        private void RegisterFailedAttempt(string user)
        {
            int attempts = 0;
            _failedAttempts.TryGetValue(user, out attempts);
            attempts++;
            _failedAttempts[user] = attempts;

            if (attempts >= 3)
            {
                _lockedUntil[user] = DateTime.Now.Add(_lockDuration);
                MessageBox.Show(
                    "Se ha bloqueado temporalmente el usuario",
                    "Error de inicio de sesión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                TxtMensaje.Text = "Se ha bloqueado temporalmente el usuario";
            }
            else
            {
                MessageBox.Show(
                    "Usuario o contraseña incorrectos",
                    "Error de inicio de sesión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                TxtMensaje.Text = "Usuario o contraseña incorrectos";
            }
        }

        private static bool IsSqlInjectionAttempt(string value)
        {
            var normalized = value.Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
            return normalized.Contains("'or'1'='1", StringComparison.Ordinal)
                || normalized.Contains("or1=1", StringComparison.Ordinal);
        }

        private void BtnNuevoUsuario_Click(object sender, RoutedEventArgs e)
        {
            var user = TxtUsuario.Text?.Trim();
            var pass = PwdPassword.Password ?? string.Empty;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                ShowMessage("Introduzca usuario y contraseña para crear la cuenta", "Datos incompletos", MessageBoxImage.Warning);
                return;
            }

            try
            {
                _database.CreateUser(user, pass);
                ShowMessage("Usuario creado correctamente", "Nuevo usuario", MessageBoxImage.Information);
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ShowMessage("El usuario ya existe", "Nuevo usuario", MessageBoxImage.Warning);
            }
            catch (MySqlException)
            {
                ShowMessage(
                    "No se ha podido conectar con la base de datos. Comprueba que MySQL está iniciado y la configuración de conexión es correcta.",
                    "Error de conexión",
                    MessageBoxImage.Error);
            }
        }

        private void ShowMessage(string message, string title, MessageBoxImage image)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, image);
            TxtMensaje.Text = message;
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
