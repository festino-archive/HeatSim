using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace HeatSim.expressions.Managing
{
    public static class ArithmUtils
    {
        public static string SimplifyNumber(string exprConstVal)
        {
            if (exprConstVal.Length == 0 || !char.IsDigit(exprConstVal[0]) && exprConstVal[0] != '-')
                return exprConstVal;
            int m = 0;
            while (m < exprConstVal.Length && exprConstVal[m] == '-')
                m++;
            int i = m;
            while (i < exprConstVal.Length && exprConstVal[i] == '0')
                i++;
            if (m < i && (i == exprConstVal.Length || exprConstVal[i] == '.'))
                i--;

            int j = exprConstVal.Length - 1;
            if (exprConstVal.IndexOf('.', i) > 0)
            {
                while (j >= 0 && exprConstVal[j] == '0')
                    j--;
                if (exprConstVal[j] == '.')
                    j--;
            }
            if (m % 2 == 0)
                return exprConstVal.Substring(i, j + 1 - i);
            return '-' + exprConstVal.Substring(i, j + 1 - i);
        }
        //public static bool EqualsLongArithm(string val, int orig)

        public class BigRational
        {
            private bool isCorrect;
            private BigInteger digits;
            private int mult10_pow;

            public BigRational()
            {
                digits = 0;
                mult10_pow = 0;
                isCorrect = true;
            }

            public BigRational(int n)
            {
                digits = n;
                mult10_pow = 0;
                isCorrect = true;
            }

            public BigRational(BigInteger digits, int digitsAfterDot)
            {
                this.digits = digits;
                mult10_pow = digitsAfterDot;
                isCorrect = true;
            }

            public BigRational(string numStr)
            {
                numStr = SimplifyNumber(numStr);

                int dotIndex = numStr.IndexOf('.');
                if (dotIndex >= 0)
                {
                    try
                    {
                        digits = BigInteger.Parse(numStr[..dotIndex] + numStr[(dotIndex + 1)..]);
                        mult10_pow = dotIndex + 1 - numStr.Length;
                        isCorrect = true;
                    }
                    catch (FormatException)
                    {
                        isCorrect = false;
                    }
                }
                else
                {
                    try
                    {
                        digits = BigInteger.Parse(numStr);
                        mult10_pow = 0;
                        isCorrect = true;
                    }
                    catch (FormatException)
                    {
                        isCorrect = false;
                    }
                }
            }
        
            public BigRational Mult10inPow(int pow)
            {
                if (!isCorrect)
                    return this;
                return new BigRational(digits * (BigInteger) Math.Pow(10, -pow), mult10_pow + pow);
            }

            public static BigRational operator +(BigRational v1, BigRational v2)
            {
                if (!v1.isCorrect)
                    return v1;
                if (!v2.isCorrect)
                    return v2;
                if (v1.mult10_pow < v2.mult10_pow)
                    v2 = v2.Mult10inPow(v1.mult10_pow - v2.mult10_pow);
                else if (v1.mult10_pow > v2.mult10_pow)
                    v1 = v1.Mult10inPow(v2.mult10_pow - v1.mult10_pow);
                return new BigRational(v1.digits + v2.digits, v1.mult10_pow);
            }

            public static BigRational operator *(BigRational v1, BigRational v2)
            {
                if (!v1.isCorrect)
                    return v1;
                if (!v2.isCorrect)
                    return v2;
                return new BigRational(v1.digits * v2.digits, v1.mult10_pow + v2.mult10_pow);
            }

            public static implicit operator BigRational(int v)
            {
                return new BigRational(v);
            }

            public static bool operator ==(BigRational v1, BigRational v2)
            {
                return v1.isCorrect && v2.isCorrect && v1.mult10_pow == v2.mult10_pow && v1.digits == v2.digits;
            }

            public static bool operator <(BigRational v1, BigRational v2)
            {
                return (v1 + (-1) * v2).digits < 0;
            }

            public static bool operator >(BigRational v1, BigRational v2)
            {
                return (v1 + (-1) * v2).digits > 0;
            }

            public static bool operator !=(BigRational v1, BigRational v2)
            {
                return !(v1 == v2);
            }

            public override string ToString()
            {
                if (!isCorrect)
                    return "NaNConst";
                string res = digits.ToString();
                bool isNegative = res[0] == '-';
                if (isNegative)
                    res = res.Substring(1);
                if (mult10_pow < 0)
                {
                    int sum = mult10_pow + res.Length;
                    if (sum > 0)
                        res = res.Insert(sum, ".");
                    else
                        res = "0." + new string('0', -sum) + res;
                }
                else if (mult10_pow > 0)
                    res = res + new string('0', mult10_pow);
                if (isNegative)
                    res = '-' + res;
                return SimplifyNumber(res);
            }

            public double ToDouble()
            {
                if (!isCorrect)
                    return 0;
                return double.Parse(ToString());
            }
        }
    }
}
