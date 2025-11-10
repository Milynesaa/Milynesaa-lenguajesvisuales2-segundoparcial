# ClientesAPI
# ClientesAPI 📁

API REST para la gestión de clientes y sus archivos asociados, construida con ASP.NET Core y Entity Framework Core.

## 🚀 Características

- **Gestión de Clientes**: CRUD completo para administrar información de clientes
- **Sistema de Archivos**: Subida y gestión de archivos asociados a clientes
- **Logs de Auditoría**: Registro automático de operaciones en la API
- **Migraciones Automáticas**: La base de datos se actualiza automáticamente al iniciar
- **Manejo de Errores**: Middleware centralizado para gestión de excepciones
- **Documentación Swagger**: Interfaz interactiva para probar endpoints
- **Resiliencia**: Reintentos automáticos ante errores transitorios de base de datos

## 📋 Requisitos Previos

- .NET 6.0 o superior
- SQL Server (configurado en el connection string)
- Visual Studio 2022 o VS Code

## ⚙️ Configuración

### 1. Connection String

Edita `appsettings.json` con tus credenciales de base de datos:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=TU_BASE_DATOS;User Id=TU_USUARIO;Password=TU_PASSWORD;Encrypt=False;MultipleActiveResultSets=True;"
  }
}
```

### 2. Configuración de Archivos

La API permite subir archivos con las siguientes restricciones (configurables en `appsettings.json`):

- **Tamaño máximo**: 100 MB (104857600 bytes)
- **Extensiones permitidas**: `.jpg`, `.jpeg`, `.png`, `.pdf`, `.doc`, `.docx`, `.mp4`, `.avi`
- **Carpeta de subida**: `Uploads`

## 🏃 Ejecución

### Opción 1: Visual Studio
1. Abre la solución en Visual Studio
2. Presiona `F5` o haz clic en "Run"

### Opción 2: Línea de Comandos
```bash
dotnet restore
dotnet run
```

### Opción 3: Compilar y Ejecutar
```bash
dotnet build
dotnet run --project ClientesAPI.csproj
```

## 📚 Documentación de la API

Una vez iniciada la aplicación, accede a Swagger UI en:

```
https://localhost:7296/
```

Aquí podrás:
- Ver todos los endpoints disponibles
- Probar las operaciones directamente
- Ver los esquemas de datos

## 🔍 Endpoints Principales

### Diagnóstico
- `GET /api/test-connection` - Verifica la conexión a la base de datos

### Clientes
- `GET /api/clientes` - Obtener todos los clientes
- `GET /api/clientes/{id}` - Obtener un cliente específico
- `POST /api/clientes` - Crear un nuevo cliente
- `PUT /api/clientes/{id}` - Actualizar un cliente
- `DELETE /api/clientes/{id}` - Eliminar un cliente

### Archivos
- `POST /api/archivos` - Subir archivo asociado a un cliente
- `GET /api/archivos/{id}` - Descargar un archivo
- `GET /api/archivos/cliente/{clienteId}` - Obtener archivos de un cliente
- `DELETE /api/archivos/{id}` - Eliminar un archivo

## 🗄️ Base de Datos

La aplicación utiliza Entity Framework Core con SQL Server. Las migraciones se aplican automáticamente al iniciar.

### Tablas Principales:
- **Clientes**: Información de clientes
- **Archivos**: Metadata de archivos subidos
- **LogsApi**: Registro de operaciones

### Crear una Nueva Migración (si modificas modelos):
```bash
dotnet ef migrations add NombreDeLaMigracion
dotnet ef database update
```

## 🛡️ Seguridad

> ⚠️ **IMPORTANTE**: El archivo `appsettings.json` contiene credenciales sensibles. 
> - **NO** subas este archivo a repositorios públicos
> - Usa variables de entorno en producción
> - Considera usar Azure Key Vault o similar para secretos

## 🔧 Middleware y Servicios

### ErrorHandlingMiddleware
Captura y formatea excepciones de manera centralizada.

### ArchivoService
Gestiona la lógica de negocio para subida, descarga y eliminación de archivos.

## 📝 Logs

Los logs se configuran en dos niveles:
- **Development**: Información detallada
- **Production**: Solo información y advertencias importantes

## 🌐 CORS

La API está configurada con CORS permisivo (`AllowAll`) para desarrollo. 

> ⚠️ En producción, configura CORS con orígenes específicos:

```csharp
policy.WithOrigins("https://tu-dominio.com")
      .AllowAnyMethod()
      .AllowAnyHeader();
```

## 📦 Estructura del Proyecto

```
ClientesAPI/
├── Controllers/        # Controladores de la API
├── Data/              # DbContext y configuración de EF
├── Middleware/        # Middleware personalizado
├── Models/            # Modelos de entidad
├── Services/          # Lógica de negocio
├── Uploads/           # Carpeta de archivos subidos
├── appsettings.json   # Configuración
└── Program.cs         # Punto de entrada
```

## 🐛 Troubleshooting

### Error de conexión a la base de datos
1. Verifica el connection string en `appsettings.json`
2. Asegúrate de que SQL Server esté en ejecución
3. Verifica credenciales y permisos del usuario
4. Usa el endpoint `/api/test-connection` para diagnóstico

### Errores al subir archivos
1. Verifica que la carpeta `Uploads` tenga permisos de escritura
2. Comprueba que el archivo no exceda el tamaño máximo
3. Verifica que la extensión esté en la lista permitida

## 🤝 Contribución

1. Haz fork del proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto es de código abierto y está disponible bajo la licencia MIT.

---

**Desarrollado con ❤️ usando ASP.NET Core**
