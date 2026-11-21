using MySql.Data.MySqlClient;

namespace PracticaVideojuegos
{
    public sealed class DatabaseHelper
    {
        // Cambia Password por la contraseña de tu instalación local de MySQL.
        private const string ConnectionString =
            "Server=localhost;Port=3306;Database=catalogo_videojuegos;Uid=root;Pwd=root;";

        public bool ValidateCredentials(string username, string password)
        {
            const string query = "SELECT COUNT(*) FROM Usuarios WHERE nombre_usuario = @username AND password = @password";

            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public void CreateUser(string username, string password)
        {
            const string query = "INSERT INTO Usuarios (nombre_usuario, password) VALUES (@username, @password)";

            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            command.ExecuteNonQuery();
        }
    }
}
