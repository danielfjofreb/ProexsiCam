namespace ProexsiCam
{
    partial class CargaDuplicado
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.txtRUTDuplicado = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(213, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Ingrese RUT sin puntos ni digito verificador:";
            // 
            // btnAceptar
            // 
            this.btnAceptar.Location = new System.Drawing.Point(146, 60);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(75, 23);
            this.btnAceptar.TabIndex = 2;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = true;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            // 
            // txtRUTDuplicado
            // 
            this.txtRUTDuplicado.Location = new System.Drawing.Point(12, 34);
            this.txtRUTDuplicado.Name = "txtRUTDuplicado";
            this.txtRUTDuplicado.Size = new System.Drawing.Size(157, 20);
            this.txtRUTDuplicado.TabIndex = 0;
            this.txtRUTDuplicado.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtRUTDuplicado.TextChanged += new System.EventHandler(this.txtRUTDuplicado_TextChanged);
            this.txtRUTDuplicado.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtRUTDuplicado_KeyDown);
            // 
            // CargaDuplicado
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(233, 85);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtRUTDuplicado);
            this.Name = "CargaDuplicado";
            this.Text = "Carga Duplicado";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAceptar;
        private System.Windows.Forms.TextBox txtRUTDuplicado;
    }
}