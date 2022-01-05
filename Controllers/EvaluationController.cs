using System;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("{userId}/publication/{publicationId}")]
    public class EvaluationController : ControllerBase
    {
        private readonly IEvaluationServices _evaluationServices;
        public EvaluationController(IEvaluationServices evaluationServices)
        {
            _evaluationServices = evaluationServices;
        }

        [Route("comment/{id}")]
        [HttpGet]
        public ActionResult getComment(string publicationId, string id){
            try{
                return Ok(_evaluationServices.getOneComment(id, publicationId));
            }catch(Exception e){return NotFound(e.Message);}
        }

        [Route("evaluation/{evaluatorId}")]
        [HttpGet]
        public ActionResult getEvaluation([FromRoute] string publicationId, [FromRoute] string evaluatorId){
            try{
                return Ok(_evaluationServices.getOneEvaluation(evaluatorId, publicationId));
            }catch(Exception e){
                if(e.HResult == 400){
                    return BadRequest(e.Message);
                }else if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{
                    return StatusCode(500);
                }
            }
        }

        [Route("evaluate")]
        [HttpPost]
        public ActionResult evaluate([FromBody] Evaluation obj){
            try{
                var result = _evaluationServices.createEvaluation(obj);
                return Created($"https://www.travel.Api/{obj.UserId}/evaluated/{obj.PubId}/", result);
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{return StatusCode(500);}
            }finally{Console.WriteLine($"{obj.UserId} passou aqui");}
        }

        [Route("comments")]
        [HttpPost]
        public ActionResult comments([FromBody] Comment obj){
            try{
                var result = _evaluationServices.createComment(obj);
                return Created($"https://www.travel.Api/{obj.UserId}/publication/{obj.PubId}/comment/{obj.PubId}/", result);
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{return StatusCode(500);}
            }finally{Console.WriteLine($"{obj.Id} passou aqui");}
        }

        [Route("reply")]
        [HttpPost]
        public ActionResult reply([FromBody] Reply obj){
            try{
                var result = _evaluationServices.reply(obj);
                return Created($"https://www.travel.Api/{obj.UserId}/publication/{obj.PubId}/comment/{obj.PubId}/", result);
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{return StatusCode(500);}
            }finally{Console.WriteLine($"{obj.Id} passou aqui");}
        }

        [Route("comments")]
        [HttpGet]
        public ActionResult getAllComments([FromRoute] string userId, [FromRoute] string publicationId){
            try{
                return Ok(_evaluationServices.getAllComments(publicationId));
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{return StatusCode(500);}
            }finally{Console.WriteLine($"A rota https://www.travel.Api/{userId}/publication/{publicationId}/comments");}
        }

        [Route("comment/{commentId}/upvote")]
        [HttpPost]
        public ActionResult upVote([FromBody] Vote obj){
            try{
                _evaluationServices.upVote(obj);
                return Created("Criado", obj);
            }catch(Exception e){
                if(e.HResult == 404){
                    return BadRequest(e.Message);
                }else{return StatusCode(500);}
            }
        }

        [Route("comment/{commentId}/delete/upvote/{evaluatorId}")]
        [HttpDelete]
        public ActionResult deleteUpVote([FromRoute] string commentId, [FromRoute] string evaluatorId){
            try{
                Vote obj = new Vote{
                    UserId = evaluatorId,
                    CommentId = commentId
                };
                _evaluationServices.deleteUpVote(obj);
                return NoContent();
            }catch(Exception e){
                if(e.HResult == 404){
                    return BadRequest(e.Message);
                }else{return StatusCode(500);}
            }
        }

        [Route("comment/{commentId}/downvote")]
        [HttpPost]
        public ActionResult downVote([FromBody] Vote obj){
            try{
                _evaluationServices.downVote(obj);
                return Created("Criado", obj);
            }catch(Exception e){
                if(e.HResult == 404){
                    return BadRequest(e.Message);
                }else{return StatusCode(500);}
            }
        }

        [Route("comment/{commentId}/delete/downvote/{evaluatorId}")]
        [HttpDelete]
        public ActionResult deleteDownVote([FromRoute] string commentId, [FromRoute] string evaluatorId){
            try{
                Vote obj = new Vote{
                    UserId = evaluatorId,
                    CommentId = commentId
                };
                _evaluationServices.deleteDownVote(obj);
                return NoContent();
            }catch(Exception e){
                if(e.HResult == 404){
                    return BadRequest(e.Message);
                }else{return StatusCode(500);}
            }
        }

        [Route("evaluations")]
        [HttpGet]
        public ActionResult getAllEvaluations([FromRoute] string id){
            try{
                return Ok(_evaluationServices.getAllEvaluations(id));
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{return StatusCode(500);}
            }
        }

        [Route("comment/delete")]
        [HttpDelete]
        public ActionResult deleteComment(string id){
            try{
                _evaluationServices.deleteComment(id);
                return NoContent();
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{return StatusCode(500);}
            }
        }

        [Route("evaluation/delete")]
        [HttpDelete]
        public ActionResult deleteEvaluation(string userId, string pubId){
            try{
                _evaluationServices.deleteEvaluation(userId, pubId);
                return NoContent();
            }catch(Exception e){
                if(e.HResult == 404){
                    return NotFound(e.Message);
                }else{return StatusCode(500);}
            }
        }
    }
}