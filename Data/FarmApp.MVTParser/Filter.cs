namespace FarmApp.MVTParser;

public abstract class Filter
{
    public abstract bool Include(string layerName);

    public static readonly Filter ALL = new FilterAll();

    private class FilterAll : Filter
    {
        public override bool Include(string layerName)
        {
            return true;
        }
    }

    public class Single : Filter
    {
        private readonly string _layerName;

        public Single(string layerName)
        {
            _layerName = layerName;
        }

        public override bool Include(string layerName)
        {
            return _layerName.Equals(layerName);
        }
    }

    public class Any : Filter
    {
        private readonly HashSet<string> _layerNames;

        public Any(HashSet<string> layerNames)
        {
            _layerNames = layerNames;
        }

        public override bool Include(string layerName)
        {
            return _layerNames.Contains(layerName);
        }
    }
}
