namespace AtelierResleriana.News
{
    public record class NewsListItem
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required Uri IconUri { get; set; }
        public required Uri Uri { get; set; }
    }
}
