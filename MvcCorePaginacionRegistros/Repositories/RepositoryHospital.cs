using Microsoft.EntityFrameworkCore;
using MvcCorePaginacionRegistros.Data;
using MvcCorePaginacionRegistros.Models;

namespace MvcCorePaginacionRegistros.Repositories
{
    #region VIEWS
    //create or alter view V_DEPARTAMENTOS_INDIVIDUAL
    //as
    //    select cast(
    //        ROW_NUMBER() over (order by DEPT_NO) as int) 
    //  as POSICION, DEPT_NO, DNOMBRE,LOC
    //        from  DEPT
    //go
    #endregion

    public class RepositoryHospital
    {
        private HospitalContext context;

        public RepositoryHospital(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<int> GetNumeroRegistrosVistaDepartamentosAsync()
        {
            return await context.VistaDepartamentos.CountAsync();
        }

        public async Task<VistaDepartamento> GetVistaDepartamentoAsync(int posicion)
        {
            VistaDepartamento departamento = await context.VistaDepartamentos.Where(z => z.Posicion == posicion).FirstOrDefaultAsync();

            return departamento;
        }

        public async Task<List<VistaDepartamento>> GetGrupoVistaDepartamentoAsync(int posicion)
        {
            var consulta = from datos in this.context.VistaDepartamentos
                           where datos.Posicion >= posicion && datos.Posicion < posicion + 2
                           select datos;

            return await consulta.ToListAsync();
        }
    }
}