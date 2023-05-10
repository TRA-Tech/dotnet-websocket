using Microsoft.AspNetCore.Mvc;
using TraTech.WebSocketHub;

namespace ExampleWebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocket1Controller : ControllerBase
    {
        private readonly WebSocketHub<int> _webSocketHub;

        public WebSocket1Controller(WebSocketHub<int> webSocketHub)
        {
            _webSocketHub = webSocketHub;
        }

        [HttpGet("GetSocketList")]
        public IActionResult GetSocketList(int id)
        {
            var socketListOfUser = _webSocketHub.GetSocketList(id);
            return Ok(socketListOfUser);
        }

        [HttpGet("GetSocketListWithSelector")]
        public IActionResult GetSocketListWithSelector(int id)
        {
            var socketListOfUser = _webSocketHub.GetSocketList((key) => key == id);
            return Ok(socketListOfUser);
        }

        [HttpGet("RemoveAsyncWithKey")]
        public async Task<IActionResult> RemoveAsync(int id)
        {
            var firstSocketOfUser = _webSocketHub.GetSocketList(id).First();
            await _webSocketHub.RemoveAsync(
                id,
                firstSocketOfUser
            );
            return Ok(firstSocketOfUser);
        }

        [HttpGet("RemoveAsyncWithSelector")]
        public async Task<IActionResult> RemoveWithSelector(int id)
        {
            var firstSocketOfUser = _webSocketHub.GetSocketList(id).First();
            await _webSocketHub.RemoveAsync(
                (key) => key == id,
                firstSocketOfUser
            );
            return Ok(firstSocketOfUser);
        }

        [HttpGet("RemoveFirstAsync")]
        public async Task<IActionResult> RemoveFirstAsync(int id)
        {
            await _webSocketHub.RemoveFirstAsync(
                (key) => key > id
            );
            return Ok();
        }

        [HttpGet("RemoveWhereAsync")]
        public async Task<IActionResult> RemoveWhereAsync(int id)
        {
            await _webSocketHub.RemoveWhereAsync(
                (key) => key > id
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
            var socketListOfUser = _webSocketHub.GetSocketList(id);
            await _webSocketHub.SendAsync(message, socketListOfUser.ToArray());
            return Ok();
        }

        [HttpGet("SendAsyncWithKey")]
        public async Task<IActionResult> SendAsyncWithKey(int id)
        {
            var message = new Message()
            {
                Type = "SendAsyncWithKey",
                Payload = new
                {
                    Data = "SendAsyncWithKey"
                }
            };
            await _webSocketHub.SendAsync(message, id);
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
            await _webSocketHub.SendAsync(message, (key) => key == id);
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
            await _webSocketHub.SendWhereAsync(message, (key) => key > id);
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