using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Exterminio_RAT_Servidor
{
    public partial class ListaClientesConectados : UserControl
    {
        private Dictionary<string, AnimatedCard> clientCards;

        public ListaClientesConectados()
        {
            InitializeComponent();
            clientCards = new Dictionary<string, AnimatedCard>();
            SetupFlowLayoutPanel();
        }

        private void SetupFlowLayoutPanel()
        {
            // Configurar el FlowLayoutPanel
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.BackColor = Color.Black;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.Padding = new Padding(10);
        }

        // Hacer público el flowLayoutPanel1 para acceso externo
        public FlowLayoutPanel FlowLayoutPanel => flowLayoutPanel1;

        public void AgregarCliente(string id, string user, string hostname, string ip, string pais, string arch, string systemOS, string typeMachine, string av = "N/A")
        {
            if (flowLayoutPanel1.InvokeRequired)
            {
                flowLayoutPanel1.Invoke(new Action(() => AgregarCliente(id, user, hostname, ip, pais, arch, systemOS, typeMachine)));
                return;
            }

            try
            {
                Console.WriteLine($"Agregando cliente: ID={id}, User={user}, Host={hostname}, IP={ip}, AV={av}");
                
                // Crear una nueva tarjeta animada
                AnimatedCard card = new AnimatedCard();
                
                // Configurar toda la información del cliente usando el método SetClientInfo
                card.SetClientInfo(id, user, hostname, systemOS, av, pais, ip, arch);
                
                // Configurar el tamaño y margen de la tarjeta
                card.Width = flowLayoutPanel1.Width - 40; // Considerar padding
                card.Height = 103;
                card.Margin = new Padding(0, 0, 0, 10);
                
                // Agregar la tarjeta al diccionario y al panel
                clientCards[id] = card;
                flowLayoutPanel1.Controls.Add(card);
                
                // Forzar el refresco del panel
                flowLayoutPanel1.Refresh();
                
                Console.WriteLine($"Cliente agregado exitosamente. Total tarjetas: {flowLayoutPanel1.Controls.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar cliente: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        public void ActualizarCliente(string id, string user, string hostname, string ip, string pais, string arch, string systemOS, string typeMachine, string av = "N/A")
        {
            if (flowLayoutPanel1.InvokeRequired)
            {
                flowLayoutPanel1.Invoke(new Action(() => ActualizarCliente(id, user, hostname, ip, pais, arch, systemOS, typeMachine)));
                return;
            }

            if (clientCards.ContainsKey(id))
            {
                AnimatedCard card = clientCards[id];
                
                // Actualizar toda la información del cliente usando el método SetClientInfo
                card.SetClientInfo(id, user, hostname, systemOS, av, pais, ip, arch);
            }
        }

        public void EliminarCliente(string id)
        {
            if (flowLayoutPanel1.InvokeRequired)
            {
                flowLayoutPanel1.Invoke(new Action(() => EliminarCliente(id)));
                return;
            }

            if (clientCards.ContainsKey(id))
            {
                AnimatedCard card = clientCards[id];
                flowLayoutPanel1.Controls.Remove(card);
                clientCards.Remove(id);
                card.Dispose();
            }
        }

        public void LimpiarLista()
        {
            if (flowLayoutPanel1.InvokeRequired)
            {
                flowLayoutPanel1.Invoke(new Action(LimpiarLista));
                return;
            }

            // Limpiar todas las tarjetas
            foreach (var card in clientCards.Values)
            {
                flowLayoutPanel1.Controls.Remove(card);
                card.Dispose();
            }
            clientCards.Clear();
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
            // Este método se llama cuando el panel necesita ser redibujado
            // Las tarjetas AnimatedCard se manejan automáticamente por el FlowLayoutPanel
            // No necesitamos hacer nada especial aquí ya que las tarjetas se agregan como controles
        }

        // Método adicional para obtener la tarjeta seleccionada
        public AnimatedCard GetClienteSeleccionado()
        {
            // Aquí puedes implementar la lógica para obtener la tarjeta seleccionada
            // Por ejemplo, puedes agregar eventos Click a las tarjetas
            return null;
        }

        // Método para agregar clientes de prueba
        public void AgregarClientesPrueba()
        {
            AgregarCliente("001", "Usuario1", "PC-OFICINA-1", "192.168.1.100", "España", "x64", "Windows 10", "Desktop");
            
        }
    }
}
