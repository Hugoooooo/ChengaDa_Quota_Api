using AutoMapper;
using AutoMapper.Execution;
using ChengDaApi.DBRepositories.IRepositories;
using ChengDaApi.Services;
using DinkToPdf.Contracts;
using Domain;
using Domain.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ChengDaApi.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _config;
        private readonly IDatabase _database;
        private readonly IAccountRepository _accountRepository;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ISystemParameterRepository _systemParameterRepository;
            
        public AuthController(ILogger<AuthController> logger, IConfiguration configuration, IMapper mapper, IDatabase database,
            IAccountRepository accountRepository, JwtTokenService jwtTokenService, ISystemParameterRepository systemParameterRepository)
        {
            _logger = logger;
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _database = database;
            _accountRepository = accountRepository;
            _jwtTokenService = jwtTokenService;
            _systemParameterRepository = systemParameterRepository;
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<LoginRes> Login(LoginReq req)
        {
            LoginRes res = new LoginRes()
            {
                message = GenericResType.QUERY_SUCCESS
            };

            try
            {
                var member = await _accountRepository.GetByAccount(req.account);

                //檢核：資料不存在
                if (member == null)
                {
                    res.isError = true;
                    res.message = GenericResType.LOGIN_FAIL + " 帳號不存在";
                    return res;
                }
                else if (member.pwd != req.password)
                {
                    res.isError = true;
                    res.message = GenericResType.LOGIN_FAIL + " 密碼錯誤";
                    return res;
                }

                TokenModel tokenModel = new TokenModel()
                {
                    memberId = member.id,
                    name = member.name,
                    iat = DateTime.Now.Ticks,
                };
                string token = _jwtTokenService.GenToken(JsonConvert.SerializeObject(tokenModel), int.Parse(_config["JWT:TokenExpiredTime:AccessToken"]), _config["JWT:key"]);
                string refreshToken = _jwtTokenService.GenToken(JsonConvert.SerializeObject(tokenModel), int.Parse(_config["JWT:TokenExpiredTime:RefreshToken"]), _config["JWT:key"]);

                member.token = token;
                member.refreshToken = refreshToken;
                _accountRepository.Update(member);
                await _database.SaveChangedAsync();

                res.name = member.name;
                res.token = token;
                res.refreshToken = refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("genToken")]
        public async Task<GenTokenRes> GenToken(string account)
        {
            var res = new GenTokenRes()
            {
                message = GenericResType.INSERT_SUCCESS
            };

            try
            {
                var members = await _accountRepository.GetAll();
                var member = members.First(p => p.account == account);
                if (member == null)
                {
                    throw new Exception(GenericResType.ITEM_NOT_FOUND);
                }

                TokenModel tokenModel = new TokenModel()
                {
                    memberId = member.id,
                    name = member.name,
                    iat = DateTime.Now.Ticks,
                };
                res.RefreshToken = Guid.NewGuid().ToString();
                res.Token = _jwtTokenService.GenToken(JsonConvert.SerializeObject(tokenModel), int.Parse(_config["JWT:TokenExpiredTime:AccessToken"]), _config["JWT:key"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                res.isError = true;
                res.message = GenericResType.INSERT_FAIL;
            }

            return res;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("refreshToken")]
        public async Task<GenTokenRes> RefreshToken(RefreshToeknReq req)
        {
            var res = new GenTokenRes()
            {
                message = GenericResType.UPDATE_SUCCESS
            };

            try
            {
                var member = await _accountRepository.GetByRefreshToken(req.token);
                if (member == null)
                {
                    throw new Exception(GenericResType.ITEM_NOT_FOUND);
                }

                bool isValid = _jwtTokenService.CheckTokenIsValid(member.refreshToken);
                if (!isValid)
                {
                    res.isError = true;
                    res.message = "登入失效，請重新登入";
                }
                else
                {
                    TokenModel tokenModel = new TokenModel()
                    {
                        memberId = member.id,
                        name = member.name,
                        iat = DateTime.Now.Ticks,
                    };
                    string newToken = _jwtTokenService.GenToken(JsonConvert.SerializeObject(tokenModel), int.Parse(_config["JWT:TokenExpiredTime:AccessToken"]), _config["JWT:key"]);
                    string newRefreshToken = _jwtTokenService.GenToken(JsonConvert.SerializeObject(tokenModel), int.Parse(_config["JWT:TokenExpiredTime:RefreshToken"]), _config["JWT:key"]);
                    member.token = newToken;
                    member.refreshToken = newRefreshToken;
                    _accountRepository.Update(member);
                    await _database.SaveChangedAsync();
                    res.Token = newToken;
                    res.RefreshToken = newRefreshToken;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                res.isError = true;
                res.message = GenericResType.UPDATE_FAIL;
            }

            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("getSystemParam")]
        public async Task<GetSystemParamRes> GetSystemParam()
        {
            var res = new GetSystemParamRes()
            {
                message = GenericResType.QUERY_SUCCESS
            };

            try
            {
                var sysParams = await _systemParameterRepository.GetAll();
                var groupedData = sysParams.GroupBy(item => item.type)
                                      .Select(group => new GetSystemParamModel
                                      {
                                          type = group.Key,
                                          values = group.Select(item => item.name).ToList()
                                      }).ToList();
         
                res.items = groupedData;
             
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }
    }
}
