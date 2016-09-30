using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RaeNet.Thesaurus
{
    /// <summary>
    /// Queries RAE (undocumented) web services for thesaurus requirements
    /// </summary>
    public class RawThesaurusClient
    {
        const string BaseUrl = "http://dle.rae.es/srv/search";

        /// <summary>
        /// Consults whether a specified word is recognized by RAE
        /// </summary>
        /// <param name="word"></param>
        public async Task<bool> WordExists(string word)
        {
            UriBuilder uriBuilder = new UriBuilder(BaseUrl);
            uriBuilder.Query = string.Format("w={0}", Uri.EscapeDataString(word));
            var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());

            using (var client = new HttpClient(new ThesaurusHttpHandler(), true))
            {
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                return response.IsSuccessStatusCode;
            }
        }
    }
}
