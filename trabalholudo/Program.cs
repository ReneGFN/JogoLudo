using System;
using System.IO;
public class Peao
{
    private int posicao;
    private int jogador;
    public Peao(int numJogador)
    {
        jogador = numJogador;
        posicao = -1;
    }
    public int PegarPosicao()
    {
        return posicao;
    }
    public void MoverPeao(int andar)
    {
        posicao += andar;
    }
    public void VoltarInicio()
    {
        posicao = -1;



    }
}
public class Jogador
{
    public int numero;
    public string cor;
    public Peao[] peoes;
    public Jogador(int num)
    {
        numero = num;
        cor = EscolherCor(num);
        peoes = new Peao[4];
        for (int i = 0; i < 4; i++)
        {
            peoes[i] = new Peao(num);
        }
    }
    private string EscolherCor(int numero)
    {
        switch (numero)
        {
            case 1: return "Vermelho";
            case 2: return "Azul";
            case 3: return "Amarelo";
            case 4: return "Verde";
            default: return "Não possui cor";
        }
    }
}
public class Tabuleiro
{
    private int[,] casas;
    private int[] casasSeguras;
    public Tabuleiro(int numJogadores)
    {
        casas = new int[numJogadores, 52];
        IniciarCasasSeguras();
    }
    private void IniciarCasasSeguras()
    {
        casasSeguras = new int[] { 0, 8, 13, 21, 26, 34, 39, 47 };
    }
    public bool VerCapturaPeao(int posicao, int jogador)
    {
        foreach (var casaSegura in casasSeguras)
        {
            if (casaSegura == posicao)
            {
                return false;
            }
        }
        return casas[jogador, posicao] != 0;
    }
    public void Captura(int posicao, int jogador)
    {
        casas[jogador, posicao] = 0;
    }
    public void IniciarPeao(int posicao, int jogador)
    {
        casas[jogador, posicao] = 1;
    }
    public int CasaInicial(int jogador)
    {
        return 0;
    }
    public int CasaFinal(int jogador)
    {
        return CasaInicial(jogador) + 51;
    }
}
public class Jogo
{
    private Jogador[] jogadores;
    private Tabuleiro tabuleiro;
    private int jogadorAtual;
    private Random random;
    private bool jogoAtivo;
    private int[] sequenciaSeis;
    private StreamWriter logWriter;
    private int logCounter;
    public Jogo(int numJogadores)
    {
        jogadores = new Jogador[numJogadores];
        for (int i = 0; i < numJogadores; i++)
        {
            jogadores[i] = new Jogador(i + 1);
        }
        tabuleiro = new Tabuleiro(numJogadores);
        jogadorAtual = 0;
        random = new Random();
        jogoAtivo = true;
        sequenciaSeis = new int[numJogadores];
        logWriter = new StreamWriter("log_ludo.txt");
        logCounter = 1;
    }
    public void Iniciar()
    {
        Log("O jogo começou com " + jogadores.Length + " jogadores.");
        while (jogoAtivo)
        {
            ExecutarRodada();
            MudarJogador();
        }
        logWriter.Close();
    }
    private void ExecutarRodada()
    {
        int resultadoDado;
        int quantSeis = 0;
        do
        {
            resultadoDado = LancarDado();
            Log("O jogador " + jogadores[jogadorAtual].cor + " girou e tirou " + resultadoDado + " no dado.");
            if (resultadoDado == 6)
            {
                quantSeis++;
                sequenciaSeis[jogadorAtual]++;
                if (sequenciaSeis[jogadorAtual] == 3)
                {
                    Log("O jogador " + jogadores[jogadorAtual].cor + " tirou 6 três vezes seguidas e perdeu sua vez.");
                    sequenciaSeis[jogadorAtual] = 0;
                    break;
                }
                Peao peao = EscolherPeaoEntrar();
                if (peao != null)
                {
                    peao.MoverPeao(1);
                    Log("O Peão do jogador " + jogadores[jogadorAtual].cor + " entrou no tabuleiro.");
                }
                else
                {
                    MoverPeao(resultadoDado);
                }
            }
            else
            {
                sequenciaSeis[jogadorAtual] = 0;
                MoverPeao(resultadoDado);
            }

        } while (resultadoDado == 6 && quantSeis < 3);
    }
    private int LancarDado()
    {
        Console.WriteLine("Jogador " + jogadores[jogadorAtual].cor + ", pressione Enter para lançar o dado.");
        Console.ReadLine();
        return random.Next(1, 7);
    }
    private void MoverPeao(int valorDado)
    {
        Peao peao = EscolherPeaoMover();
        if (peao != null && peao.PegarPosicao() != -1)
        {
            int posAtual = peao.PegarPosicao();
            int novaPos = posAtual + valorDado;
            int posFinal = tabuleiro.CasaFinal(jogadores[jogadorAtual].numero);
            if (novaPos == posFinal)
            {
                peao.MoverPeao(valorDado);
                Log("O peão do jogador " + jogadores[jogadorAtual].cor + " chegou ao fim.");
                if (VerVitoria())
                {
                    Log("Parabéns!!!! O jogador " + jogadores[jogadorAtual].cor + " venceu o jogo!");
                    jogoAtivo = false;
                }
            }
            else if (novaPos < posFinal)
            {
                peao.MoverPeao(valorDado);
                Log("O peão do jogador " + jogadores[jogadorAtual].cor + " moveu-se para a posição " + peao.PegarPosicao() + ".");
            }
            else
            {
                Log("Movimento inválido. Você precisa tirar  " + (posFinal - posAtual) + " para finalizar com o peão.");
                if (!TentarMoverOutroPeao(valorDado))
                {
                    Log("Nenhum movimento válido disponível. Turno perdido.");
                }
            }
        }
        else
        {
            Log("Peão não pode se mover. É necessário tirar 6.");
        }
    }
    private bool TentarMoverOutroPeao(int valorDado)
    {
        foreach (Peao peao in jogadores[jogadorAtual].peoes)
        {
            int novaPosicao = peao.PegarPosicao() + valorDado;
            int posicaoFinal = tabuleiro.CasaFinal(jogadores[jogadorAtual].numero);

            if (peao.PegarPosicao() != -1 && novaPosicao < posicaoFinal)
            {
                peao.MoverPeao(valorDado);
                Log("Peão do jogador " + jogadores[jogadorAtual].cor + " se moveu para a posição " + peao.PegarPosicao() + ".");
                return true;
            }
            else if (peao.PegarPosicao() != -1 && novaPosicao == posicaoFinal)
            {
                Log("Peão do jogador " + jogadores[jogadorAtual].cor + " finalizou.");
                peao.MoverPeao(valorDado);
                if (VerVitoria())
                {
                    Log("Jogador " + jogadores[jogadorAtual].cor + " venceu o jogo!");
                    jogoAtivo = false;
                }
                return true;
            }
        }
        return false;
    }
    private Peao EscolherPeaoEntrar()
    {
        foreach (Peao peao in jogadores[jogadorAtual].peoes)
        {
            if (peao.PegarPosicao() == -1)
            {
                return peao;
            }
        }
        return null;
    }
    private Peao EscolherPeaoMover()
    {
        foreach (Peao peao in jogadores[jogadorAtual].peoes)
        {
            if (peao.PegarPosicao() != -1)
            {
                return peao;
            }
        }
        return null;
    }
    private bool VerVitoria()
    {
        foreach (Peao peao in jogadores[jogadorAtual].peoes)
        {
            if (peao.PegarPosicao() != tabuleiro.CasaFinal(jogadores[jogadorAtual].numero))
            {
                return false;
            }
        }
        return true;
    }
    private void MudarJogador()
    {
        jogadorAtual = (jogadorAtual + 1) % jogadores.Length;
    }
    private void Log(string mensagem)
    {
        logWriter.WriteLine("Log " + logCounter++ + ": " + mensagem);
        Console.WriteLine(mensagem);
    }
}
class Program
{
    static void Main(string[] args)
    {
        int numJogadores;
        do
        {
            Console.WriteLine("Digite o número de jogadores (2, 3 ou 4):");
            numJogadores = int.Parse(Console.ReadLine());
        }
        while (numJogadores < 2 || numJogadores > 4);

        Jogo jogo = new Jogo(numJogadores);
        jogo.Iniciar();
    }
}
