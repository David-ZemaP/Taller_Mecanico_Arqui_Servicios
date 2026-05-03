using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Infrastructure.Persistence;
using Taller_Mecanico_Arqui.Infrastructure.Services;

namespace Taller_Mecanico_Arqui.Infrastructure.RepositoryCreators
{
    public class OrdenTrabajoRepositoryCreator : RepositoryCreator<OrdenTrabajo>
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly SqlEntityQueryService _queryService;
        private readonly AuthenticationHelper _authHelper;

        public OrdenTrabajoRepositoryCreator(ISqlConnectionFactory connectionFactory, SqlEntityQueryService queryService, AuthenticationHelper authHelper)
        {
            _connectionFactory = connectionFactory;
            _queryService = queryService;
            _authHelper = authHelper;
        }

        public override IRepository<OrdenTrabajo> CreateRepository()
        {
            return new Infrastructure.Persistence.Repositories.OrdenTrabajoRepository(
                _connectionFactory,
                _queryService,
                _authHelper);
        }
    }
}