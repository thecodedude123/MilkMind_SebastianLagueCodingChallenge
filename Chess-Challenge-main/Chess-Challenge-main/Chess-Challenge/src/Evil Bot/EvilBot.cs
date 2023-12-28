using ChessChallenge.API;
using System;

namespace ChessChallenge.Example
{

    public class EvilBot : IChessBot
    {
        float[] probabilities;
        float[] pieceValues = { 0, 1, 3, 3, 5, 9, 100 };
        int[] maxAttack = { 1, 2, 8, 16, 16, 32, 8};
        float lastEval = 0;
        Move bestMove;
        Move prevMove;
        int maxDepth = 30;
        int currentDepth;
        int breakDepth;
        public Move Think(Board board, Timer timer)
        {
            bestMove = board.GetLegalMoves()[0];
            for (int itdeep = 1; itdeep < maxDepth; itdeep++)
            {
                currentDepth = itdeep;
                lastEval = alphaBeta(board, float.NegativeInfinity, float.PositiveInfinity, itdeep, timer, 0);
                prevMove = bestMove;
                if (timer.MillisecondsElapsedThisTurn > (timer.MillisecondsRemaining / 60))
                {
                    break;
                }

            }
            Console.WriteLine(breakDepth);
            Console.WriteLine(lastEval * (board.IsWhiteToMove ? 1 : -1));
            return bestMove;
        }

        float alphaBeta(Board board, float alpha, float beta, int depthleft, Timer timer, int plyCount)
        {
            Move[] moves = board.GetLegalMoves();
            moveOrdering(moves, board);
            float bestscore = float.NegativeInfinity;

            if (board.IsInCheckmate())
            {
                return -1000000 + plyCount;
            }
            else if(board.IsDraw())
            {
                return 0;
            }

            if (depthleft == 0)
            {
                return quiesce(alpha, beta, board);
            }

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                float eval = -alphaBeta(board, -beta, -alpha, depthleft - 1, timer, plyCount + 1);
                board.UndoMove(move);

                if (eval >= beta)
                    return eval;
                if (eval > bestscore)
                {
                    bestscore = eval;
                    if (depthleft == currentDepth)
                    {
                        bestMove = move;
                    }
                    if (eval > alpha)
                        alpha = eval;
                }
                if (timer.MillisecondsElapsedThisTurn > (timer.MillisecondsRemaining / 60))
                {
                    bestMove = prevMove;
                    bestscore = lastEval;
                    breakDepth = currentDepth;
                    break;
                }
            }
            return bestscore;
        }
        float quiesce(float alpha, float beta, Board board)
        {
            Move[] moves = board.GetLegalMoves(true);
            moveOrdering(moves, board);
            float evaluation = evaluate(board);
            if (evaluation >= beta)
                return beta;
            if (alpha < evaluation)
                alpha = evaluation;

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                evaluation = -quiesce(-beta, -alpha, board);
                board.UndoMove(move);

                if (evaluation >= beta)
                    return beta;
                if (evaluation > alpha)
                    alpha = evaluation;
            }
            return alpha;
        }
        float evaluate(Board board)
        {
            float evaluation = 0;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Square square = new Square(i, j);
                    Piece piece = board.GetPiece(square);
                    int mobility = BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks(piece.PieceType, square, board, piece.IsWhite));
                    evaluation += (pieceValues[(int)piece.PieceType] + 0.1f * mobility / (maxAttack[(int)piece.PieceType])) * (piece.IsWhite ? 1 : -1);
                }

            }
            return evaluation * (board.IsWhiteToMove ? 1 : -1);

        }
        void moveOrdering(Move[] moves, Board board)
        {
            int index = 0;
            probabilities = new float[218];
            foreach (Move move in moves)
            {
                float probability = 0;
                int movePiece = (int)board.GetPiece(move.StartSquare).PieceType;
                int capturedPiece = (int)board.GetPiece(move.TargetSquare).PieceType;
                if (capturedPiece != 0)
                {
                    probability = 10 * pieceValues[capturedPiece] - pieceValues[movePiece];
                }
                if (move.IsPromotion)
                {
                    probability += pieceValues[(int)move.PromotionPieceType];
                }
                probabilities[index] = probability;
                index++;
            }
            for (int i = 0; i < moves.Length; i++)
            {
                for (int j = 0; j < moves.Length; j++)
                {
                    if (probabilities[i] > probabilities[j])
                    {
                        (probabilities[i], probabilities[j]) = (probabilities[j], probabilities[i]);
                        (moves[i], moves[j]) = (moves[j], moves[i]);
                    }
                }
            }
        }
    }
}