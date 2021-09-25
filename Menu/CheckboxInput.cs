namespace Average.Client.Menu
{
    public class CheckboxInput
    {
        public string Text { get; set; }
        public bool IsChecked { get; set; }

        public CheckboxInput(string text, bool isChecked)
        {
            Text = text;
            IsChecked = isChecked;
        }
    }
}
