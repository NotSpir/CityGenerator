namespace CityGeneration.Core.Data.DataModels;

public struct Vector2(float x, float y) //I forgot about the Numerics Vector2, oops. Too late now.
{
    public float X {get; set; } = x;
    public float Y {get; set; } = y;

    public float Length()
    {
        return (float)Math.Sqrt(X * X + Y * Y);
    }
    
    public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator -(Vector2 a) => new Vector2(-a.X, -a.Y);
    public static Vector2 operator *(Vector2 a, float b) => new Vector2(a.X * b, a.Y * b);
    public static Vector2 operator *(float b, Vector2 a) => new Vector2(a.X * b, a.Y * b);
    public static Vector2 operator /(Vector2 a, float b) => new Vector2(a.X / b, a.Y / b);

    private const float CmpEpsilon = 1e-5f; // Godot 4's CMP_EPSILON

    public static Vector2? GetSegmentIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        Vector2 B = a2 - a1; 
        Vector2 C = b1 - a1; 
        Vector2 D = b2 - b1; 

        float AB_x_D = Cross(B, D);
        float C_x_D  = Cross(C, D);
        
        if (MathF.Abs(AB_x_D) < CmpEpsilon)
            return null;

        float t = C_x_D      / AB_x_D;
        float u = Cross(C, B) / AB_x_D;

        if (t < 0f || t > 1f || u < 0f || u > 1f)
            return null;

        return a1 + B * t;
    }

    private static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

    public static bool IsPointOnSegment(Vector2 p, Vector2 a, Vector2 b, float epsilon = 1e-5f)
    {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;
        float lenSq = dx * dx + dy * dy;
        
        float scale = MathF.Max(1f, MathF.Sqrt(lenSq));

        if (lenSq <= (epsilon * scale) * (epsilon * scale))
        {
            float px = p.X - a.X;
            float py = p.Y - a.Y;
            float d = MathF.Sqrt(px * px + py * py);
            return d <= epsilon * scale;
        }
        
        float cross = dx * (p.Y - a.Y) - dy * (p.X - a.X);
        float perpDist = MathF.Abs(cross) / MathF.Sqrt(lenSq);
        if (perpDist > epsilon * scale)
            return false;
        
        float dot = (p.X - a.X) * dx + (p.Y - a.Y) * dy;
        float slack = epsilon * lenSq;
        return dot >= -slack && dot <= lenSq + slack;
    }
    
    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        t = Math.Clamp(t, 0, 1);
        return new Vector2(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t
        );
    }
    
    public Vector2 Rotate(float radians)
    {
        float c = MathF.Cos(radians);
        float s = MathF.Sin(radians);
        return new Vector2(
            X * c - Y * s,
            X * s + Y * c
        );
    }
    
    public Vector2 Normalized()
    {
        float lenSq = X * X + Y * Y;
        if (lenSq <= 1e-12f)
            return new Vector2(0f, 0f);
        float invLen = 1f / MathF.Sqrt(lenSq);
        return new Vector2(X * invLen, Y * invLen);
    }

    public static float SignedAngleBetweenVectors(Vector2 a, Vector2 b)
    {
        float dot   = a.X * b.X + a.Y * b.Y;
        float cross = a.X * b.Y - a.Y * b.X;
        float signedAngle = MathF.Atan2(cross, dot);   // radians, in (-π, π]
        return signedAngle;
    }
    
    public static float AngleBetweenVectors(Vector2 a, Vector2 b)
    {
        float dot = a.X * b.X + a.Y * b.Y;
        float cross = a.X * b.Y - a.Y * b.X;
        return MathF.Abs(MathF.Atan2(cross, dot));   // radians, in [0, π]
    }
}