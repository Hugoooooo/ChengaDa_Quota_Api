using System.Collections.Generic;

namespace Domain.Models.Auth
{
    public class GenTokenRes : GenericRes
    {
        public string Token { get; set; } 

        public string RefreshToken { get; set; } 
    }

    public class LoginRes : GenericRes
    {
        public string name { get; set; }
        public string token { get; set; }
        public string refreshToken { get; set; }
    }


    public class GetSystemParamRes : GenericRes
    {
        public List<GetSystemParamModel> items { get; set; } = new List<GetSystemParamModel>();
    }

    public class GetSystemParamModel
    {
        public string type { get; set; }
        public List<string> values { get; set; }
    }

}
