using InventarioVideojuegos.Models;
using Microsoft.Data.Sqlite;

namespace InventarioVideojuegos.Data;

/// <summary>
/// Acceso a datos con ADO.NET. Es la ÚNICA clase que escribe SQL.
/// Todas las consultas con datos del usuario son parametrizadas.
/// </summary>
public class VideojuegoDAO
{
    // CREATE
    public int Insertar(Videojuego v)
    {
        const string sql = @"
            INSERT INTO Videojuegos (Titulo, Plataforma, Genero, Precio, Anio, Estado)
            VALUES (@titulo, @plataforma, @genero, @precio, @anio, @estado);
            SELECT last_insert_rowid();";

        using var conexion = BaseDatos.CrearConexion();
        conexion.Open();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        AgregarParametros(comando, v);
        return Convert.ToInt32(comando.ExecuteScalar());
    }

    // READ
    public List<Videojuego> Listar(string? filtro = null)
    {
        string sql = "SELECT Id, Titulo, Plataforma, Genero, Precio, Anio, Estado FROM Videojuegos";
        bool hayFiltro = !string.IsNullOrWhiteSpace(filtro);
        if (hayFiltro)
            sql += " WHERE Titulo LIKE @filtro OR Plataforma LIKE @filtro OR Genero LIKE @filtro";
        sql += " ORDER BY Titulo COLLATE NOCASE";

        var lista = new List<Videojuego>();

        using var conexion = BaseDatos.CrearConexion();
        conexion.Open();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        if (hayFiltro)
            comando.Parameters.AddWithValue("@filtro", $"%{filtro!.Trim()}%");

        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            lista.Add(new Videojuego(
                lector.GetInt32(0),
                lector.GetString(1),
                lector.GetString(2),
                lector.GetString(3),
                lector.GetDouble(4),
                lector.GetInt32(5),
                lector.GetString(6)));
        }
        return lista;
    }

    // UPDATE
    public bool Actualizar(Videojuego v)
    {
        const string sql = @"
            UPDATE Videojuegos
            SET Titulo = @titulo, Plataforma = @plataforma, Genero = @genero,
                Precio = @precio, Anio = @anio, Estado = @estado
            WHERE Id = @id;";

        using var conexion = BaseDatos.CrearConexion();
        conexion.Open();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        AgregarParametros(comando, v);
        comando.Parameters.AddWithValue("@id", v.Id);
        return comando.ExecuteNonQuery() > 0;
    }

    // DELETE
    public bool Eliminar(int id)
    {
        using var conexion = BaseDatos.CrearConexion();
        conexion.Open();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "DELETE FROM Videojuegos WHERE Id = @id;";
        comando.Parameters.AddWithValue("@id", id);
        return comando.ExecuteNonQuery() > 0;
    }

    /// <summary>¿Ya existe un juego con ese título en esa plataforma? (excluye al propio registro al editar)</summary>
    public bool Existe(string titulo, string plataforma, int? excluirId)
    {
        const string sql = @"
            SELECT COUNT(*) FROM Videojuegos
            WHERE Titulo = @titulo COLLATE NOCASE
              AND Plataforma = @plataforma COLLATE NOCASE
              AND Id <> @id;";

        using var conexion = BaseDatos.CrearConexion();
        conexion.Open();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        comando.Parameters.AddWithValue("@titulo", titulo);
        comando.Parameters.AddWithValue("@plataforma", plataforma);
        comando.Parameters.AddWithValue("@id", excluirId ?? -1);
        return Convert.ToInt32(comando.ExecuteScalar()) > 0;
    }

    private static void AgregarParametros(SqliteCommand comando, Videojuego v)
    {
        comando.Parameters.AddWithValue("@titulo", v.Titulo);
        comando.Parameters.AddWithValue("@plataforma", v.Plataforma);
        comando.Parameters.AddWithValue("@genero", v.Genero);
        comando.Parameters.AddWithValue("@precio", v.Precio);
        comando.Parameters.AddWithValue("@anio", v.Anio);
        comando.Parameters.AddWithValue("@estado", v.Estado);
    }
}
