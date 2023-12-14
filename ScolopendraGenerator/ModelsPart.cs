

namespace scolopendra;
public class ModelsPart
{
    public readonly string? pathToModel=null;
    public readonly int? trisCount = null;
    public readonly int? lengthX = null;
    ModelsPart(string path)
    {
        pathToModel = path;
        trisCount = getTrisCount(path);
        lengthX = getLengthX(path);
    }

    private int getTrisCount(string path)
    {

    }

    private int getLengthX(string path)
    {

    }
}