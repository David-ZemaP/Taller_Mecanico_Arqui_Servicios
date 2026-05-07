namespace Taller_Mecanico_Arqui.Frontend.Authorization;

public enum NivelAcceso
{
    Parcial = 1,
    Completo = 2,
    Gerente = 3
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireAccessLevelAttribute : Attribute
{
    public NivelAcceso RequiredLevel { get; }
    public NivelAcceso[] AllowedLevels { get; }

    public RequireAccessLevelAttribute(NivelAcceso requiredLevel, NivelAcceso[]? allowedLevels = null)
    {
        RequiredLevel = requiredLevel;
        AllowedLevels = allowedLevels ?? new[] { requiredLevel };
    }
}