using System;

namespace Average.Client.Menu
{
    public class Vector3Input
    {
        public string PlaceHolder { get; set; }
        public string Pattern { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public object Value { get; set; }

        public Vector3Input(string placeHolder, string pattern, int minLength, int maxLength, object value)
        {
            PlaceHolder = placeHolder;
            Pattern = pattern;
            MinLength = minLength;
            MaxLength = maxLength;
            Value = value;
        }

        public event EventHandler PropertyChanged;
    }
}
