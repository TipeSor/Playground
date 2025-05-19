using System.Numerics;

namespace Playground.Math
{
    public static class Euler
    {
#pragma warning disable IDE0055
        public static Vector3 Rotate(Vector3 vec, float yaw, float pitch, float roll)
        {
            return Roll(Pitch(Yaw(vec, yaw), pitch), roll);
        }

        public static Vector3 Yaw(Vector3 vec, float rot)
        {
            Vector3 vec2 = new(0, 0, 0)
            {
                X = (vec.X * MathF.Cos(rot)) - (vec.Y * MathF.Sin(rot)) + (vec.Z * 0),
                Y = (vec.X * MathF.Sin(rot)) + (vec.Y * MathF.Cos(rot)) - (vec.Z * 0),
                Z = (vec.X * 0)              + (vec.Y * 0)              + (vec.Z * 1)
            };
            return vec2;
        }

        public static Vector3 Pitch(Vector3 vec, float rot)
        {
            Vector3 vec2 = new(0, 0, 0)
            {
                X =  (vec.X * MathF.Cos(rot)) + (vec.Y * 0) + (vec.Z * MathF.Sin(rot)),
                Y =  (vec.X * 0)              + (vec.Y * 1) + (vec.Z * 0),
                Z = -(vec.X * MathF.Sin(rot)) + (vec.Y * 0) + (vec.Z * MathF.Cos(rot))
            };
            return vec2;
        }

        public static Vector3 Roll(Vector3 vec, float rot)
        {
            Vector3 vec2 = new(0, 0, 0)
            {
                X = (vec.X * 1) + (vec.Y * 0)              + (vec.Z * 0),
                Y = (vec.X * 0) + (vec.Y * MathF.Cos(rot)) - (vec.Z * MathF.Sin(rot)),
                Z = (vec.X * 0) + (vec.Y * MathF.Sin(rot)) + (vec.Z * MathF.Cos(rot))
            };
            return vec2;
        }
#pragma warning restore IDE0055
    }
}
