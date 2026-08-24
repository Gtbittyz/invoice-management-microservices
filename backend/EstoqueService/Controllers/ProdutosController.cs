using EstoqueService.Data;
using EstoqueService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly EstoqueDbContext _db;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(EstoqueDbContext db, ILogger<ProdutosController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetAll([FromQuery] bool somenteComSaldo = false)
    {
        var query = _db.Produtos.AsQueryable();

        if (somenteComSaldo)
        {
            query = query.Where(p => p.Saldo > 0);
        }

        var produtos = await query
            .OrderBy(p => p.Codigo)
            .Select(p => new ProdutoDto(p.Id, p.Codigo, p.Descricao, p.Saldo))
            .ToListAsync();
        return Ok(produtos);

    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoDto>> GetById(int id)
    {
        var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id)
               ?? throw new KeyNotFoundException($"Produto {id} não encontrado.");

        return Ok(new ProdutoDto(produto.Id, produto.Codigo, produto.Descricao, produto.Saldo));
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoDto>> Create(ProdutoCreateDto dto)
    {
        var codigoDuplicado = await _db.Produtos.AnyAsync(p => p.Codigo == dto.Codigo);
        if (codigoDuplicado)
        {
            throw new InvalidOperationException($"Ja existe um produto com o codigo'{dto.Codigo}'.");
        }

        var produto = new Produto
        {
            Codigo = dto.Codigo,
            Descricao = dto.Descricao,
            Saldo = dto.Saldo,
        };
        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync();

        var result = new ProdutoDto(produto.Id, produto.Codigo, produto.Descricao, produto.Saldo);
        return CreatedAtAction(nameof(GetById), new { id = produto.Id }, result);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProdutoUpdateDto dto)
    {
        var produto = await _db.Produtos.FindAsync(id)
            ?? throw new KeyNotFoundException($"Produto{id} não encontrado.");
        produto.Descricao = dto.Descricao;
        produto.Saldo = dto.Saldo;

        await _db.SaveChangesAsync();
        return NoContent();
    }
    [HttpPost("{id:int}/baixa")]
    public async Task<ActionResult<MovimentoEstoqueResultDto>> DarBaixa(int id, [FromBody] MovimentoEstoqueDto dto)
    {
        var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Produto {id} não encontrado.");

        if (produto.Saldo < dto.Quantidade)
        {
            _logger.LogWarning("Saldo insuficiente para produto {Codigo}: solicitado {Qtd}, disponível {Saldo}",
                produto.Codigo, dto.Quantidade, produto.Saldo);

            return UnprocessableEntity(new MovimentoEstoqueResultDto(
                Sucesso: false,
                Mensagem: $"Saldo insuficiente para o produto {produto.Codigo}. Disponível: {produto.Saldo}, solicitado: {dto.Quantidade}.",
                SaldoAtual: produto.Saldo));
        }

        produto.Saldo -= dto.Quantidade;
        await _db.SaveChangesAsync();

        return Ok(new MovimentoEstoqueResultDto(true, null, produto.Saldo));
    }

    [HttpPost("{id:int}/estorno")]
    public async Task<ActionResult<MovimentoEstoqueResultDto>> Estornar(int id, [FromBody] MovimentoEstoqueDto dto)
    {
        var produto = await (_db.Produtos.FirstOrDefaultAsync(p => p.Id == id))
            ?? throw new KeyNotFoundException($"Produto {id} não encontrado.");
        produto.Saldo += dto.Quantidade;
        await _db.SaveChangesAsync();

        return Ok(new MovimentoEstoqueResultDto(true, null, produto.Saldo));

    }
}