using ChessChallenge.API;
using Stockfish.NET;
using System;
using System.Security;

public class Stockfish12 : IChessBot
{
    IStockfish stockfish = new Stockfish.NET.Stockfish(@"C:\Users\jimmy\Downloads\Chess-Challenge_Updated\Chess-Challenge-main\Chess-Challenge-main\Chess-Challenge\src\Stockfish\stockfish_20090216_x64.exe", 5);

    Move bestMove;

    public Move Think(Board board, Timer timer)
    {
        stockfish.SetFenPosition(board.GetFenString());
        bestMove = new Move(stockfish.GetBestMove(), board);

        return bestMove;
    }
   
}

