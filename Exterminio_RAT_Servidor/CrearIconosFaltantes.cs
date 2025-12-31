using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace Exterminio_RAT_Servidor
{
    /// <summary>
    /// Clase temporal para crear iconos PNG faltantes
    /// </summary>
    public static class CrearIconosFaltantes
    {
        public static void CrearIconosNecesarios()
        {
            try
            {
                string rutaIconos = Path.Combine(Application.StartupPath, "Iconos");
                
                if (!Directory.Exists(rutaIconos))
                {
                    Console.WriteLine("Carpeta de iconos no encontrada");
                    return;
                }

                // Crear icono de carpeta
                CrearIconoCarpeta(rutaIconos);
                
                // Crear iconos genéricos para extensiones faltantes
                CrearIconosGenericos(rutaIconos);
                
                Console.WriteLine("✅ Iconos faltantes creados exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creando iconos: {ex.Message}");
            }
        }

        private static void CrearIconoCarpeta(string rutaIconos)
        {
            string rutaFolderPNG = Path.Combine(rutaIconos, "folder.png");
            
            if (!File.Exists(rutaFolderPNG))
            {
                try
                {
                    // Crear un icono de carpeta en alta calidad (32x32)
                    using (Bitmap bmp = new Bitmap(32, 32))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            // Configurar alta calidad
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            
                            // Fondo transparente
                            g.Clear(Color.Transparent);
                            
                            // Dibujar carpeta amarilla con mejor detalle
                            using (Brush brush = new SolidBrush(Color.Yellow))
                            {
                                g.FillRectangle(brush, 4, 8, 24, 20);
                            }
                            
                            // Contorno negro más grueso para mejor visibilidad
                            using (Pen pen = new Pen(Color.Black, 2))
                            {
                                g.DrawRectangle(pen, 4, 8, 24, 20);
                                g.DrawRectangle(pen, 2, 12, 8, 4);
                            }
                            
                            // Agregar sombra sutil para mejor apariencia
                            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                            {
                                g.FillRectangle(shadowBrush, 6, 10, 24, 20);
                            }
                        }
                        
                        bmp.Save(rutaFolderPNG, System.Drawing.Imaging.ImageFormat.Png);
                        Console.WriteLine("✅ Icono de carpeta creado en alta calidad: folder.png");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creando icono de carpeta: {ex.Message}");
                }
            }
        }

        private static void CrearIconosGenericos(string rutaIconos)
        {
            // Lista de extensiones que necesitan iconos genéricos
            string[] extensionesFaltantes = {
                "exe", "mp3", "mp4", "doc", "pdf", "odt", "com", "ps1"
            };

            foreach (string ext in extensionesFaltantes)
            {
                string rutaIcono = Path.Combine(rutaIconos, $"{ext}.png");
                
                if (!File.Exists(rutaIcono))
                {
                    try
                    {
                        // Crear icono genérico en alta calidad (32x32)
                        using (Bitmap bmp = new Bitmap(32, 32))
                        {
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                // Configurar alta calidad
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                
                                g.Clear(Color.Transparent);
                                
                                Color colorFondo = ObtenerColorPorExtension(ext);
                                
                                // Fondo del icono con bordes redondeados
                                using (Brush brush = new SolidBrush(colorFondo))
                                {
                                    g.FillRoundedRectangle(brush, 2, 2, 28, 28, 4);
                                }
                                
                                // Contorno más grueso
                                using (Pen pen = new Pen(Color.Black, 2))
                                {
                                    g.DrawRoundedRectangle(pen, 2, 2, 28, 28, 4);
                                }
                                
                                // Texto de la extensión con mejor fuente
                                using (Font font = new Font("Arial", 10, FontStyle.Bold))
                                using (Brush textBrush = new SolidBrush(Color.White))
                                {
                                    string texto = ext.ToUpper();
                                    SizeF textSize = g.MeasureString(texto, font);
                                    float x = (32 - textSize.Width) / 2;
                                    float y = (32 - textSize.Height) / 2;
                                    g.DrawString(texto, font, textBrush, x, y);
                                }
                                
                                // Agregar brillo sutil
                                using (Brush shineBrush = new LinearGradientBrush(
                                    new Point(2, 2), new Point(30, 30),
                                    Color.FromArgb(50, Color.White), Color.FromArgb(0, Color.White)))
                                {
                                    g.FillRoundedRectangle(shineBrush, 2, 2, 28, 28, 4);
                                }
                            }
                            
                            bmp.Save(rutaIcono, System.Drawing.Imaging.ImageFormat.Png);
                            Console.WriteLine($"✅ Icono genérico creado en alta calidad: {ext}.png");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creando icono {ext}.png: {ex.Message}");
                    }
                }
            }
        }

        private static Color ObtenerColorPorExtension(string extension)
        {
            switch (extension.ToLower())
            {
                case "exe":
                    return Color.Green;
                case "mp3":
                case "mp4":
                    return Color.Purple;
                case "doc":
                case "pdf":
                case "odt":
                    return Color.Blue;
                case "com":
                case "ps1":
                    return Color.Orange;
                default:
                    return Color.Gray;
            }
        }

        /// <summary>
        /// Método de extensión para dibujar rectángulos redondeados
        /// </summary>
        private static void FillRoundedRectangle(this Graphics g, Brush brush, float x, float y, float width, float height, float radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(x, y, radius, radius, 180, 90);
                path.AddArc(x + width - radius, y, radius, radius, 270, 90);
                path.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90);
                path.AddArc(x, y + height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                
                g.FillPath(brush, path);
            }
        }

        /// <summary>
        /// Método de extensión para dibujar contornos de rectángulos redondeados
        /// </summary>
        private static void DrawRoundedRectangle(this Graphics g, Pen pen, float x, float y, float width, float height, float radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(x, y, radius, radius, 180, 90);
                path.AddArc(x + width - radius, y, radius, radius, 270, 90);
                path.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90);
                path.AddArc(x, y + height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                
                g.DrawPath(pen, path);
            }
        }
    }
}
