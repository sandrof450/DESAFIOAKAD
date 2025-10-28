using MicroServicoVendas.Aplication.DTOs;
using MicroServicoVendas.Aplication.Implementations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroServicoVendas.Aplication.Services.Interfaces;
using MicroServicoVendas.Domain.Models;

namespace MicroServicoVendas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidoController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;

        public PedidoController(PedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        [HttpPost]
        // [Authorize(Roles = "Admin, Editor")]
        public async Task<IActionResult> Create([FromBody] PedidoDTO pedidoDTO)
        {
            try
            {
                var result = await _pedidoService.CreatePedidoAsync(pedidoDTO);
                return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while saving the Pedido.", details = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Editor, Viewer")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var listaPedidos = await _pedidoService.GetPedidosAsync();
                return Ok(listaPedidos);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the Pedido.", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Editor, Viewer")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var pedido = await _pedidoService.GetPedidoAsync(id);
                return Ok(pedido);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the Pedido.", details = ex.Message });
            }
        }

        [HttpGet("GetListCountVendasMensaisByProduto")]
        [Authorize(Roles = "Admin, Editor, Viewer")]
        public async Task<IActionResult> GetListCountVendasMensaisByProdutoAsync(int ProdutoId)
        {
            try
            {
                var listCountVendasMensaisConsumer = await _pedidoService.GetListCountVendasMensaisByProdutoAsync(ProdutoId);
                return Ok(listCountVendasMensaisConsumer);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the Pedido.", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Update(int id, [FromBody] Pedido pedido)
        {
            try
            {
                var result = await _pedidoService.UpdatePedidoAsync(id, pedido);
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while update the Pedido.", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _pedidoService.DeletePedidoAsync(id);
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while delete the Pedido.", details = ex.Message });
            }
        }

    }
}