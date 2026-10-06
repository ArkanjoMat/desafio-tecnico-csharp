using System.Globalization;
using System.Text;
using System.Text.Json;
using DesafioTecnico;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");

if (args.Contains("--test"))
{
    Environment.ExitCode = Testes.Executar();
    return;
}

try
{
    var vendas = LerJson<DadosVendas>("vendas.json");
    var dadosEstoque = LerJson<DadosEstoque>("estoque.json");
    if (vendas.Vendas is null || dadosEstoque.Estoque is null)
        throw new InvalidDataException("JSON sem a lista de vendas ou estoque.");
    var estoque = new ControleEstoque(dadosEstoque.Estoque);

    while (true)
    {
        Console.WriteLine("\n1 - Comissões\n2 - Movimentar estoque\n3 - Calcular juros\n4 - Histórico de estoque\n0 - Sair");
        Console.Write("Opção: ");
        string? opcao = Console.ReadLine();
        if (opcao is null or "0") break;
        try
        {
            switch (opcao)
            {
                case "1":
                    foreach (var item in Comissoes.CalcularTotais(vendas.Vendas))
                        Console.WriteLine($"{item.Key}: {item.Value:C2}");
                    break;
                case "2":
                    foreach (var produto in estoque.Produtos)
                        Console.WriteLine($"{produto.CodigoProduto} - {produto.DescricaoProduto}: {produto.Estoque} unidades");
                    int codigo = LerInteiro("Código do produto: ");
                    var tipo = (TipoMovimentacao)LerInteiro("Tipo (1 = entrada, 2 = saída): ");
                    int quantidade = LerInteiro("Quantidade: ");
                    string descricao = LerTexto("Descrição da movimentação: ");
                    var movimento = estoque.Movimentar(codigo, tipo, quantidade, descricao);
                    Console.WriteLine($"Movimentação nº {movimento.Id} registrada. Estoque final: {movimento.EstoqueFinal}.");
                    break;
                case "3":
                    string entrada = LerTexto("Valor em reais (ex.: 1000,50; sem separador de milhar): ");
                    if (!decimal.TryParse(entrada, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                        CultureInfo.CurrentCulture, out decimal valor) || decimal.Round(valor, 2) != valor)
                        throw new ArgumentException("Informe um valor em reais com até duas casas decimais e vírgula decimal.");
                    if (!DateOnly.TryParseExact(LerTexto("Vencimento (dd/MM/aaaa): "), "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var vencimento))
                        throw new ArgumentException("Data inválida. Use dd/MM/aaaa.");
                    var resultado = CalculadoraJuros.Calcular(valor, vencimento, DateOnly.FromDateTime(DateTime.Today));
                    Console.WriteLine($"Dias de atraso: {resultado.DiasAtraso}\nJuros: {resultado.Juros:C2}\nTotal: {resultado.Total:C2}");
                    break;
                case "4":
                    if (estoque.Historico.Count == 0) Console.WriteLine("Nenhuma movimentação registrada.");
                    foreach (var m in estoque.Historico)
                        Console.WriteLine($"#{m.Id} | Produto {m.CodigoProduto} | {m.Tipo} | {m.Descricao} | Quantidade: {m.Quantidade} | Saldo: {m.EstoqueFinal}");
                    break;
                default:
                    Console.WriteLine("Opção inválida.");
                    break;
            }
        }
        catch (ArgumentException ex) { Console.WriteLine($"Erro: {ex.Message}"); }
        catch (OverflowException) { Console.WriteLine("Erro: valor acima do limite permitido."); }
    }
}
catch (EndOfStreamException) { Console.WriteLine("\nEntrada encerrada."); }
catch (Exception ex) when (ex is IOException or JsonException or ArgumentException or UnauthorizedAccessException)
{
    Console.Error.WriteLine($"Não foi possível executar: {ex.Message}");
    Environment.ExitCode = 1;
}

static T LerJson<T>(string nome)
{
    string caminho = Path.Combine(AppContext.BaseDirectory, "dados", nome);
    return JsonSerializer.Deserialize<T>(File.ReadAllText(caminho),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidDataException($"Arquivo {nome} vazio ou inválido.");
}

static string LerTexto(string mensagem)
{
    Console.Write(mensagem);
    return Console.ReadLine()?.Trim() ?? throw new EndOfStreamException();
}

static int LerInteiro(string mensagem)
{
    if (!int.TryParse(LerTexto(mensagem), out int valor))
        throw new ArgumentException("Informe um número inteiro válido.");
    return valor;
}
