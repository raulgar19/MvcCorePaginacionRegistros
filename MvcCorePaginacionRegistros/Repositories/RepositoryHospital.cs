using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MvcCorePaginacionRegistros.Data;
using MvcCorePaginacionRegistros.Models;
using System.Data;

namespace MvcCorePaginacionRegistros.Repositories
{
    #region VIEWS AND PROCEDURES
    //create or alter view V_DEPARTAMENTOS_INDIVIDUAL
    //as
    //    select cast(
    //    ROW_NUMBER() over (order by DEPT_NO) as int) 
    //    as POSICION, DEPT_NO, DNOMBRE,LOC
    //    from  DEPT
    //go

    //create view V_GRUPO_EMPLEADOS
    //as
    //    select cast(ROW_NUMBER() over (order by APELLIDO) as int)
    //    as POSICION, EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO
    //    from EMP
    //go

    //create procedure SP_GRUPO_DEPARTAMENTOS
    //(@posicion int)
    //as
    //    select DEPT_NO, DNOMBRE, LOC from V_DEPARTAMENTOS_INDIVIDUAL
    //    where POSICION >= @posicion and POSICION < (@posicion + 2)
    //go

    //create procedure SP_GRUPO_EMPLEADOS
    //(@posicion int)
    //as
    //    select EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO
    //    from V_GRUPO_EMPLEADOS
    //    where POSICION >= @posicion and POSICION < (@posicion + 3)
    //go

    //create procedure SP_GRUPO_EMPLEADOS_OFICIO
    //(@posicion int, @oficio nvarchar(50), @registros int out)
    //as
    //    select @registros = count(EMP_NO) from EMP where OFICIO = @oficio
    //    select EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO from
    //    (select cast(ROW_NUMBER() over (order by APELLIDO) as int)
    //    as POSICION, EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO
    //    from EMP
    //    where OFICIO = @oficio) QUERY
    //    where(QUERY.POSICION >= @posicion and QUERY.POSICION<(@posicion + 3))
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

        public async Task<List<Departamento>> GetGrupoDepartamentosAsync(int posicion)
        {
            string sql = "SP_GRUPO_DEPARTAMENTOS @posicion";
            SqlParameter pamPosicion = new SqlParameter("@posicion", posicion);

            var consulta = this.context.Departamentos.FromSqlRaw(sql, pamPosicion);

            return await consulta.ToListAsync();
        }

        public async Task<int> GetEmpleadosCountAsync()
        {
            return await this.context.Empleados.CountAsync();
        }

        public async Task<List<Empleado>> GetGrupoEmpleadosAsync(int posicion)
        {
            string sql = "SP_GRUPO_EMPLEADOS @posicion";
            SqlParameter pamPosicion = new SqlParameter("@posicion", posicion);

            var consulta = this.context.Empleados.FromSqlRaw(sql, pamPosicion);

            return await consulta.ToListAsync();
        }

        public async Task<int> GetEmpleadosOficioCountAsync(string oficio)
        {
            return await this.context.Empleados.Where(z => z.Oficio == oficio).CountAsync();
        }

        public async Task<ModelEmpleadoOficio> GetGrupoEmpleadosOficioAsync(int posicion, string oficio)
        {
            string sql = "SP_GRUPO_EMPLEADOS_OFICIO @posicion, @oficio,@registros out";
            SqlParameter pamPosicion = new SqlParameter("@posicion", posicion);
            SqlParameter pamOficio = new SqlParameter("@oficio", oficio);
            SqlParameter pamRegistros = new SqlParameter("@registros", 0);
            pamRegistros.Direction = ParameterDirection.Output;
            pamRegistros.DbType = DbType.Int32;

            var consulta = this.context.Empleados.FromSqlRaw(sql, pamPosicion, pamOficio, pamRegistros);

            ModelEmpleadoOficio model = new ModelEmpleadoOficio();

            model.Empleados = await consulta.ToListAsync();
            model.NumeroRegistros = (int)pamRegistros.Value;

            return model;
        }
    }
}