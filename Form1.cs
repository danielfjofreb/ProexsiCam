using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using AForge.Video;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ProexsiCam
{
    public partial class Ventana : Form
    {
        public string Ruta;
        private FilterInfoCollection MisDispositivos;
        private VideoCaptureDevice MiWebCam;
        private string txt = "";
        private Color color;
        private bool showGrid = false;  // Controla la rejilla de la foto
        private Bitmap currentFrame;    // Almacena el frame actual sin rejilla
        private int Ancho;
        private int Alto;

        public Ventana()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Carga las configuraciones desde los archivos de texto
            await CargaColor();
            await CargaDispositivosAsync();
            showGrid = await leerRejillaAsync();
            checkBoxGrid.Checked = showGrid;

            pictureBox2.Visible = false;

            // Esta no es una configuracion que se guarde, pero podria hacerse si quisiese, ya que existen los controles necesarios
            Ruta = textBox1.Text;
        }


        public async Task CargaColor()
        {
            await leerColor();
            this.BackColor = color;
            button2.BackColor = color;
        }

        /* La carga de dispositivos es asincrona, pero no la carga de la resolución porque la aplicación
           carga las resoluciones soportadas por el dispositivo de video */
        public async Task CargaDispositivosAsync()
        {
            bool anteriorConectado = false;
            string dispositivoAnterior = await leerDispositivoAsync();

            while (!HayDispositivos())
            {
                MessageBox.Show("No hay dispositivos conectados. Conecte un dispositivo y reintente.");
            }

            for (int i = 0; i < MisDispositivos.Count; i++)
            {
                comboBox1.Items.Add(MisDispositivos[i].Name);
                if (!anteriorConectado)
                {
                    anteriorConectado = MisDispositivos[i].Name.Equals(dispositivoAnterior);
                }
            }

            comboBox1.Text = anteriorConectado ? dispositivoAnterior : MisDispositivos[0].Name;
            CargaResolucion();
        }

        private bool HayDispositivos()
        {
            MisDispositivos = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            return MisDispositivos.Count > 0;
        }

        public void CerrarWebCam()
        {
            if (MiWebCam != null && MiWebCam.IsRunning)
            {
                MiWebCam.SignalToStop();
                MiWebCam = null;
            }
        }

        private void CargaResolucion()
        {
            bool resolucionValidacion = false;
            string resolucionAnterior = leerResolucion();
            if (comboBox2.Items.Count >= 0)
            {
                comboBox2.Items.Clear();
            }
            for (int j = 0; j < MiWebCam.VideoCapabilities.Length; j++)
            {
                string resolution_size = MiWebCam.VideoCapabilities[j].FrameSize
                                                                      .ToString()
                                                                      .Replace("Width", "Ancho")
                                                                      .Replace("Height", "Alto");
                comboBox2.Items.Add(resolution_size);
                if (!resolucionValidacion)
                {
                    resolucionValidacion = resolution_size.Equals(resolucionAnterior) ? true : false;
                }
                comboBox2.Text = resolucionValidacion ? resolucionAnterior
                                 : selectResolution(MiWebCam).FrameSize.ToString();
            }
        }

        private async Task llamarWebCam()
        {
            int i = comboBox1.SelectedIndex;
            string NombreVideo = MisDispositivos[i].MonikerString;
            MiWebCam = new VideoCaptureDevice(NombreVideo);
            await escribirTxt(comboBox1.Text);
        }


        private static VideoCapabilities selectResolution(VideoCaptureDevice device)
        {
            var dispositivo = device.VideoCapabilities.Last();
            return dispositivo;
        }

        private void Capturando(object sender, NewFrameEventArgs eventArgs)
        {
            // Guarda el frame actual sin la rejilla
            currentFrame?.Dispose();
            currentFrame = (Bitmap)eventArgs.Frame.Clone();

            // Crea una copia para dibujar la rejilla
            Bitmap frameWithGrid = (Bitmap)currentFrame.Clone();

            using (Graphics g = Graphics.FromImage(frameWithGrid))
            {
                if (showGrid)
                {
                    DrawGrid(g, frameWithGrid.Width, frameWithGrid.Height);
                }
            }

            pictureBox1.BackgroundImage = frameWithGrid;
            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CerrarWebCam();
        }

        int boton = 0;
        private void button2_Click(object sender, EventArgs e)
        {
            TomaFoto();
        }

        private void TomaFoto()
        {
            if (currentFrame != null)
            {
                pictureBox2.BackgroundImage = ForceAspectRatio((Bitmap)currentFrame.Clone(), 4, 3);
                pictureBox1.Visible = true;
                pictureBox2.BackgroundImage.Save(Path.Combine(Ruta, $"{boton}.jpg"), ImageFormat.Jpeg);
                Thread.Sleep(1000);
                boton++;
                pictureBox2.Visible = false;
            }
        }


        private int CambiaTamañoPreview()
        {
            double calculo = (double)Ancho / Alto;
            pictureBox1.Width =
                calculo >= 1.77 ? 957 : 750;
            pictureBox1.Location =
                calculo >= 1.77 ? new Point(60, 53) : new Point(200, 53);
            return 0;
        }

        private Bitmap ForceAspectRatio(Bitmap original, int targetWidth, int targetHeight)
        {
            float desiredRatio = (float)targetWidth / targetHeight;
            int originalWidth = original.Width;
            int originalHeight = original.Height;
            float originalRatio = (float)originalWidth / originalHeight;

            Rectangle cropRect;
            if (originalRatio > desiredRatio) // Imagen es más ancha que el 4:3
            {
                int newWidth = (int)(originalHeight * desiredRatio);
                int xOffset = (originalWidth - newWidth) / 2;
                cropRect = new Rectangle(xOffset, 0, newWidth, originalHeight);
            }
            else // Imagen es más alta que el 4:3
            {
                int newHeight = (int)(originalWidth / desiredRatio);
                int yOffset = (originalHeight - newHeight) / 2;
                cropRect = new Rectangle(0, yOffset, originalWidth, newHeight);
            }

            Bitmap croppedBitmap = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(croppedBitmap))
            {
                g.DrawImage(original, new Rectangle(0, 0, cropRect.Width, cropRect.Height),
                    cropRect, GraphicsUnit.Pixel);
            }

            return croppedBitmap;
        }


        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            MiWebCam.Stop();

            CerrarWebCam();
            llamarWebCam();

            int i = comboBox2.SelectedIndex;

            MiWebCam.VideoResolution = MiWebCam.VideoCapabilities[i];

            //Eliminar despues
            string input = MiWebCam.VideoResolution.FrameSize.ToString();

            // Expresión regular para encontrar solo los dígitos
            string pattern = @"(\d+)";

            // Buscar todas las coincidencias en la cadena
            MatchCollection matches = Regex.Matches(input, pattern);

            // Verificar que hemos encontrado al menos dos números
            if (matches.Count >= 2)
            {
                // Convertir las coincidencias en números y asignarlos a variables
                Ancho = int.Parse(matches[0].Value);
                Alto = int.Parse(matches[1].Value);
                CambiaTamañoPreview();
            }
            else
            {
                Console.WriteLine("No se encontraron números en la cadena.");
            }

            /////////
            MiWebCam.NewFrame += new NewFrameEventHandler(Capturando);
            MiWebCam.Start();
            escribirResolucion(comboBox2.Text);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Aseguro de cerrar el proceso anterior para iniciar uno nuevo con el dispositivo correspondiente
            CerrarWebCam();
            llamarWebCam();

            try
            {
                MiWebCam.VideoResolution = selectResolution(MiWebCam);
            }
            catch
            {
                MessageBox.Show("Revise su dispositivo, puede que tenga algun problema" +
                    "\nSi se trata de una cámara virtual como" +
                    "\nNVIDIA BROADCAST, OBS o similares, considere que puede presentar fallos");
                CargaResolucion();
            }

            MiWebCam.NewFrame += new NewFrameEventHandler(Capturando);
            MiWebCam.Start();
            CargaResolucion();
        }

        private async Task<bool> leerRejillaAsync()
        {
            bool rejilla;
            try
            {
                using (TextReader leerDispositivo = new StreamReader(@"C:\MyCam\Rejilla.txt"))
                {
                    rejilla = Convert.ToBoolean(await leerDispositivo.ReadLineAsync());
                }
            }
            catch
            {
                rejilla = false;
            }
            return rejilla;
        }

        private async Task<string> leerDispositivoAsync()
        {
            try
            {
                using (TextReader leerDispositivo = new StreamReader(@"C:\MyCam\dispositivo.txt"))
                {
                    txt = await leerDispositivo.ReadLineAsync();
                }
            }
            catch
            {
                txt = "";
            }
            return txt;
        }

        private async Task<Color> leerColor()
        {
            try
            {
                color = new Color();
                using (TextReader leerColor = new StreamReader(@"C:\MyCam\Color.txt"))
                {
                    string colorString = await leerColor.ReadLineAsync();

                    if (colorString.StartsWith("Color [A=") && colorString.Contains("R=") && colorString.Contains("G=") && colorString.Contains("B="))
                    {
                        // Extraer los valores de A, R, G, y B
                        string[] components = colorString.Trim(' ', 'C', 'o', 'l', 'o', 'r', '[', ']').Split(',');

                        int a = int.Parse(components[0].Split('=')[1]);
                        int r = int.Parse(components[1].Split('=')[1]);
                        int g = int.Parse(components[2].Split('=')[1]);
                        int b = int.Parse(components[3].Split('=')[1]);

                        color = Color.FromArgb(a, r, g, b);
                    }
                    else if (colorString.StartsWith("Color [") && colorString.EndsWith("]"))
                    {
                        // Extraer el nombre del color entre los corchetes
                        colorString = colorString.Substring(7, colorString.Length - 8);
                        color = Color.FromName(colorString);
                    }
                    else if (colorString.StartsWith("#"))
                    {
                        // Convertir código hexadecimal
                        color = ColorTranslator.FromHtml(colorString);
                    }
                    else
                    {
                        // Intentar convertir por nombre directamente
                        color = Color.FromName(colorString);
                    }
                }
            }
            catch
            {
                color = Color.Gray;
            }
            return color;
        }

        private string leerResolucion()
        {
            try
            {
                using (TextReader leerResolucion = new StreamReader(@"C:\MyCam\resolucion.txt"))
                {
                    txt = leerResolucion.ReadLine();
                }
            }
            catch
            {
                txt = "";
            }
            return txt;
        }

        private async Task escribirRejilla(bool rejilla)
        {
            using (TextWriter escribeRejilla = new StreamWriter(@"C:\MyCam\Rejilla.txt"))
            {
                await escribeRejilla.WriteLineAsync(rejilla.ToString());
            }
        }

        private async Task escribirTxt(string nombre)
        {
            using (TextWriter escribeDispositivo = new StreamWriter(@"C:\MyCam\dispositivo.txt"))
            {
                await escribeDispositivo.WriteLineAsync(nombre);
            }
        }

        private async Task escribirResolucion(string resolucion)
        {
            using (TextWriter escribeResolucion = new StreamWriter(@"C:\MyCam\resolucion.txt"))
            {
                await escribeResolucion.WriteLineAsync(resolucion);
            }
        }

        private async Task escribirColor(Color color)
        {
            using (TextWriter escribeResolucion = new StreamWriter(@"C:\MyCam\Color.txt"))
            {
                await escribeResolucion.WriteLineAsync(color.ToString());
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = dialog.SelectedPath;
                    Ruta = dialog.SelectedPath;
                }
            }
        }

        // Método para dibujar la rejilla
        private void DrawGrid(Graphics g, int width, int height)
        {
            Pen pen = new Pen(Color.WhiteSmoke, 1);

            // Dibuja líneas horizontales
            for (int i = 1; i < 3; i++)
            {
                int y = (height / 3) * i;
                g.DrawLine(pen, 0, y, width, y);
            }

            // Dibuja líneas verticales
            for (int i = 1; i < 3; i++)
            {
                int x = (width / 3) * i;
                g.DrawLine(pen, x, 0, x, height);
            }
        }

        // Método para manejar el cambio del CheckBox
        private void checkBoxGrid_CheckedChanged(object sender, EventArgs e)
        {
            showGrid = checkBoxGrid.Checked;
            escribirRejilla(showGrid);
        }

        // Nuevo método para cambiar el color de fondo del Form1 y button2
        private async void buttonChangeColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    this.BackColor = colorDialog.Color;
                    button2.BackColor = colorDialog.Color;
                    await escribirColor(colorDialog.Color);
                }
            }
        }


        // Si el boton para tomar la foto esta en focus, al presionar espacio tomará la foto
        private void button2_KeyDown(object sender, KeyEventArgs e)
        {
            if((e.KeyCode == Keys.Space))
            {
                TomaFoto();
            }
        }

        private void btnDuplicado_Click(object sender, EventArgs e)
        {
            CargaDuplicado cargaDuplicado = new CargaDuplicado();
            cargaDuplicado.ShowDialog();
        }
    }
}
