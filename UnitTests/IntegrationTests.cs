using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;

namespace IntegrationTests
{
    public class IntegrationTest
    {
        private static readonly RegisterDto NewUser = new RegisterDto() { Name = "newUser", Password = "newUserPass" };
        private static readonly HttpClient client = new HttpClient();

        [Fact]
        public async Task RegisterTests()
        {
            var res = await client.PostAsync("api/account/register", new StringContent(JsonConvert.SerializeObject(NewUser)));
        }
    }
}
