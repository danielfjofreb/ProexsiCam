using System;
using System.Windows.Forms;

namespace ProexsiCam
{
    public partial class Adjunta : Form
    {
        public Adjunta()
        {
            InitializeComponent();
        }

        // Cuando el usuario presiona la tecla Enter en el TextBox hace lo mismo que al hacer clic en el botón Aceptar
        private void txtRUT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Aceptar();
            }
        }

        // Cuando el usuario hace clic en el botón Aceptar
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
                // Rellenamos con ceros a la izquierda hasta que tenga un largo de 8, sin considerar el digito verificador
                txtRUT.Text = input.PadLeft(8, '0');
                RUT = txtRUT.Text;
                this.Close();
                return RUT;
            }
            else
            {
                // Si no es un número válido, mostramos un mensaje de error
                MessageBox.Show("Por favor, ingrese un número válido.");
            }
            return "";
        }

        // Esto al final no hace nada, pero lo dejamos por si acaso
        private void txtRUT_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
