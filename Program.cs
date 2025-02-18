using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProexsiCam
{
    static class Program
    {
        // Declaramos el mutex a nivel global para que persista durante toda la ejecución de la aplicación
        static Mutex mutex;

        [STAThread]
        static void Main()
        {
            bool createdNew;

            // Nombre único para el mutex, puede ser cualquier nombre, pero debe ser único para la app
            mutex = new Mutex(true, "ProexsiCam", out createdNew);

            if (!createdNew)
            {
                // Si ya existe otra instancia de la aplicación, traemos esa ventana al frente
                ObtenerAplicacionExistente();
                return; // Salimos de la nueva instancia
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Ventana());
        }

        static void ObtenerAplicacionExistente()
        {
            // Obtener la instancia actual de la aplicación (MainForm en este caso)
            var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            foreach (var process in System.Diagnostics.Process.GetProcessesByName(currentProcess.ProcessName))
            {
                if (process.Id != currentProcess.Id)
                {
                    // Traer al frente la ventana existente
                    IntPtr handle = process.MainWindowHandle;
                    if (handle != IntPtr.Zero)
                    {
                        // Restablecer si estaba minimizada y traer al frente
                        ShowWindow(handle, 9); // Equivalente de SW_RESTORE de WPF
                        SetForegroundWindow(handle);
                    }
                    break;
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
