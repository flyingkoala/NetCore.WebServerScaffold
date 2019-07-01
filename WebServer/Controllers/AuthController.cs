using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Model.Request;
using Model.Response;

namespace WebServer.Controllers
{


    /// <summary>
    /// api授权
    /// </summary>
    [Produces("application/json")]
    [Route("auth")]
    [EnableCors("CorsPolicy")]
    public class AuthController : Controller
    {

        private readonly ApiAuth apiauth;

        public AuthController(ConfigService service)
        {
            apiauth = service.config.apiauth;
        }


        [Route("login")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ApiAuthToken([FromBody]AuthReq_login request)
        {
            ResultModel resResult = new ResultModel();
            if (!ModelState.IsValid)
            {
                resResult.Failure(ResultCode.ArgumentVerifyFail.Description(), (int)ResultCode.ArgumentVerifyFail);
                await Task.Run(() => resResult);
                return Ok(resResult);
            }
            try
            {
                string msg = string.Empty;

                //验证用户名和密码

                //验证完成之后

                string authid = Guid.NewGuid().ToString();
                var claims = new[] {
                        new Claim("auth",authid)
                    };

                //sign the token using a secret key.This secret will be shared between your API and anything that needs to check that the token is legit.
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiauth.securitykey)); //_configuration["SecurityKey"]
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: DataKey.JWT_ValidIssuer,//签发者
                    audience: DataKey.JWT_ValidAudience,//接收方
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(apiauth.exp_auth),
                    signingCredentials: creds);

                var refreshToken = new JwtSecurityToken(
                    issuer: $"{DataKey.JWT_ValidIssuer}.",//签发者
                    audience: $"{DataKey.JWT_ValidAudience}.",//接收方
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(apiauth.exp_auth_refresh),
                    signingCredentials: creds);

                AuthRes_login res = new AuthRes_login();
                res.token = new JwtSecurityTokenHandler().WriteToken(token);
                res.refresh_token = new JwtSecurityTokenHandler().WriteToken(refreshToken);
                resResult.Success(res, "");

            }
            catch (Exception ex)
            {
                await Task.Run(() => ex.ToString());
                resResult.Failure(ex.ToString(), (int)ResultCode.ServiceInternalAbnormal);
            }
            return Ok(resResult);
        }
    }
}