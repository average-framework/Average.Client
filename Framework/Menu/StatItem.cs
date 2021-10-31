using static Average.Client.Framework.GameAPI;

namespace Average.Client.Framework.Menu
{
    internal class StatItem
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public StatBarType StatsType { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int Value { get; set; }
        public bool Visible { get; set; }
        public bool Disabled { get; set; }

        public StatItem(string label, StatBarType statsBarType, int min, int max, int value, bool visible = true, bool disabled = false)
        {
            Id = RandomString();
            Label = label;
            StatsType = statsBarType;
            Min = min;
            Max = max;
            Value = value;
            Visible = visible;
            Disabled = disabled;
        }

        public object OnRender() => new
        {
            type = GetType().Name,
            id = Id,
            label = Label,
            statsType = StatsType,
            percentage = (float)((float)Value / (float)Max),
            visible = Visible,
            disabled = Disabled
        };
    }
}
