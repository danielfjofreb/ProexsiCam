# ProexsiCam

**ProexsiCam** es una aplicación desarrollada en C# con Windows Forms, diseñada para capturar, adjuntar y gestionar fotografías asociadas a trámites de licencias de conducir. Su principal enfoque es la compatibilidad con cámaras web y cámaras réflex utilizadas en municipalidades.

## ✨ Características

- 📸 **Captura de fotografías** desde cámara web.
- 📂 **Adjuntar fotografías** tomadas con cámaras réflex o externas.
- 📐 **Forzado de relación de aspecto 4:3** en imágenes guardadas.
- 🎨 Personalización del color de fondo.
- 🗂️ Modo configurable: Captura o Adjuntar.
- 📏 Soporte para selección de resolución y dispositivo.
- 📄 Configuración persistente mediante archivos de texto.
- 🔳 Rejilla de encuadre opcional.
- 📁 Opción para seleccionar carpeta de guardado.
- 🔁 Función de duplicado para recuperar fotos antiguas por RUT.

## 🧰 Tecnologías utilizadas

- C# (.NET Framework 4.8.1)
- Windows Forms (WinForms)
- AForge.NET (para manejo de cámara)

## 📷 Casos de uso

- Municipalidades que toman fotografías directamente desde software para licencias (Modo Fotografía).
- Oficinas donde se recibe la fotografía desde cámaras externas y se desea almacenarlas uniformemente (Modo Adjuntar).
- Recuperación de fotos antiguas desde una red compartida (`M:\LICENCIA DE CONDUCIR\Fotos\fotosres\`) para trámites de duplicado.

## 📦 Estructura del proyecto

- `Ventana.cs` – Lógica principal de la interfaz.
- `Adjunta.cs` – Diálogo para ingresar RUT al adjuntar imagen.
- `CargaDuplicado.cs` – Funcionalidad para cargar duplicado desde red.
- Archivos de configuración:
  - `C:\MyCam\dispositivo.txt`
  - `C:\MyCam\resolucion.txt`
  - `C:\MyCam\Color.txt`
  - `C:\MyCam\modo.txt`
  - `C:\MyCam\ruta.txt`
  - `C:\MyCam\Rejilla.txt`

## 📁 Ejemplo de uso

1. Selecciona el modo: Captura o Adjuntar.
2. Si usas "Captura", selecciona el dispositivo y resolución.
3. Toma la foto o adjunta una desde tu equipo.
4. El software guardará la imagen en la carpeta seleccionada, con el nombre basado en el RUT en caso de ser un adjunto o con un numero que va en aumento de acuerdo a la cantidad de fotos que tome.

## 🛑 Consideraciones

- Algunas rutas están "a fuego" en el código, como `M:\` para recuperación de duplicados, esto lo puedes adaptar a tu gusto o borrarlo dentro de tu proyecto si no lo necesitas.
- La configuración se guarda en `C:\MyCam\` como archivos `.txt`. Asegúrate de que exista esta carpeta (aunque el programa igual la va a crear si se te olvida, no te preocupes 👀)

## 🧾 Licencia

Este proyecto está disponible públicamente con fines educativos y prácticos. Puedes usarlo, adaptarlo y compartirlo. Agradecimientos son bienvenidos 😄

---

## 🙌 Autor

Desarrollado por Daniel Jofré para apoyar el trabajo de atención a público en los clientes municipales de Proexsi.
