using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("home")]
    public class HomeController : ControllerBase
    {
        
        private readonly IHomeServices _homeServices;
        public HomeController(IHomeServices homeServices)
        {
            _homeServices = homeServices;
        }

        [Route("search/{query}")]
        [HttpGet]
        [Authorize]
        public ActionResult search([FromRoute] string query){
            try{
                return Ok(_homeServices.search(query));
            }catch(Exception e){
                if(e.HResult == 400){
                    return BadRequest(e.Message);
                }else{
                    return StatusCode(500);
                }
            }
        }

        [Route("recommended/{userId}")]
        [HttpGet]
        [Authorize]
        public ActionResult showsPublicationsAccordingUserInterests([FromRoute]string userId){
            try{
                //IEnumerable[] publicationsArray = new Enumerable[];
                var publicationsInteresting = _homeServices.showsPublicationsAccordingUserInterests(userId);
                var recentPublications = _homeServices.showsMostRecentPublicationsAccordingUserInterests(userId);
                var followingPublications = _homeServices.showsMostPopularPublicationsAccordingUserFollowing(userId);
                IEnumerable<PublicationDto> publications = Enumerable.Empty<PublicationDto>();
                IEnumerable<PublicationDto>[] x = new  IEnumerable<PublicationDto>[3]{publicationsInteresting, recentPublications, followingPublications};
                for (int i = 0;  i < x.Count(); i++)
                {
                    if(x[i].Any())
                     publications = publications.Concat(publications.Concat(x[i]));
                }
                
                if(publications.Any()){
                    return Ok(new{publications = publications});
                }else{
                    return NoContent();
                }
            }catch{
                return StatusCode(500);
            }
        }

        [Route("popular")]
        [HttpGet]
        [Authorize]
        public ActionResult showsMostPopularPublications(){
            try{
                var publications = _homeServices.showsMostPopularPublications();
                if(publications.Any()){
                    return Ok(new{publications = publications});
                }else{
                    return NoContent();
                }
            }catch{
                return StatusCode(500);
            }
        }

        [Route("notifications/{userId}")]
        [HttpGet]
        // [Authorize]
        public ActionResult notifications([FromRoute] string userId){
            try{
                var followers = _homeServices.showsMostRecentFollowers(userId);
                var comments = _homeServices.showsMostRecentComments(userId);
                var evaluations = _homeServices.showsMostRecentEvaluations(userId);
                return Ok(new {
                    followers = followers,
                    comments = comments,
                    evaluations = evaluations
                });
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{
                    return StatusCode(500);
                }
            }
        }

    }
}