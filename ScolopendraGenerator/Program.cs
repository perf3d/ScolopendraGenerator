using System;
using System.Reflection;
using static System.Console;
using static System.IO.Directory;
using static System.IO.Path;

GenerateScolopendra();


static void GenerateScolopendra()
{
    ModelsPart head = new("Parts/Head.stl");
    ModelsPart mid = new("Parts/Midle.stl");
    ModelsPart back = new("Parts/Tail.stl");
    WriteLine("Insert middle segments count: ");
    uint countMiddleSegments = Convert.ToUInt32(ReadLine());
    uint totalTrisCount = head.TrisCount + (mid.TrisCount * countMiddleSegments) + back.TrisCount;

    string newModelPath = Combine(GetCurrentDirectory(), $"Scolopendra_{countMiddleSegments}S.stl");
    using (FileStream writeFileStream = File.Open(newModelPath,FileMode.OpenOrCreate))
    {
        using(FileStream readFileStream = File.Open(head.PathToModel,FileMode.Open))
        {
            using(BinaryReader reader = new BinaryReader(readFileStream))
            {
                using(BinaryWriter writer = new BinaryWriter(writeFileStream))
                {
                    byte[] headerBuf = reader.ReadBytes(80);
                    writer.Write(headerBuf);
                    writer.Write(totalTrisCount);
                }
            }
        }
    }
    float delta = 4f;
    float headXCoef = 0;
    float middleXCoef = (mid.LengthX/2) + (head.LengthX/2);
    float backXCoef = (head.LengthX/2) + ((mid.LengthX - delta) * countMiddleSegments) + (back.LengthX / 2);


    InstanceTriangles(head, newModelPath, headXCoef);
    InstanceTriangles(mid, newModelPath, middleXCoef, countMiddleSegments, delta);
    InstanceTriangles(back, newModelPath, backXCoef, delta: delta);



}

static void InstanceTriangles(ModelsPart model, string pathTo, float xPosition, uint countCopyes = 1, float delta = 0)
{
    float x;
    float y;
    float z;
    ushort attributes;
    WriteLine("Copy triangles from {0} to {1}", model.PathToModel, pathTo);
    using (FileStream writeFileStream = File.Open(pathTo, FileMode.Open))
    {
        using (FileStream readFileStream = File.OpenRead(model.PathToModel))
        {
            using (BinaryReader binaryReader = new BinaryReader(readFileStream))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(writeFileStream))
                {
                    writeFileStream.Seek(0, SeekOrigin.End);
                    for (int copyNumber = 0; copyNumber < countCopyes; copyNumber++)
                    {
                        readFileStream.Seek(84, SeekOrigin.Begin);
                        Write(".");
                        for (uint tri = 0; tri < model.TrisCount; tri++)
                        {
                            //normal vector read
                            x = binaryReader.ReadSingle();
                            y = binaryReader.ReadSingle();
                            z = binaryReader.ReadSingle();
                            //normal vector write
                            binaryWriter.Write(x);
                            binaryWriter.Write(y);
                            binaryWriter.Write(z);
                            //points of triangle read and write
                            for (int pointNumber = 0; pointNumber < 3; pointNumber++)
                            {
                                //point read
                                x = binaryReader.ReadSingle();
                                y = binaryReader.ReadSingle();
                                z = binaryReader.ReadSingle();
                                //point write
                                binaryWriter.Write(x + model.ToZeroXCoeff + xPosition - delta + ((model.LengthX - delta) * copyNumber));
                                binaryWriter.Write(y + model.ToZeroYCoeff);
                                binaryWriter.Write(z);
                            }
                            //attributes read and write
                            attributes = binaryReader.ReadUInt16();
                            binaryWriter.Write(attributes);
                        }
                    }
                }
            }
        }
    }
    WriteLine();
}
