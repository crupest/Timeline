namespace Timeline.Services.Api
{
    public class SearchResultItem<TItem>
    {
        public SearchResultItem(TItem item, int score)
        {
            Item = item;
            Score = score;
        }

        public TItem Item { get; set; } = default!;

        /// <summary>
        /// Bigger is better.
        /// </summary>
        public int Score { get; set; }
    }
}
