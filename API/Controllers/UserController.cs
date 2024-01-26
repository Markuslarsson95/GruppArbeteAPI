﻿using Application.Commands.UserCommands.RegisterUser;
using Application.Queries.UserQueries;
using Application.Queries.UserQueries.GetAllUsers;
using Application.Validators;
using Infrastructure.Repository.UserRepository;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Commands.WalletCommands;
using Application.Queries.UserQueries.LoginUser;
using Application.Commands.UserCommands.UpdateUser;
using Application.Dtos;
using Application.Queries.PurchaseHistoriesQueries;


namespace API.Controllers
{
    public class UserController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IUserRepository _userRepository;
        private readonly PasswordValidator _passwordValidator;
        private readonly UsernameValidator _usernameValidator;


        public UserController(IMediator mediator, IUserRepository userRepository, PasswordValidator passwordValidator, UsernameValidator usernameValidator)
        {
            _mediator = mediator;
            _userRepository = userRepository;
            _passwordValidator = passwordValidator;
            _usernameValidator = usernameValidator;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult> Register(string username, string password)
        {
            var validationResult = _usernameValidator.Validate(username);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var passwordResult = _passwordValidator.Validate(password);
            if (!passwordResult.IsValid)
            {
                return BadRequest(passwordResult.Errors);
            }

            var user = await _mediator.Send(new RegisterUserCommand { Username = username, Password = password });

            return Ok(new { Message = "Register successful", user.UserId, user.UserName });
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login(string username, string password)
        {
            var usernameResult = _usernameValidator.Validate(username);
            if (!usernameResult.IsValid)
            {
                return BadRequest(usernameResult.Errors);
            }

            var passwordResult = _passwordValidator.Validate(password);
            if (!passwordResult.IsValid)
            {
                return BadRequest(passwordResult.Errors);
            }


            var user = await _mediator.Send(new LoginUserQuery { Username = username, Password = password });

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return Unauthorized("Invalid username or password.");
            }

            // Return some kind of success response, without JWT for now
            // You might want to return a user object or a simple success 
            return Ok(new { Message = "Login successful", user.UserId, user.UserName });
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                return Ok(await _mediator.Send(new GetAllUsersQuery()));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("GetAllPurchaseHistories")]
        public async Task<IActionResult> GetAllPurchaseHistoriesAsync()
        {
            try
            {
                var purchaseHistories = await _mediator.Send(new GetAllPurchaseHistoriesQuery());
                return Ok(purchaseHistories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("delete/{userId:guid}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            try
            { //validator??
                var deletedUser = await _userRepository.DeleteUserAsync(userId);

                if (deletedUser != null)
                {

                    return Ok(new { Message = "User deleted successfully", deletedUser.UserId, deletedUser.UserName });
                }
                else
                {
                    return NotFound("User not found");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("{userId}/wallet")]
        public async Task<ActionResult<WalletDto>> UpdateWalletById([FromRoute] Guid userId, [FromBody] WalletDto walletDto)
        {
            var walletValidator = new WalletValidator();
            var validationResult = walletValidator.Validate(walletDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var command = new UpdateWalletByIdCommand(userId, walletDto);
            var updatedWallet = await _mediator.Send(command);
            return Ok(updatedWallet);
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserByIdCommand command)
        {
            if (id != command.UserId)
            {
                return BadRequest("Mismatched User ID.");
            }

            var result = await _mediator.Send(command);
            if (result == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(result);
        }


    }
}
