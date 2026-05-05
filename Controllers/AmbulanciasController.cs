using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ambulancia_MIS.Data;
using Ambulancia_MIS.Dominio;
using Ambulancia_MIS.DTOs;

namespace Ambulancia_MIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmbulanciasController : ControllerBase
    {
        private readonly AmbulanciaContext _context;

        public AmbulanciasController(AmbulanciaContext context)
        {
            _context = context;
        }

        // GET: api/Ambulancias - Listado general de ambulancias activas
        [HttpGet]
        public async Task<IActionResult> GetAmbulancias()
        {
            var ambulancias = await _context.Ambulancias
                .Where(a => a.Estado == "ACTIVO")
                .Select(a => new AmbulanciaDTO
                {
                    Codigo = a.Codigo,
                    Placa = a.Placa,
                    Tipo = a.Tipo,
                    Marca = a.Marca,
                    Modelo = a.Modelo,
                    AnioFabricacion = a.AnioFabricacion,
                    CapacidadOxigenoLitros = a.CapacidadOxigenoLitros,
                    TieneDesfibrilador = a.TieneDesfibrilador,
                    Kilometraje = a.Kilometraje,
                    Estado = a.Estado
                })
                .ToListAsync();

            return Ok(ambulancias);
        }

        // GET: api/Ambulancias/AMB-001 - Buscar por código
        [HttpGet("{codigo}")]
        public async Task<IActionResult> GetAmbulanciaByCodigo(string codigo)
        {
            var ambulancia = await _context.Ambulancias
                .Where(a => a.Codigo == codigo && a.Estado == "ACTIVO")
                .Select(a => new AmbulanciaDTO
                {
                    Codigo = a.Codigo,
                    Placa = a.Placa,
                    Tipo = a.Tipo,
                    Marca = a.Marca,
                    Modelo = a.Modelo,
                    AnioFabricacion = a.AnioFabricacion,
                    CapacidadOxigenoLitros = a.CapacidadOxigenoLitros,
                    TieneDesfibrilador = a.TieneDesfibrilador,
                    Kilometraje = a.Kilometraje,
                    Estado = a.Estado
                })
                .FirstOrDefaultAsync();

            if (ambulancia == null) return NotFound();
            return Ok(ambulancia);
        }

        // POST: api/Ambulancias
        [HttpPost]
        public async Task<IActionResult> CreateAmbulancia(AmbulanciaDTO ambulanciaDto)
        {
            var existe = await _context.Ambulancias.AnyAsync(a => a.Codigo == ambulanciaDto.Codigo);
            if (existe) return BadRequest("El código ya existe");

            var ambulancia = new Ambulancia
            {
                Codigo = ambulanciaDto.Codigo,
                Placa = ambulanciaDto.Placa,
                Tipo = ambulanciaDto.Tipo,
                Marca = ambulanciaDto.Marca,
                Modelo = ambulanciaDto.Modelo,
                AnioFabricacion = ambulanciaDto.AnioFabricacion,
                CapacidadOxigenoLitros = ambulanciaDto.CapacidadOxigenoLitros,
                TieneDesfibrilador = ambulanciaDto.TieneDesfibrilador,
                Kilometraje = ambulanciaDto.Kilometraje,
                Estado = "ACTIVO"
            };

            _context.Ambulancias.Add(ambulancia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAmbulanciaByCodigo), new { codigo = ambulancia.Codigo }, ambulanciaDto);
        }

        // PUT: api/Ambulancias/AMB-001
        [HttpPut("{codigo}")]
        public async Task<IActionResult> UpdateAmbulancia(string codigo, int kilometraje, int capacidadOxigeno)
        {
            var ambulancia = await _context.Ambulancias
                .FirstOrDefaultAsync(a => a.Codigo == codigo && a.Estado == "ACTIVO");

            if (ambulancia == null) return NotFound();

            ambulancia.Kilometraje = kilometraje;
            ambulancia.CapacidadOxigenoLitros = capacidadOxigeno;
            await _context.SaveChangesAsync();

            return Ok(new { ambulancia.Codigo, ambulancia.Kilometraje, ambulancia.CapacidadOxigenoLitros });
        }

        // DELETE: api/Ambulancias/AMB-001 (Soft Delete)
        [HttpDelete("{codigo}")]
        public async Task<IActionResult> DeleteAmbulancia(string codigo)
        {
            var ambulancia = await _context.Ambulancias.FirstOrDefaultAsync(a => a.Codigo == codigo);
            if (ambulancia == null) return NotFound();

            ambulancia.Estado = "INACTIVO";
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
