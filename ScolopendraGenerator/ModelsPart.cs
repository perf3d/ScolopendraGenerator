//using SixLabors.ImageSharp.Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static System.Console;

public class ModelsPart : IEnumerable<Coords>
{
    private string? pathToModel = null;
    private uint? trisCount = null;
    private float? lengthX = null;
    public ModelsPart(string path)
    {
        pathToModel = path;
        trisCount = getTrisCount(path);
        lengthX = getLengthX();
        WriteLine($"{PathToModel} contain {TrisCount} tris and have length {lengthX:N2} units(mm?)");
    }

    public string? PathToModel
    {
        get
        {
            return pathToModel;
        }
    }
    public uint? TrisCount
    {
        get
        {
            return trisCount;
        }
    }
    public float? LengthX
    {
        get
        {
            return lengthX;
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
            throw new Exception("invalid path");
        }
    }

    private float getLengthX()
    {
        //First method _____>
        //float maxHead = this.Max(x => x.X);
        //float minHead = this.Min(x => x.X);
        //return(float) (maxHead - minHead);

        //Second method(~50% faster) _____>
        bool startFlag = false;
        float minX = 0;
        float maxX = 0;
        foreach (Coords coord in this)
        {
            if (startFlag)
            {
                minX = coord.X;
                minX = coord.X;
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
            }
        }
        return (maxX - minX);
    }

    IEnumerator<Coords> IEnumerable<Coords>.GetEnumerator() => new CoordsEnumerator(pathToModel);

    IEnumerator IEnumerable.GetEnumerator() => new CoordsEnumerator(pathToModel);

}

class CoordsEnumerator : IEnumerator<Coords>
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

    //object System.Collections.IEnumerator.Current => throw new NotImplementedException();

    bool System.Collections.IEnumerator.MoveNext()
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
            return false;
    }

    void System.Collections.IEnumerator.Reset()
    {
        position = startPosition; //80 + 4 + 12 - 26;
    }

    void IDisposable.Dispose()
    {
        //throw new NotImplementedException();
    }

}

struct Coords
{
    public float X;
    public float Y;
    public float Z;
}