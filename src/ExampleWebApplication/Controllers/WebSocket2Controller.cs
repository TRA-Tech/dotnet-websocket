using ExampleWebApplication.WebSocketHubKeys;
using Microsoft.AspNetCore.Mvc;
using TraTech.WebSocketHub.Core;

namespace ExampleWebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocket2Controller : ControllerBase
    {
        private readonly WebSocketHub<SocketUser> _webSocketHub;

        public WebSocket2Controller(WebSocketHub<SocketUser> webSocketHub)
        {
            _webSocketHub = webSocketHub;
        }

        [HttpGet("GetSocketListWithSelector")]
        public IActionResult GetSocketListWithSelector(int id)
        {
            var socketListOfUser = _webSocketHub.GetSocketList((key) => key.Id == id);
            return Ok(socketListOfUser);
        }

        [HttpGet("RemoveAsyncWithSelector")]
        public async Task<IActionResult> RemoveWithSelector(int id)
        {
            var firstSocketOfUser = _webSocketHub.GetSocketList((key) => key.Id == id).First();
            await _webSocketHub.RemoveAsync(
                (key) => key.Id == id,
                firstSocketOfUser
            );
            return Ok(firstSocketOfUser);
        }

        [HttpGet("RemoveFirstAsync")]
        public async Task<IActionResult> RemoveFirstAsync(int id)
        {
            await _webSocketHub.RemoveFirstAsync(
                (key) => key.Id > id
            );
            return Ok();
        }

        [HttpGet("RemoveWhereAsync")]
        public async Task<IActionResult> RemoveWhereAsync(int id)
        {
            await _webSocketHub.RemoveWhereAsync(
                (key) => key.Id > id
            );
            return Ok();
        }

        [HttpGet("RemoveAllAsync")]
        public async Task<IActionResult> RemoveAllAsync()
        {
            await _webSocketHub.RemoveAllAsync();
            return Ok();
        }

        [HttpGet("SendAsyncWithSocketList")]
        public async Task<IActionResult> SendAsyncWithSocketList(int id)
        {
            var message = new Message()
            {
                Type = "SendAsyncWithSocketList",
                Payload = new
                {
                    Data = "SendAsyncWithSocketList"
                }
            };
            var socketListOfUser = _webSocketHub.GetSocketList((key) => key.Id == id);
            await _webSocketHub.SendAsync(message, socketListOfUser.ToArray());
            return Ok();
        }

        [HttpGet("SendAsyncWithSelector")]
        public async Task<IActionResult> SendAsyncWithSelector(int id)
        {
            var message = new Message()
            {
                Type = "SendAsyncWithSelector",
                Payload = new
                {
                    Data = "SendAsyncWithSelector"
                }
            };
            await _webSocketHub.SendAsync(message, (key) => key.Id == id);
            return Ok();
        }

        [HttpGet("SendWhereAsync")]
        public async Task<IActionResult> SendWhereAsync(int id)
        {
            var message = new Message()
            {
                Type = "SendWhereAsync",
                Payload = new
                {
                    Data = "SendWhereAsync"
                }
            };
            await _webSocketHub.SendWhereAsync(message, (key) => key.Id > id);
            return Ok();
        }

        [HttpGet("SendAllAsync")]
        public async Task<IActionResult> SendAllAsync()
        {
            var message = new Message()
            {
                Type = "SendAllAsync",
                Payload = new
                {
                    Data = "SendAllAsync"
                }
            };
            await _webSocketHub.SendAllAsync(message);
            return Ok();
        }
    }
}