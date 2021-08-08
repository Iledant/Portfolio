#nullable enable

namespace Portfolio.ViewModel
{
    public static class Converters
    {
        public static string LabelNullableString(string label, string? value)
        {
            return label + " : " + (value ?? "-");
        }
    }
}
