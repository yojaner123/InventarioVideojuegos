using InventarioVideojuegos.Data;
using InventarioVideojuegos.Models;
using Microsoft.Data.Sqlite;

namespace InventarioVideojuegos.Services;

/// <summary>Resultado de una operación: la interfaz solo muestra el mensaje.</summary>
public record Resultado(bool Exito, string Mensaje);

/// <summary>
/// Lógica del programa: validaciones y reglas de negocio. Entre la interfaz y el DAO.
/// </summary>
public class VideojuegoServicio
{
    public static readonly string[] Plataformas =
        { "PC", "PlayStation", "Xbox", "Nintendo Switch", "Móvil", "Otra" };

    public static readonly string[] Estados = { "Pendiente", "Jugando", "Completado" };

    private readonly VideojuegoDAO _dao = new();

    public VideojuegoServicio()
    {
        BaseDatos.Inicializar();
    }

    public List<Videojuego> Listar(string? filtro = null) => _dao.Listar(filtro);

    public Resultado Agregar(Videojuego v)
    {
        string? error = Validar(v);
        if (error != null) return new Resultado(false, error);

        try
        {
            if (_dao.Existe(v.Titulo, v.Plataforma, null))
                return new Resultado(false, $"Ya existe «{v.Titulo}» para {v.Plataforma}.");

            v.AsignarId(_dao.Insertar(v));
            return new Resultado(true, $"«{v.Titulo}» se registró correctamente.");
        }
        catch (SqliteException ex)
        {
            return new Resultado(false, "Error de base de datos: " + ex.Message);
        }
    }

    public Resultado Actualizar(Videojuego v)
    {
        if (v.Id <= 0) return new Resultado(false, "Selecciona un videojuego de la lista para modificarlo.");

        string? error = Validar(v);
        if (error != null) return new Resultado(false, error);

        try
        {
            if (_dao.Existe(v.Titulo, v.Plataforma, v.Id))
                return new Resultado(false, $"Ya existe otro registro «{v.Titulo}» para {v.Plataforma}.");

            return _dao.Actualizar(v)
                ? new Resultado(true, $"«{v.Titulo}» se actualizó correctamente.")
                : new Resultado(false, "No se encontró el registro (¿ya fue eliminado?).");
        }
        catch (SqliteException ex)
        {
            return new Resultado(false, "Error de base de datos: " + ex.Message);
        }
    }

    public Resultado Eliminar(Videojuego v)
    {
        try
        {
            return _dao.Eliminar(v.Id)
                ? new Resultado(true, $"«{v.Titulo}» se eliminó correctamente.")
                : new Resultado(false, "No se encontró el registro.");
        }
        catch (SqliteException ex)
        {
            return new Resultado(false, "Error de base de datos: " + ex.Message);
        }
    }

    /// <summary>Devuelve el mensaje de error, o null si el videojuego es válido.</summary>
    public static string? Validar(Videojuego v)
    {
        if (string.IsNullOrWhiteSpace(v.Titulo)) return "El título es obligatorio.";
        if (v.Titulo.Length > 100) return "El título no puede superar 100 caracteres.";
        if (string.IsNullOrWhiteSpace(v.Plataforma)) return "Selecciona una plataforma.";
        if (v.Precio < 0) return "El precio no puede ser negativo.";
        int anioMaximo = DateTime.Now.Year + 2;
        if (v.Anio < 1970 || v.Anio > anioMaximo)
            return $"El año debe estar entre 1970 y {anioMaximo}.";
        if (!Estados.Contains(v.Estado)) return "Estado no válido.";
        return null;
    }
}
