using MicroServicoEstoque.Models;
using MicroServicoEstoque.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicoEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly ProdutoService _produtoService;

        public ProdutoController(ProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        [HttpPost]    
        [Authorize(Roles = "Admin, Editor")]    
        public async Task<IActionResult> Create([FromBody] Produto produto)
        {
            try
            {
                var result = await _produtoService.CreateProduto(produto);
                return CreatedAtAction(nameof(Create), new { id = produto.Id }, result);
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
                return StatusCode(500, new { message = "An error occurred while saving the Produto.", details = ex.Message });
            }
        }

        [HttpGet("GetProdutoPorId/{id}")]
        [Authorize(Roles = "Admin, Editor")]
        public IActionResult GetProdutoPorId(int id)
        {
            try
            {
                var result = _produtoService.GetProdutoPorId(id);
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
                return StatusCode(500, new { message = "An error occurred while retrieving the Pedido.", details = ex.Message });
            }
        }

        [HttpGet("GetProdutoAsName/{name}")]
        [Authorize(Roles = "Admin, Editor")]
        public async Task<IActionResult> GetProdutoAsName(string name)
        {
            try
            {
                var result = await _produtoService.GetProdutoAsName(name);
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
                return StatusCode(500, new { message = "An error occurred while retrieving the Produto.", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Editor")]
        public IActionResult Update(int id, Produto produto)
        {
            try
            {
                var result = _produtoService.UpdateProduto(id, produto);
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
                return StatusCode(500, new { message = "An error occurred while update the produto.", details = ex.Message });
            }
        }
    }
}