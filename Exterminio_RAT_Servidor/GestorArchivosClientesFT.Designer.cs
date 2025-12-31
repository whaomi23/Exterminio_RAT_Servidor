namespace Exterminio_RAT_Servidor
{
    partial class GestorArchivosClientesFT
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PanelDiscosCliente = new System.Windows.Forms.FlowLayoutPanel();
            this.panelCompresion = new System.Windows.Forms.Panel();
            this.labelTituloCompresion = new System.Windows.Forms.Label();
            this.labelEstadoCompresion = new System.Windows.Forms.Label();
            this.progressBarEstadoDeCompresionArchivoOCarpera = new System.Windows.Forms.ProgressBar();
            this.listViewArchivos = new System.Windows.Forms.ListView();
            this.panelCompresion.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelDiscosCliente
            // 
            this.PanelDiscosCliente.AutoScroll = true;
            this.PanelDiscosCliente.Location = new System.Drawing.Point(0, 0);
            this.PanelDiscosCliente.Name = "PanelDiscosCliente";
            this.PanelDiscosCliente.Padding = new System.Windows.Forms.Padding(10);
            this.PanelDiscosCliente.Size = new System.Drawing.Size(318, 488);
            this.PanelDiscosCliente.TabIndex = 0;
            // 
            // panelCompresion
            // 
            this.panelCompresion.BackColor = System.Drawing.Color.Black;
            this.panelCompresion.Controls.Add(this.labelTituloCompresion);
            this.panelCompresion.Controls.Add(this.labelEstadoCompresion);
            this.panelCompresion.Controls.Add(this.progressBarEstadoDeCompresionArchivoOCarpera);
            this.panelCompresion.Location = new System.Drawing.Point(0, 299);
            this.panelCompresion.Name = "panelCompresion";
            this.panelCompresion.Size = new System.Drawing.Size(318, 301);
            this.panelCompresion.TabIndex = 2;
            // 
            // labelTituloCompresion
            // 
            this.labelTituloCompresion.AutoSize = true;
            this.labelTituloCompresion.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTituloCompresion.ForeColor = System.Drawing.Color.White;
            this.labelTituloCompresion.Location = new System.Drawing.Point(10, 10);
            this.labelTituloCompresion.Name = "labelTituloCompresion";
            this.labelTituloCompresion.Size = new System.Drawing.Size(193, 19);
            this.labelTituloCompresion.TabIndex = 2;
            this.labelTituloCompresion.Text = "🗜️ PANEL DE COMPRESIÓN";
            // 
            // labelEstadoCompresion
            // 
            this.labelEstadoCompresion.AutoSize = true;
            this.labelEstadoCompresion.ForeColor = System.Drawing.Color.Orange;
            this.labelEstadoCompresion.Location = new System.Drawing.Point(10, 30);
            this.labelEstadoCompresion.Name = "labelEstadoCompresion";
            this.labelEstadoCompresion.Size = new System.Drawing.Size(112, 13);
            this.labelEstadoCompresion.TabIndex = 1;
            this.labelEstadoCompresion.Text = "Estado de compresión";
            // 
            // progressBarEstadoDeCompresionArchivoOCarpera
            // 
            this.progressBarEstadoDeCompresionArchivoOCarpera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.progressBarEstadoDeCompresionArchivoOCarpera.ForeColor = System.Drawing.Color.Lime;
            this.progressBarEstadoDeCompresionArchivoOCarpera.Location = new System.Drawing.Point(10, 66);
            this.progressBarEstadoDeCompresionArchivoOCarpera.Name = "progressBarEstadoDeCompresionArchivoOCarpera";
            this.progressBarEstadoDeCompresionArchivoOCarpera.Size = new System.Drawing.Size(296, 23);
            this.progressBarEstadoDeCompresionArchivoOCarpera.TabIndex = 0;
            // 
            // listViewArchivos
            // 
            this.listViewArchivos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.listViewArchivos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewArchivos.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewArchivos.ForeColor = System.Drawing.Color.Red;
            this.listViewArchivos.FullRowSelect = true;
            this.listViewArchivos.GridLines = true;
            this.listViewArchivos.HideSelection = false;
            this.listViewArchivos.Location = new System.Drawing.Point(318, 0);
            this.listViewArchivos.Name = "listViewArchivos";
            this.listViewArchivos.Size = new System.Drawing.Size(531, 600);
            this.listViewArchivos.TabIndex = 1;
            this.listViewArchivos.UseCompatibleStateImageBehavior = false;
            this.listViewArchivos.View = System.Windows.Forms.View.Details;
            // 
            // GestorArchivosClientesFT
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.ClientSize = new System.Drawing.Size(849, 600);
            this.Controls.Add(this.listViewArchivos);
            this.Controls.Add(this.panelCompresion);
            this.Controls.Add(this.PanelDiscosCliente);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "GestorArchivosClientesFT";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gestor de Archivos - Cliente";
            this.panelCompresion.ResumeLayout(false);
            this.panelCompresion.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel PanelDiscosCliente;
        private System.Windows.Forms.Panel panelCompresion;
        private System.Windows.Forms.ListView listViewArchivos;
        private System.Windows.Forms.ProgressBar progressBarEstadoDeCompresionArchivoOCarpera;
        private System.Windows.Forms.Label labelEstadoCompresion;
        private System.Windows.Forms.Label labelTituloCompresion;
    }
}
