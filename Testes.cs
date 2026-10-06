namespace DesafioTecnico;

public static class Testes
{
    public static int Executar()
    {
        var casos = new (string Nome, Action Verificar)[]
        {
            ("Faixas de comissão e arredondamento", () =>
            {
                Igual(0m, Comissoes.CalcularVenda(99.99m));
                Igual(1m, Comissoes.CalcularVenda(100m));
                Igual(5m, Comissoes.CalcularVenda(499.99m));
                Igual(25m, Comissoes.CalcularVenda(500m));
                Igual(60.03m, Comissoes.CalcularVenda(1200.50m));
                Rejeita(() => Comissoes.CalcularVenda(-1m));
            }),
            ("Comissão por venda antes de somar por vendedor", () =>
            {
                var totais = Comissoes.CalcularTotais(new[]
                {
                    new Venda("Ana", 250m), new Venda("Ana", 250m), new Venda("Bia", 90m)
                });
                Igual(5m, totais["Ana"]);
                Igual(0m, totais["Bia"]);
            }),
            ("Entradas, saídas e identificadores", () =>
            {
                var estoque = NovoEstoque();
                var entrada = estoque.Movimentar(101, TipoMovimentacao.Entrada, 10, "Compra");
                var saida = estoque.Movimentar(101, TipoMovimentacao.Saida, 160, "Venda");
                Igual(160, entrada.EstoqueFinal);
                Igual(0, saida.EstoqueFinal);
                Igual(1L, entrada.Id);
                Igual(2L, saida.Id);
                Igual(2, estoque.Historico.Count);
            }),
            ("Movimentações inválidas preservam saldo e histórico", () =>
            {
                var estoque = NovoEstoque();
                Rejeita(() => estoque.Movimentar(101, TipoMovimentacao.Saida, 151, "Venda"));
                Rejeita(() => estoque.Movimentar(999, TipoMovimentacao.Entrada, 1, "Compra"));
                Rejeita(() => estoque.Movimentar(101, TipoMovimentacao.Entrada, 0, "Compra"));
                Rejeita(() => estoque.Movimentar(101, TipoMovimentacao.Entrada, -1, "Compra"));
                Rejeita(() => estoque.Movimentar(101, (TipoMovimentacao)3, 1, "Compra"));
                Rejeita(() => estoque.Movimentar(101, TipoMovimentacao.Entrada, 1, " "));
                Igual(150, estoque.Produtos.Single().Estoque);
                Igual(0, estoque.Historico.Count);
            }),
            ("Juros simples, vencimento hoje e futuro", () =>
            {
                var hoje = new DateOnly(2026, 10, 6);
                var atraso = CalculadoraJuros.Calcular(100m, hoje.AddDays(-10), hoje);
                Igual(10, atraso.DiasAtraso);
                Igual(25m, atraso.Juros);
                Igual(125m, atraso.Total);
                Igual(0m, CalculadoraJuros.Calcular(100m, hoje, hoje).Juros);
                Igual(0m, CalculadoraJuros.Calcular(100m, hoje.AddDays(1), hoje).Juros);
                Igual(0.03m, CalculadoraJuros.Calcular(1m, hoje.AddDays(-1), hoje).Juros);
                Rejeita(() => CalculadoraJuros.Calcular(-1m, hoje, hoje));
            }),
            ("Dias de atraso em ano bissexto", () =>
                Igual(2, CalculadoraJuros.Calcular(100m, new DateOnly(2024, 2, 28), new DateOnly(2024, 3, 1)).DiasAtraso))
        };

        int falhas = 0;
        foreach (var caso in casos)
        {
            try { caso.Verificar(); Console.WriteLine($"OK: {caso.Nome}"); }
            catch (Exception ex) { falhas++; Console.WriteLine($"FALHOU: {caso.Nome}: {ex.Message}"); }
        }
        Console.WriteLine($"{casos.Length - falhas}/{casos.Length} grupos de testes passaram.");
        return falhas == 0 ? 0 : 1;
    }

    private static ControleEstoque NovoEstoque() => new(new[] { new Produto(101, "Caneta Azul", 150) });

    private static void Igual<T>(T esperado, T obtido)
    {
        if (!EqualityComparer<T>.Default.Equals(esperado, obtido))
            throw new Exception($"Esperado: {esperado}; obtido: {obtido}");
    }

    private static void Rejeita(Action acao)
    {
        try { acao(); }
        catch (ArgumentException) { return; }
        throw new Exception("A operação inválida deveria ter sido rejeitada.");
    }
}
