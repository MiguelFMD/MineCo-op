Instalar paquete multiplayer services: https://docs.unity.com/en-us/mps-sdk/install-and-upgrade


1. Modificar el protocolo a WebSockets (WSS)
En los métodos donde llamas a tu gestor de Relay, cambia el parámetro de conexión de "udp" (o "dtls") a "wss".

Nota: Asegúrate de que el componente UnityTransport adjunto a tu NetworkManager tiene activada la opción Use WebSockets. Unity 6 suele gestionarlo automáticamente al pasar "wss" a SetRelayServerData, pero es recomendable verificarlo en el Inspector.

2. Cambiar la plataforma a WebGL
Abre File > Build Settings.

En la lista Platform, selecciona WebGL.

Haz clic en Switch Platform (esto recompilará los scripts y puede tardar unos minutos).

Si no tienes WebGL instalado, Unity te mostrará un botón para instalar el módulo a través de Unity Hub.

3. Ajustar los Player Settings para Web
Ve a Edit > Project Settings > Player y selecciona la pestaña con el icono de HTML5 (WebGL).

En Resolution and Presentation, establece la resolución base (por ejemplo, 1280x720) y marca Run in Background para que la red no se desconecte si el jugador cambia de pestaña.

En Publishing Settings:

Cambia Compression Format a Gzip. (Brotli comprime más, pero algunos servidores web gratuitos no lo configuran correctamente y provocan errores de descompresión).

Marca la casilla Decompression Fallback. Esto garantiza que el juego cargue incluso si el servidor no tiene las cabeceras HTTP correctas.

Para solucionarlo y vincular tu proyecto, sigue estos pasos:

Verifica tu sesión: Asegúrate de que has iniciado sesión con tu cuenta de Unity (Unity ID) en el Unity Hub y en el propio Unity Editor (puedes verlo en la esquina superior izquierda o derecha del editor, dependiendo de tu versión).

Abre los servicios: En el menú superior de Unity, ve a Edit > Project Settings y selecciona la pestaña Services (Servicios).

Vincula el proyecto:

Si ves un botón que dice Use an existing Unity project ID o Create a new Unity project ID, haz clic en crear uno nuevo (o selecciona uno existente si ya lo habías creado previamente en el panel de control de Unity).

Te pedirá que elijas tu Organización (tu nombre de usuario normalmente) y que le des un nombre al proyecto en la nube.

Habilita Relay (si no lo estaba): Asegúrate de que, al vincularlo, el servicio de Relay y Authentication siguen marcados como activos o configurados en tu panel.

4. Compilar el juego
Vuelve a File > Build Settings.

Asegúrate de que tu escena MainScene (y cualquier otra, como el Lobby) estén en la lista Scenes In Build.

Haz clic en Build.

Crea una carpeta nueva y vacía en tu ordenador (fuera de la carpeta de Unity) y selecciónala. Unity generará un archivo index.html y una carpeta Build.

5. Subir a Unity Play
Unity Play es la plataforma oficial y gratuita de Unity para compartir prototipos web.

Ve a la carpeta donde compilaste el juego. Selecciona todos los archivos generados (index.html, la carpeta Build, etc.), haz clic derecho y comprímelos en un único archivo .zip.

Abre tu navegador y entra en play.unity.com.

Inicia sesión con tu Unity ID.

Haz clic en tu foto de perfil (arriba a la derecha) y selecciona Post a game.

Rellena el título y la descripción.

En la sección de archivos, arrastra y suelta el archivo .zip que acabas de crear.

Añade una imagen de miniatura y haz clic en Publish.