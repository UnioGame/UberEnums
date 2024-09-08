namespace Game.Editor.UberEnums
{
    using System.Collections.Generic;

    public interface IUberEnumConvertible
    {
        public IEnumerable<IUberEnumValueConvertible> Values { get; }
    }

    public interface IUberEnumValueConvertible
    {
        public string Name { get; }
        public int Value { set; }
    }
}