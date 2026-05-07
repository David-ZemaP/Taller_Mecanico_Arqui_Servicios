using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Users.Domain.Common;

namespace Taller_Mecanico_Users.Controllers
{
    internal static class ApiResultMapper
    {
        public static IActionResult MapError(ControllerBase controller, Result result)
        {
            var message = result.ErrorMessage ?? "Ocurrio un error en la operacion.";

            return result.ErrorCode switch
            {
                ErrorCodes.UsuarioLoginNotFound or ErrorCodes.NotFound or ErrorCodes.ClienteNotFound or ErrorCodes.EmpleadoNotFound
                    => controller.NotFound(new { code = result.ErrorCode, message }),

                ErrorCodes.UsuarioEmailDuplicado or ErrorCodes.UsuarioEmpleadoDuplicado or ErrorCodes.ValidationDuplicateValue
                    => controller.Conflict(new { code = result.ErrorCode, message }),

                ErrorCodes.ValidationRequired or ErrorCodes.ValidationInvalidValue or ErrorCodes.ValidationInvalidAccessLevel or ErrorCodes.ValidationAdminEmailRequired
                    => controller.BadRequest(new { code = result.ErrorCode, message }),

                ErrorCodes.EmailSendFailed or ErrorCodes.EmailNotConfigured
                    => controller.StatusCode(StatusCodes.Status503ServiceUnavailable, new { code = result.ErrorCode, message }),

                ErrorCodes.DbError or ErrorCodes.DbInsertFailed
                    => controller.StatusCode(StatusCodes.Status500InternalServerError, new { code = result.ErrorCode, message }),

                _ => controller.BadRequest(new { code = result.ErrorCode, message })
            };
        }
    }
}
