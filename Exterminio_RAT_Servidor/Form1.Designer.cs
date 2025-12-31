namespace Exterminio_RAT_Servidor
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.guna2Separator1 = new Guna.UI2.WinForms.Guna2Separator();
            this.guna2Separator2 = new Guna.UI2.WinForms.Guna2Separator();
            this.btnVista_GaleriaClientes = new Guna.UI2.WinForms.Guna2ImageButton();
            this.guna2ImageButton1 = new Guna.UI2.WinForms.Guna2ImageButton();
            this.btnVista_ListaClientes = new Guna.UI2.WinForms.Guna2ImageButton();
            this.lblBytesEnviados = new System.Windows.Forms.Label();
            this.lblBytesRecibidos = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // guna2Separator1
            // 
            this.guna2Separator1.FillColor = System.Drawing.Color.Red;
            this.guna2Separator1.Location = new System.Drawing.Point(12, 72);
            this.guna2Separator1.Name = "guna2Separator1";
            this.guna2Separator1.Size = new System.Drawing.Size(51, 18);
            this.guna2Separator1.TabIndex = 1;
            // 
            // guna2Separator2
            // 
            this.guna2Separator2.FillColor = System.Drawing.Color.Red;
            this.guna2Separator2.Location = new System.Drawing.Point(12, 129);
            this.guna2Separator2.Name = "guna2Separator2";
            this.guna2Separator2.Size = new System.Drawing.Size(51, 18);
            this.guna2Separator2.TabIndex = 4;
            // 
            // btnVista_GaleriaClientes
            // 
            this.btnVista_GaleriaClientes.CheckedState.ImageSize = new System.Drawing.Size(64, 64);
            this.btnVista_GaleriaClientes.HoverState.ImageSize = new System.Drawing.Size(32, 32);
            this.btnVista_GaleriaClientes.Image = ((System.Drawing.Image)(resources.GetObject("btnVista_GaleriaClientes.Image")));
            this.btnVista_GaleriaClientes.ImageOffset = new System.Drawing.Point(0, 0);
            this.btnVista_GaleriaClientes.ImageRotate = 0F;
            this.btnVista_GaleriaClientes.ImageSize = new System.Drawing.Size(28, 28);
            this.btnVista_GaleriaClientes.Location = new System.Drawing.Point(12, 142);
            this.btnVista_GaleriaClientes.Name = "btnVista_GaleriaClientes";
            this.btnVista_GaleriaClientes.PressedState.ImageSize = new System.Drawing.Size(28, 28);
            this.btnVista_GaleriaClientes.Size = new System.Drawing.Size(51, 48);
            this.btnVista_GaleriaClientes.TabIndex = 3;
            this.btnVista_GaleriaClientes.Click += new System.EventHandler(this.btnVista_GaleriaClientes_Click);
            // 
            // guna2ImageButton1
            // 
            this.guna2ImageButton1.CheckedState.ImageSize = new System.Drawing.Size(64, 64);
            this.guna2ImageButton1.HoverState.ImageSize = new System.Drawing.Size(32, 32);
            this.guna2ImageButton1.Image = ((System.Drawing.Image)(resources.GetObject("guna2ImageButton1.Image")));
            this.guna2ImageButton1.ImageOffset = new System.Drawing.Point(0, 0);
            this.guna2ImageButton1.ImageRotate = 0F;
            this.guna2ImageButton1.ImageSize = new System.Drawing.Size(28, 28);
            this.guna2ImageButton1.Location = new System.Drawing.Point(12, 31);
            this.guna2ImageButton1.Name = "guna2ImageButton1";
            this.guna2ImageButton1.PressedState.ImageSize = new System.Drawing.Size(28, 28);
            this.guna2ImageButton1.Size = new System.Drawing.Size(51, 48);
            this.guna2ImageButton1.TabIndex = 2;
            // 
            // btnVista_ListaClientes
            // 
            this.btnVista_ListaClientes.CheckedState.ImageSize = new System.Drawing.Size(64, 64);
            this.btnVista_ListaClientes.HoverState.ImageSize = new System.Drawing.Size(32, 32);
            this.btnVista_ListaClientes.Image = ((System.Drawing.Image)(resources.GetObject("btnVista_ListaClientes.Image")));
            this.btnVista_ListaClientes.ImageOffset = new System.Drawing.Point(0, 0);
            this.btnVista_ListaClientes.ImageRotate = 0F;
            this.btnVista_ListaClientes.ImageSize = new System.Drawing.Size(28, 28);
            this.btnVista_ListaClientes.Location = new System.Drawing.Point(12, 85);
            this.btnVista_ListaClientes.Name = "btnVista_ListaClientes";
            this.btnVista_ListaClientes.PressedState.ImageSize = new System.Drawing.Size(28, 28);
            this.btnVista_ListaClientes.Size = new System.Drawing.Size(51, 48);
            this.btnVista_ListaClientes.TabIndex = 0;
            this.btnVista_ListaClientes.Click += new System.EventHandler(this.guna2ImageButton1_Click);
            // 
            // lblBytesEnviados
            // 
            this.lblBytesEnviados.AutoSize = true;
            this.lblBytesEnviados.BackColor = System.Drawing.Color.Transparent;
            this.lblBytesEnviados.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBytesEnviados.ForeColor = System.Drawing.Color.Red;
            this.lblBytesEnviados.Location = new System.Drawing.Point(80, 40);
            this.lblBytesEnviados.Name = "lblBytesEnviados";
            this.lblBytesEnviados.Size = new System.Drawing.Size(129, 15);
            this.lblBytesEnviados.TabIndex = 5;
            this.lblBytesEnviados.Text = "📤 Bytes Enviados: 0 B";
            // 
            // lblBytesRecibidos
            // 
            this.lblBytesRecibidos.AutoSize = true;
            this.lblBytesRecibidos.BackColor = System.Drawing.Color.Transparent;
            this.lblBytesRecibidos.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBytesRecibidos.ForeColor = System.Drawing.Color.Red;
            this.lblBytesRecibidos.Location = new System.Drawing.Point(80, 60);
            this.lblBytesRecibidos.Name = "lblBytesRecibidos";
            this.lblBytesRecibidos.Size = new System.Drawing.Size(134, 15);
            this.lblBytesRecibidos.TabIndex = 6;
            this.lblBytesRecibidos.Text = "📥 Bytes Recibidos: 0 B";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1085, 450);
            this.Controls.Add(this.lblBytesRecibidos);
            this.Controls.Add(this.lblBytesEnviados);
            this.Controls.Add(this.guna2Separator2);
            this.Controls.Add(this.btnVista_GaleriaClientes);
            this.Controls.Add(this.guna2ImageButton1);
            this.Controls.Add(this.guna2Separator1);
            this.Controls.Add(this.btnVista_ListaClientes);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Guna.UI2.WinForms.Guna2AnimateWindow Pic;
        private Guna.UI2.WinForms.Guna2ImageButton btnVista_ListaClientes;
        private Guna.UI2.WinForms.Guna2Separator guna2Separator1;
        private Guna.UI2.WinForms.Guna2ImageButton guna2ImageButton1;
        private Guna.UI2.WinForms.Guna2ImageButton btnVista_GaleriaClientes;
        private Guna.UI2.WinForms.Guna2Separator guna2Separator2;
        private System.Windows.Forms.Label lblBytesEnviados;
        private System.Windows.Forms.Label lblBytesRecibidos;
    }
}

