using Raylib_cs;
using System.Numerics;

namespace _2D_Rubiks_Cube_Projection
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Camera3D cam = new Camera3D(new Vector3(0, 0, -1.5f), new Vector3(0, 0, 0), Vector3.UnitY, 60, CameraProjection.CAMERA_PERSPECTIVE);
            Triangle[] tris = new Triangle[1]
            {
                new Triangle(0, 0, 0, 1, 0, 0, 1, 1, 0, Color.RED),
            };
            Quad[] quads = new Quad[cubeNxN * cubeNxN * 6];
                //new Quad(0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, Color.RED),

            Raylib.InitWindow(800, 600, "2D Cube Projection");
            InitialiseCube(ref quads, defaultColorScheme);

            while (!Raylib.WindowShouldClose())
            {
                if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT) || Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
                {
                    float x = cam.position.X;
                    float z = cam.position.Z;
                    float t = Raylib.GetFrameTime() * 2 * (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT) ? 1 : -1);

                    cam.position.X = (float)((x * Math.Cos(t)) - (z * Math.Sin(t)));
                    cam.position.Z = (float)((x * Math.Sin(t)) + (z * Math.Cos(t)));
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.LIGHTGRAY);

                Raylib.BeginMode3D(cam);

                for (int i = 0; i < quads.Length; i++)
                {
                    DrawQuadFromStruct(quads[i]);
                }

                Raylib.EndMode3D();
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        static void DrawTriangleFromStruct(Triangle triangle)
        {
            Raylib.DrawTriangle3D(triangle.a, triangle.b, triangle.c, triangle.color);
        }
        
        static void DrawQuadFromStruct(Quad quad)
        {
            Raylib.DrawTriangle3D(quad.a, quad.c, quad.b, quad.color);
            Raylib.DrawTriangle3D(quad.a, quad.d, quad.c, quad.color);
        }

        static Vector3 cubePositionOffset = new Vector3(-0.5f);
        static readonly float cubeFaceScale = 0.8f;
        static readonly float cubeStickerScale = 0.9f;
        static readonly int cubeNxN = 3;

        static int CubeStickersPerSide => cubeNxN * cubeNxN;
        static float CubeStickerWidth => cubeStickerScale / cubeNxN;
        static float CubeGapWidth => (1 - cubeStickerScale) / (cubeNxN - 1);

        static readonly Vector3[] corners = new Vector3[8]
        {
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 1),
            new Vector3(1, 1, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
            new Vector3(0, 0, 1)
        };

        //Side order 0-5 = U,L,F,R,B,D
        static readonly int[,] sideCornerIndexes = new int[6, 4]
        {
            { 0, 1, 2, 3 },
            { 0, 3, 4, 7 },
            { 3, 2, 5, 4 },
            { 2, 1, 6, 5 },
            { 1, 0, 7, 6 },
            { 4, 5, 6, 7 },
        };

        static readonly CubeColorScheme defaultColorScheme = new CubeColorScheme(
            Color.WHITE,
            Color.ORANGE,
            Color.GREEN,
            Color.RED,
            Color.BLUE,
            Color.YELLOW
            );

        static readonly Vector3[] QuadVertexMultipliers = new Vector3[6]
        {
            new Vector3(cubeFaceScale, 1, cubeFaceScale),
            new Vector3(1, cubeFaceScale, cubeFaceScale),
            new Vector3(cubeFaceScale, cubeFaceScale, 1),
            new Vector3(1, cubeFaceScale, cubeFaceScale),
            new Vector3(cubeFaceScale, cubeFaceScale, 1),
            new Vector3(cubeFaceScale, 1, cubeFaceScale),
        };

        static Vector3 GetQuadVertex(int side, int corner)
        {
            return Vector3.Multiply(corners[sideCornerIndexes[side, corner]] + cubePositionOffset, QuadVertexMultipliers[side]);
        }

        static Vector3 GetStickerQuadVertex(int side, int sticker, int corner)
        {
            int x = sticker % cubeNxN;
            int y = sticker / cubeNxN;

            Vector3 vertexA = GetQuadVertex(side, 0);
            Vector3 vertexB = GetQuadVertex(side, 2);

            float lerpStepX = (x * CubeStickerWidth) + (Math.Max(0, x ) * CubeGapWidth);
            float lerpStepY = (y * CubeStickerWidth) + (Math.Max(0, y ) * CubeGapWidth);

            if (corner == 1 || corner == 2)
            {
                lerpStepX += CubeStickerWidth;
            }
            if (corner > 1)
            {
                lerpStepY += CubeStickerWidth;
            }

            Vector3 output = side switch
            {
                0 => new Vector3(Lerp(vertexA.X, vertexB.X, lerpStepX), vertexA.Y, Lerp(vertexA.Z, vertexB.Z, lerpStepY)),
                1 => new Vector3(vertexA.X, Lerp(vertexA.Y, vertexB.Y, lerpStepY), Lerp(vertexA.Z, vertexB.Z, lerpStepX)),
                2 => new Vector3(Lerp(vertexA.X, vertexB.X, lerpStepX), Lerp(vertexA.Y, vertexB.Y, lerpStepY), vertexA.Z),
                3 => new Vector3(vertexA.X, Lerp(vertexA.Y, vertexB.Y, lerpStepY), Lerp(vertexA.Z, vertexB.Z, lerpStepX)),
                4 => new Vector3(Lerp(vertexA.X, vertexB.X, lerpStepX), Lerp(vertexA.Y, vertexB.Y, lerpStepY), vertexA.Z),
                5 => new Vector3(Lerp(vertexA.X, vertexB.X, lerpStepX), vertexA.Y, Lerp(vertexA.Z, vertexB.Z, lerpStepY)),
                _ => throw new Exception($"Side index {side} is invalid."),
            };

            return output;
        }

        static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        static void InitialiseCube(ref Quad[] quads, CubeColorScheme colorScheme)
        {
            for (int side = 0; side < 6; side++)
            {
                for (int sticker = 0; sticker < CubeStickersPerSide; sticker++)
                {
                    quads[(side * CubeStickersPerSide) + sticker] = new Quad()
                    {
                        a = GetStickerQuadVertex(side, sticker, 0),
                        b = GetStickerQuadVertex(side, sticker, 1),
                        c = GetStickerQuadVertex(side, sticker, 2),
                        d = GetStickerQuadVertex(side, sticker, 3),
                        color = colorScheme[side],
                    };

                }
            }
        }

        struct Triangle
        {
            public Vector3 a, b, c;
            public Color color;

            public Triangle(float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3, Color col)
            {
                a = new Vector3(x1, y1, z1);
                b = new Vector3(x2, y2, z2);
                c = new Vector3(x3, y3, z3);
                color = col;
            }
        }

        struct Quad
        {
            public Vector3 a, b, c, d;
            public Color color;
            
            public Quad(float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3, float x4, float y4, float z4, Color col)
            {
                a = new Vector3(x1, y1, z1);
                b = new Vector3(x2, y2, z2);
                c = new Vector3(x3, y3, z3);
                d = new Vector3(x4, y4, z4);
                color = col;
            }
        }

        struct CubeColorScheme
        {
            private Color[] colors;

            public Color this[int i] { get => colors[i]; set => colors[i] = value; }

            public Color U { get => colors[0]; set => colors[0] = value; }
            public Color L { get => colors[1]; set => colors[1] = value; }
            public Color F { get => colors[2]; set => colors[2] = value; }
            public Color R { get => colors[3]; set => colors[3] = value; }
            public Color B { get => colors[4]; set => colors[4] = value; }
            public Color D { get => colors[5]; set => colors[5] = value; }

            public CubeColorScheme(Color U, Color L, Color F, Color R, Color B, Color D)
            {
                colors = new Color[6] { U, L, F, R, B, D };
            }
        }
    }
}