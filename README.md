# Desafio técnico em C#

Aplicação de console com cálculo de comissões, movimentações de estoque e cálculo de juros. Os arquivos JSON contêm os dados fornecidos no enunciado. Não há dependências NuGet externas.

## Executar

Pré-requisito: SDK do .NET 8 instalado (ou SDK posterior com suporte ao alvo net8.0 e runtime .NET 8 disponível).

Abra um terminal na pasta que contém `DesafioTecnico.csproj`:

```sh
dotnet build
dotnet run
```

O menu oferece os três exercícios e a consulta ao histórico do estoque. Para executar os testes das regras de negócio:

```sh
dotnet run -- --test
```

Os testes usam verificações próprias, sem framework externo, e retornam código de saída 1 em caso de falha. Cobrem faixas e limites da comissão, agrupamento por vendedor, arredondamento, entradas e saídas, saldo insuficiente, dados inválidos, identificadores e datas de vencimento.

## Decisões adotadas

- Dinheiro é representado por `decimal`. Comissões são calculadas e arredondadas por venda, para duas casas decimais, com `MidpointRounding.AwayFromZero`, antes da soma por vendedor. O enunciado não define o momento do arredondamento; esta é uma escolha explícita.
- As faixas são: valor menor que R$ 100,00 = 0%; de R$ 100,00 até menos de R$ 500,00 = 1%; a partir de R$ 500,00 = 5%.
- Cada movimentação possui número sequencial único durante a execução, código do produto, tipo, descrição, quantidade e saldo final. O saldo e o histórico ficam em memória e são reiniciados ao fechar o programa. Persistência não é exigida no enunciado; os identificadores não são globais entre execuções.
- As saídas não podem exceder o saldo. Quantidades devem ser inteiras e positivas. Operações inválidas não alteram o estoque nem geram histórico.
- O exercício de juros menciona “multa de 2,5% ao dia”, mas não define capitalização. Adota-se juros simples: `juros = valor × 0,025 × dias de atraso`. O total é o principal mais os juros. Não há multa fixa adicional.
- O vencimento hoje ou no futuro não gera juros. A aplicação considera dias corridos e a data local do computador. A função recebe a data atual como parâmetro para permitir testes reproduzíveis.
- Digite valores monetários com vírgula decimal e sem separador de milhar, como `1000,50`. Datas usam `dd/MM/aaaa`.

## Exemplos para conferir

Estoque: a caneta azul (101) começa com 150 unidades. Uma entrada de 10 retorna 160; uma saída posterior de 20 retorna 140.

Juros: R$ 100,00 com 10 dias de atraso gera R$ 25,00 de juros e R$ 125,00 de total.

## Organização

- `Program.cs`: menu, leitura de JSON, entrada e apresentação dos resultados.
- `Servicos.cs`: modelos e regras de negócio, separados da interface de console.
- `Testes.cs`: verificações automatizadas das regras.
- `dados/`: vendas e estoque inicial do enunciado.

## Comissões esperadas

Resultados com arredondamento da comissão por venda:

- João Silva: R$ 495,69
- Maria Souza: R$ 465,96
- Carlos Oliveira: R$ 379,38
- Ana Lima: R$ 404,99
