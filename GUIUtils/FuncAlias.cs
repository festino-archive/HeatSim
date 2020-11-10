using System.Collections.Generic;

namespace HeatSim
{
    public class FuncAlias
    {
        public readonly string Name;
        public List<string> Aliases = new List<string>();
        public readonly int ArgCount;

        public FuncAlias(string name, int argCount, IEnumerable<string> aliases)
        {
            Name = name;
            Aliases.AddRange(aliases);
            ArgCount = argCount;
        }

        public FuncAlias(string name, IEnumerable<string> aliases)
        {
            Name = name;
            Aliases.AddRange(aliases);
            ArgCount = 0;
        }
    }
}
