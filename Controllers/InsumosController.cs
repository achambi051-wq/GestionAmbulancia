using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ambulancia_MIS.Data;
using Ambulancia_MIS.Dominio;
using Ambulancia_MIS.DTOs;

namespace Ambulancia_MIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsumosController : ControllerBase
    {
        private readonly AmbulanciaContext _context;

        public InsumosController(AmbulanciaContext context)
        {
            _context = context;
        }

        // GET: api/Insumos
        [HttpGet]
        public async Task<IActionResult> GetInsumos()
        {
            var insumos = await _context.Insumos
                .Where(i => i.Estado == "ACTIVO")
                .Select(i => new InsumoDTO
                {
                    Codigo = i.Codigo,
                    Nombre = i.Nombre,
                    Categoria = i.Categoria,
                    UnidadMedida = i.UnidadMedida,
                    PrecioUnitario = i.PrecioUnitario
                })
                .ToListAsync();

            return Ok(insumos);
        }

        // GET: api/Insumos/INS-ADR-01
        [HttpGet("{codigo}")]
        public async Task<IActionResult> GetInsumoByCodigo(string codigo)
        {
            var insumo = await _context.Insumos
                .Where(i => i.Codigo == codigo && i.Estado == "ACTIVO")
                .Select(i => new InsumoDTO
                {
                    Codigo = i.Codigo,
                    Nombre = i.Nombre,
                    Categoria = i.Categoria,
                    UnidadMedida = i.UnidadMedida,
                    PrecioUnitario = i.PrecioUnitario
                })
                .FirstOrDefaultAsync();

            if (insumo == null) return NotFound();
            return Ok(insumo);
        }

        // POST: api/Insumos
        [HttpPost]
        public async Task<IActionResult> CreateInsumo(InsumoDTO insumoDto)
        {
            var existe = await _context.Insumos.AnyAsync(i => i.Codigo == insumoDto.Codigo);
            if (existe) return BadRequest("El código ya existe");

            var insumo = new Insumo
            {
                Codigo = insumoDto.Codigo,
                Nombre = insumoDto.Nombre,
                Categoria = insumoDto.Categoria,
                UnidadMedida = insumoDto.UnidadMedida,
                PrecioUnitario = insumoDto.PrecioUnitario,
                Estado = "ACTIVO"
            };

            _context.Insumos.Add(insumo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInsumoByCodigo), new { codigo = insumo.Codigo }, insumoDto);
        }

        // DELETE: api/Insumos/INS-ADR-01 (Soft Delete)
        [HttpDelete("{codigo}")]
        public async Task<IActionResult> DeleteInsumo(string codigo)
        {
            var insumo = await _context.Insumos.FirstOrDefaultAsync(i => i.Codigo == codigo);
            if (insumo == null) return NotFound();

            insumo.Estado = "INACTIVO";
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}