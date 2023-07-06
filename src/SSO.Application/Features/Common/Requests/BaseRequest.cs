namespace SSO.Application.Features.Common.Requests
{
    public class BaseRequest
    {
        public string Keyword { get; internal set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public ICollection<string> OrderBy { get; set; }
        public ICollection<object[]> Filters { get; set; }
    }
}
