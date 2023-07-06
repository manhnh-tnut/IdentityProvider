using System.Runtime.Serialization;

namespace SSO.Application.Features.Common.Queries
{
    public class BaseQuery
    {
        [DataMember]
        public string Keyword { get; internal set; }
        [DataMember]
        public int Limit { get; set; }
        [DataMember]
        public int Offset { get; set; }
        [DataMember]
        public ICollection<string> OrderBy { get; set; }
        [DataMember]
        public ICollection<object[]> Filters { get; set; }
    }
}
