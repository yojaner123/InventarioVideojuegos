-- Script de referencia. La aplicación ejecuta esta misma instrucción
-- automáticamente al iniciar (ver Data/BaseDatos.cs).
CREATE TABLE IF NOT EXISTS Videojuegos (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    Titulo     TEXT    NOT NULL,
    Plataforma TEXT    NOT NULL,
    Genero     TEXT    NOT NULL DEFAULT '',
    Precio     REAL    NOT NULL DEFAULT 0,
    Anio       INTEGER NOT NULL,
    Estado     TEXT    NOT NULL DEFAULT 'Pendiente'
);
