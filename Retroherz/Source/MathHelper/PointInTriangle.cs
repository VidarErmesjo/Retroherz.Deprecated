using MonoGame.Extended;
using Microsoft.Xna.Framework;
using System;

namespace MonoGame.Trigonometry
{
    public class PointInTriangle
    {
        public const float EPSYLON = 0f;//0.000001f;

        public enum Orientation
        {
            Clockwise,
            CounterClockwise,
            Collinear
        }

        public static Orientation GetOrientation(Point A, Point B, Point C)
        {
            float value = (B.Y - A.Y) * (C.X - B.X) - (B.X - A.X) * (C.Y - B.Y);

            return (value < EPSYLON && value > -EPSYLON) ?
                Orientation.Collinear : (value < 0) ?
                    Orientation.CounterClockwise : Orientation.Clockwise;
        }

        public static bool Barycentric(Point2 P, Point2 A, Point2 B, Point2 C)
        {
            float BY_CY = B.Y - C.Y;
            float CX_BX = C.X - B.X;
            float BXCY_CXBY = B.X * C.Y - C.X * B.Y;
            float CY_AY = C.Y - A.Y;
            float AX_CX = A.X - C.X;
            float CXAY_AXCY = C.X * A.Y - A.X * C.Y;
            float AY_BY = A.Y - B.Y;
            float BX_AX = B.X - A.X;
            float AXBY_BXAY = A.X * B.Y - B.X * A.Y;

            /*Matrix M = new Matrix(
                new Vector4(A.X, A.Y, 0, 0),
                new Vector4(B.X, B.Y, 0, 0),
                new Vector4(C.X, C.Y, 0, 0),
                new Vector4(0, 0, 0, 1));*/
            //System.Console.WriteLine(determinant + ", " + M.Determinant());
            //Matrix det; det.Determinant();
            
            //Matrix2 M = new Matrix2(A.X, A.Y, B.X, B.Y, C.X, C.Y);

            //Point2 BC = B - C;
            //Point2 CD = C - B;
        

            float D = A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y);
            //System.Console.WriteLine(D + ", " + M.Determinant());
            if(MathF.Abs(D) < EPSYLON)
                return false;

            D = 1 / D;

            float A1 = (P.X * (BY_CY) + P.Y * (CX_BX) + (BXCY_CXBY)) * D;
            float A2 = (P.X * (CY_AY) + P.Y * (AX_CX) + (CXAY_AXCY)) * D;
            float A3 = (P.X * (AY_BY) + P.Y * (BX_AX) + (AXBY_BXAY)) * D;

            //System.Console.WriteLine("A1:{0}, A2:{1}, A3:{2}, A1+A2+A3:{3}", A1, A2, A3, A1 + A2 + A3);

            return A1 >= 0 && A2 >= 0 && A3 >= 0 && A1 + A2 + A3 <= 1 + EPSYLON;
        }

        // Not working as expected?
        public static bool Orientations(Point2 A, Point2 B, Point2 C, Point2 P)
        {
            // P matches A, B or C?
            if( MathF.Abs(A.X - P.X) < EPSYLON && MathF.Abs(A.Y - P.Y) < EPSYLON ||
                MathF.Abs(B.X - P.X) < EPSYLON && MathF.Abs(B.Y - P.Y) < EPSYLON ||
                MathF.Abs(C.X - P.X) < EPSYLON && MathF.Abs(C.Y - P.Y) < EPSYLON)
                    return true;

            float value1 = (A.Y - P.Y) * (B.X - A.X) - (A.X - P.X) * (B.Y - A.Y);
            // value1 is zero and P.X is between A.X and B.X, and P.Y is between A.Y and B.Y?
            if(MathF.Abs(value1) < EPSYLON)
                return  ((P.X >= A.X && P.X <= B.X) || (P.X >= B.X && P.X <= A.X)) &&
                        ((P.Y >= A.Y && P.Y <= B.Y) || (P.Y >= B.Y && P.Y <= A.Y));

            Orientation first = value1 < 0 ? Orientation.CounterClockwise : Orientation.Clockwise;

            float value2 = (B.Y - P.Y) * (C.X - B.X) - (B.X - P.X) * (C.Y - B.Y);
            // value2 is zero and P.X is between B.X and C.X, and P.Y is between B.Y and C.Y?
            if(MathF.Abs(value2) < EPSYLON)
                return  ((P.X >= B.X && P.X <= C.X) || (P.X >= C.X && P.X <= B.X)) &&
                        ((P.Y >= B.Y && P.Y <= C.Y) || (P.Y >= C.Y && P.Y <= B.Y));

            Orientation central = value2 < 0 ? Orientation.CounterClockwise : Orientation.Clockwise;


            float value3 = (C.Y - P.Y) * (A.X - B.X) - (C.X - P.X) * (A.Y - B.Y);
            // value3 is zero and P.X is between A.X and C.X, and P.Y is between A.Y and C.Y?
            if(MathF.Abs(value3) < EPSYLON)
                return  ((P.X >= C.X && P.X <= A.X) || (P.X >= A.X && P.X <= C.X)) &&
                        ((P.Y >= C.Y && P.Y <= A.Y) || (P.Y >= A.Y && P.Y <= C.Y));

            Orientation third = value3 < 0 ? Orientation.CounterClockwise : Orientation.Clockwise;

            // Orientations match?
            return first == central && central == third;
        }
    }
}