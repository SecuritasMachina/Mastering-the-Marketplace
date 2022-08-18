using BackupCoordinatorV2.Utils;
using Common.DTO.V2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace BackupCoordinatorV2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DirListingController : ControllerBase
    {
       

        private readonly ILogger<DirListingController> _logger;

        public DirListingController(ILogger<DirListingController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<DirListingDTO> Get()
        {
            string stm = "select id,msg from mycache where id=@myId";
            SqliteCommand cmd2 = new SqliteCommand(stm, DBSingleTon.Instance.getCon());
            cmd2.Parameters.AddWithValue("@myId", "dirListing-ab50c41e-3814-4533-8f68-a691b4da9043");

            cmd2.Prepare();
            GenericMessage genericMessage = new GenericMessage();
            SqliteDataReader myReader = cmd2.ExecuteReader();
            while (myReader.Read())
            {
                
                genericMessage = JsonConvert.DeserializeObject<GenericMessage>(myReader.GetString(1));
            }
            //json = JsonConvert.SerializeObject(genericMessage);
            //genericMessage.msg
            return Enumerable.Range(0,1000).Select(index => new DirListingDTO
            {
               // Date = DateTime.Now.AddDays(index),
                //TemperatureC = Random.Shared.Next(-20, 55),
               // Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}