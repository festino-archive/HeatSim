using System;
using System.Collections.Generic;

namespace HeatSim
{
    public class ExprParser
    {
        private Dictionary<string, int> Aliases = new Dictionary<string, int>();
        
        public bool AddAlias(string alias, int argCount)
        {
            if (Aliases.ContainsKey(alias) && Aliases[alias] <= argCount)
                return false;
            
            Aliases[alias] = argCount;
            return true;
        }
        public void AddAliases(IEnumerable<FuncAlias> aliases)
        {
            foreach (FuncAlias alias in aliases)
                AddAlias(alias.Name, alias.ArgCount);
        }

        public IExpression Parse(string str)
        {
            return GetExpr(Priority.INIT, str, 0, out _, false);
        }

        private IExpression GetExpr(Priority minPriority, string str, int from, out int to, bool hasBracket)
        {
            IExpression res = new ExprNaN();
            Lexem prevLexem = new Lexem(Lexem.Form.END, "");
            Lexem lexem = GetLexem(str, from, out from, false);
            Priority lexemPriority = lexem.GetPriority();
            while (lexemPriority != Priority.OTHER && minPriority < lexemPriority)
            {
                if (lexemPriority == Priority.NUMBERABLE)
                {
                    IExpression expr = new ExprNaN();
                    if (lexem.type == Lexem.Form.BRACKET)
                    {
                        expr = GetExpr(Priority.INIT, str, from, out from, true);
                    }
                    else if (lexem.type == Lexem.Form.NUMBER)
                    {
                        expr = new ExprConst(lexem.orig);
                    }
                    else if (lexem.type == Lexem.Form.NAME)
                    {
                        //string convertedName = MathUtils.ConvertName(lexem.orig);
                        string[] parts = lexem.orig.Split("_");
                        string convertedName = MathAliases.ConvertName(parts[0]);
                        for (int i = 1; i < parts.Length; i++)
                            convertedName += "_" + MathAliases.ConvertName(parts[i]);
                        if (Aliases.ContainsKey(convertedName))
                        {
                            int argCount = Aliases[convertedName];
                            expr = new ExprRegFunc(convertedName, argCount);
                            if (argCount > 0)
                            {
                                if (argCount == 1)
                                    expr.AddArg(GetExpr(Priority.SINGLE_ARG, str, from, out from, false));
                                else
                                    GetArgs((ExprRegFunc)expr, str, from, out from);
                            }
                        }
                    }

                    if (res is ExprNaN)
                    {
                        res = expr;
                    }
                    else if (prevLexem.GetPriority() == Priority.NUMBERABLE) // STRONG_MULT
                    {
                        if (res is ExprMult)
                        {
                            res.AddArg(expr);
                        }
                        else
                        {
                            IExpression resTemp = res;
                            res = new ExprMult();
                            res.AddArg(resTemp);
                            res.AddArg(expr);
                        }
                    }
                }
                else
                {
                    IExpression expr;
                    if (lexem.type == Lexem.Form.POW)
                        expr = GetExpr(lexemPriority - 1, str, from, out from, false);
                    else
                        expr = GetExpr(lexemPriority, str, from, out from, false);

                    IExpression opType = new ExprNaN();
                    if (lexem.type == Lexem.Form.ADD)
                        opType = new ExprSum();
                    if (lexem.type == Lexem.Form.SUB)
                        opType = new ExprSum();
                    if (lexem.type == Lexem.Form.UNARY_MINUS)
                        opType = new ExprSum();
                    if (lexem.type == Lexem.Form.MULT)
                        opType = new ExprMult();
                    if (lexem.type == Lexem.Form.DIV)
                        opType = new ExprDiv();
                    if (lexem.type == Lexem.Form.POW)
                        opType = new ExprPow();
                    if (lexem.type == Lexem.Form.SUB || lexem.type == Lexem.Form.UNARY_MINUS)
                    {
                        IExpression exprTemp = expr;
                        expr = new ExprMult(new ExprConst("-1"), exprTemp); // "-1" * "10"
                    }
                    bool unitable = res.GetType() == opType.GetType();
                    bool inverseUnitable = expr.GetType() == opType.GetType();
                    if ((opType is ExprSum || opType is ExprMult) && (unitable || inverseUnitable))
                    {
                        bool similar = res.GetType() == expr.GetType();
                        if (similar)
                            foreach (IExpression arg in expr.GetArgs())
                                res.AddArg(arg);
                        else if (unitable)
                            res.AddArg(expr);
                        else
                        {
                            expr.AddArg(res);
                            res = expr;
                        }
                    }
                    else
                    {
                        opType.AddArg(res);
                        opType.AddArg(expr);
                        res = opType;
                    }
                }

                prevLexem = lexem;
                lexem = GetLexem(str, from, out from, false);
                lexemPriority = lexem.GetPriority();
            }

            if (minPriority >= lexemPriority || lexem.type == Lexem.Form.COMMA || lexem.type == Lexem.Form.END_BRACKET && !hasBracket)
            {
                from -= lexem.orig.Length;
            }

            to = from;
            return res.GetOrNaN();
        }

        private void GetArgs(ExprRegFunc func, string str, int from, out int to)
        {
            Lexem lexem = GetLexem(str, from, out from, false);
            if (lexem.type != Lexem.Form.BRACKET)
            {
                to = from - lexem.orig.Length;
                return;
            }
            func.AddArg(GetExpr(Priority.INIT, str, from, out from, false));
            for (int i = 1; i < func.ArgCount; i++)
            {
                lexem = GetLexem(str, from, out from, false);
                while (lexem.type != Lexem.Form.COMMA && lexem.type != Lexem.Form.END)
                    lexem = GetLexem(str, from, out from, false);
                func.AddArg(GetExpr(Priority.INIT, str, from, out from, false));
            }
            lexem = GetLexem(str, from, out from, false);
            while (lexem.type != Lexem.Form.END_BRACKET && lexem.type != Lexem.Form.END)
                lexem = GetLexem(str, from, out from, false);
            to = from;
        }

        private static Lexem GetLexem(string str, int from, out int to, bool inSummation)
        {
            while (from < str.Length) {
                char c = str[from];
                from++;
                if (char.IsLetter(c))
                {
                    string name = c.ToString();
                    while (from < str.Length)
                    {
                        c = str[from];
                        if (!('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || c == '_' || char.IsDigit(c)))
                            break;
                        name += c;
                        from++;
                    }
                    to = from;
                    return new Lexem(Lexem.Form.NAME, name);
                }
                if (char.IsDigit(c)) // '-'
                {
                    string num = c.ToString();
                    while (from < str.Length)
                    {
                        c = str[from];
                        if (!('0' <= c && c <= '9' || c == '.'))
                            break;
                        num += c;
                        from++;
                    }
                    to = from;
                    return new Lexem(Lexem.Form.NUMBER, num);
                }
                if (c == '-' && !inSummation)
                {
                    to = from;
                    return new Lexem(Lexem.Form.UNARY_MINUS, c.ToString());
                }
                if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^')
                {
                    Lexem.Form type;
                    switch (c)
                    {
                        case '+': type = Lexem.Form.ADD; break;
                        case '-': type = Lexem.Form.SUB; break;
                        case '*': type = Lexem.Form.MULT; break;
                        case '/': type = Lexem.Form.DIV; break;
                        case '^': type = Lexem.Form.POW; break;
                        default: type = Lexem.Form.ADD; break;
                    }
                    to = from;
                    return new Lexem(type, c.ToString());
                }
                if (c == '(')
                {
                    to = from;
                    return new Lexem(Lexem.Form.BRACKET, c.ToString());
                }
                if (c == ')')
                {
                    to = from;
                    return new Lexem(Lexem.Form.END_BRACKET, c.ToString());
                }
                if (c == ',')
                {
                    to = from;
                    return new Lexem(Lexem.Form.COMMA, c.ToString());
                }
            }
            to = from;
            return new Lexem(Lexem.Form.END, "");
        }
    }
}
