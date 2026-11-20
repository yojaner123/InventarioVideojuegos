using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using InventarioVideojuegos.Models;
using InventarioVideojuegos.Services;

namespace InventarioVideojuegos.Views;

/// <summary>
/// Capa de interfaz: lee los controles, llama al servicio y muestra el resultado.
/// No contiene SQL ni reglas de validación del negocio.
/// </summary>
public partial class MainWindow : Window
{
    private readonly VideojuegoServicio _servicio;
    private Videojuego? _seleccionado;
    private bool _cargandoFormulario;

    public MainWindow()
    {
        InitializeComponent();

        CmbPlataforma.ItemsSource = VideojuegoServicio.Plataformas;
        CmbEstado.ItemsSource = VideojuegoServicio.Estados;
        CmbEstado.SelectedIndex = 0;

        try
        {
            _servicio = new VideojuegoServicio(); // crea la BD y la tabla si no existen
            CargarLista();
        }
        catch (Exception ex)
        {
            _servicio = null!;
            Mostrar("No se pudo abrir la base de datos: " + ex.Message, exito: false);
            BtnAgregar.IsEnabled = BtnActualizar.IsEnabled = BtnEliminar.IsEnabled = false;
        }
    }

    // ---------- Eventos ----------

    private void BtnAgregar_Click(object? sender, RoutedEventArgs e)
    {
        if (!TryLeerFormulario(out var juego, out var error))
        {
            Mostrar(error, exito: false);
            return;
        }

        var resultado = _servicio.Agregar(juego!);
        Mostrar(resultado.Mensaje, resultado.Exito);
        if (resultado.Exito)
        {
            LimpiarFormulario();
            CargarLista();
        }
    }

    private void BtnActualizar_Click(object? sender, RoutedEventArgs e)
    {
        if (_seleccionado == null)
        {
            Mostrar("Selecciona un videojuego de la lista para modificarlo.", exito: false);
            return;
        }

        if (!TryLeerFormulario(out var juego, out var error, _seleccionado.Id))
        {
            Mostrar(error, exito: false);
            return;
        }

        var resultado = _servicio.Actualizar(juego!);
        Mostrar(resultado.Mensaje, resultado.Exito);
        if (resultado.Exito)
        {
            LimpiarFormulario();
            CargarLista();
        }
    }

    private async void BtnEliminar_Click(object? sender, RoutedEventArgs e)
    {
        if (_seleccionado == null)
        {
            Mostrar("Selecciona un videojuego de la lista para eliminarlo.", exito: false);
            return;
        }

        bool confirmado = await ConfirmarAsync($"¿Eliminar «{_seleccionado.Titulo}»? Esta acción no se puede deshacer.");
        if (!confirmado) return;

        var resultado = _servicio.Eliminar(_seleccionado);
        Mostrar(resultado.Mensaje, resultado.Exito);
        if (resultado.Exito)
        {
            LimpiarFormulario();
            CargarLista();
        }
    }

    private void BtnLimpiar_Click(object? sender, RoutedEventArgs e)
    {
        LimpiarFormulario();
        Mostrar("Formulario limpio.", exito: true);
    }

    private void TxtBuscar_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_servicio != null) CargarLista();
    }

    private void GridJuegos_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GridJuegos.SelectedItem is not Videojuego juego) return;

        _seleccionado = juego;
        _cargandoFormulario = true;
        TxtTitulo.Text = juego.Titulo;
        CmbPlataforma.SelectedItem = juego.Plataforma;
        TxtGenero.Text = juego.Genero;
        TxtPrecio.Text = juego.Precio.ToString("0.##", CultureInfo.InvariantCulture);
        TxtAnio.Text = juego.Anio.ToString();
        CmbEstado.SelectedItem = juego.Estado;
        _cargandoFormulario = false;
    }

    // ---------- Apoyo ----------

    private void CargarLista()
    {
        try
        {
            var lista = _servicio.Listar(TxtBuscar.Text);
            _seleccionado = null;
            GridJuegos.ItemsSource = lista;
        }
        catch (Exception ex)
        {
            Mostrar("No se pudo leer la información: " + ex.Message, exito: false);
        }
    }

    /// <summary>Convierte el texto de los controles en un objeto. Informa errores de formato.</summary>
    private bool TryLeerFormulario(out Videojuego? juego, out string error, int? id = null)
    {
        juego = null;
        error = "";

        string precioTexto = (TxtPrecio.Text ?? "").Trim().Replace(',', '.');
        double precio = 0;
        if (precioTexto.Length > 0 &&
            !double.TryParse(precioTexto, NumberStyles.Float, CultureInfo.InvariantCulture, out precio))
        {
            error = "El precio debe ser un número (ej: 59.99).";
            return false;
        }

        if (!int.TryParse((TxtAnio.Text ?? "").Trim(), out int anio))
        {
            error = "El año debe ser un número entero (ej: 2023).";
            return false;
        }

        string titulo = TxtTitulo.Text ?? "";
        string plataforma = CmbPlataforma.SelectedItem as string ?? "";
        string genero = TxtGenero.Text ?? "";
        string estado = CmbEstado.SelectedItem as string ?? "Pendiente";

        juego = id.HasValue
            ? new Videojuego(id.Value, titulo, plataforma, genero, precio, anio, estado)
            : new Videojuego(titulo, plataforma, genero, precio, anio, estado);
        return true;
    }

    private void LimpiarFormulario()
    {
        _cargandoFormulario = true;
        TxtTitulo.Text = "";
        CmbPlataforma.SelectedItem = null;
        TxtGenero.Text = "";
        TxtPrecio.Text = "";
        TxtAnio.Text = "";
        CmbEstado.SelectedIndex = 0;
        GridJuegos.SelectedItem = null;
        _seleccionado = null;
        _cargandoFormulario = false;
    }

    private void Mostrar(string mensaje, bool exito)
    {
        TxtMensaje.Text = (exito ? "✔ " : "✖ ") + mensaje;
        TxtMensaje.Foreground = exito ? Brushes.DarkGreen : Brushes.Firebrick;
    }

    private async Task<bool> ConfirmarAsync(string mensaje)
    {
        bool resultado = false;
        var ventana = new Window
        {
            Title = "Confirmar",
            Width = 380,
            Height = 170,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var btnSi = new Button { Content = "Eliminar", Width = 100, HorizontalContentAlignment = HorizontalAlignment.Center };
        var btnNo = new Button { Content = "Cancelar", Width = 100, HorizontalContentAlignment = HorizontalAlignment.Center };
        btnSi.Click += (_, _) => { resultado = true; ventana.Close(); };
        btnNo.Click += (_, _) => ventana.Close();

        ventana.Content = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 18,
            Children =
            {
                new TextBlock { Text = mensaje, TextWrapping = TextWrapping.Wrap },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Children = { btnSi, btnNo }
                }
            }
        };

        await ventana.ShowDialog(this);
        return resultado;
    }
}
