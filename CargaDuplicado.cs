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

namespace ProexsiCam
{
    public partial class CargaDuplicado : Form
    {
        public CargaDuplicado()
        {
            InitializeComponent();
        }

        private void txtRUTDuplicado_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Aceptar();
            }
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            Aceptar();
        }

        public void Aceptar()
        {
            // Supongamos que el usuario ingresa el número en un TextBox
            string input = txtRUTDuplicado.Text.Trim();

            // Verificamos si es un número válido
            if (int.TryParse(input, out int numero))
            {
                // Rellenamos con ceros a la izquierda hasta que tenga un largo de 8
                txtRUTDuplicado.Text = input.PadLeft(8, '0');
            }
            else
            {
                MessageBox.Show("Por favor, ingrese un número válido.");
                return;
            }

            string origen = @"M:\LICENCIA DE CONDUCIR\Fotos\fotosres\" + txtRUTDuplicado.Text+".jpg";
            string destino = @"C:\Proexsi\FOTOS\"+txtRUTDuplicado.Text+".jpg";

            try
            {
                // Copiar el archivo
                File.Copy(origen, destino, true); // 'true' sobrescribe si ya existe
                MessageBox.Show("Foto rescatada!\n Vuelve al sistema de Licencias y en\n" +
                    "la ventana de fotos, da click en 'Actualizar'.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al rescatar foto de duplicado:\n {ex.Message}");
            }
        }

        private void txtRUTDuplicado_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
