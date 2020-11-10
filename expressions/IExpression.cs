namespace HeatSim
{
    public interface IExpression
    {
        public IExpression GetOrNaN();
        public void AddArg(IExpression arg);
        public IExpression[] GetArgs();
        public int GetArgsCount();
        public string AsString();
    }
}
