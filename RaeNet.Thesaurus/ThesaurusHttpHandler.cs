using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RaeNet.Thesaurus
{
    class ThesaurusHttpHandler: HttpClientHandler
    {
        readonly int MaxLinesToRead = 30;
        static Regex HtmlArticleBeginRegex;
        static ThesaurusHttpHandler()
        {
            HtmlArticleBeginRegex = new Regex(@"\<(\s*?)article", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
        public ThesaurusHttpHandler()
            : base()
        {
            this.AllowAutoRedirect = false;
            this.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            this.ClientCertificateOptions = ClientCertificateOption.Automatic;
            this.MaxAutomaticRedirections = 100; //magin number
            this.PreAuthenticate = false;
            this.UseCookies = false;
            this.UseDefaultCredentials = false;
            this.UseProxy = false;
        }

        public override bool SupportsAutomaticDecompression
        {
            get
            {
                return true;
            }
        }

        public override bool SupportsRedirectConfiguration
        {
            get
            {
                return true;
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return response;

            response.EnsureSuccessStatusCode();
            //process message to know if word is found
            return await CheckIfFound(response);
        }

        private async Task<HttpResponseMessage> CheckIfFound(HttpResponseMessage response)
        {
            bool foundHtmlArticle = false;
            using (var responseBodyStream = await response.Content.ReadAsStreamAsync())
            using (var responseReader = new StreamReader(responseBodyStream))
            {
                int currentLine = 0;
                while (currentLine++ < MaxLinesToRead)
                {
                    var line = await responseReader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    foundHtmlArticle = HtmlArticleBeginRegex.IsMatch(line);
                    if (foundHtmlArticle)
                        break;
                }
            }

            if (!foundHtmlArticle)
            {
                response.StatusCode = HttpStatusCode.NotFound;
            }
            return response;
        }
    }
}
