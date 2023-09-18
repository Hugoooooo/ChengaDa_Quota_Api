using Domain.Models.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ChengDaApi.Services
{
    public class JwtTokenService
    {
        public string GenToken(string claimStr, int expireTime, string key)
        {
            var claimList = new List<Claim>();

            var claimObj = JsonConvert.DeserializeObject<TokenModel>(claimStr);

            claimList.Add(new Claim("memberId", claimObj.memberId.ToString()));
            claimList.Add(new Claim("name", claimObj.name));
            claimList.Add(new Claim("iat", claimObj.iat.ToString()));

            var token = new JwtSecurityToken(
                claims: claimList,
                expires: DateTime.Now.AddSeconds(expireTime), //exp
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public HttpContext AddToken(HttpContext httpContext, string token)
        {
            httpContext.Request.Headers.Add("Authorization", $@"Bearer {token}");
            return httpContext;
        }

        public ClaimData GetClaim(HttpContext httpContext)
        {
            ClaimData data = null;

            StringValues authorization;
            httpContext.Request.Headers.TryGetValue("Authorization", out authorization);

            if (!StringValues.IsNullOrEmpty(authorization))
            {
                var token = authorization.ToString();
                //[bearer ]
                data = (token.Length <= 7) ? null : GetClaimInfoByToken(token.Substring(7));
            }

            return data;
        }

        public ClaimData GetClaimInfoByToken(string token)
        {
            var data = new ClaimData();

            var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.ToList();

            var memberIdClaim = claims.Where(x => x.Type == "memberId").FirstOrDefault();
            var nameClaim = claims.Where(x => x.Type == "name").FirstOrDefault();
            var iat = claims.Where(x => x.Type == "iat").FirstOrDefault();

            data.memberId = (memberIdClaim != null) ? memberIdClaim.Value : "";
            data.name = (nameClaim != null) ? nameClaim.Value : "";
            data.iat = (iat != null) ? long.Parse(iat.Value) : 0;

            return data;
        }

        public long GetTokenExpirationTime(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            var tokenExp = jwtSecurityToken.Claims.First(claim => claim.Type.Equals("exp")).Value;
            var ticks = long.Parse(tokenExp);
            return ticks;
        }

        public bool CheckTokenIsValid(string token)
        {
            var tokenTicks = GetTokenExpirationTime(token);
            var tokenDate = DateTimeOffset.FromUnixTimeSeconds(tokenTicks).UtcDateTime;

            var now = DateTime.Now.ToUniversalTime();

            var valid = tokenDate >= now;

            return valid;
        }
    }
}
