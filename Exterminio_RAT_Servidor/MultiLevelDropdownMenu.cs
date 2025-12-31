using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Exterminio_RAT_Servidor
{
    public partial class MultiLevelDropdownMenu : Form
    {
        private List<MenuItem> menuItems;
        private bool isExpanded = false;
        private int animationStep = 0;
        private Timer animationTimer;
        private int maxHeight = 400; // Reducido para mejor UX
        private Color backgroundColor = Color.FromArgb(25, 25, 25);
        private Color hoverColor = Color.FromArgb(40, 40, 40);
        private Color textColor = Color.Red;
        private Color borderColor = Color.FromArgb(60, 60, 60);
        private int borderRadius = 20;
        private Point clickPosition;
        private int scrollOffset = 0;
        private int itemHeight = 35;
        private int visibleItems = 0;
        private bool isScrolling = false;
        private Point lastMousePosition;

        public event EventHandler<MenuItemSelectedEventArgs> MenuItemSelected;

        public MultiLevelDropdownMenu(Point position)
        {
            try
            {
                Console.WriteLine($"Constructor MultiLevelDropdownMenu llamado con posición: {position}");
                
                InitializeComponent();
                Console.WriteLine("InitializeComponent completado");
                
                clickPosition = position;
                InitializeMenu();
                Console.WriteLine("InitializeMenu completado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en constructor MultiLevelDropdownMenu: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        private void InitializeMenu()
        {
            try
            {
                Console.WriteLine("Iniciando InitializeMenu");

                menuItems = new List<MenuItem>();
                Console.WriteLine("Lista menuItems creada");

                // Configurar el formulario como menú flotante
                this.FormBorderStyle = FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.TopMost = true;
                this.BackColor = backgroundColor;
                this.TransparencyKey = Color.Fuchsia; // Para hacer transparente el fondo
                this.Size = new Size(250, 40);
                this.StartPosition = FormStartPosition.Manual;
                this.Location = clickPosition;
                this.Cursor = Cursors.Hand;
                
                // Configurar doble buffer para evitar parpadeo
                this.SetStyle(ControlStyles.DoubleBuffer, true);
                this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                this.SetStyle(ControlStyles.UserPaint, true);
                this.SetStyle(ControlStyles.ResizeRedraw, true);

                Console.WriteLine($"Formulario configurado: Location={this.Location}, Size={this.Size}");

                // Configurar timer de animación
                animationTimer = new Timer();
                animationTimer.Interval = 10;
                animationTimer.Tick += AnimationTimer_Tick;

                // Agregar eventos
                this.Paint += MultiLevelDropdownMenu_Paint;
                this.Deactivate += MultiLevelDropdownMenu_Deactivate;
                this.MouseWheel += MultiLevelDropdownMenu_MouseWheel;

                // Agregar algunos elementos de ejemplo con iconos
                AddMenuItem("Acciones del Cliente", null, "🎯");
                AddSubMenuItem("Acciones del Cliente", "Capturar Pantalla", "screen_capture", "📸");
                AddSubMenuItem("Acciones del Cliente", "Tomar Screenshot", "screenshot", "🖼️");
                AddSubMenuItem("Acciones del Cliente", "Webcam", "webcam", "📹");
                AddSubMenuItem("Acciones del Cliente", "Micrófono", "microphone", "🎤");

                AddMenuItem("Sistema", null, "⚙️");
                AddSubMenuItem("Sistema", "Información del Sistema", "system_info", "ℹ️");
                AddSubMenuItem("Sistema", "Procesos", "processes", "📊");
                AddSubMenuItem("Sistema", "Servicios", "services", "🔧");
                AddSubMenuItem("Sistema", "Registro", "registry", "📝");

                AddMenuItem("Red", null, "🌐");
                AddSubMenuItem("Red", "Puertos Abiertos", "open_ports", "🔌");
                AddSubMenuItem("Red", "Conexiones Activas", "active_connections", "🔗");
                AddSubMenuItem("Red", "Configuración de Red", "network_config", "⚡");

                AddMenuItem("Archivos", null, "📁");
                AddSubMenuItem("Archivos", "Explorador de Archivos", "file_explorer", "📂");
                AddSubMenuItem("Archivos", "Descargar Archivo", "download_file", "⬇️");
                AddSubMenuItem("Archivos", "Subir Archivo", "upload_file", "⬆️");

                AddMenuItem("Galería", null, "🖼️");
                AddSubMenuItem("Galería", "Galería Clientes", "gallery_clients", "🖼️");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en InitializeMenu: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MultiLevelDropdownMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(250, 40);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MultiLevelDropdownMenu";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.Fuchsia;
            this.ResumeLayout(false);
        }

        public void AddMenuItem(string text, string action, string icon = "")
        {
            menuItems.Add(new MenuItem { Text = text, Action = action, Level = 0, Icon = icon });
            this.Invalidate();
        }

        public void AddSubMenuItem(string parentText, string text, string action, string icon = "")
        {
            var parent = menuItems.Find(m => m.Text == parentText);
            if (parent != null)
            {
                menuItems.Add(new MenuItem { Text = text, Action = action, Level = 1, Parent = parent, Icon = icon });
            }
        }

        // Método estático para mostrar el menú contextual
        public static void ShowContextMenu(Point position, EventHandler<MenuItemSelectedEventArgs> menuItemSelectedHandler)
        {
            try
            {
                Console.WriteLine($"ShowContextMenu llamado en posición: {position}");
                
                var menu = new MultiLevelDropdownMenu(position);
                Console.WriteLine("MultiLevelDropdownMenu creado");
                
                menu.MenuItemSelected += menuItemSelectedHandler;
                Console.WriteLine("Evento MenuItemSelected suscrito");
                
                menu.ExpandMenu();
                Console.WriteLine("Menú expandido");
                
                menu.Show();
                Console.WriteLine("Menú mostrado");
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Error en ShowContextMenu: {ex.Message}");
               Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        private void MultiLevelDropdownMenu_Deactivate(object sender, EventArgs e)
        {
            // Cerrar el menú cuando pierde el foco
            this.Close();
        }

        private void ToggleMenu()
        {
            if (isExpanded)
            {
                CollapseMenu();
            }
            else
            {
                ExpandMenu();
            }
        }

        private void ExpandMenu()
        {
            isExpanded = true;
            animationStep = 0;
            int totalHeight = CalculateMenuHeight();
            int finalHeight = Math.Min(maxHeight, totalHeight);
            this.Size = new Size(250, finalHeight);
            
            // Calcular cuántos elementos son visibles
            visibleItems = (finalHeight - 20) / itemHeight; // 20 es el padding
            scrollOffset = 0;
            
            animationTimer.Start();
        }

        private void CollapseMenu()
        {
            isExpanded = false;
            animationStep = 0;
            animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (isExpanded)
            {
                animationStep++;
                int totalHeight = CalculateMenuHeight();
                int targetHeight = Math.Min(maxHeight, totalHeight);
                int currentHeight = 40 + (targetHeight - 40) * animationStep / 20;
                
                this.Height = currentHeight;
                
                if (animationStep >= 20)
                {
                    animationTimer.Stop();
                    this.Height = targetHeight;
                    visibleItems = (targetHeight - 20) / itemHeight;
                }
            }
            else
            {
                animationStep++;
                int currentHeight = this.Height - (this.Height - 40) * animationStep / 20;
                
                this.Height = currentHeight;
                
                if (animationStep >= 20)
                {
                    animationTimer.Stop();
                    this.Height = 40;
                    this.Close();
                }
            }
        }

        private int CalculateMenuHeight()
        {
            int height = 20; // Padding superior e inferior
            foreach (var item in menuItems)
            {
                height += 35; // Altura de cada elemento del menú
            }
            return height;
        }

        private void MultiLevelDropdownMenu_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Crear rectángulo redondeado
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            
            // Dibujar fondo con bordes redondeados
            using (SolidBrush brush = new SolidBrush(backgroundColor))
            {
                g.FillRoundedRectangle(brush, rect, borderRadius);
            }

            // Dibujar borde redondeado
            using (Pen pen = new Pen(borderColor, 2))
            {
                g.DrawRoundedRectangle(pen, rect, borderRadius);
            }

            // Si el menú está expandido, dibujar los elementos
            if (isExpanded)
            {
                DrawMenuItems(g);
                
                // Dibujar scrollbar si es necesario
                if (menuItems.Count > visibleItems)
                {
                    DrawScrollBar(g);
                }
            }
        }

        private void DrawMenuItems(Graphics g)
        {
            int y = 10;
            int itemsToShow = Math.Min(visibleItems, menuItems.Count - scrollOffset);
            
            using (Font font = new Font("Segoe UI", 9F, FontStyle.Regular))
            using (Font iconFont = new Font("Segoe UI Emoji", 12F, FontStyle.Regular))
            using (SolidBrush textBrush = new SolidBrush(textColor))
            using (SolidBrush hoverBrush = new SolidBrush(hoverColor))
            {
                for (int i = scrollOffset; i < scrollOffset + itemsToShow; i++)
                {
                    if (i >= menuItems.Count) break;
                    
                    var item = menuItems[i];
                    Rectangle itemRect = new Rectangle(5, y, this.Width - 15, itemHeight - 5);
                    
                    // Verificar si el mouse está sobre este elemento
                    Point mousePos = this.PointToClient(Cursor.Position);
                    if (itemRect.Contains(mousePos))
                    {
                        // Dibujar fondo redondeado para hover
                        g.FillRoundedRectangle(hoverBrush, itemRect, 8);
                        
                        // Manejar clic
                        if (Control.MouseButtons == MouseButtons.Left && !isScrolling)
                        {
                            OnMenuItemSelected(item);
                        }
                    }

                    // Dibujar icono si existe
                    if (!string.IsNullOrEmpty(item.Icon))
                    {
                        g.DrawString(item.Icon, iconFont, textBrush, new Point(15, y + 5));
                    }

                    // Dibujar texto con indentación según el nivel
                    string displayText = new string(' ', item.Level * 2) + item.Text;
                    int textX = !string.IsNullOrEmpty(item.Icon) ? 45 : 15;
                    g.DrawString(displayText, font, textBrush, new Point(textX, y + 8));
                    
                    y += itemHeight;
                }
            }
        }

        protected virtual void OnMenuItemSelected(MenuItem item)
        {
            MenuItemSelected?.Invoke(this, new MenuItemSelectedEventArgs(item));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isExpanded)
            {
                // Solo invalidar si la posición del mouse cambió significativamente
                if (Math.Abs(e.X - lastMousePosition.X) > 2 || Math.Abs(e.Y - lastMousePosition.Y) > 2)
                {
                    lastMousePosition = e.Location;
                    this.Invalidate();
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (isExpanded)
            {
                this.Invalidate();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (isExpanded && menuItems.Count > visibleItems)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        if (scrollOffset > 0)
                        {
                            scrollOffset--;
                            this.Invalidate();
                        }
                        e.Handled = true;
                        break;
                    case Keys.Down:
                        if (scrollOffset < menuItems.Count - visibleItems)
                        {
                            scrollOffset++;
                            this.Invalidate();
                        }
                        e.Handled = true;
                        break;
                    case Keys.PageUp:
                        scrollOffset = Math.Max(0, scrollOffset - visibleItems);
                        this.Invalidate();
                        e.Handled = true;
                        break;
                    case Keys.PageDown:
                        scrollOffset = Math.Min(menuItems.Count - visibleItems, scrollOffset + visibleItems);
                        this.Invalidate();
                        e.Handled = true;
                        break;
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (isExpanded && !isScrolling)
            {
                // Calcular qué elemento fue clickeado
                int itemIndex = (e.Y - 10) / itemHeight + scrollOffset;
                if (itemIndex >= scrollOffset && itemIndex < scrollOffset + visibleItems && itemIndex < menuItems.Count)
                {
                    OnMenuItemSelected(menuItems[itemIndex]);
                }
            }
        }

        private void MultiLevelDropdownMenu_MouseWheel(object sender, MouseEventArgs e)
        {
            if (isExpanded && menuItems.Count > visibleItems)
            {
                int delta = e.Delta > 0 ? -1 : 1;
                int newScrollOffset = scrollOffset + delta;
                
                // Limitar el scroll
                if (newScrollOffset >= 0 && newScrollOffset <= menuItems.Count - visibleItems)
                {
                    scrollOffset = newScrollOffset;
                    this.Invalidate();
                }
            }
        }

        private void DrawScrollBar(Graphics g)
        {
            if (menuItems.Count <= visibleItems) return;

            int scrollBarWidth = 8;
            int scrollBarX = this.Width - scrollBarWidth - 5;
            int scrollBarY = 10;
            int scrollBarHeight = this.Height - 20;

            // Dibujar fondo de la barra de scroll
            using (SolidBrush scrollBackground = new SolidBrush(Color.FromArgb(40, 40, 40)))
            {
                g.FillRectangle(scrollBackground, scrollBarX, scrollBarY, scrollBarWidth, scrollBarHeight);
            }

            // Calcular el tamaño del thumb
            float thumbHeight = (float)visibleItems / menuItems.Count * scrollBarHeight;
            float thumbY = scrollBarY + (float)scrollOffset / menuItems.Count * scrollBarHeight;

            // Dibujar el thumb
            using (SolidBrush thumbBrush = new SolidBrush(Color.FromArgb(100, 100, 100)))
            {
                g.FillRectangle(thumbBrush, scrollBarX, thumbY, scrollBarWidth, thumbHeight);
            }
        }
    }

    // Métodos de extensión para dibujar rectángulos redondeados
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (var path = GetRoundedRectanglePath(rect, radius))
            {
                g.FillPath(brush, path);
            }
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle rect, int radius)
        {
            using (var path = GetRoundedRectanglePath(rect, radius))
            {
                g.DrawPath(pen, path);
            }
        }

        private static System.Drawing.Drawing2D.GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            
            int diameter = radius * 2;
            
            // Esquinas redondeadas
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90); // Superior izquierda
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90); // Superior derecha
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90); // Inferior derecha
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90); // Inferior izquierda
            
            path.CloseFigure();
            return path;
        }
    }

    public class MenuItem
    {
        public string Text { get; set; }
        public string Action { get; set; }
        public int Level { get; set; }
        public MenuItem Parent { get; set; }
        public string Icon { get; set; }
    }

    public class MenuItemSelectedEventArgs : EventArgs
    {
        public MenuItem MenuItem { get; }

        public MenuItemSelectedEventArgs(MenuItem menuItem)
        {
            MenuItem = menuItem;
        }
    }
}
