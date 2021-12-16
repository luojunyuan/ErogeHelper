using ReactiveUI;

namespace ErogeHelper.XamlTool.Converters;

// https://github.com/reactiveui/ReactiveUI/blob/b31fabf3e99b5e61370dd007a243166d7c1fed8e/src/ReactiveUI.Uwp/Common/BooleanToVisibilityTypeConverter.cs
internal class ColorConverter : IBindingTypeConverter
{
    public int GetAffinityForObjects(Type fromType, Type toType) => throw new NotImplementedException();
    public bool TryConvert(object? from, Type toType, object? conversionHint, out object? result) => throw new NotImplementedException();
}
