namespace ErogeHelper.Common.Constraint
{
    public static class ConstraintValues
    {
        // FIXME: HCode最后一个是:应该不允许通过 
        public const string CodeRegExp = @"/?H\S+@[A-Fa-f0-9]+(:\S+)?|/?RS@[A-Fa-f0-9]+";
    }
}