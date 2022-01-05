using System;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{   
    [EnableCors]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly ITokenServices _tokenServices;
        public LoginController(IUserServices userServices, ITokenServices tokenServices)
        {
            _userServices = userServices;
            _tokenServices = tokenServices;
        }

        [HttpPost]
        [Route("login")]
        public ActionResult authenticate([FromBody] Login login){
            try{
                var user = _userServices.login(login);
                var token = _tokenServices.generateToken(user, login);
                var refreshToken = _tokenServices.generateRefreshToken();
                _tokenServices.saveRefreshToken(login.Email, refreshToken);
                return Ok(
                    new{
                        user = user,
                        token = token,
                        refreshToken = refreshToken
                    }
                );
            }catch(Exception e){
                if(e.HResult == 400)
                    return BadRequest(e.Message);
                else if(e.HResult == 404)
                    return BadRequest("Senha ou email incorretos");
                else
                    return StatusCode(500);
            }finally{
                Console.WriteLine($"{login.Email} acessou este controller");
            }

        }

        // [HttpPost]
        // [Route("refresh")]
        // public ActionResult getNewToken(string token, string refreshToken){
        //     try{
        //         var principal = _tokenServices.getPrincipalFromExpiredToken(token);
        //         var email = principal.Identity.Email;
        //         var savedRefreshToken = _tokenServices.getRefreshToken(email);
        //         if(savedRefreshToken != refreshToken){throw new SecurityTokenException("RefreshToken invalido");}

        //         var newtoken = _tokenServices.generateToken()
        //     }catch(Exception e){

        //     };
        // }

        [HttpPost]
        [Route("google")]
        [Authorize]
        public ActionResult loginWithGoogle([FromBody] Login login){
            try{
                var user = _userServices.loginWithGoogle(login);
                var token = _tokenServices.generateToken(user, login);
                var refreshToken = _tokenServices.generateRefreshToken();
                _tokenServices.saveRefreshToken(login.Email, refreshToken);
                return Ok(
                    new{
                        user = user,
                        token = token,
                        refreshToken = refreshToken
                    }
                );
            }catch(Exception e){
                if(e.HResult == 400)
                    return BadRequest(e.Message);
                else
                    return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("facebook")]
        [Authorize]
        public ActionResult loginWithFacebook([FromBody] Login login){
            try{
                var user = _userServices.loginWithFacebook(login);
                var token = _tokenServices.generateToken(user, login);
                var refreshToken = _tokenServices.generateRefreshToken();
                _tokenServices.saveRefreshToken(login.Email, refreshToken);
                return Ok(
                    new{
                        user = user,
                        token = token,
                        refreshToken = refreshToken
                    }
                );
            }catch(Exception e){
                if(e.HResult == 400)
                    return BadRequest(e.Message);
                else
                    return StatusCode(500);
            }
        }
    }
}