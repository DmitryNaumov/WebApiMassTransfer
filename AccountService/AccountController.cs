using System.Web.Http;

namespace AccountService
{
    public sealed class AccountController : ApiController
    {
        [HttpPost]
        [Route("api/account")]
        public void Post(AccountChange[] accountChanges)
        {
        }
    }
}