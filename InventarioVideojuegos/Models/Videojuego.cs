namespace InventarioVideojuegos.Models;

/// <summary>
/// Entidad principal del sistema. Solo representa datos (no sabe nada de SQLite ni de la interfaz).
/// </summary>
public class Videojuego
{
    private string _titulo = "";
    private string _plataforma = "";
    private string _genero = "";
    private string _estado = "Pendiente";

    // Encapsulamiento: el Id solo lo asigna la base de datos (o el constructor al leer).
    public int Id { get; private set; }

    public string Titulo
    {
        get => _titulo;
        set => _titulo = value?.Trim() ?? "";
    }

    public string Plataforma
    {
        get => _plataforma;
        set => _plataforma = value?.Trim() ?? "";
    }

    public string Genero
    {
        get => _genero;
        set => _genero = value?.Trim() ?? "";
    }

    public double Precio { get; set; }
    public int Anio { get; set; }

    public string Estado
    {
        get => _estado;
        set => _estado = string.IsNullOrWhiteSpace(value) ? "Pendiente" : value.Trim();
    }

    // Constructor para un registro nuevo (aún sin Id).
    public Videojuego(string titulo, string plataforma, string genero,
                      double precio, int anio, string estado)
    {
        Titulo = titulo;
        Plataforma = plataforma;
        Genero = genero;
        Precio = precio;
        Anio = anio;
        Estado = estado;
    }

    // Constructor para un registro leído de la base de datos.
    public Videojuego(int id, string titulo, string plataforma, string genero,
                      double precio, int anio, string estado)
        : this(titulo, plataforma, genero, precio, anio, estado)
    {
        Id = id;
    }

    public void AsignarId(int id) => Id = id;

    public override string ToString() => $"{Titulo} ({Plataforma}, {Anio})";
}
