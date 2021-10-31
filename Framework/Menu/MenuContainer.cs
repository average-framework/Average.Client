using Average.Client.Framework.Interfaces;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class MenuContainer
    {
        public TopContainer TopContainer { get; private set; }
        public BottomContainer BottomContainer { get; private set; }
        public IMenuInfo MiddleContainer { get; private set; }

        public string Id { get; set; } = RandomString();
        public string BannerTitle { get; set; }

        public MenuContainer(TopContainer topContainer, BottomContainer bottomContainer, IMenuInfo menuInfo)
        {
            TopContainer = topContainer;
            BottomContainer = bottomContainer;
            MiddleContainer = menuInfo;
        }

        internal IPrimaryMenuItem GetPrimaryItem(string id)
        {
            return TopContainer.GetItem<IPrimaryMenuItem>(id);
        }

        internal ISecondaryMenuItem GetSecondaryItem(string id)
        {
            return BottomContainer.GetItem<ISecondaryMenuItem>(id);
        }
    }
}
