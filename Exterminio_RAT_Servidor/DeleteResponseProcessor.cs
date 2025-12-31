using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Exterminio_RAT_Servidor
{
    public class DeleteResult
    {
        public string Estado { get; set; } // "completado", "fallido"
        public string Mensaje { get; set; }
        public string RutaArchivo { get; set; }
        public bool ExisteDespues { get; set; }
    }

    public static class DeleteResponseProcessor
    {
        public static string ProcesarRespuestaBorrado(string datosBase64)
        {
            try
            {
                Console.WriteLine($"Procesando respuesta de borrado: {datosBase64.Length} caracteres");
                
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
                
                Console.WriteLine($"Respuesta de borrado descomprimida: {informacionDescomprimida}");
                return informacionDescomprimida;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando respuesta de borrado: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }

        public static DeleteResult ParsearRespuestaBorrado(string informacion)
        {
            try
            {
                Console.WriteLine($"Parseando respuesta de borrado: {informacion}");
                
                // Separar la información por |
                string[] partes = informacion.Split('|');
                
                if (partes.Length >= 4)
                {
                    DeleteResult result = new DeleteResult
                    {
                        Estado = partes[0],
                        Mensaje = partes[1],
                        RutaArchivo = partes[2],
                        ExisteDespues = bool.TryParse(partes[3], out bool existe) ? existe : true
                    };
                    
                    Console.WriteLine($"Respuesta de borrado parseada: Estado={result.Estado}, Mensaje={result.Mensaje}, ExisteDespues={result.ExisteDespues}");
                    return result;
                }
                else
                {
                    throw new Exception("Formato de respuesta de borrado inválido");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parseando respuesta de borrado: {ex.Message}");
                return new DeleteResult
                {
                    Estado = "fallido",
                    Mensaje = $"Error parseando respuesta: {ex.Message}",
                    ExisteDespues = true
                };
            }
        }
    }
}
