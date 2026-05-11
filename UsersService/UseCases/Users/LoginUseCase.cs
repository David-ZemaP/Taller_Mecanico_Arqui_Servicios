using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.UseCases.Users
{
    /// <summary>
    /// UseCase: Autenticación de usuario y generación de JWT.
    /// 
    /// Responsabilidades:
    /// 1. Validar credenciales (email + password)
    /// 2. Verificar que el usuario esté activo
    /// 3. Registrar último acceso
    /// 4. Generar token JWT
    /// 
    /// Arquitectura:
    /// - Repository: Persistencia ✅ (IUsuarioLoginRepository)
    /// - PasswordHasher: Validar contraseña ✅ (IPasswordHasher)
    /// - TokenGenerator: Crear JWT ✅ (IJwtTokenGenerator)
    /// 
    /// Nota: Esta lógica estaba en AuthController, ahora está en su propio UseCase.
    /// </summary>
    public class LoginUseCase
    {
        private readonly IUsuarioLoginRepository _loginRepository;
        private readonly Domain.Ports.IPasswordHasher _passwordHasher;
        private readonly Taller_Mecanico_Users.Application.Services.IJwtTokenGenerator _tokenGenerator;

        public LoginUseCase(
            IUsuarioLoginRepository loginRepository,
            Domain.Ports.IPasswordHasher passwordHasher,
            Taller_Mecanico_Users.Application.Services.IJwtTokenGenerator tokenGenerator)
        {
            _loginRepository = loginRepository;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
        }

        /// <summary>
        /// Ejecuta el proceso de autenticación.
        /// Retorna: Result<LoginResponse> con token JWT o error.
        /// </summary>
        public async Task<Result<LoginResponse>> ExecuteAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Result<LoginResponse>.Failure(ErrorCodes.ValidationRequired, "El email y la contraseña son obligatorios.");
            }

            // 1. Buscar usuario por email
            var usuario = await _loginRepository.GetByEmailAsync(email);
            
            if (usuario == null)
            {
                // ⚠️ Por seguridad: No revelar si el usuario existe o no
                return Result<LoginResponse>.Failure(
                    ErrorCodes.UsuarioLoginNotFound,
                    "Credenciales inválidas.");
            }

            // 2. Verificar que está activo
            if (!usuario.Activo)
            {
                return Result<LoginResponse>.Failure(
                    ErrorCodes.ValidationInvalidValue,
                    "Credenciales inválidas.");
            }

            // 3. Validar contraseña
            if (!_passwordHasher.VerifyPassword(password, usuario.PasswordHash))
            {
                return Result<LoginResponse>.Failure(
                    ErrorCodes.ValidationInvalidValue,
                    "Credenciales inválidas.");
            }

            // 4. Registrar acceso
            usuario.RegistrarAcceso();
            var updateResult = await _loginRepository.UpdateAsync(usuario);
            
            if (updateResult.IsFailure)
            {
                // Si falla la auditoría, aún permitimos el login pero reportamos error
                System.Diagnostics.Debug.WriteLine($"Advertencia: No se pudo registrar acceso para {email}");
            }

            // 5. Generar token JWT
            var token = _tokenGenerator.GenerateToken(usuario);

            return Result<LoginResponse>.Success(new LoginResponse
            {
                Token = token,
                RequiereCambioPassword = usuario.RequiereCambioPassword,
                EsCliente = usuario.EsCliente
            });
        }
    }

    /// <summary>
    /// DTO de respuesta para el login.
    /// Contiene el token JWT y flags del usuario.
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public bool RequiereCambioPassword { get; set; }
        public bool EsCliente { get; set; }
    }
}
