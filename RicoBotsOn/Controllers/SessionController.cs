using Application.Exceptions;
using Application.Sessions;
using Microsoft.AspNetCore.Mvc;

namespace RicoBotsOn.Controllers
{
    [ApiController]
    [Route("sessions")]
    public class SessionsController : ControllerBase
    {
        private readonly SessionService _sessionService;

        public SessionsController(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpPost]
        public ActionResult<CreateSessionResponse> CreateSession([FromBody] CreateSessionRequest request)
        {
            try 
            { 
                var result = _sessionService.CreateSession(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{code}/join")]
        public ActionResult<JoinResponse> Join(string code, [FromBody] JoinRequest request)
        {
            try
            {
                var result = _sessionService.Join(code, request);
                return Ok(result);
            }
            catch (SessionNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{code}/move")]
        public ActionResult<MoveResponse> SubmitMove(string code, [FromBody] MoveRequest request)
        {
            try
            {
                var result = _sessionService.SubmitMove(code, request);
                return Ok(result);
            }
            catch (SessionNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{code}/undo")]
        public ActionResult<MoveResponse> UndoMove(string code, [FromBody] UndoRequest request)
        {
            try
            {
                return Ok(_sessionService.UndoMove(code, request));
            }
            catch (SessionNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{code}/board")]
        public ActionResult GetBoard(string code)
        {
            try
            {
                var boardInfo = _sessionService.GetBoardState(code);
                return Ok(boardInfo);
            }
            catch (SessionNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("{code}/claims")]
        public ActionResult<ClaimResponse> SubmitClaim(string code, [FromBody] ClaimRequest request)
        {
            try
            {
                var result = _sessionService.SubmitClaim(code, request);
                return Ok(result);
            }
            catch (SessionNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{code}/state")]
        public ActionResult<SessionStateResponse> GetState(string code)
        {
            try
            {
                var sessionState = _sessionService.GetSessionState(code);
                return Ok(sessionState);
            }
            catch (SessionNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{code}/next-round")]
        public ActionResult<NextRoundResponse> StartNextRound(string code)
        {
            try
            {
                var result = _sessionService.StartNextRound(code);
                return Ok(result);
            }
            catch (SessionNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{code}/reset")]
        public ActionResult<ResetResponse> ResetProvingAttempt(string code, [FromBody] ResetRequest request)
        {
            try
            {
                var result = _sessionService.ResetProvingAttempt(code, request);
                return Ok(result);
            }
            catch (SessionNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
