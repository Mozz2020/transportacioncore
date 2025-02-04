﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportationCore.CustomError;
using TransportationCore.Data;
using TransportationCore.Data.Dtos.Operador;
using TransportationCore.Data.Interfaces;
using TransportationCore.Data.Models;
using TransportationCore.Data.Utilidades;
using TransportationCore.Enumeradores;

namespace TransportationCore.Controllers
{
    // ////[Authorize(Policy = "EditarDirectoresPolicy", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class DirectorController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly int tipoEmpleado;
        private readonly int segmentoEmpleado;
        private readonly IMapper mapper;
        private readonly ISoftDeleteService softDeleteService;

        public DirectorController(ApplicationDbContext context, IMapper mapper, ISoftDeleteService softDeleteService)
        {
            _context = context;
            this.mapper = mapper;
            this.softDeleteService = softDeleteService;
            this.tipoEmpleado = (int)EnumTipoEmpleado.Director;
            this.segmentoEmpleado = (int)EnumSegmentoEmpleado.Interno;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<OperadorConsultarDto>>> GetDirectores()
        {
            if (_context.Empleados == null)
            {
                return NotFound();
            }

            var operador = await _context.Empleados.Include(x => x.Municipio).Include(x => x.Municipio.Estado)
                                          .Where(x => x.IdTipoEmpleado == tipoEmpleado).ToListAsync();

            var operadorDto = mapper.Map<List<OperadorConsultarDto>>(operador);

            return operadorDto;
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<OperadorConsultarDto>> GetDirector(long id)
        {
            if (_context.Empleados == null)
            {
                return NotFound();
            }
            var operador = await _context.Empleados.Include(x => x.Municipio).Include(x => x.Municipio.Estado)
                                                   .FirstAsync(b => b.IdEmpleado == id && b.IdTipoEmpleado == tipoEmpleado);

            if (operador == null)
            {
                return NotFound();
            }

            var operadorDto = mapper.Map<OperadorConsultarDto>(operador);

            return operadorDto;
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutDirector(long id, OperadorActualizarDto operadorDto)
        {
            if (id != operadorDto.IdEmpleado)
            {
                return BadRequest();
            }

            operadorDto.NumeroContrato = 0;

            var operador = mapper.Map<Empleado>(operadorDto);
            operador.IdTipoEmpleado = tipoEmpleado;
            operador.IdSegmento = segmentoEmpleado;

            if (ValidarEmail(operador.IdEmpleado, operador.Correo))
            {
                return BadRequest(new ErrorResponse("El correo ya existe,favor validar."));
            }

            _context.Entry(operador).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OperadorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        [HttpPost]
        public async Task<ActionResult> PostDirector(OperadorCrearDto operadorDto)
        {
            if (_context.Empleados == null)
            {
                return BadRequest(new ErrorResponse("El registro no existe"));
            }

            operadorDto.NumeroContrato = 0;

            var operador = mapper.Map<Empleado>(operadorDto);
            operador.IdTipoEmpleado = tipoEmpleado;
            operador.IdSegmento = segmentoEmpleado;

            if (ValidarEmail(operador.IdEmpleado, operador.Correo))
            {
                return BadRequest(new ErrorResponse("El correo ya existe,favor validar."));
            }

            _context.Empleados.Add(operador);
            await _context.SaveChangesAsync();

            return Ok(operador.IdEmpleado);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDirector(long id)
        {
            await softDeleteService.SoftDelete<Empleado>(id);
            return Ok(new ErrorResponse("El registro se elimino correctamente."));
        }

        private bool OperadorExists(long id)
        {
            return (_context.Empleados?.Any(e => e.IdEmpleado == id)).GetValueOrDefault();
        }

        private bool ValidarNumeroContrato(long IdEmleado, decimal nroContrato)
        {
            return (_context.Empleados?.Any(e => e.NumeroContrato == nroContrato && e.IdEmpleado != IdEmleado)).GetValueOrDefault();
        }

        private bool ValidarEmail(long IdEmpleado, string email)
        {

            if (IdEmpleado != 0)
                return (_context.Empleados?.Any(e => e.Correo.Trim().ToUpper() == email.Trim().ToUpper()
                                               && e.IdEmpleado != IdEmpleado)).GetValueOrDefault();
            else
                return (_context.Empleados?.Any(e => e.Correo.Trim().ToUpper() == email.Trim().ToUpper()))
                       .GetValueOrDefault();
        }
    }
}
