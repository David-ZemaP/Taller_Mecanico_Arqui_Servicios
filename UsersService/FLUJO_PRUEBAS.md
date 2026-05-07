# Flujo de Pruebas del Servicio `Taller_Mecanico_Users`

Este documento describe un flujo práctico para validar el sistema end-to-end, desde la autenticación hasta el envío de correos SMTP y la auditoría.

## 1. Preparación del entorno

1. Levanta PostgreSQL y verifica que la base de datos apunte a `ConnectionStrings:DefaultConnection`.
2. Revisa `App/appsettings.json` y confirma:
   - `JwtSettings` configurado.
   - `Smtp:Enabled = true`.
   - Credenciales SMTP válidas.
   - `Smtp:TestRecipient` definido para pruebas.
3. Verifica que la solución compile.

```bash
cd "c:\Arqui_Soft\Taller_Mecanico_Users"
dotnet build Taller_Mecanico_Users.sln
```

## 2. Flujo de autenticación

### 2.1 Login exitoso

1. Llama al endpoint de login con un usuario existente y contraseña válida.
2. Verifica que la respuesta devuelva:
   - `200 OK`
   - JWT en la respuesta
   - claim de cambio de contraseña cuando corresponda

### 2.2 Login fallido

1. Usa una contraseña incorrecta.
2. Verifica que el sistema responda con `401 Unauthorized`.

### 2.3 Primer ingreso con cambio de contraseña

1. Inicia sesión con un usuario que tenga `RequiereCambioPassword = true`.
2. Verifica que el token incluya la condición de cambio obligatorio.
3. Intenta acceder a un endpoint protegido.
4. Verifica que el middleware bloquee la operación hasta cambiar la contraseña.

## 3. Flujo de gestión de usuarios

### 3.1 Crear usuario

1. Envía una solicitud de alta con un email nuevo.
2. Verifica `201 Created` o respuesta equivalente del controlador.
3. Comprueba que el usuario exista en base de datos.

### 3.2 Crear usuario duplicado

1. Repite el alta con el mismo email.
2. Verifica `409 Conflict`.

### 3.3 Actualizar usuario

1. Modifica email o estado del usuario.
2. Verifica que el cambio se persista sin romper invariantes del dominio.

### 3.4 Eliminar usuario

1. Ejecuta la eliminación lógica o física según la implementación vigente.
2. Verifica que el registro deje de aparecer en consultas activas.

## 4. Flujo de contraseña

### 4.1 Cambiar contraseña

1. Ejecuta el caso de uso de cambio con contraseña actual correcta.
2. Usa una nueva contraseña que cumpla la política.
3. Verifica que la contraseña quede hasheada y que el usuario ya no pueda iniciar sesión con la anterior.

### 4.2 Cambio con contraseña actual inválida

1. Envía una contraseña actual incorrecta.
2. Verifica `401 Unauthorized` o el error de negocio equivalente.

### 4.3 Reset de contraseña

1. Ejecuta el reset desde el caso de uso.
2. Verifica que:
   - Se genere una contraseña temporal.
   - Se marque `RequiereCambioPassword = true`.
   - Se envíe el correo SMTP al destinatario configurado.

## 5. Flujo de auditoría

1. Ejecuta creación, actualización, login, cambio de contraseña y eliminación.
2. Revisa la tabla `audit_logs`.
3. Valida que cada operación deje registro con:
   - tabla afectada
   - id del registro
   - acción realizada
   - actor asociado
   - fecha/hora

## 6. Flujo SMTP

### 6.1 Envío de prueba

1. Ejecuta la utilidad `Tools/SmtpTest`.
2. Verifica que el correo se intente enviar al destinatario configurado.
3. Si Gmail rechaza autenticación, revisa que la contraseña sea una App Password válida.

```bash
cd "c:\Arqui_Soft\Taller_Mecanico_Users\Tools\SmtpTest"
dotnet run --project SmtpTest.csproj
```

## 7. Validaciones mínimas esperadas

- `Login` devuelve JWT válido.
- `ChangePassword` exige contraseña actual.
- `ResetPassword` genera temporal y envía email.
- `CreateUser` evita duplicados por email.
- `UpdateUser` respeta invariantes del dominio.
- `AuditService` registra cambios en transacción.
- `RequirePasswordChangeMiddleware` bloquea acceso hasta actualizar contraseña.

## 8. Tabla comparativa

| Área | Estado actual | Deberíamos tener |
|---|---|---|
| Autenticación | Login con JWT funcionando | Login con JWT, expiración controlada y claims de negocio correctos |
| Cambio de contraseña | Valida contraseña actual y política | Cambio obligatorio cuando aplique, con respuesta clara al usuario |
| Reset de contraseña | Genera temporal y dispara correo | Reset seguro, trazable y con correo SMTP entregado correctamente |
| Usuario duplicado | Se bloquea por email duplicado | Evitar duplicados por email, idealmente con validación de dominio y restricción de BD |
| Auditoría | Se registra en `audit_logs` dentro de transacción | Auditoría completa, consistente y con actor autenticado |
| SMTP | Implementación real con configuración en `appsettings.json` | Envío robusto, con manejo de fallos, retry opcional y credenciales seguras |
| Clean Architecture | Separación por capas mantenida | Mantener dependencias hacia adentro y evitar lógica de negocio en controllers |
| Middleware de primer acceso | Bloquea acceso si requiere cambio de contraseña | Bloqueo uniforme en endpoints protegidos y mensajes de error consistentes |

## 9. Criterio de aceptación

El sistema se considera validado cuando todos los flujos anteriores pasan sin errores y la tabla comparativa no muestra brechas funcionales críticas entre el estado actual y el esperado.
