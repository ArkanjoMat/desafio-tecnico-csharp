namespace DesafioTecnico;

public record Venda(string Vendedor, decimal Valor);
public record DadosVendas(List<Venda> Vendas);
public record Produto(int CodigoProduto, string DescricaoProduto, int Estoque);
public record DadosEstoque(List<Produto> Estoque);
public enum TipoMovimentacao { Entrada = 1, Saida = 2 }
public record Movimentacao(long Id, int CodigoProduto, TipoMovimentacao Tipo,
    string Descricao, int Quantidade, int EstoqueFinal);
public record ResultadoJuros(int DiasAtraso, decimal Juros, decimal Total);

public static class Comissoes
{
    public static decimal CalcularVenda(decimal valor)
    {
        if (valor < 0) throw new ArgumentException("O valor da venda não pode ser negativo.");
        decimal taxa = valor < 100m ? 0m : valor < 500m ? 0.01m : 0.05m;
        return decimal.Round(valor * taxa, 2, MidpointRounding.AwayFromZero);
    }

    public static Dictionary<string, decimal> CalcularTotais(IEnumerable<Venda> vendas)
    {
        var totais = new Dictionary<string, decimal>();
        foreach (var venda in vendas)
        {
            if (string.IsNullOrWhiteSpace(venda.Vendedor))
                throw new ArgumentException("Venda sem nome de vendedor.");
            totais.TryGetValue(venda.Vendedor, out decimal acumulado);
            totais[venda.Vendedor] = acumulado + CalcularVenda(venda.Valor);
        }
        return totais;
    }
}

public sealed class ControleEstoque
{
    private readonly Dictionary<int, Produto> produtos;
    private readonly List<Movimentacao> historico = new();
    private long proximoId = 1;

    public ControleEstoque(IEnumerable<Produto> estoqueInicial)
    {
        produtos = new();
        foreach (var produto in estoqueInicial)
        {
            if (produto.Estoque < 0 || string.IsNullOrWhiteSpace(produto.DescricaoProduto))
                throw new ArgumentException("Produto com descrição ou estoque inválido.");
            if (!produtos.TryAdd(produto.CodigoProduto, produto))
                throw new ArgumentException("Código de produto duplicado.");
        }
    }

    public IEnumerable<Produto> Produtos => produtos.Values.OrderBy(p => p.CodigoProduto);
    public IReadOnlyList<Movimentacao> Historico => historico.AsReadOnly();

    public Movimentacao Movimentar(int codigo, TipoMovimentacao tipo, int quantidade, string descricao)
    {
        if (!produtos.TryGetValue(codigo, out var produto))
            throw new ArgumentException("Produto não encontrado.");
        if (tipo != TipoMovimentacao.Entrada && tipo != TipoMovimentacao.Saida)
            throw new ArgumentException("Tipo de movimentação inválido.");
        if (quantidade <= 0) throw new ArgumentException("A quantidade deve ser positiva.");
        if (string.IsNullOrWhiteSpace(descricao)) throw new ArgumentException("Informe a descrição.");
        if (tipo == TipoMovimentacao.Saida && quantidade > produto.Estoque)
            throw new ArgumentException("Estoque insuficiente.");

        int saldo = checked(produto.Estoque + (tipo == TipoMovimentacao.Entrada ? quantidade : -quantidade));
        var movimento = new Movimentacao(proximoId, codigo, tipo, descricao.Trim(), quantidade, saldo);
        produtos[codigo] = produto with { Estoque = saldo };
        historico.Add(movimento);
        proximoId++;
        return movimento;
    }
}

public static class CalculadoraJuros
{
    public static ResultadoJuros Calcular(decimal valor, DateOnly vencimento, DateOnly hoje)
    {
        if (valor < 0) throw new ArgumentException("O valor não pode ser negativo.");
        int dias = Math.Max(0, hoje.DayNumber - vencimento.DayNumber);
        decimal juros = decimal.Round(valor * 0.025m * dias, 2, MidpointRounding.AwayFromZero);
        return new ResultadoJuros(dias, juros, valor + juros);
    }
}
