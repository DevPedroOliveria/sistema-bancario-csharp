using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

List<ContaBancaria> contas = CarregarDados();

while(true)
{
    Console.WriteLine("\n=== Escolha a opção desejada ===");
    Console.WriteLine("1 - Criar Conta");
    Console.WriteLine("2 - Listar Contas");
    Console.WriteLine("3 - Depositar");
    Console.WriteLine("4 - Sacar");
    Console.WriteLine("5 - Ver Saldo");
    Console.WriteLine("6 - Ver Extrato");
    Console.WriteLine("7 - Sair");

    if (!int.TryParse(Console.ReadLine(), out int opcao))
    {
        Console.WriteLine("Erro => Digite apenas números.");
        continue;
    }

    switch(opcao)
    {
        case 1:
        CriarConta(contas);
        break;
        
        case 2:
        ListarContas(contas);
        break;
        
        case 3:
        FazerDeposito(contas);
        break;
        
        case 4:
        FazerSaque(contas);
        break;
        
        case 5:
        VerSaldo(contas);
        break;
        
        case 6:
        VerExtrato(contas);
        break;

        case 7:
        Console.WriteLine("Encerrando o sistema...");
        return;

        default:
        Console.WriteLine("Erro => Opção inválida!");
        break;

    }
}

//criar conta
static void CriarConta(List<ContaBancaria> contas)
{
    ContaBancaria novaConta = new ContaBancaria();

    Console.Write("\nNome do Titular: ");
    novaConta.Titular = Console.ReadLine() ?? string.Empty;

    Console.Write("Número da Conta: ");
    if (!int.TryParse(Console.ReadLine(), out int numeroConta))
    {
        Console.WriteLine("Erro => Número da conta inválido.");
        return;
    }

    // valida duplicidade (melhoria importante)
    if (contas.Exists(c => c.numeroConta == numeroConta))
    {
        Console.WriteLine("Erro => Número de conta já existe.");
        return;
    }

    novaConta.numeroConta = numeroConta;

    contas.Add(novaConta);
    SalvarDados(contas);

    Console.WriteLine("\nSucesso => Conta cadastrada!\n");
}

static void ListarContas(List<ContaBancaria> contas)
{
    if(contas.Count == 0)
    {
        Console.WriteLine("Nenhuma Conta Cadastrada");
        return;
    }

    for (int i = 0; i < contas.Count; i++)
    {
        Console.WriteLine($"\n--- Dados da Conta [{i + 1}] ---");
        Console.WriteLine($"Titular: {contas[i].Titular}");
        Console.WriteLine($"Numero da Conta: {contas[i].numeroConta}");
        Console.WriteLine($"Saldo: R$ {contas[i].Saldo:F2}\n");
    }
}

static ContaBancaria? BuscarConta(List<ContaBancaria> contas)
{
    Console.WriteLine("\nDigite o número da conta: ");

    if (!int.TryParse(Console.ReadLine(), out int numeroConta))
    {
        Console.WriteLine("Erro => Número da conta inválido.");
        return null;
    }

    ContaBancaria? conta = contas.Find(c => c.numeroConta == numeroConta);

    if(conta == null)
    {
        Console.WriteLine("Erro => Conta não encontrada.");
        return null;        
    }

    return conta;
}

static void FazerDeposito(List<ContaBancaria> contas)
{

    ContaBancaria? conta = BuscarConta(contas);

    if (conta == null)
    return;

    Console.WriteLine("\nDigite o valor do depósito: ");
    
    if( !decimal.TryParse(Console.ReadLine(), out decimal valor))
    {
        Console.WriteLine("Erro => Valor inválido.");
        return;
    }

    if (!conta.Depositar(valor))
    {
        Console.WriteLine("Erro => Valor deve ser maior que zero.");
        return;
    }

    SalvarDados(contas);

    Console.WriteLine("\nDepósito realizado com sucesso");
    Console.WriteLine($"Saldo atualizado: R$ {conta.Saldo:F2}\n");
    
}

static void FazerSaque(List<ContaBancaria> contas)
{    
    ContaBancaria? conta = BuscarConta(contas);

    if (conta == null)
        return;

    Console.WriteLine("\nDigite o valor do saque: ");
    
    if( !decimal.TryParse(Console.ReadLine(), out decimal valor))
    {
        Console.WriteLine("Valor inválido.");
        return;
    }
    
    if(!conta.Sacar(valor))
    {
        Console.WriteLine("\nErro => Saque inválido ou saldo insuficiente.");
        return;
    }

    SalvarDados(contas);

    Console.WriteLine("\nSucesso => Saque realizado!");
    Console.WriteLine($"Saldo atualizado: R$ {conta.Saldo:F2}\n");
}

static void VerSaldo(List<ContaBancaria> contas)
{
    ContaBancaria? conta = BuscarConta(contas);

    if(conta == null)
    return;

    Console.WriteLine($"\nSaldo Atual: R$ {conta.Saldo:F2}\n");
}

static void VerExtrato(List<ContaBancaria> contas)
{
    ContaBancaria? conta = BuscarConta(contas);

    if (conta == null)
        return;

    if (conta.Transacoes.Count == 0)
    {
        Console.WriteLine("Nenhuma movimentação registrada.");
        return;
    }

    Console.WriteLine("\nFiltrar por tipo:");
    Console.WriteLine("1 - Todos");
    Console.WriteLine("2 - Depósito");
    Console.WriteLine("3 - Saque");

    if(!int.TryParse(Console.ReadLine(), out int filtroTipo))
    {
        Console.WriteLine("Erro => Opção inválida");
        return;
    }

    Console.WriteLine("\nFiltrar por período:");
    Console.WriteLine("1 - Todos");
    Console.WriteLine("2 - Hoje");
    Console.WriteLine("3 - Últimos 7 dias");

    if(!int.TryParse(Console.ReadLine(), out int filtroPeriodo))
    {
        Console.WriteLine("Erro => Opção inválida");
    }

    IEnumerable<Transacao> lista = conta.Transacoes;

    switch(filtroTipo)
    {
        case 2:
        lista = lista.Where(t => t.Tipo == "Depósito");
        break;
    
        case 3:
        lista = lista.Where(t => t.Tipo == "Saque");
        break;
    }


    switch(filtroPeriodo)
    {
        case 2:
        lista = lista.Where(t => t.Data.Date == DateTime.Now.Date);
        break;
    
        case 3:
        lista = lista.Where(t => t.Data >= DateTime.Now.AddDays(-7));
        break;
    }

    lista = lista.OrderByDescending(t => t.Data);

    Console.WriteLine("\n=== EXTRATO ===\n");
    Console.WriteLine("Data                | Tipo       | Valor");
    Console.WriteLine("------------------------------------------------");

    foreach (var t in lista)
    {
        string sinal = t.Tipo == "Depósito" ? "+" : "-";

        Console.WriteLine($"{t.Data:dd/MM/yyyy HH:mm}   | {t.Tipo,-10} | {sinal}R$ {t.Valor:F2}");
    }

    Console.WriteLine("------------------------------------------------");
    Console.WriteLine($"Saldo atual: R$ {conta.Saldo:F2}\n");
}

static void SalvarDados(List<ContaBancaria> contas)
{
    string Json= JsonSerializer.Serialize(contas, new JsonSerializerOptions
    {
        WriteIndented = true
    });

    File.WriteAllText("contas.json", Json);
}

static List<ContaBancaria> CarregarDados()
{
    if(!File.Exists("contas.json"))
        return new List<ContaBancaria>();

    string json = File.ReadAllText("contas.json");

    return JsonSerializer.Deserialize<List<ContaBancaria>>(json)
        ?? new List<ContaBancaria>();
}
class ContaBancaria
{
    public string Titular {get; set;} = string.Empty;
    public int numeroConta {get; set;}
    public decimal Saldo {get; set;}

    public List<Transacao> Transacoes { get; set; } = new List<Transacao>();
    public bool Depositar(decimal valor)
    {
        if(valor <= 0)
            return false;
        
        Saldo += valor;

        Transacoes.Add(new Transacao
        {
            Tipo = "Depósito",
            Valor = valor,
            Data = DateTime.Now
        });

        return true;
    }

    public bool Sacar(decimal valor)
    {
        if(valor <= 0 || valor > Saldo)
            return false;
        
        Saldo -= valor;

        Transacoes.Add(new Transacao
        {
            Tipo = "Saque",
            Valor = valor,
            Data = DateTime.Now
        });

        return true;
    }
}

class Transacao
{
    public int Id { get; set; }
    public string Tipo{get; set;} = string.Empty; //"Depósito" ou "Saque"
    public decimal Valor{get; set;}
    public DateTime Data{get; set;}
}