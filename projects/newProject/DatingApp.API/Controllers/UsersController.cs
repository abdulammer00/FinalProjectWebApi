using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
   
    [ServiceFilter(typeof(LogUserActivity))]
  
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;


        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // GET: api/Users
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userParams">Typically, .NET CORE will figure out where to
        /// get the parameters from.  But we will give it a hint [FromQuery] so
        /// this will allow us to send an empty query string and .NET CORE
        /// will use the default values we specified in the UserParams class</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var userFromRepo = await _repo.GetUser(currentUserId, true);

            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = string.Equals(userFromRepo.Gender?.ToLowerInvariant(), "male") 
                    ? "female" : "male";

            var users = await _repo.GetUsers(userParams);

            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

         
            Response.AddPagination(users.CurrentPage, users.PageSize,
                users.TotalCount, users.TotalPages);

            return Ok(usersToReturn);
        }

      
        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser([FromRoute] int id)
        {
          
            var isCurrentUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) == id;

            var userFromRepo = await _repo.GetUser(id, isCurrentUser);

            if (userFromRepo == null)
                return NotFound();

            var userToReturn = _mapper.Map<UserForDetailedDto>(userFromRepo);

            return Ok(userToReturn);
        }

     
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser([FromRoute] int id, 
            [FromBody] UserForUpdateDto userForUpdateDto)
        {
           
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(id, true);

           
            _mapper.Map(userForUpdateDto, userFromRepo);

           
            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception($"Updating user {id} failed on saved.");
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
          
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            
            var like = await _repo.GetLike(id, recipientId);

            if (like != null)
                return BadRequest("You already liked this user.");

            if (await _repo.GetUser(recipientId, false) == null)
                return NotFound();

            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };

            _repo.Add(like);

            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to like user.");
        }
    }
}