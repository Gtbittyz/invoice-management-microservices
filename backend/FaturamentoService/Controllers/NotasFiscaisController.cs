using FaturamentoService.Data;
using FaturamentoService.Middleware;
using FaturamentoService.Models;
using FaturamentoService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FaturamentoService.Controllers;

[ApiController]
[Route("api/notas-fiscais")]
public class NotasFiscaisController : ControllerBase
{
    private readonly FaturamentoDbContext _db;
    private readonly IEstoqueClient _estoqueClient;
    private readonly ILogger<NotasFiscaisController> _logger;

    public NotasFiscaisController(FaturamentoDbContext db, IEstoqueClient estoqueClient, ILogger<NotasFiscaisController> logger)
    {
        _db = db;
        _logger = logger;
        _estoqueClient = estoqueClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotaFiscalDto>>> GetAll()
    {
        var notas = await _db.NotasFiscais
            .Include(n => n.Itens)
            .OrderByDescending(n => n.Numero)
            .Select(n => ToDto(n))
            .ToListAsync();

        return Ok(notas);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NotaFiscalDto>> GetById(int id)
    {
        var nota = await _db.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id)
            ?? throw new KeyNotFoundException($"Nota fiscal {id} não encontrada");

        return Ok(ToDto(nota));
    }

    [HttpPost]
    public async Task<ActionResult<NotaFiscalDto>> Create(NotaFiscalCreateDto dto, CancellationToken ct)
    {
        if (dto.Itens is null || dto.Itens.Count == 0)
        {
            throw new InvalidOperationException("A nota fiscal precisa ter ao menos um produto");
        }

        foreach (var item in dto.Itens)
        {
            var produtoEstoque = await _estoqueClient.ObterPorIdAsync(item.ProdutoId, ct);

            if (produtoEstoque is null)
            {
                throw new InvalidOperationException($"Produto '{item.ProdutoDescricao}' (ID: {item.ProdutoId}) não encontrado no estoque.");
            }

            if (produtoEstoque.Saldo < item.Quantidade)
            {
                throw new InvalidOperationException(
                    $"Saldo insuficiente para o produto '{item.ProdutoDescricao}'. Disponível: {produtoEstoque.Saldo}, Solicitado: {item.Quantidade}."
                );
            }
        }

        var proximoNumero = await _db.NotasFiscais.AnyAsync(ct)
            ? await _db.NotasFiscais.MaxAsync(n => n.Numero, ct) + 1
            : 1;

        var nota = new NotaFiscal
        {
            Numero = proximoNumero,
            Status = StatusNotaFiscal.Aberta,
            CriadaEm = DateTime.UtcNow,
            Itens = dto.Itens.Select(i => new ItemNotaFiscal
            {
                ProdutoId = i.ProdutoId,
                ProdutoCodigo = i.ProdutoCodigo,
                ProdutoDescricao = i.ProdutoDescricao,
                Quantidade = i.Quantidade
            }).ToList()
        };

        _db.NotasFiscais.Add(nota);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = nota.Id }, ToDto(nota));
    }

    [HttpPost("{id:int}/imprimir")]
    public async Task<ActionResult<NotaFiscalDto>> Imprimir(int id, CancellationToken ct)
    {
        var nota = await _db.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new KeyNotFoundException($"Nota Fiscal {id} não encontrada");

        if (nota.Status != StatusNotaFiscal.Aberta)
        {
            throw new InvalidOperationException("Só é possível imprimir notas fiscais com status aberta");
        }

        var itensBaixados = new List<ItemNotaFiscal>();

        foreach (var item in nota.Itens)
        {
            var resultado = await _estoqueClient.DarBaixaAsync(item.ProdutoId, item.Quantidade, ct);

            if (!resultado.Sucesso)
            {
                _logger.LogWarning("Falha ao baixar item da nota {Numero}: {Mensagem}", nota.Numero, resultado.Mensagem);

                foreach (var jaBaixado in itensBaixados)
                {
                    await _estoqueClient.EstornarAsync(jaBaixado.ProdutoId, jaBaixado.Quantidade, ct);
                }
                throw new InvalidOperationException(resultado.Mensagem ?? "Não foi possível dar baixa no estoque");
            }
            itensBaixados.Add(item);
        }

        nota.Status = StatusNotaFiscal.Fechada;
        nota.ImpressaEm = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(ToDto(nota));
    }

    private static NotaFiscalDto ToDto(NotaFiscal n) => new(
        n.Id,
        n.Numero,
        n.Status,
        n.CriadaEm,
        n.ImpressaEm,
        n.Itens.Select(i => new ItemNotaFiscalDto(i.ProdutoId, i.ProdutoCodigo, i.ProdutoDescricao, i.Quantidade)).ToList()
    );
}