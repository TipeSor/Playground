namespace Playground.Drawing
{
    public readonly struct Color(byte r, byte g, byte b) : IEquatable<Color>
    {
        public byte R { get; } = r;
        public byte G { get; } = g;
        public byte B { get; } = b;

        public bool Equals(Color other)
        {
            return R == other.R &&
                   G == other.G &&
                   B == other.B;
        }

        public override bool Equals(object? obj)
        {
            return obj is not null && obj.GetType() == GetType() && Equals((Color)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B);
        }

        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Color left, Color right)
        {
            return !left.Equals(right);
        }
    }
}
