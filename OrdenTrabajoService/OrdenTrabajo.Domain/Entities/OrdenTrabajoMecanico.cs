namespace Taller_Mecanico_Arqui.Domain.Entities
{
    public class OrdenTrabajoMecanico
    {
        public int OrdenTrabajoId { get; private set; }
        public int MecanicoId { get; private set; }
        public DateTime FechaAsignacion { get; private set; }

        public OrdenTrabajo? OrdenTrabajo { get; private set; }
        public Mecanico? Mecanico { get; private set; }

        private OrdenTrabajoMecanico() { }

        public static OrdenTrabajoMecanico Crear(int ordenTrabajoId, int mecanicoId, DateTime? fechaAsignacion = null)
        {
            return new OrdenTrabajoMecanico
            {
                OrdenTrabajoId = ordenTrabajoId,
                MecanicoId = mecanicoId,
                FechaAsignacion = fechaAsignacion ?? DateTime.UtcNow
            };
        }
    }
}
