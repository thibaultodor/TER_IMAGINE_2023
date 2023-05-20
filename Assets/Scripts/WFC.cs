using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = UnityEngine.Random;

public class WFC
{
    public int outWidth;
    public int outHeight;
    public int limit = 0;
    public int N = 2;

    private Dictionary<ulong, uint> patternsHashed;
    private uint[][] patterns;
    private double[] patternWeights;
    private PalettedImage palettedImage;
    private List<uint>[,,] propagator;
    private Sprite spriteLastCreated;

    private class PalettedImage
    {
        public int width;
        public int height;
        public uint[] data;
        public List<Color32> palette;

        public PalettedImage(ref Texture2D input)
        {
            Color32[] pixels = input.GetPixels32();
            width = input.width;
            height = input.height;

            data = new uint[width * height];
            palette = new List<Color32>();

            for (int i = 0; i < width * height; ++i)
            {
                Color32 color = pixels[i];
                int index = palette.IndexOf(color);
                if (index == -1)
                {
                    index = palette.Count;
                    palette.Add(color);
                }
                data[i] = (uint)index;
            }
        }

        public uint at(uint row, uint col)
        {
            return data[(row % height) * width + (col % width)];
        }
    };

    public WFC( int oW, int oH, int lim = 0, int n = 2)
    {
        outWidth = oW;
        outHeight = oH;
        limit = lim;
        N = n;
    }

    ulong HashFromPattern(ref uint[] pattern, uint paletteSize)
    {
        ulong result = 0;
        ulong power = 1;
        for (int i = 0; i < pattern.Length; ++i)
        {
            result += pattern[pattern.Length - 1 - i] * power;
            power *= paletteSize;
        }
        return result;
    }

    uint[] patternFromHash(ulong hash, int patternWidth, uint paletteSize)
    {
        ulong residue = hash;
        ulong power = (ulong)Math.Pow(paletteSize, patternWidth * patternWidth);
        uint[] result = new uint[patternWidth * patternWidth];

        for (int i = 0; i < result.Length; ++i)
        {
            power /= paletteSize;
            ulong count = 0;

            while (residue >= power)
            {
                residue -= power;
                count++;
            }

            result[i] = (uint)(count);
        }

        return result;
    }

    private uint[] MakePattern(ref PalettedImage palettedImage, int imageRow, int imageCol)
    {
        uint[] pattern = new uint[N * N];
        for (int row = 0; row < N; ++row)
        {
            for (int col = 0; col < N; ++col)
            {
                pattern[row * N + col] =
                    palettedImage.at((uint)(imageRow + row), (uint)(imageCol + col));
            }
        }

        return pattern;
    }

    uint[] Reflect(ref uint[] p)
    {
        uint[] newP = new uint[N * N];
        for (int row = 0; row < N; ++row)
        {
            for (int col = 0; col < N; ++col)
            {
                newP[row * N + col] = p[row * N + (N - 1 - col)];
            }
        }

        return newP;
    }

    uint[] Rotate(ref uint[] p)
    {
        uint[] newP = new uint[N * N];
        for (int row = 0; row < N; ++row)
        {
            for (int col = 0; col < N; ++col)
            {
                newP[row * N + col] = p[col * N + (N - 1 - row)];
            }
        }
        return newP;
    }

    private Dictionary<ulong, uint> GenPatterns(ref PalettedImage palettedImage)
    {
        Dictionary<ulong, uint> res = new Dictionary<ulong, uint>();

        for (int row = 0; row < palettedImage.height; ++row)
        {
            for (int col = 0; col < palettedImage.width; ++col)
            {
                uint[][] patterns = new uint[8][];
                patterns[0] = MakePattern(ref palettedImage, row, col);
                patterns[1] = Reflect(ref patterns[0]);
                patterns[2] = Rotate(ref patterns[0]);
                patterns[3] = Reflect(ref patterns[2]);
                patterns[4] = Rotate(ref patterns[2]);
                patterns[5] = Reflect(ref patterns[4]);
                patterns[6] = Rotate(ref patterns[4]);
                patterns[7] = Reflect(ref patterns[6]);

                for (int i = 0; i < 8; ++i)
                {
                    ulong hash = HashFromPattern(ref patterns[i], (uint)palettedImage.palette.Count);
                    if (res.ContainsKey(hash))
                        ++res[hash];
                    else
                        res.Add(hash, 1);
                }
            }
        }

        return res;
    }

    private bool PatternAgrees(ref uint[] p1, ref uint[] p2, int dx, int dy, int n)
    {
        int xmin = dx < 0 ? 0 : dx;
        int xmax = dx < 0 ? dx + n : n;
        int ymin = dy < 0 ? 0 : dy;
        int ymax = dy < 0 ? dy + n : n;
        for (int y = ymin; y < ymax; ++y)
        {
            for (int x = xmin; x < xmax; ++x)
            {
                if (p1[x + n * y] != p2[x - dx + n * (y - dy)])
                    return false;
            }
        }
        return true;
    }

    enum Result
    {
        Success,
        Fail,
        Unfinished,
    }

    class Output
    {
        public bool[,,] wave;
        public bool[,] changes;
    }

    Output CreateOutput(uint width, uint height, uint numPatterns)
    {
        Output output = new Output();
        output.wave = new bool[width, height, numPatterns];
        for (int w = 0; w < width; ++w)
            for (int h = 0; h < height; ++h)
                for (int n = 0; n < numPatterns; ++n)
                    output.wave[w, h, n] = true;
        output.changes = new bool[width, height];
        for (int w = 0; w < width; ++w)
            for (int h = 0; h < height; ++h)
                output.changes[w, h] = false;
        return output;
    }

    (Result, int, int) FindLowestEntropy(ref Output output, ref double[] patternWeights, uint width, uint height, uint numPatterns)
    {
        double min = double.PositiveInfinity;
        int argminx = -1, argminy = -1;
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                // if (model.on_boundary(x, y)) { continue; }

                uint numSuperimposed = 0;
                double entropy = 0.0;

                for (int t = 0; t < numPatterns; ++t)
                {
                    if (output.wave[x, y, t])
                    {
                        numSuperimposed += 1;
                        entropy += patternWeights[t];
                    }
                }

                if (entropy == 0 || numSuperimposed == 0)
                {
                    return (Result.Fail, -1, -1);
                }

                if (numSuperimposed == 1)
                {
                    continue;
                }

                double noise = 0.5 * (double)UnityEngine.Random.Range(0.0f, 1.0f);
                entropy += noise;

                if (entropy < min)
                {
                    min = entropy;
                    argminx = x;
                    argminy = y;
                }
            }
        }

        if (double.IsPositiveInfinity(min))
            return (Result.Success, argminx, argminy);
        else
            return (Result.Unfinished, argminx, argminy);
    }

    uint RandomWeightedIndex(ref double[] distrib, double rand)
    {
        double sum = distrib.Sum();
        if (sum == 0.0)
            return (uint)Math.Floor(rand * distrib.Length);

        double scaled = rand * sum;
        double accumulator = 0.0;
        for (uint i = 0; i < distrib.Length; ++i)
        {
            accumulator += distrib[i];
            if (scaled <= accumulator)
                return i;
        }

        return 0;
    }

    Result Observe(ref Output output, ref double[] patternWeights, uint width, uint height, uint numPatterns)
    {
        Result result;
        int argminx, argminy;
        (result, argminx, argminy) = FindLowestEntropy(ref output, ref patternWeights, width, height, numPatterns);
        if (result != Result.Unfinished)
            return result;

        double[] distribution = new double[numPatterns];
        for (int t = 0; t < numPatterns; ++t)
            distribution[t] = output.wave[argminx, argminy, t] ? patternWeights[t] : 0;

        uint r = RandomWeightedIndex(ref distribution, (double)UnityEngine.Random.Range(0.0f, 1.0f));
        for (int t = 0; t < numPatterns; ++t)
        {
            output.wave[argminx, argminy, t] = t == r;
        }
        output.changes[argminx, argminy] = true;

        return Result.Unfinished;
    }

    bool Propagate(ref Output output, int N, int width, int height, int numPatterns, ref List<uint>[,,] propagator)
    {
        bool changed = false;

        for (int x1 = 0; x1 < width; ++x1)
        {
            for (int y1 = 0; y1 < height; ++y1)
            {
                if (!output.changes[x1, y1])
                    continue;
                output.changes[x1, y1] = false;

                for (int dx = -N + 1; dx < N; ++dx)
                {
                    for (int dy = -N + 1; dy < N; ++dy)
                    {
                        int x2 = x1 + dx;
                        int y2 = y1 + dy;

                        int sx = x2;
                        if (sx < 0)
                            sx += width;
                        else if (sx >= width)
                            sx -= width;

                        int sy = y2;
                        if (sy < 0)
                            sy += height;
                        else if (sy >= height)
                            sy -= height;

                        for (int t2 = 0; t2 < numPatterns; ++t2)
                        {
                            if (!output.wave[sx, sy, t2])
                                continue;

                            bool fits = false;

                            List<uint> prop = propagator[t2, N - 1 - dx, N - 1 - dy];
                            for (int i = 0; i < prop.Count; ++i)
                            {
                                if (output.wave[x1, y1, prop[i]])
                                {
                                    fits = true;
                                    break;
                                }
                            }

                            if (!fits)
                            {
                                output.changes[sx, sy] = true;
                                output.wave[sx, sy, t2] = false;
                                changed = true;
                            }
                        }
                    }
                }
            }
        }

        return changed;
    }

    Color32[,] CreateImage(ref Output output, ref List<Color32> palette, ref uint[][] patterns, int width, int height, int N, int numPatterns)
    {
        List<uint>[,] results = new List<uint>[width, height];
        for (int row = 0; row < height; ++row)
        {
            for (int col = 0; col < width; ++col)
            {
                results[col, row] = new List<uint>();
                for (int dy = 0; dy < N; ++dy)
                {
                    for (int dx = 0; dx < N; ++dx)
                    {
                        int sx = col - dx;
                        if (sx < 0)
                            sx += width;

                        int sy = row - dy;
                        if (sy < 0)
                            sy += height;

                        // if (on_boundary(sx, sy)) { continue; }

                        for (int t = 0; t < numPatterns; ++t)
                        {

                            if (output.wave[sx, sy, t])
                            {
                                results[col, row].Add(patterns[t][dx + dy * N]);
                            }
                        }
                    }
                }
            }
        }

        Color32[,] newImage = new Color32[width, height];
        for (int row = 0; row < height; ++row)
        {
            for (int col = 0; col < width; ++col)
            {
                List<uint> contributors = results[col, row];
                if (contributors.Count == 0)
                {
                    newImage[col, row] = new Color32(0, 0, 0, 255);
                }
                else if (contributors.Count == 1)
                {
                    newImage[col, row] = palette[newImage[col, row][0]];
                }
                else
                {
                    uint r = 0;
                    uint g = 0;
                    uint b = 0;
                    uint a = 0;
                    foreach (int color in contributors)
                    {
                        r += palette[color].r;
                        g += palette[color].g;
                        b += palette[color].b;
                        a += palette[color].a;
                    }
                    r = (uint)(r / contributors.Count);
                    g = (uint)(g / contributors.Count);
                    b = (uint)(b / contributors.Count);
                    a = (uint)(a / contributors.Count);
                    newImage[col, row] = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
                }
            }
        }

        return newImage;
    }

    public void GenContent(string SpritePath)
    {
        Texture2D texture = Resources.Load<Texture2D>(SpritePath);// "WFCSamples/3Bricks");
        Debug.Log("Loaded input image");

        palettedImage = new PalettedImage(ref texture);
        Debug.Log("Created palette");

        // uint[] test = {0, 2, 3,
        //             2, 2, 1,
        //             5, 4, 2};
        //
        // ulong hashTest = HashFromPattern(ref test, 6);
        // uint[] res = patternFromHash(hashTest, 3, 6);

        patternsHashed = GenPatterns(ref palettedImage);
        Debug.Log("Generated patterns");

        patterns = new uint[patternsHashed.Count][];
        patternWeights = new double[patternsHashed.Count];

        int i = 0;
        foreach (var pattern in patternsHashed)
        {
            patterns[i] = patternFromHash(pattern.Key, (int)N, (uint)palettedImage.palette.Count);
            patternWeights[i] = pattern.Value;
            ++i;
        }
        Debug.Log("Created pattern weights");

        propagator = new List<uint>[patternsHashed.Count, 2 * N - 1, 2 * N - 1];
        for (uint t = 0; t < patterns.Length; ++t)
        {
            for (int x = 0; x < 2 * N - 1; ++x)
            {
                for (int y = 0; y < 2 * N - 1; ++y)
                {
                    propagator[t, x, y] = new List<uint>();
                    List<uint> list = propagator[t, x, y];

                    for (uint t2 = 0; t2 < patterns.Length; ++t2)
                    {
                        if (PatternAgrees(ref patterns[t], ref patterns[t2], (int)(x - N + 1), (int)(y - N + 1), (int)N))
                        {
                            list.Add(t2);
                        }
                    }
                    list.TrimExcess();
                }
            }
        }
        Debug.Log("Built propagator");
    }

    public Sprite GenSprite()
    {
        Debug.Log("Running WFC...");
        bool success = false;
        Output output = CreateOutput((uint)outWidth, (uint)outHeight, (uint)patternsHashed.Count);
        uint l;
        for (l = 0; l < limit || limit == 0; ++l)
        {
            Result res = Observe(ref output, ref patternWeights, (uint)outWidth, (uint)outHeight, (uint)patterns.Length);
            if (res != Result.Unfinished)
            {
                Debug.Log("Finished WFC in " + l + " iterations");
                success = true;
                break;
            }

            while (Propagate(ref output, (int)N, outWidth, outHeight, patterns.Length, ref propagator)) { }
        }

        if (success)
        {
            Color32[,] newPixels = CreateImage(ref output, ref palettedImage.palette, ref patterns, outWidth, outHeight, (int)N, patterns.Length);
            Texture2D newTex = new Texture2D(outWidth, outHeight, TextureFormat.ARGB32, false);
            newTex.filterMode = FilterMode.Point;

            for (int row = 0; row < outHeight; ++row)
            {
                for (int col = 0; col < outWidth; ++col)
                {
                    // newTex.SetPixel(col, outHeight-row, newPixels[col, row]);
                    newTex.SetPixel(col, row, newPixels[col, row]);
                }
            }
            newTex.Apply();

            /*
            Transform LevelOpen = levelStart.Find("LevelOpen");
            print(LevelOpen.transform.GetChild((0)).transform.localScale);
            GameObject Floor = LevelOpen.transform.GetChild((0));
            SpriteRenderer renderer = LevelOpen.transform.GetChild((0)).GetComponentsInChildren<SpriteRenderer>()[0]; */
            return spriteLastCreated = Sprite.Create(newTex, new Rect(0, 0, outWidth, outHeight), new Vector2(0.5f, 0.5f), 100);
            //Debug.Log("Changed sprite");
        }
        else if (l == limit)
        {
            Debug.Log("Limit reached");
        }

        return null;
    }

    public void CreateRoomSprites(ref Dictionary<int, Sprite> RoomsSprite, ref byte[] layout, int w, int h, int Wall, string s)
    {
        this.GenContent(s);

        for (int i = 0; i < w * h; i++)
            if (layout[i] != Wall)
            {
                Sprite t = this.GenSprite();
                RoomsSprite.Add(i, t);
            }
    }
}
