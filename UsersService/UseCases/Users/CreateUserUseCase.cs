using System;
using System.Threading.Tasks;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Framework.Services;

namespace Taller_Mecanico_Users.UseCases.Users
{
    public class CreateUserUseCase
    {
        private readonly IUsuarioLoginRepository _repository;
        private readonly IMailSender _mailSender;

        public CreateUserUseCase(IUsuarioLoginRepository repository, IMailSender mailSender)
        {
            _repository = repository;
            _mailSender = mailSender;
        }

        public async Task<UsuarioLogin> ExecuteAsync(int empleadoId, string email)
        {
            // 1. Generar contraseña segura temporal
            string plainPassword = PasswordSecurity.GenerateSecurePassword();
            
            // 2. Encriptar la contraseña
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            // 3. Crear entidad forzando el primer inicio de sesión (requiereCambioPassword = true)
            var nuevoUsuario = UsuarioLogin.Crear(empleadoId, email, passwordHash, requiereCambioPassword: true);

            // 4. Guardar en el repositorio
            var addResult = await _repository.AddAsync(nuevoUsuario);
            if (addResult.IsFailure)
            {
                throw new InvalidOperationException($"Error al crear usuario: {addResult.ErrorMessage}");
            }

            // 5. Enviar credenciales por correo simulado
            string mailBody = $"Hola,\nTu cuenta ha sido creada exitosamente.\nTu contraseña temporal es: {plainPassword}\nPor favor, cámbiala al iniciar sesión por primera vez.";
            await _mailSender.SendEmailAsync(email, "Credenciales de Acceso - Taller Mecánico", mailBody);

            return nuevoUsuario;
        }
    }
}