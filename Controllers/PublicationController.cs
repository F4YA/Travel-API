using System;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("{userId}/publication")]
    public class PublicationController : ControllerBase
    {

        private readonly IPublicationServices _publicationServices;

        public PublicationController(IPublicationServices publicationServices)
        {
            _publicationServices = publicationServices;
        }

        [Route("single/{id}")]
        [HttpGet]
        [Authorize]
        public ActionResult getPublication(string id){
            try{
                return Ok(_publicationServices.getOne(id));
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{
                    return StatusCode(500);
                }
            }finally{
                Console.WriteLine("A rota https://www.travel.Api/{userId}/publication/single/{id} foi acessada");
            }
        }

        [Route("all")]
        [HttpGet]
        [Authorize]
        public ActionResult getAllPublications(string userId){
            try{
                return Ok(_publicationServices.getAll(userId));
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{
                    return StatusCode(500);
                }
            }finally{
                Console.WriteLine("A rota https://www.travel.Api/{userId}/publication/all foi acessada");
            }
        }

        [Route("{id}")]
        [HttpGet]
        [Authorize]
        public ActionResult getThread(string userId, string id){
            try{
                return Ok(_publicationServices.getThread(userId, id));
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{
                    return StatusCode(500);
                }
            }finally{
                Console.WriteLine("A rota https://www.travel.Api/{userId}/publication/{id} foi acessada");
            }
        }

        [Route("publish")]
        [HttpPost]
        //[Authorize]
        public ActionResult create([FromRoute] string userId, [FromBody] Publication publication){
            try{
                var result = _publicationServices.create(userId: userId, publication: publication);
                return Created($"https://www.travel.Api/{userId}/publication/{result.Id}",result);
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else if(e.HResult == 400){
                    return BadRequest(e.Message);
                }else{
                    return StatusCode(500);
                }
            }finally{
                Console.WriteLine($"A rota https://www.travel.Api/{userId}/publication/publish foi acessada");
            }
        }

        [Route("{id}/delete")]
        [HttpDelete]
        [Authorize]
        public ActionResult delete(string id){
            try{
                _publicationServices.deleteThread(id);
                return NoContent();
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else if(e.HResult == 400){
                    return BadRequest(e.Message);
                }else{
                    return StatusCode(500);
                }
            }
        }

        [Route("search/{query}")]
        [HttpGet]
        [Authorize]
        public ActionResult search([FromRoute] string userId, [FromRoute] string query){
            try{
                return Ok(_publicationServices.searchPublications(userId, query));
            }catch(Exception e){
                if(e.HResult == 404){
                    return BadRequest(e.Message);
                }else{return StatusCode(500);}
            }
        }

    }
}