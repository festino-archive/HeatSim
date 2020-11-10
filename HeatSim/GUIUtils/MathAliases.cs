namespace HeatSim
{
    public static class MathAliases
    {
        public static FuncAlias[] GetDefaultFunctions()
        {
            return DEFAULTS;
        }
        public static string ConvertName(string name)
        {
            foreach (FuncAlias aliases in GREEK_LETTERS)
            {
                if (aliases.Name == name)
                    return name;

                foreach (string alias in aliases.Aliases)
                    if (alias == name)
                        return aliases.Name;
            }
            return name;
        }

        private static readonly FuncAlias[] GREEK_LETTERS =
        {
            new FuncAlias("α", new string[] {"alpha"}),
            new FuncAlias("Α", new string[] {"Alpha", "А"}),
            new FuncAlias("β", new string[] { "beta" }),
            new FuncAlias("Β", new string[] { "Beta", "В" }),
            new FuncAlias("γ", new string[] { "gamma" }),
            new FuncAlias("Γ", new string[] { "Gamma", "Г" }),
            new FuncAlias("δ", new string[] { "delta" }),
            new FuncAlias("Δ", new string[] { "Delta" }),
            new FuncAlias("ε", new string[] { "epsilon" }),
            new FuncAlias("Ε", new string[] { "Epsilon", "Е" }),
            new FuncAlias("ζ", new string[] { "zeta" }),
            new FuncAlias("Ζ", new string[] { "Zeta" }),
            new FuncAlias("η", new string[] { "eta" }),
            new FuncAlias("Η", new string[] { "Eta", "Н" }),
            new FuncAlias("θ", new string[] { "theta" }),
            new FuncAlias("Θ", new string[] { "Theta" }),
            new FuncAlias("ι", new string[] { "iota" }),
            new FuncAlias("Ι", new string[] { "Iota" }),
            new FuncAlias("ϰ", new string[] { "kappa" }),
            new FuncAlias("Κ", new string[] { "Kappa", "К" }),
            new FuncAlias("λ", new string[] { "lamda" }),
            new FuncAlias("Λ", new string[] { "Lamda" }),
            new FuncAlias("μ", new string[] { "mu" }),
            new FuncAlias("Μ", new string[] { "Mu", "М" }),
            new FuncAlias("ν", new string[] { "nu" }),
            new FuncAlias("Ν", new string[] { "Nu" }),
            new FuncAlias("ξ", new string[] { "xi" }),
            new FuncAlias("Ξ", new string[] { "Xi" }),
            new FuncAlias("ο", new string[] { "omicron", "о" }),
            new FuncAlias("Ο", new string[] { "Omicron", "О" }),
            new FuncAlias("π", new string[] { "pi" }),
            new FuncAlias("Π", new string[] { "Pi", "П" }),
            new FuncAlias("ρ", new string[] { "rho" }),
            new FuncAlias("Ρ", new string[] { "Rho", "Р" }),
            new FuncAlias("σ", new string[] { "sigma" }),
            new FuncAlias("Σ", new string[] { "Sigma" }),
            new FuncAlias("𝜏", new string[] { "tau", "т" }),
            new FuncAlias("Τ", new string[] { "tau", "Т" }),
            new FuncAlias("υ", new string[] { "upsilon" }),
            new FuncAlias("Υ", new string[] { "Upsilon" }),
            new FuncAlias("φ", new string[] { "phi", "ф" }),
            new FuncAlias("Φ", new string[] { "Phi", "Ф" }),
            new FuncAlias("χ", new string[] { "chi", "х" }),
            new FuncAlias("Χ", new string[] { "Chi", "Х" }),
            new FuncAlias("ψ", new string[] { "psi" }),
            new FuncAlias("Ψ", new string[] { "Psi" }),
            new FuncAlias("ω", new string[] { "omega" }),
            new FuncAlias("Ω", new string[] { "Omega" }),
        };

        private static readonly FuncAlias[] DEFAULTS =
        {
            new FuncAlias(ConvertName("pi"), new string[0]),
            new FuncAlias("e", new string[0]),
            new FuncAlias("ln", 1, new string[0]),
            new FuncAlias("log", 2, new string[0]),
            new FuncAlias("sin", 1, new string[0]),
            new FuncAlias("cos", 1, new string[0]),
            new FuncAlias("tg", 1, new string[] { "tan" }),
            new FuncAlias("ctg", 1, new string[] { "cotan" }),
            new FuncAlias("arcsin", 1, new string[0]),
            new FuncAlias("arccos", 1, new string[0]),
            new FuncAlias("arctg", 1, new string[] { "arctan" }),
            new FuncAlias("arcctg", 1, new string[] { "arccotan" }),
            new FuncAlias("sh", 1, new string[0]),
            new FuncAlias("ch", 1, new string[0]),
            new FuncAlias("sqrt", 1, new string[0]),
            new FuncAlias("abs", 1, new string[0]),
            new FuncAlias("pow", 1, new string[] { "power" }),
        };
    }
}
