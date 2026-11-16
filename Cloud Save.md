1. Instalar paquete de Cloud Save
2. Instalar el Cloud Save Sample para tener de ejemplo

El script de ejemplo de Unity Cloud Save SDK se divide en bloques funcionales según el tipo de datos que manipula. A continuación, se detalla el propósito de cada función y su contexto de uso.

Inicialización y Flujo Principal
Awake(): Es el punto de entrada del script. Inicializa los servicios de Unity (UnityServices.InitializeAsync()), autentica al usuario de forma anónima y ejecuta secuencialmente todas las demás funciones a modo de demostración.

Ejemplo de uso: En un juego comercial, la inicialización y el login no suelen ir integrados en la clase que gestiona el guardado, sino en un gestor de arranque central. Aquí simplemente orquesta la ejecución para demostrar el ciclo completo de vida (crear, leer, actualizar y borrar).

Gestión de Datos JSON (Player Data)
Permite al jugador guardar y cargar variables y objetos estructurados.

ForceSaveSingleData(key, value): Recibe un valor en texto, intenta convertirlo en entero (int) o decimal (float), y lo guarda en la nube. Al usar la palabra "Force" (en el contexto de este script), ignora los conflictos de concurrencia al no enviar un write lock.

Ejemplo de uso: Guardar el nombre del jugador ("PlayerName", "Hero123"), donde no importa si se sobreescribe incondicionalmente.

ForceSaveObjectData(key, value): Guarda un objeto C# complejo (la clase serializable SampleObject) en la nube de forma forzada. Retorna el write lock (un código de versión) que el servidor genera tras este guardado.

Ejemplo de uso: Crear el estado inicial del jugador la primera vez que se conecta (nivel, vida base, inventario inicial).

SaveObjectData(key, value, writeLock): Actualiza un objeto complejo utilizando la clase SaveItem, la cual exige pasar el write lock. Si el dato en el servidor fue modificado recientemente desde otro dispositivo (cambiando su write lock), la operación fallará por seguridad.

Ejemplo de uso: Evitar que un jugador que tiene el juego abierto en su PC y en su móvil simultáneamente sobreescriba accidentalmente su progreso más avanzado con un guardado antiguo.

RetrieveSpecificData<T>(key): Pide a la nube la clave solicitada y deserializa su valor transformándolo automáticamente en el tipo C# indicado por <T>.

Ejemplo de uso: Cargar la configuración de audio al iniciar la aplicación (RetrieveSpecificData<AudioSettings>("AudioPrefs")).

RetrieveEverything(): Descarga de golpe todas las claves y valores que el jugador tiene en la nube.

Ejemplo de uso: Realizar un volcado completo de la partida en la pantalla de carga principal para evitar hacer peticiones de red intermitentes durante el gameplay.

ListAllKeys(): Obtiene únicamente la lista de nombres de las claves almacenadas, sin descargar sus valores.

Ejemplo de uso: Comprobar rápidamente si las claves "SaveSlot1" o "SaveSlot2" existen para decidir si el botón de "Continuar partida" en el menú principal debe estar habilitado.

Gestión de Datos Personalizados (Custom Data)
Los Custom Data son variables configuradas desde el panel de control (Dashboard) de Unity por los desarrolladores. Son accesibles para el cliente pero son de solo lectura.

ListAllCustomKeys(customId) y RetrieveAllCustomData(customId): Funcionan exactamente igual que las funciones del jugador, pero apuntan al servicio Data.Custom.

Ejemplo de uso: Descargar las estadísticas base de los enemigos, configuraciones de eventos temporales o balanceo de armas creadas en el servidor (customId = "HalloweenEvent2026"), asegurando que los jugadores no puedan alterar esos valores mediante trucos.

Borrado de Datos (Player Data)
ForceDeleteSpecificData(key): Elimina una clave de la base de datos de la nube incondicionalmente.

Ejemplo de uso: Borrar un estado temporal o vaciar un hueco de guardado.

DeleteSpecificData(key, writeLock): Elimina la clave pasando opciones de seguridad (DeleteOptions). Solo se ejecutará si el write lock proporcionado coincide con el del servidor.

Ejemplo de uso: Borrar un listado de transacciones pendientes solo si nos aseguramos de que estamos interactuando con la versión más reciente del archivo.

Gestión de Archivos (Files)
Además de pares clave-valor, Cloud Save permite subir archivos binarios (imágenes, audios o partidas binarias de gran tamaño).

SaveFileBytes(key, bytes) y SaveFileStream(key, stream): Suben un archivo a los servidores de Unity, ya sea volcando una matriz de bytes directamente o utilizando un Stream.

Ejemplo de uso: Subir una miniatura (thumbnail) tomada en el juego para asociarla a un nivel creado por el jugador.

LoadFileBytes(key) y LoadFileStream(key): Descargan un archivo alojado. Usar LoadFileStream es ideal para gestionar la memoria con archivos pesados, mientras que LoadFileBytes es rápido y práctico para archivos muy pequeños.

Ejemplo de uso: Descargar un archivo binario .dat encriptado que contiene el diseño de una ciudad en un juego de construcción.

ListAllFiles() y GetFileMetadata(key): Leen los metadatos de los archivos del jugador (nombre de la clave y peso en bytes), sin llegar a descargar el contenido real.

Ejemplo de uso: Leer el campo Size del archivo y mostrarle al jugador un mensaje que diga "El mundo que vas a descargar pesa 15 MB" antes de iniciar la descarga.

DeleteFile(key): Elimina permanentemente el archivo alojado en la base de datos del jugador.