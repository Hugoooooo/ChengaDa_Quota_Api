using System;

namespace Domain.Models.Auth

{
    public class LoginReq
    {
        public string account { get; set; }
        public string password { get; set; }
    }

    public class RefreshToeknReq
    {
        public string token { get; set; }
    }

}
