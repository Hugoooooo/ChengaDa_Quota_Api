
namespace Domain.Models.Auth
{
    public class TokenModel
    {
        public string memberId { set; get; }
        public string name { get; set; }

        public long iat { set; get; }
    }

    public class ClaimData : TokenModel
    {
    }
}
