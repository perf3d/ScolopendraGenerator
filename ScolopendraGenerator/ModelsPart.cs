//using SixLabors.ImageSharp.Drawing;
using System;
using System.Collections;
using System.IO;
using System.Text;
using static System.Console;

public class ModelsPart : IEnumerable
{
    private string pathToModel;
    private uint trisCount;
    private float lengthX;
    private float toZeroXCoeff;
    private float toZeroYCoeff;
    public ModelsPart(string path)
    {
        pathToModel = path;
        try
        {
            trisCount = getTrisCount(path);
            lengthX = getLengthX();
        }
        catch(Exception) 
        {
            throw;
        }
        WriteLine($"{PathToModel} contain {TrisCount} tris and have length {lengthX:N2} units(mm?)");
    }
    public string PathToModel
    {
        get
        {
            return pathToModel;
        }
    }
    public uint TrisCount
    {
        get
        {
            return trisCount;
        }
    }
    public float LengthX
    {
        get
        {
            return lengthX;
        }
    }

    public float ToZeroXCoeff
    {
        get
        {
            return toZeroXCoeff;
        }
    }

    public float ToZeroYCoeff
    {
        get
        {
            return toZeroYCoeff;
        }
    }

    private uint getTrisCount(string path)
    {
        uint trisCount = 0;
        if (File.Exists(path))
        {
            using (FileStream stream = File.Open(path, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    stream.Seek(80, SeekOrigin.Begin);
                    trisCount = reader.ReadUInt32();
                }
            }
            return trisCount;
        }
        else
        {
            WriteLine("Invalid path: " + path);
            throw new Exception("invalid path: " + path);
        }
    }
    private float getLengthX()
    {
        bool startFlag = false;
        float minX = 0;
        float maxX = 0;
        float minY = 0;
        float maxY = 0;
        foreach (Coords coord in this)
        {
            if (startFlag)
            {
                minX = coord.X;
                maxX = coord.X;
                minY = coord.Y;
                maxY = coord.Y;
            }
            else
            {
                if (coord.X < minX)
                {
                    minX = coord.X;
                }
                if (coord.X > maxX)
                {
                    maxX = coord.X;
                }
                if (coord.Y < minY)
                {
                    minY = coord.Y;
                }
                if (coord.Y > maxY)
                {
                    maxY = coord.Y;
                }
            }
        }
        float centerX = minX + ((maxX - minX) / 2);
        float centerY = minY + ((maxY - minY) / 2);
        toZeroXCoeff = 0 - centerX;
        toZeroYCoeff = 0 - centerY;
        return (maxX - minX);
    }
    IEnumerator IEnumerable.GetEnumerator() => new CoordsEnumerator(pathToModel);
}
class CoordsEnumerator : IEnumerator
{
    static int startPosition = 70;
    FileInfo info;
    int position = startPosition;
    byte jumpCounter = 1;
    public CoordsEnumerator(string? path)
    {
        if (!File.Exists(path))
        {
            throw new Exception("invalid path");
        }
        info = new FileInfo(path);
    }
    object System.Collections.IEnumerator.Current
    {
        get{
            Coords coords = new Coords();
            if (position == startPosition || position > info.Length)
            {
                throw new ArgumentException();
            }
            using (FileStream stream = File.Open(info.FullName, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    stream.Seek(position, SeekOrigin.Begin);
                    coords.X = reader.ReadSingle();
                    coords.Y = reader.ReadSingle();
                    coords.Z = reader.ReadSingle();
                }
            }
            return coords;
        }
    }
    bool IEnumerator.MoveNext()
    {
        int increment=0;
        switch (jumpCounter)
        {
            case 0b0001:
                increment = 26;
                jumpCounter <<= 1;
                break;
            case 0b0010:
                increment = 12;
                jumpCounter <<= 1;
                break;
            case 0b0100:
                increment = 12;
                jumpCounter = 1;
                break;
        }
        if ((position + increment) <= info.Length)
        {
            position += increment;
            return true;
        }
        else
        {
            return false;
        } 
    }

    void IEnumerator.Reset()
    {
        position = startPosition; //80 + 4 + 12 - 26;
    }
}

struct Coords
{
    public float X;
    public float Y;
    public float Z;
}