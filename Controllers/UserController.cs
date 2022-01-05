using API.Models;
using Microsoft.AspNetCore.Mvc;
using API.Interfaces;
using System;
using API.DTOs;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{

    [EnableCors("MyPolicy")]
    [Route("user")]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly IEmailServices _emailServices;
        private readonly ITokenServices _tokenServices;
        public UserController(IUserServices userServices, IEmailServices emailServices, ITokenServices tokenServices)
        {
            _userServices = userServices;
            _emailServices = emailServices;
            _tokenServices = tokenServices;
        }

        //user/id
        [Route("{id}")]
        [HttpGet]
        public ActionResult get(string id){
            try{
                return Ok(_userServices.getUserById(id));
            }catch(Exception e){
                return NotFound(e.Message);
            }
        }

        [Route("{id}/followers")]
        [HttpGet]
        [Authorize]
        public ActionResult getFollowers(string id){
            try{
                return Ok(_userServices.getFollowers(id));
            }catch(Exception e){
                return NotFound(e.Message);
            }
        }

        [Route("{id}/followeds")]
        [HttpGet]
        [Authorize]
        public ActionResult getFolloweds(string id){
            try{
                return Ok(_userServices.getFolloweds(id));
            }catch(Exception e){
                return NotFound(e.Message);
            }
        }

        [Route("validate/id/{id}")]
        [HttpGet]
        public ActionResult IsUserRegistered([FromRoute] string id){
            try{
                return Ok(
                    new {
                    result = _userServices.IsUserRegistered(id)});
            }catch(Exception e){
                if(e.HResult == 400){
                    return BadRequest(e.Message);
                }else{
                    return StatusCode(500);
                }
            }
        }

        [Route("validate/email/{email}")]
        [HttpGet]
        public ActionResult IsEmailRegistered([FromRoute] string email){
            try{
                return Ok(new {result = _userServices.IsEmailRegistered(email)});
            }catch(Exception e){
                if(e.HResult == 400){
                    return BadRequest(e.Message);
                }else{
                    return StatusCode(500);
                }
            }
        }
        [Route("verify/email/{email}&{code}")]
        [HttpGet]
        public ActionResult verifyEmail([FromRoute] string email, [FromRoute] string code){
            try{
                _emailServices.sendEmail(email, code);
                return Ok("enviado");
            }catch{
                return StatusCode(500);
            }
        }

        [Route("validate/phone/{phone}")]
        [HttpGet]
        public ActionResult IsPhoneRegistered([FromRoute] string phone){
            try{
                return Ok(new {
                        result = _userServices.IsPhoneRegistered(phone)});

            }catch(Exception e){
                if(e.HResult == 400){
                    return BadRequest(e.Message);
                }else{
                    return StatusCode(500);
                }
            }
        }

        [Route("create")]
        [HttpPost]
        public ActionResult create([FromBody] User user){
            try{
                var result = _userServices.create(user);
                var login = new Login{
                    Email = user.Email,
                    Password = user.Password
                };
                var token = _tokenServices.generateToken(result, login);
                var refreshToken = _tokenServices.generateRefreshToken();
                _tokenServices.saveRefreshToken(user.Email, refreshToken);

                var response = new {
                    user = result,
                    token = token,
                    refreshToken = refreshToken
                };

                return Created($"https://www.travel.Api/user/{result.Id}",response);
            }catch(Exception e){
                if(e.HResult == 400){
                    return BadRequest(e.Message);
                }else if(e.HResult == 200){
                    return Ok(e.Message);
                }else{return StatusCode(500);}
            }
        }

        //Update methods
        [Route("update/id")]
        [HttpPut]
        [Authorize]
        public ActionResult updateId([FromBody] UserIdDto obj){
            try{
                var result = _userServices.updateUserId(obj);
                return Ok(new{
                    message = $"Id atualizado para: {result.Id}"
                });
            }catch(Exception e){
                if(int.Parse(e.Message) == 200){
                    return Ok("Identificador não disponivel");
                }else if(e.HResult == 404){
                    return NotFound("404: usuario não encontrado");
                }else{return StatusCode(500);}
            }       
        }
        
        [Route("update/name")]
        [HttpPut]
        [Authorize]
        public ActionResult updateName([FromBody] UserNameDto obj){
            try{
                var result = _userServices.updateUserName(obj);
                return Ok(new{
                    message = $"Nome atualizado para: {result.Name}"
                });
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{return StatusCode(500);}
            }   
        }

        [Route("update/email")]
        [HttpPut]
        [Authorize]
        public ActionResult updateEmail([FromBody] UserEmailDto obj){
            try{
                var result = _userServices.updateUserEmail(obj);
                return Ok(new{
                    message = "Email atualizado"
                });
            }catch (Exception e){
                if(e.HResult == 200){
                    return Ok(e.Message);
                }else if(e.HResult == 400){
                    return BadRequest(e.Message);
                }
                else if(e.HResult == 404){
                    return BadRequest(e.Message);  
                }else{return StatusCode(500);}
              }
        }

        [Route("update/phone")]
        [HttpPut]
        [Authorize]
        public ActionResult updatePhone([FromBody] UserPhoneDto obj){
            try{
                var result = _userServices.updateUserPhone(obj);
                return Ok(new{
                    message = "Telefone atualizado"
                });
            }catch (Exception e){
                if(e.HResult == 200){
                    return Ok(e.Message);
                }else if(e.HResult == 400){
                    return BadRequest(e.Message);
                }else if(e.HResult == 404){
                    return NotFound(e.Message);  
                }else{return StatusCode(500);}
            }
        }

        [Route("update/password")]
        [HttpPut]
        [Authorize]
        public ActionResult updatePassword([FromBody] UserPasswordDto obj){
            try{
                var result = _userServices.updateUserPassword(obj);
                return Ok(new{
                    message = "Senha atualizada"
                });
            }catch (Exception e){
                if(e.HResult == 400){
                    return BadRequest(e.Message);
                }else if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{return StatusCode(500);}
            }
        }

        [Route("forgot/{email}")]
        [HttpPut]
        [Authorize]
        public ActionResult resetPassword([FromRoute] string email){
            try{
                var password = Guid.NewGuid().ToString("d").Substring(1,16);
                _userServices.resetPassword(email, password);
                _emailServices.sendNewPassword(email, password);
                return Ok(new{
                    message = "Verifique sua caixa de email"
                });
            }catch (Exception e){
                if(e.HResult == 400){
                    return BadRequest(e.Message);
                }else if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{return StatusCode(500);}
            }
        }

        [Route("update/profileImage")]
        [HttpPut]
        [Authorize]
        public ActionResult updateProfileImage([FromBody] UserImageDto obj){
            try{
                _userServices.updateUserProfileImageUrl(obj.userId, obj.ImageUrl);
                return Ok(new{
                    message = "Imagem atualizada"
                });
            }catch (Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{return StatusCode(500);}
            }
        }

        [Route("update/profileBanner")]
        [HttpPut]
        [Authorize]
        public ActionResult updateBannerImage([FromBody] UserImageDto obj){
            try{
                _userServices.updateUserBannerUrl(obj.userId, obj.ImageUrl);
                return Ok(new{
                    message = "Imagem atualizada"
                });
            }catch (Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{return StatusCode(500);}
            }
        }
        
        [Route("delete")]
        [HttpDelete]
        [Authorize]
        public ActionResult deleteUser([FromBody] DeleteUserDto obj){
            try{
                _userServices.deleteUser(obj);
                return NoContent();
            }catch(Exception e){
                if(e.HResult == 400){
                    return BadRequest(e.Message);
                }else if(e.HResult == 404){
                    return NotFound(e.Message + obj.Id);
                }else{return StatusCode(500);}
            }
        }

        [Route("follow")]
        [HttpPost]
        [Authorize]
        public ActionResult follow([FromBody] FollowDto obj){
            try{
                _userServices.followUser(obj);
                return Created("https://www.travel.Api.com/follow", obj);
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{return StatusCode(500);}
            }
        }

        [Route("unfollow")]
        [HttpDelete]
        [Authorize]
        public ActionResult unfollow([FromBody] FollowDto obj){
            try{
                _userServices.unfollowUser(obj);
                return NoContent();
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{return StatusCode(500);}
            }
        }

    }
}

