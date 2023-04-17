using Microsoft.AspNetCore.Mvc;
using TraTech.WebSocketHub;

namespace ExampleWebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketController : ControllerBase
    {
        private readonly WebSocketHub<int> _webSocketHub;

        public WebSocketController(WebSocketHub<int> webSocketHub)
        {
            _webSocketHub = webSocketHub;
        }

        [HttpGet("SendWhere")]
        public async Task<IActionResult> SendWhere(int id)
        {
            await _webSocketHub.SendWhere(
                new Message
                {
                    Type = "SendWhere",
                    Payload = new
                    {
                        data1 = "SendWhereData1",
                        data2 = "SendWhereData2"
                    }
                },
                (key) => key > id
            );
            return Ok();
        }

        [HttpGet("Send")]
        public async Task<IActionResult> Send(int id)
        {
            await _webSocketHub.Send(
                new Message
                {
                    Type = "Send",
                    Payload = new
                    {
                        data1 = "SendData1",
                        data2 = "SendData2"
                    }
                },
                id
            );
            return Ok();
        }

        [HttpGet("Send2")]
        public async Task<IActionResult> Send2(int id)
        {
            var socketList = _webSocketHub.GetSocketList(id);
            await _webSocketHub.Send(
                new Message
                {
                    Type = "Send2",
                    Payload = new
                    {
                        data1 = "Send2Data1",
                        data2 = "Send2data2"
                    }
                },
                socketList.First()
            );
            return Ok();
        }

        [HttpGet("Send3")]
        public async Task<IActionResult> Send4(int id)
        {
            var socketList = _webSocketHub.GetSocketList(id);
            await _webSocketHub.Send(
                new Message
                {
                    Type = "Send3",
                    Payload = new
                    {
                        data1 = "Send3Data1",
                        data2 = "Send3Data2"
                    }
                },
                socketList.ToArray()
            );
            return Ok();
        }

        [HttpGet("SendAll")]
        public async Task<IActionResult> SendAll()
        {
            await _webSocketHub.SendAll(
                new Message
                {
                    Type = "SendAll",
                    Payload = new
                    {
                        data1 = "SendAllData1",
                        data2 = "SendAllData2"
                    }
                }
            );
            return Ok();
        }

        [HttpGet("RemoveFirst")]
        public async Task<IActionResult> RemoveFirst(int id)
        {
            await _webSocketHub.RemoveFirst(
                (key) => key == id
            );
            return Ok();
        }

        [HttpGet("RemoveWhere")]
        public async Task<IActionResult> RemoveWhere(int id)
        {
            await _webSocketHub.RemoveWhere(
                (key) => key > id
            );
            return Ok();
        }

        [HttpGet("RemoveAll")]
        public async Task<IActionResult> RemoveAll()
        {
            await _webSocketHub.RemoveAll();
            return Ok();
        }
    }
}