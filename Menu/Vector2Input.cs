using System;

namespace Average.Client.Menu
{
    public class Vector2Input
    {
        public string Text { get; set; }
        public string PlaceHolder { get; set; }
        public string Pattern { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public object Value { get; set; }

        public Vector2Input(string text, string placeHolder, string pattern, int minLength, int maxLength, object value)
        {
            Text = text;
            PlaceHolder = placeHolder;
            Pattern = pattern;
            MinLength = minLength;
            MaxLength = maxLength;
            Value = value;
        }

        public event EventHandler PropertyChanged;

        protected void OnPropertyChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(null, null);
            }
        }
    }
}
