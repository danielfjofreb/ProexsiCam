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
using ProexsiCam.Properties;

namespace ProexsiCam
{
    public partial class Ventana : Form
    {
        public string Ruta;
        // Colección de dispositivos de video
        private FilterInfoCollection MisDispositivos;
        // Dispositivo de captura de video seleccionado
        private VideoCaptureDevice MiWebCam;
        // Texto para guardar las configuraciones en archivos de texto
        private string txt = "";
        // Color de fondo del formulario
        private Color color;
        private bool showGrid = false;  // Controla la rejilla de la foto
        private Bitmap currentFrame;    // Almacena el frame actual sin rejilla
        private int Ancho;
        private int Alto;
        // Modo de operación: 0 para tomar foto, 1 para adjuntar foto
        private int modo = 0;

        public Ventana()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Carga las configuraciones desde los archivos de texto
            await CargaColor();
            showGrid = await leerRejillaAsync();
            checkBoxGrid.Checked = showGrid;
            CargaModo();
            
            pictureBox2.Visible = false;

            // Esta no es una configuracion que se guarde, pero podria hacerse si quisiese, ya que existen los controles necesarios
            // Actualizacion: Ahora si guarda y carga la ruta de guardado
            Ruta = await leerRuta();
            textBox1.Text = Ruta;
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


        // Función para verificar si hay dispositivos de video conectados
        private bool HayDispositivos()
        {
            MisDispositivos = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            return MisDispositivos.Count > 0;
        }


        /* Cierra los procesos de la webcam si están activos, esto sirve para evitar
           conflictos al cambiar de dispositivo o resolución y ayuda a liberar memoria */
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

            /* Limpia las resoluciones soportadas al cambiar de dispositivo, ya que no todos tienen 
               las mismas resoluciones soportadas, lo mismo pasaría si esta fuese una aplicación de videos
               con el framerate */
            if (comboBox2.Items.Count >= 0)
            {
                comboBox2.Items.Clear();
            }

            // Verifica las resoluciones soportadas por el dispositivo de video y las agrega al ComboBox
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

            // Si la rejilla está activada, dibuja la rejilla en el frame usando Graphics
            using (Graphics g = Graphics.FromImage(frameWithGrid))
            {
                if (showGrid)
                {
                    DrawGrid(g, frameWithGrid.Width, frameWithGrid.Height);
                }
            }
            // Asigna la imagen con la rejilla al PictureBox
            pictureBox1.BackgroundImage = frameWithGrid;
            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CerrarWebCam();
        }

        int boton = 0;

        // Evento del botón para tomar o adjuntar foto, según el modo seleccionado
        private void button2_Click(object sender, EventArgs e)
        {
            switch (modo)
            {
                case 0:
                    TomaFoto();
                    break;
                case 1:
                    AdjuntarFoto();
                    break;
            }
            
        }

        private void TomaFoto()
        {
            if (currentFrame != null)
            {
                // Forza la relación de aspecto 4:3 y guarda la imagen
                pictureBox2.BackgroundImage = ForceAspectRatio((Bitmap)currentFrame.Clone(), 4, 3);
                pictureBox1.Visible = true;
                pictureBox2.BackgroundImage.Save(Path.Combine(Ruta, $"{boton}.jpg"), ImageFormat.Jpeg);
                // Simula un retraso para mostrar la imagen guardada
                Thread.Sleep(1000);
                /* Contador sube por si la persona quiere tomarse varias fotos, sin llegar a reemplazar la primera foto,
                   de este modo se puede elegir entre varias fotos la mejor */
                boton++;
                pictureBox2.Visible = false;
            }
        }


        /* Método para adjuntar una foto desde el disco duro o desde un dispositivo de almacenamiento
           Este metodo fue desarrollado para Victoria por su modo de trabajo */
        private void AdjuntarFoto()
        {
            try
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Archivos de imagen|*.jpg;*.png;*.bmp;*.gif";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        pictureBox1.Image = Image.FromFile(dialog.FileName);
                        if (MessageBox.Show("¿Desea importar fotografía al servidor?", "Importar", MessageBoxButtons.OKCancel) == DialogResult.OK)
                        {
                            if (pictureBox1.Image != null)
                            {
                                Adjunta adjunta = new Adjunta();
                                // Muestra el formulario Adjunta para ingresar el RUT
                                adjunta.ShowDialog();
                                // Obtiene el RUT ingresado por el usuario en el formulario Adjunta
                                string rutjpg = adjunta.Aceptar();

                                // Verifica si el RUT no es nulo o vacío
                                if (rutjpg != null || !rutjpg.Equals(""))
                                {
                                    // Forza la relación de aspecto 4:3 y guarda la imagen
                                    pictureBox2.BackgroundImage = ForceAspectRatio((Bitmap)pictureBox1.Image.Clone(), 4, 3);
                                    pictureBox1.Visible = true;
                                    pictureBox2.BackgroundImage.Save(Path.Combine(Ruta, $"{rutjpg}.jpg"), ImageFormat.Jpeg);
                                    Thread.Sleep(1000);
                                    MessageBox.Show("Foto guardada correctamente", "Confirmación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                pictureBox2.Visible = false;
                            }
                        }
                    }
                }
            }
            // Captura cualquier excepción que ocurra durante el proceso de adjuntar foto
            catch (Exception ex) { MessageBox.Show("Error al guardar: \n" + ex); }
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox1.BackgroundImage = null;
            pictureBox2.BackgroundImage = null;
        }


        // Método para cambiar el tamaño del PictureBox según la relación de aspecto
        private int CambiaTamañoPreview()
        {
            double calculo = (double)Ancho / Alto;
            pictureBox1.Width =
                calculo >= 1.77 ? 957 : 750;
            pictureBox1.Location =
                calculo >= 1.77 ? new Point(60, 53) : new Point(200, 53);
            return 0;
        }


        // Calcula y ajusta la imagen a una relación de aspecto 4:3, recortando si es necesario
        private Bitmap ForceAspectRatio(Bitmap original, int targetWidth, int targetHeight)
        {
            float desiredRatio = (float)targetWidth / targetHeight;
            int originalWidth = original.Width;
            int originalHeight = original.Height;
            float originalRatio = (float)originalWidth / originalHeight;

            // Determinar el área de recorte
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

            // Recortar la imagen
            Bitmap croppedBitmap = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(croppedBitmap))
            {
                g.DrawImage(original, new Rectangle(0, 0, cropRect.Width, cropRect.Height),
                    cropRect, GraphicsUnit.Pixel);
            }

            // Si la imagen es mayor a 960x720, reducirla
            int maxWidth = 960;
            int maxHeight = 720;

            if (croppedBitmap.Width > maxWidth || croppedBitmap.Height > maxHeight)
            {
                Bitmap resizedBitmap = new Bitmap(maxWidth, maxHeight);
                using (Graphics g = Graphics.FromImage(resizedBitmap))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(croppedBitmap, new Rectangle(0, 0, maxWidth, maxHeight));
                }
                croppedBitmap.Dispose(); // Liberar la memoria de la imagen anterior
                return resizedBitmap;
            }

            return croppedBitmap;
        }


        // Evento del ComboBox para seleccionar la resolución de video
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            MiWebCam.Stop();

            CerrarWebCam();
            llamarWebCam();

            int i = comboBox2.SelectedIndex;

            MiWebCam.VideoResolution = MiWebCam.VideoCapabilities[i];

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

        // Evento del ComboBox para seleccionar el dispositivo de video
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


        /* Todos los métodos Leer y Escribir son asincronos, para evitar bloqueos en la interfaz de usuario
           ademas, todos los metodos Leer y Escribir sirven para guardar y cargar las configuraciones del programa */
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

        private string leerModo()
        {
            try
            {
                using (TextReader leerResolucion = new StreamReader(@"C:\MyCam\modo.txt"))
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

        private async Task<string> leerRuta()
        {
            try 
            {
                using (TextReader leerRuta = new StreamReader(@"C:\MyCam\ruta.txt"))
                {
                    txt = leerRuta.ReadLine();
                }
            }
            catch { txt = @"C:\Proexsi\FOTOS"; } return txt;
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

        private async Task escribirModo(string modo)
        {
            try
            {
                using (TextWriter escribeResolucion = new StreamWriter(@"C:\MyCam\modo.txt"))
                {
                    await escribeResolucion.WriteLineAsync(modo.ToString());
                }
            }
            catch
            {
                MessageBox.Show("No se pudo guardar la configuracion, verifique existencia de carpeta 'MyCam'");
            }
        }

        private async Task escribirRuta(string ruta)
        {
            try
            {
                using (TextWriter escribeRuta = new StreamWriter(@"C:\MyCam\ruta.txt"))
                {
                    await escribeRuta.WriteLineAsync(ruta.ToString());
                }
            }
            catch
            {
                MessageBox.Show("No se pudo guardar la configuracion, verifique existencia de carpeta 'MyCam' en disco C");
            }
        }


        // Evento del botón para seleccionar la carpeta de guardado
        private async void button1_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = dialog.SelectedPath;
                    Ruta = dialog.SelectedPath;
                    await escribirRuta(dialog.SelectedPath);
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
            CargaDuplicado cargaDuplicado = new CargaDuplicado(Ruta);
            cargaDuplicado.ShowDialog();
        }

        private async void cbxModo_SelectedIndexChanged(object sender, EventArgs e)
        {
            modo = cbxModo.SelectedIndex;
            switch (cbxModo.SelectedIndex)
            {
                case 0: modoFoto();
                    await escribirModo(cbxModo.SelectedIndex.ToString());
                    break;
                case 1: modoAdjuntar();
                    pictureBox1.Image = null; pictureBox2.Image = null;
                    pictureBox1.BackgroundImage = null; pictureBox2.BackgroundImage = null;
                    await escribirModo(cbxModo.SelectedIndex.ToString());
                    break;
            }
        }


        // Método para cambiar al modo de tomar foto, esto hace que se carguen los elementos necesarios para este modo
        public async void modoFoto()
        {
            comboBox1.Text = "";
            comboBox1.Items.Clear();
            label3.Visible = true;
            label2.Visible = true;
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            comboBox1.Visible = true;
            comboBox2.Visible = true;
            await CargaDispositivosAsync();
            button2.BackgroundImage = Resources.Camara130;
            toolTip1.SetToolTip(button2,"Tomar Foto");

        }

        // Método para cambiar al modo de adjuntar foto, esto hace que se oculten los elementos necesarios para este modo

        public void modoAdjuntar()
        {
            CerrarWebCam();
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
            comboBox1.Visible = false;
            comboBox2.Visible = false;
            label3.Visible = false;
            label2.Visible = false;
            button2.BackgroundImage = Resources.Adjuntar;
            toolTip1.SetToolTip(button2, "Adjuntar Foto");
        }

        // Método para cargar el modo de operación al iniciar la aplicación
        private void CargaModo()
        {
            string modoAnterior = leerModo();
            cbxModo.SelectedIndex = int.TryParse(modoAnterior, out modo) ? modo : 0;
            
        }
    }
}
