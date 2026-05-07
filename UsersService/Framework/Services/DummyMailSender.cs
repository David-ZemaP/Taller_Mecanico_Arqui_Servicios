using System;
using System.Threading.Tasks;

namespace Taller_Mecanico_Users.App.Services // Mantenemos el namespace que registraste
{
    public class DummyMailSender : Taller_Mecanico_Users.Framework.Services.IMailSender, Taller_Mecanico_Users.Domain.Ports.IMailSender
    {
        public Task SendEmailAsync(string to, string subject, string body)
        {
            Console.WriteLine("\n================ SIMULACIÓN DE CORREO ================");
            Console.WriteLine($"Para: {to}");
            Console.WriteLine($"Asunto: {subject}");
            Console.WriteLine($"Mensaje:\n{body}");
            Console.WriteLine("======================================================\n");
            
            return Task.CompletedTask;
        }
    }
}