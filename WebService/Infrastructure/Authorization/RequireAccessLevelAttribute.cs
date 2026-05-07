using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Taller_Mecanico_WebService.Domain;

namespace Taller_Mecanico_WebService.Infrastructure.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireAccessLevelAttribute : Attribute, IPageFilter
    {
        private readonly NivelAcceso _nivelRequerido;

        public RequireAccessLevelAttribute(NivelAcceso nivelRequerido)
        {
            _nivelRequerido = nivelRequerido;
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated == true)
            {
                context.Result = new RedirectToPageResult("/Login");
                return;
            }

            var nivelClaim = user.FindFirst("NivelAcceso")?.Value;
            if (nivelClaim == null || !Enum.TryParse<NivelAcceso>(nivelClaim, out var nivelUsuario)
                || nivelUsuario != _nivelRequerido)
            {
                context.Result = new RedirectToPageResult("/AccesoDenegado");
            }
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
    }
}
