using Taller_Mecanico_Users.Domain.Common;
using System;
using System.Threading.Tasks;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.UseCases.Users
{
    
    public class CreateUserUseCase
    {
        private readonly IUsuarioLoginRepository _repository;
        private readonly Domain.Ports.IMailSender _mailSender;
        private readonly Domain.Ports.IPasswordSecurity _passwordSecurity;
        private readonly Domain.Ports.IPasswordHasher _passwordHasher;

        public CreateUserUseCase(
            IUsuarioLoginRepository repository,
            Domain.Ports.IMailSender mailSender,
            Domain.Ports.IPasswordSecurity passwordSecurity,
            Domain.Ports.IPasswordHasher passwordHasher)
        {
            _repository = repository;
            _mailSender = mailSender;
            _passwordSecurity = passwordSecurity;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<UsuarioLogin>> ExecuteAsync(int empleadoId, string email)
        {
            // 0. Validar que el empleado no tenga ya un login
            var existingByEmployee = await _repository.GetByEmpleadoIdAsync(empleadoId);
            if (existingByEmployee != null)
            {
                return Result<UsuarioLogin>.Failure(ErrorCodes.UsuarioEmpleadoDuplicado, "El empleado ya tiene un usuario asignado.");
            }

            // 0. Validar email duplicado
            var existing = await _repository.GetByEmailAsync(email);
            if (existing != null)
            {
                return Result<UsuarioLogin>.Failure(ErrorCodes.UsuarioEmailDuplicado, "El email ya está registrado.");
            }

            // 1. Generar contraseña segura temporal
            string plainPassword = _passwordSecurity.GenerateSecurePassword();
            
            // 2. Hashear la contraseña
            string passwordHash = _passwordHasher.HashPassword(plainPassword);

            // 3. Crear entidad forzando cambio de contraseña en primer acceso
            var nuevoUsuarioResult = UsuarioLogin.Crear(empleadoId, email, passwordHash, requiereCambioPassword: true);
            if (nuevoUsuarioResult.IsFailure)
            {
                return Result<UsuarioLogin>.Failure(nuevoUsuarioResult.ErrorCode!, nuevoUsuarioResult.ErrorMessage!);
            }

            var nuevoUsuario = nuevoUsuarioResult.Value!;

            // 4. Persistir en el repositorio
            var addResult = await _repository.AddAsync(nuevoUsuario);
            if (addResult.IsFailure)
            {
                return Result<UsuarioLogin>.Failure(addResult.ErrorCode ?? ErrorCodes.DbError, addResult.ErrorMessage ?? "Error al crear usuario.");
            }

            // 5. Enviar credenciales por correo
            string mailBody = $"Hola,\nTu cuenta ha sido creada exitosamente.\nTu contraseña temporal es: {plainPassword}\nPor favor, cámbiala al iniciar sesión por primera vez.";
            await _mailSender.SendEmailAsync(email, "Credenciales de Acceso - Taller Mecánico", mailBody);

            return Result<UsuarioLogin>.Success(nuevoUsuario);
        }
    }
}