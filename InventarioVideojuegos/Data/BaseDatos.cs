using Microsoft.Data.Sqlite;

namespace InventarioVideojuegos.Data;

/// <summary>
/// Se encarga de ubicar el archivo SQLite, entregar conexiones y crear la tabla si no existe.
/// </summary>
public static class BaseDatos
{
    public static string RutaArchivo { get; } = ObtenerRuta();

    public static string CadenaConexion => $"Data Source={RutaArchivo}";

    private static string ObtenerRuta()
    {
        // Carpeta del usuario: el archivo sobrevive a recompilar y a borrar bin/ y obj/.
        string carpeta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InventarioVideojuegos");
        Directory.CreateDirectory(carpeta);
        return Path.Combine(carpeta, "videojuegos.db");
    }

    public static SqliteConnection CrearConexion() => new SqliteConnection(CadenaConexion);

    /// <summary>Crea el archivo .db y la tabla la primera vez que se ejecuta.</summary>
    public static void Inicializar()
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS Videojuegos (
                Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                Titulo     TEXT    NOT NULL,
                Plataforma TEXT    NOT NULL,
                Genero     TEXT    NOT NULL DEFAULT '',
                Precio     REAL    NOT NULL DEFAULT 0,
                Anio       INTEGER NOT NULL,
                Estado     TEXT    NOT NULL DEFAULT 'Pendiente'
            );";

        using var conexion = CrearConexion();
        conexion.Open();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        comando.ExecuteNonQuery();
    }
}
