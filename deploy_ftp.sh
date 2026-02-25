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

# Subir cada archivo recursivamente
(
    cd "$PUBLISH_DIR"
    find . -type f | while read -r file; do
        # Limpiar el path del archivo
        remote_file="${file#./}"
        remote_dir=$(dirname "$remote_file")
        
        # Crear subdirectorios remotos si es necesario
        if [ "$remote_dir" != "." ]; then
            # Intentar crear el directorio. Se ignora el error si ya existe.
            curl -u "$FTP_USER:$FTP_PASS" "$FTP_URL/" -Q "MKD $remote_dir" --silent --output /dev/null
        fi
        
        echo "  -> Subiendo: $remote_file"
        curl -T "$file" -u "$FTP_USER:$FTP_PASS" "$FTP_URL/$remote_file" --silent
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
