namespace HeatSim
{
    public class Lexem
    {
        public enum Form
        {
            ADD, SUB, UNARY_MINUS, MULT, DIV, POW, NUMBER, NAME, BRACKET, COMMA, END_BRACKET, END
        }
        public readonly Form type;
        public readonly string orig;

        public Lexem(Form type, string orig)
        {
            this.type = type;
            this.orig = orig;
        }

        public Priority GetPriority()
        {
            switch (type)
            {
                case Form.ADD: return Priority.ADD;
                case Form.SUB: return Priority.SUB;
                case Form.UNARY_MINUS: return Priority.SUB;
                case Form.MULT: return Priority.WEAK_MULT;
                case Form.DIV: return Priority.DIV;
                case Form.POW: return Priority.POW;
                case Form.NUMBER:
                case Form.NAME:
                case Form.BRACKET: return Priority.NUMBERABLE;
                case Form.COMMA:
                case Form.END_BRACKET:
                case Form.END: return Priority.OTHER;
                default: return Priority.OTHER;
            }
        }
    }
}
