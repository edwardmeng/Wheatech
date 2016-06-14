namespace Wheatech
{
    public interface IVersionComparator
    {
        bool Satisfies(Version version);
    }
}
