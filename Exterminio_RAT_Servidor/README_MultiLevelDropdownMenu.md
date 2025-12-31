# MultiLevelDropdownMenu - Menú Contextual Personalizado

## 📋 Descripción
Menú contextual personalizado que se muestra como un formulario flotante cuando se hace clic izquierdo en una AnimatedCard. Permite crear menús jerárquicos con animaciones suaves y se posiciona exactamente donde se hizo clic.

## 🎨 Características

### ✨ **Funcionalidades Principales**
- **Menú contextual**: Se muestra como formulario flotante
- **Posicionamiento preciso**: Aparece exactamente donde se hace clic
- **Animación suave**: Expansión/contracción animada
- **Multinivel**: Soporte para elementos padre e hijo
- **Hover effects**: Resaltado al pasar el mouse
- **Eventos personalizables**: Evento `MenuItemSelected` para manejar selecciones
- **Diseño moderno**: Bordes redondeados, colores oscuros y texto rojo
- **Iconos**: Emojis apropiados para cada elemento del menú
- **Auto-cierre**: Se cierra automáticamente al perder el foco
- **Feedback visual**: Cursor de mano, tooltip y efectos hover en las tarjetas
- **Audio feedback**: Sonido al hacer clic en las tarjetas

### 🎯 **Elementos del Menú**
- **🎯 Acciones del Cliente**
  - 📸 Capturar Pantalla
  - 🖼️ Tomar Screenshot
  - 📹 Webcam
  - 🎤 Micrófono

- **⚙️ Sistema**
  - ℹ️ Información del Sistema
  - 📊 Procesos
  - 🔧 Servicios
  - 📝 Registro

- **🌐 Red**
  - 🔌 Puertos Abiertos
  - 🔗 Conexiones Activas
  - ⚡ Configuración de Red

- **📁 Archivos**
  - 📂 Explorador de Archivos
  - ⬇️ Descargar Archivo
  - ⬆️ Subir Archivo

## 🚀 Cómo Usar

### **1. En AnimatedCard (Automático)**
El menú se muestra automáticamente al hacer clic izquierdo en cualquier AnimatedCard:

```csharp
// En AnimatedCard.cs - Ya implementado
private void AnimatedCard_MouseClick(object sender, MouseEventArgs e)
{
    if (e.Button == MouseButtons.Left)
    {
        Point screenPosition = this.PointToScreen(e.Location);
        MultiLevelDropdownMenu.ShowContextMenu(screenPosition, OnMenuItemSelected);
    }
}
```

### **2. Manejar Eventos (En AnimatedCard)**
```csharp
private void OnMenuItemSelected(object sender, MenuItemSelectedEventArgs e)
{
    string clientId = this.ClientId; // ID del cliente específico
    string action = e.MenuItem.Action;
    string text = e.MenuItem.Text;
    
    System.Diagnostics.Debug.WriteLine($"Cliente {clientId}: {text} - {action}");
    
    switch (action)
    {
        case "screen_capture":
            // Enviar comando al cliente específico
            break;
        case "webcam":
            // Enviar comando al cliente específico
            break;
        // ... más casos
    }
}
```

### **3. Uso Manual (Desde Cualquier Lugar)**
```csharp
// Mostrar el menú desde cualquier punto de la pantalla
Point position = new Point(100, 100);
MultiLevelDropdownMenu.ShowContextMenu(position, OnMenuItemSelected);

// O desde coordenadas de pantalla
Point screenPosition = this.PointToScreen(new Point(50, 50));
MultiLevelDropdownMenu.ShowContextMenu(screenPosition, OnMenuItemSelected);
```

## 🎨 Personalización

### **Colores y Estilo**
```csharp
// En el constructor del control
private Color backgroundColor = Color.FromArgb(25, 25, 25);  // Fondo muy oscuro
private Color hoverColor = Color.FromArgb(40, 40, 40);       // Hover gris medio
private Color textColor = Color.Red;                         // Texto rojo
private Color borderColor = Color.FromArgb(60, 60, 60);      // Borde gris
private int borderRadius = 20;                               // Radio de borde
```

### **Tamaños**
```csharp
// Altura máxima del menú expandido
private int maxHeight = 300;

// Tamaño del control
this.Size = new Size(200, 40);
```

## 📁 Estructura de Archivos

```
Exterminio_RAT_Servidor/
├── MultiLevelDropdownMenu.cs          # Control principal
├── Form1.cs                           # Ejemplo de uso
└── README_MultiLevelDropdownMenu.md   # Esta documentación
```

## 🔧 Clases y Eventos

### **MenuItem**
```csharp
public class MenuItem
{
    public string Text { get; set; }      // Texto mostrado
    public string Action { get; set; }    // Identificador de acción
    public int Level { get; set; }        // Nivel de indentación
    public MenuItem Parent { get; set; }  // Elemento padre
    public string Icon { get; set; }      // Icono emoji
}
```

### **MenuItemSelectedEventArgs**
```csharp
public class MenuItemSelectedEventArgs : EventArgs
{
    public MenuItem MenuItem { get; }     // Elemento seleccionado
}
```

### **Evento MenuItemSelected**
```csharp
public event EventHandler<MenuItemSelectedEventArgs> MenuItemSelected;
```

## 🎯 Ejemplo Completo

```csharp
// En AnimatedCard.cs - Ya implementado automáticamente
public partial class AnimatedCard : UserControl
{
    public AnimatedCard()
    {
        InitializeComponent();
        
        // Configurar cursor y tooltip
        this.Cursor = Cursors.Hand;
        ToolTip tooltip = new ToolTip();
        tooltip.SetToolTip(this, "Clic izquierdo para mostrar menú de acciones");
        
        // Agregar eventos de mouse
        this.MouseClick += AnimatedCard_MouseClick;
        this.MouseEnter += AnimatedCard_MouseEnter;
        this.MouseLeave += AnimatedCard_MouseLeave;
    }

    private void AnimatedCard_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            // Reproducir sonido y mostrar menú
            ReproducirSonidoClic();
            Point screenPosition = this.PointToScreen(e.Location);
            MultiLevelDropdownMenu.ShowContextMenu(screenPosition, OnMenuItemSelected);
        }
    }

    private void AnimatedCard_MouseEnter(object sender, EventArgs e)
    {
        // Efectos hover: cursor, color y borde
        this.Cursor = Cursors.Hand;
        this.BackColor = Color.FromArgb(35, 35, 35);
        this.BorderStyle = BorderStyle.FixedSingle;
    }

    private void AnimatedCard_MouseLeave(object sender, EventArgs e)
    {
        // Restaurar estado normal
        this.Cursor = Cursors.Default;
        this.BackColor = Color.FromArgb(25, 25, 25);
        this.BorderStyle = BorderStyle.None;
    }

    private void OnMenuItemSelected(object sender, MenuItemSelectedEventArgs e)
    {
        string clientId = this.ClientId;
        string action = e.MenuItem.Action;
        
        // Manejar la acción específica para este cliente
        System.Diagnostics.Debug.WriteLine($"Cliente {clientId}: {action}");
    }
}
```

## 🎵 Integración con Audio

El control se integra perfectamente con el sistema de audio del servidor:

```csharp
private void OnMenuItemSelected(object sender, MenuItemSelectedEventArgs e)
{
    // Reproducir sonido al seleccionar
    ReproducirSonidoConexion();
    
    // Manejar la acción
    switch (e.MenuItem.Action)
    {
        case "screen_capture":
            // Lógica de captura de pantalla
            break;
    }
}
```

## 🎨 Diseño Visual

### **Menú Contextual**
- **Fondo**: Gris muy oscuro (#191919)
- **Hover**: Gris medio (#282828)
- **Texto**: Rojo (#FF0000)
- **Borde**: Gris (#3C3C3C)
- **Bordes redondeados**: Radio de 20px
- **Iconos**: Emojis coloridos
- **Animación**: Suave con timer de 10ms

### **AnimatedCard (Tarjetas de Cliente)**
- **Cursor**: Mano (Hand) al pasar el mouse
- **Tooltip**: "Clic izquierdo para mostrar menú de acciones"
- **Hover**: Cambio de color de fondo y borde
- **Sonido**: Audio feedback al hacer clic
- **Efectos visuales**: Transiciones suaves

## 🔄 Animación

El menú usa un sistema de animación basado en timer:
- **Duración**: 200ms (20 pasos × 10ms)
- **Tipo**: Interpolación lineal
- **Propiedad**: Height del control

## 📝 Notas

- El control es completamente personalizable
- Soporta múltiples niveles de menú
- Integración nativa con Windows Forms
- Compatible con el sistema de eventos del servidor
- Diseño responsive y moderno

## 🎯 Próximas Mejoras

- [ ] Soporte para iconos
- [ ] Temas personalizables
- [ ] Animaciones más complejas
- [ ] Soporte para atajos de teclado
- [ ] Menús contextuales
