#!/bin/bash

# Configuración del FTP
FTP_USER="desarrollo"
FTP_PASS='Aplicacionesichn88!'
FTP_URL="ftp://norteamericano.com/apimp"

# Configuración Local
PROJECT_PATH="./MercadoPagoIntegration"
PUBLISH_DIR="./publish"

echo "🚀 Iniciando publicación vía FTP..."

# 1. Limpiar versiones anteriores
echo "🧹 Limpiando directorio de publicación local..."
rm -rf $PUBLISH_DIR

# 2. Publicar la aplicación
echo "📦 Publicando la aplicación (.NET 10 para Windows)..."
dotnet publish $PROJECT_PATH -c Release -o $PUBLISH_DIR -r win-x64 --self-contained false

if [ $? -ne 0 ]; then
    echo "❌ Error en la publicación de dotnet. Abortando."
    exit 1
fi

# 3. Subir app_offline.htm para detener el sitio temporalmente
echo "⏹️ Deteniendo la aplicación temporalmente (app_offline.htm)..."
echo "<h1>Actualizando aplicacion... por favor espera unos segundos.</h1>" > app_offline.htm
curl -T "app_offline.htm" -u "$FTP_USER:$FTP_PASS" "$FTP_URL/app_offline.htm" --silent

# 4. Subir archivos vía FTP usando curl
echo "🚚 Subiendo archivos al servidor FTP..."

(
    cd "$PUBLISH_DIR"
    # Buscamos todos los archivos
    find . -type f | while read -r file; do
        # remote_path será algo como "app.dll" o "wwwroot/index.html"
        remote_path="${file#./}"
        
        # Obtenemos el directorio si lo hay
        remote_dir=$(dirname "$remote_path")
        
        if [ "$remote_dir" != "." ]; then
            # Intentar crear el directorio remoto. El -Q envía un comando crudo antes de la transferencia.
            # No usamos el path completo del FTP_URL aquí para evitar confusiones de ruta absoluta.
            echo "  [Folder] Creando/Verificando: $remote_dir"
            curl -u "$FTP_USER:$FTP_PASS" "ftp://norteamericano.com/" -Q "MKD /apimp/$remote_dir" --silent --output /dev/null
        fi
        
        echo "  [File] Subiendo: $remote_path"
        # Subida directa al path completo
        curl -u "$FTP_USER:$FTP_PASS" -T "$remote_path" "$FTP_URL/$remote_path" --silent
    done
)

if [ $? -eq 0 ]; then
    echo "✅ Archivos subidos con éxito al FTP."
    # 5. Eliminar app_offline.htm para reactivar el sitio
    echo "▶️ Reactivando la aplicación..."
    # Usamos -X DELE que es más compatible para borrar archivos vía FTP con curl
    curl -u "$FTP_USER:$FTP_PASS" "ftp://norteamericano.com/" -Q "DELE /apimp/app_offline.htm" --silent --output /dev/null
    echo "📂 Destino: $FTP_URL"
else
    echo "❌ Hubo un error durante la subida FTP."
fi

# Limpiar local
echo "🧹 Limpiando archivos temporales..."
rm -rf "$PUBLISH_DIR"
rm -f app_offline.htm
