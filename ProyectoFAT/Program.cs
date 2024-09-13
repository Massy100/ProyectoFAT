using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

public class FATFile
{
    public string Nombre { get; set; }
    public string RutaInicio { get; set; }
    public bool PapeleraReciclaje { get; set; }
    public int TotalCaracteres { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public class FragmentoArchivo
{
    public string Datos { get; set; }
    public string SiguienteArchivo { get; set; }
    public bool EOF { get; set; }
}

public class Program
{
    static List<FATFile> archivosFAT = new List<FATFile>();

    public static void Main()
    {
        int opcion;
        do
        {
            Console.Clear();
            Console.WriteLine("Menú de opciones:");
            Console.WriteLine("1. Crear un archivo y agregar datos");
            Console.WriteLine("2. Listar archivos");
            Console.WriteLine("3. Abrir un archivo");
            Console.WriteLine("4. Modificar un archivo");
            Console.WriteLine("5. Eliminar un archivo");
            Console.WriteLine("6. Recuperar un archivo");
            Console.WriteLine("7. Salir");
            Console.Write("Selecciona una opción: ");
            opcion = int.Parse(Console.ReadLine());

            switch (opcion)
            {
                case 1:
                    Console.Write("Ingresa el nombre del archivo: ");
                    string nombreArchivoCrear = Console.ReadLine();
                    Console.Write("Ingresa el contenido del archivo: ");
                    string contenido = Console.ReadLine();
                    CrearArchivo(nombreArchivoCrear, contenido);
                    break;
                case 2:
                    ListarArchivos(); // Mostrar todos los archivos
                    break;
                case 3:
                    AbrirArchivo();
                    break;
                case 4:
                    ModificarArchivo();
                    break;
                case 5:
                    EliminarArchivo();
                    break;
                case 6:
                    RecuperarArchivo();
                    break;
                case 7:
                    Console.WriteLine("Saliendo...");
                    break;
                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }

            if (opcion != 7)
            {
                Console.WriteLine("Presiona cualquier tecla para continuar...");
                Console.ReadKey();
            }

        } while (opcion != 7);
    }

    public static void CrearArchivo(string nombreArchivo, string contenido)
    {
        string fatFilePath = nombreArchivo + "_fat.json";

        // Verificar si el archivo FAT ya existe
        if (File.Exists(fatFilePath))
        {
            Console.WriteLine("El archivo FAT ya existe. No se creará uno nuevo.");
            return;  // Salir para no sobrescribir el archivo
        }

        // Crear un nuevo archivo FAT y fragmentos
        FATFile archivoFAT = new FATFile
        {
            Nombre = nombreArchivo,
            RutaInicio = nombreArchivo + "_0.json",
            PapeleraReciclaje = false,
            TotalCaracteres = contenido.Length,
            FechaCreacion = DateTime.Now,
            FechaModificacion = null,
            FechaEliminacion = null
        };

        archivosFAT.Add(archivoFAT);
        string archivoFATJson = JsonSerializer.Serialize(archivoFAT);
        File.WriteAllText(fatFilePath, archivoFATJson);

        // Fragmentar el contenido y crear los archivos de datos
        int indice = 0;
        string rutaActual = archivoFAT.RutaInicio;

        for (int i = 0; i < contenido.Length; i += 20)
        {
            string fragmentoDatos = contenido.Substring(i, Math.Min(20, contenido.Length - i));
            FragmentoArchivo fragmento = new FragmentoArchivo
            {
                Datos = fragmentoDatos,
                SiguienteArchivo = (i + 20 < contenido.Length) ? $"{nombreArchivo}_{indice + 1}.json" : null,
                EOF = (i + 20 >= contenido.Length)
            };

            string fragmentoJson = JsonSerializer.Serialize(fragmento);
            File.WriteAllText(rutaActual, fragmentoJson);

            rutaActual = fragmento.SiguienteArchivo;
            indice++;
        }
        Console.WriteLine("Archivo creado exitosamente.");
    }

    public static void ListarArchivos(bool enPapelera = false)
    {
        string[] archivosFAT = Directory.GetFiles(Directory.GetCurrentDirectory(), "*_fat.json");
        List<FATFile> archivos = new List<FATFile>();

        foreach (var archivoPath in archivosFAT)
        {
            FATFile archivoFAT = JsonSerializer.Deserialize<FATFile>(File.ReadAllText(archivoPath));
            if (archivoFAT.PapeleraReciclaje == enPapelera)
            {
                archivos.Add(archivoFAT);
            }
        }

        if (archivos.Count == 0)
        {
            Console.WriteLine(enPapelera ? "No hay archivos en la papelera de reciclaje." : "No hay archivos disponibles.");
            return;
        }

        for (int i = 0; i < archivos.Count; i++)
        {
            FATFile archivoFAT = archivos[i];
            Console.WriteLine($"{i + 1}. Nombre: {archivoFAT.Nombre}, Tamaño: {archivoFAT.TotalCaracteres} caracteres, Creación: {archivoFAT.FechaCreacion}, Modificación: {archivoFAT.FechaModificacion}");
        }
    }

    public static void ListarPapelera()
    {
        string[] archivosFAT = Directory.GetFiles(Directory.GetCurrentDirectory(), "*_reciclaje.json");
        List<FATFile> archivos = new List<FATFile>();

        foreach (var archivoPath in archivosFAT)
        {
            FATFile archivoFAT = JsonSerializer.Deserialize<FATFile>(File.ReadAllText(archivoPath));
            if (archivoFAT.PapeleraReciclaje) // Mostrar solo archivos que están en la papelera de reciclaje
            {
                archivos.Add(archivoFAT);
            }
        }

        if (archivos.Count == 0)
        {
            Console.WriteLine("No hay archivos en la papelera de reciclaje.");
            return;
        }

        for (int i = 0; i < archivos.Count; i++)
        {
            FATFile archivoFAT = archivos[i];
            Console.WriteLine($"{i + 1}. Nombre: {archivoFAT.Nombre}, Tamaño: {archivoFAT.TotalCaracteres} caracteres, Creación: {archivoFAT.FechaCreacion}, Modificación: {archivoFAT.FechaModificacion}");
        }
    }

    public static void AbrirArchivo()
    {
        ListarArchivos();
        Console.Write("Selecciona el número del archivo que deseas abrir: ");

        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= archivosFAT.Count)
        {
            FATFile archivo = archivosFAT[index - 1];
            string contenido = LeerContenidoArchivo(archivo.RutaInicio);
            Console.WriteLine($"Archivo: {archivo.Nombre}, Tamaño: {archivo.TotalCaracteres}, Fecha de Creación: {archivo.FechaCreacion}, Fecha de Modificación: {archivo.FechaModificacion}");
            Console.WriteLine("Contenido:");
            Console.WriteLine(contenido);
        }
        else
        {
            Console.WriteLine("Número de archivo no válido.");
        }
    }

    public static void ModificarArchivo()
    {
        ListarArchivos();
        Console.Write("Selecciona el número del archivo que deseas modificar: ");

        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= archivosFAT.Count)
        {
            FATFile archivoFAT = archivosFAT[index - 1];

            if (archivoFAT.PapeleraReciclaje)
            {
                Console.WriteLine("No puedes modificar un archivo que está en la papelera de reciclaje.");
                return;
            }

            string fatFilePath = archivoFAT.Nombre + "_fat.json";
            string contenidoActual = LeerContenidoArchivo(archivoFAT.RutaInicio);
            Console.WriteLine($"Contenido actual del archivo '{archivoFAT.Nombre}':\n{contenidoActual}");

            Console.WriteLine("Ingresa los nuevos datos (presiona ESC para finalizar):");
            string nuevoContenido = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Escape)
                {
                    nuevoContenido += key.KeyChar;
                    Console.Write(key.KeyChar);
                }
            } while (key.Key != ConsoleKey.Escape);

            Console.Write("\n¿Deseas guardar los cambios? (s/n): ");
            if (Console.ReadLine().ToLower() == "s")
            {
                archivoFAT.FechaModificacion = DateTime.Now;
                archivoFAT.TotalCaracteres = nuevoContenido.Length;

                int indice = 0;
                string rutaActual = archivoFAT.RutaInicio;

                for (int i = 0; i < nuevoContenido.Length; i += 20)
                {
                    string fragmentoDatos = nuevoContenido.Substring(i, Math.Min(20, nuevoContenido.Length - i));
                    FragmentoArchivo fragmento = new FragmentoArchivo
                    {
                        Datos = fragmentoDatos,
                        SiguienteArchivo = (i + 20 < nuevoContenido.Length) ? $"{archivoFAT.Nombre}_{indice + 1}.json" : null,
                        EOF = (i + 20 >= nuevoContenido.Length)
                    };

                    string fragmentoJson = JsonSerializer.Serialize(fragmento);
                    File.WriteAllText(rutaActual, fragmentoJson);

                    rutaActual = fragmento.SiguienteArchivo;
                    indice++;
                }

                File.WriteAllText(fatFilePath, JsonSerializer.Serialize(archivoFAT));
                Console.WriteLine("Archivo modificado y guardado.");
            }
            else
            {
                Console.WriteLine("Cambios descartados.");
            }
        }
        else
        {
            Console.WriteLine("Número de archivo no válido.");
        }
    }

    public static void EliminarArchivo()
    {
        ListarArchivos(); // Mostrar archivos disponibles
        Console.Write("Selecciona el número del archivo que deseas eliminar: ");

        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= archivosFAT.Count)
        {
            FATFile archivoFAT = archivosFAT[index - 1];

            if (archivoFAT.PapeleraReciclaje)
            {
                Console.WriteLine("El archivo ya está en la papelera de reciclaje.");
                return;
            }

            archivoFAT.FechaEliminacion = DateTime.Now;
            archivoFAT.PapeleraReciclaje = true;

            string fatFilePath = archivoFAT.Nombre + "_fat.json";
            string reciclajeFilePath = archivoFAT.Nombre + "_reciclaje.json";

            // Mover archivos a la papelera de reciclaje
            File.Move(fatFilePath, reciclajeFilePath);

            // Actualizar archivo de metadatos en la papelera de reciclaje
            File.WriteAllText(reciclajeFilePath, JsonSerializer.Serialize(archivoFAT));

            // Eliminar de la lista de archivos activos
            archivosFAT.RemoveAt(index - 1);

            Console.WriteLine("Archivo eliminado y movido a la papelera de reciclaje.");
        }
        else
        {
            Console.WriteLine("Número de archivo no válido.");
        }
    }

    public static void RecuperarArchivo()
    {
        ListarPapelera(); // Mostrar archivos en la papelera de reciclaje
        Console.Write("Selecciona el número del archivo que deseas recuperar: ");

        // Cargar la lista de archivos en papelera de reciclaje
        string[] archivosReciclaje = Directory.GetFiles(Directory.GetCurrentDirectory(), "*_reciclaje.json");
        List<FATFile> archivosPapelera = new List<FATFile>();

        foreach (var archivoPath in archivosReciclaje)
        {
            FATFile archivoFAT = JsonSerializer.Deserialize<FATFile>(File.ReadAllText(archivoPath));
            if (archivoFAT.PapeleraReciclaje)
            {
                archivosPapelera.Add(archivoFAT);
            }
        }

        if (archivosPapelera.Count == 0)
        {
            Console.WriteLine("No hay archivos en la papelera de reciclaje.");
            return;
        }

        // Validar y procesar la entrada del usuario
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= archivosPapelera.Count)
        {
            FATFile archivoFAT = archivosPapelera[index - 1];

            Console.Write("¿Estás seguro de que deseas recuperar el archivo? (s/n): ");
            if (Console.ReadLine().Trim().ToLower() != "s")
            {
                Console.WriteLine("Operación cancelada.");
                return;
            }

            archivoFAT.PapeleraReciclaje = false;
            archivoFAT.FechaEliminacion = null;

            string reciclajeFilePath = archivoFAT.Nombre + "_reciclaje.json";
            string nuevoNombreFat = archivoFAT.Nombre + "_fat.json";

            // Mover el archivo de reciclaje a la ubicación original
            if (File.Exists(reciclajeFilePath))
            {
                File.Move(reciclajeFilePath, nuevoNombreFat);
            }
            else
            {
                Console.WriteLine($"El archivo {reciclajeFilePath} no se encuentra.");
                return;
            }

            // Actualizar archivo de metadatos
            File.WriteAllText(nuevoNombreFat, JsonSerializer.Serialize(archivoFAT));
            Console.WriteLine("Archivo recuperado de la papelera de reciclaje.");
        }
        else
        {
            Console.WriteLine("Número de archivo no válido.");
        }
    }
    private static string LeerContenidoArchivo(string rutaInicio)
    {
        string contenido = "";
        string rutaActual = rutaInicio;

        while (rutaActual != null)
        {
            if (File.Exists(rutaActual))
            {
                FragmentoArchivo fragmento = JsonSerializer.Deserialize<FragmentoArchivo>(File.ReadAllText(rutaActual));
                contenido += fragmento.Datos;

                if (fragmento.EOF)
                    break;

                rutaActual = fragmento.SiguienteArchivo;
            }
            else
            {
                break;
            }
        }

        return contenido;
    }
}
