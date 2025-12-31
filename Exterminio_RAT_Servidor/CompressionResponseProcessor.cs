using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Exterminio_RAT_Servidor
{
    public class CompressionResult
    {
        public string Estado { get; set; } // "completado", "fallido", "progreso"
        public string Mensaje { get; set; }
        public int Progreso { get; set; } // 0-100
        public string RutaArchivo { get; set; }
        public string RutaDestino { get; set; }
        public long TamañoOriginal { get; set; }
        public long TamañoComprimido { get; set; }
    }

    public static class CompressionResponseProcessor
    {
        public static string ProcesarRespuestaCompresion(string datosBase64)
        {
            try
            {
                Console.WriteLine($"Procesando respuesta de compresión: {datosBase64.Length} caracteres");
                
                // Decodificar Base64
                byte[] datosComprimidos = Convert.FromBase64String(datosBase64);
                
                // Descomprimir con Deflate
                string informacionDescomprimida;
                using (MemoryStream inputStream = new MemoryStream(datosComprimidos))
                {
                    using (DeflateStream deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                    {
                        using (StreamReader reader = new StreamReader(deflateStream, Encoding.UTF8))
                        {
                            informacionDescomprimida = reader.ReadToEnd();
                        }
                    }
                }
                
                Console.WriteLine($"Respuesta descomprimida: {informacionDescomprimida}");
                return informacionDescomprimida;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando respuesta de compresión: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }

        public static CompressionResult ParsearRespuestaCompresion(string informacion)
        {
            try
            {
                Console.WriteLine($"Parseando respuesta: {informacion}");
                
                // Separar la información por |
                string[] partes = informacion.Split('|');
                
                if (partes.Length >= 7)
                {
                    CompressionResult result = new CompressionResult
                    {
                        Estado = partes[0],
                        Mensaje = partes[1],
                        Progreso = int.TryParse(partes[2], out int progreso) ? progreso : 0,
                        RutaArchivo = partes[3],
                        RutaDestino = partes[4],
                        TamañoOriginal = long.TryParse(partes[5], out long tamañoOriginal) ? tamañoOriginal : 0,
                        TamañoComprimido = long.TryParse(partes[6], out long tamañoComprimido) ? tamañoComprimido : 0
                    };
                    
                    Console.WriteLine($"Respuesta parseada: Estado={result.Estado}, Progreso={result.Progreso}%, Mensaje={result.Mensaje}");
                    return result;
                }
                else
                {
                    throw new Exception("Formato de respuesta inválido");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parseando respuesta de compresión: {ex.Message}");
                return new CompressionResult
                {
                    Estado = "fallido",
                    Mensaje = $"Error parseando respuesta: {ex.Message}",
                    Progreso = 0
                };
            }
        }
    }
}
