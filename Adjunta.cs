using System;
using System.IO;
using System.Windows.Forms;

namespace ProexsiCam
{
    public partial class Adjunta : Form
    {
        public Adjunta()
        {
            InitializeComponent();
        }

        private void txtRUT_KeyDown(object sender, KeyEventArgs e)
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

        string RUT = "";
        public string Aceptar()
        {
            // Supongamos que el usuario ingresa el número en un TextBox
            string input = txtRUT.Text.Trim();

            // Verificamos si es un número válido
            if (int.TryParse(input, out int numero))
            {
                // Rellenamos con ceros a la izquierda hasta que tenga un largo de 8
                txtRUT.Text = input.PadLeft(8, '0');
                RUT = txtRUT.Text;
                this.Close();
                return RUT;
            }
            else
            {
                if (!txtRUT.Text.Equals(""))
                {
                    MessageBox.Show("Por favor, ingrese un número válido.");
                } 
            }
            return "";
        }

        private void txtRUT_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
