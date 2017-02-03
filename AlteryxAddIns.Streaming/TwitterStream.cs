namespace JDunkerley.AlteryxAddIns.Streaming
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;

    using AlteryxRecordInfoNet;

    using Framework;

    using Framework.Factories;
    using Framework.Interfaces;

    public class TwitterStream : BaseTool<TwitterStream.Config, TwitterStream.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public class Config : IRecordLimit
        {
            public string ConsumerKey { get; set; }

            public string ConsumerSecret { get; set; }

            public string Keywords { get; set; }

            public string UserIds { get; set; }

            public long RecordLimit { get; set; }
        }

        public class Tweet
        {
            public string UserName { get; set; }

            public string Body { get; set; }

            public DateTime TimeStamp { get; set; }
        }

        public class Engine : BaseStreamEngine<Config, Tweet>
        {
            private string _bearerToken;

            private string _streamFilter;

            /// <summary>
            /// Constructor for Alteryx Engine
            /// </summary>
            public Engine()
                : this(new OutputHelperFactory())
            {
            }

            /// <summary>
            /// Create An Engine for unit testing.
            /// </summary>
            /// <param name="outputHelperFactory">Factory to create output helpers</param>
            internal Engine(IOutputHelperFactory outputHelperFactory)
                : base(null, outputHelperFactory)
            {
            }

            protected override IEnumerable<FieldDescription> CreateFieldDescriptions()
            {
                return
                    new [] {
                        OutputType.VWString.OutputDescription(nameof(Tweet.UserName), 256),
                        OutputType.VWString.OutputDescription(nameof(Tweet.Body), 256),
                        OutputType.DateTime.OutputDescription(nameof(Tweet.TimeStamp))
                        };
            }

            protected override void OnInitCalled()
            {
                this._streamFilter = "";
                if (!string.IsNullOrWhiteSpace(this.ConfigObject.Keywords))
                {
                    this._streamFilter += "&track=" + this.ConfigObject.Keywords;
                }
                if (!string.IsNullOrWhiteSpace(this.ConfigObject.UserIds))
                {
                    this._streamFilter += "&follow=" + this.ConfigObject.UserIds;
                }
                this._streamFilter = this._streamFilter.TrimStart('&');

                // Create Signature
                string signature = "POST&" + Uri.EscapeDataString("https://stream.twitter.com/1.1/statuses/filter.json") + "&" + Uri.EscapeDataString(this._streamFilter);

                // Create OAuth header
                var authHeader = new StringBuilder();
                authHeader.Append($"oauth_consumer_key = \"{this.ConfigObject.ConsumerKey}\"");
                authHeader.Append($", oauth_nonce = \"{Guid.NewGuid():N}\"");
                authHeader.Append($", oauth_signature = \"{signature}\"");
                authHeader.Append(", oauth_signature_method = \"HMAC-SHA1\"");
                authHeader.Append($", oauth_timestamp = \"{(DateTimeOffset.UtcNow - new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero)).TotalSeconds:0}\"");
                authHeader.Append(", oauth_version = \"1.0\"");

                this._bearerToken = authHeader.ToString();
            }

            protected override Action CreateBackgroundAction(CancellationToken token)
            {
                return () =>
                    {
                        using (var client = new HttpClient())
                        {
                            client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
                            client.BaseAddress = new Uri("https://stream.twitter.com/");

                            var request = new HttpRequestMessage(HttpMethod.Post, "/1.1/statuses/filter.json");
                            request.Content = new StringContent(this._streamFilter);
                            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };
                            request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", this._bearerToken);


                            var response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
                            var stream = response.Content.ReadAsStreamAsync().Result;

                            using (var reader = new StreamReader(stream))
                            {

                                while (!reader.EndOfStream)
                                {

                                    //We are ready to read the stream
                                    var currentLine = reader.ReadLine();
                                }
                            }
                        }
                    };
            }

            protected override void UpdateRecord(Record record, Tweet value)
            {
            }

            private static string Base64Encode(string plainText)
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return System.Convert.ToBase64String(plainTextBytes);
            }
        }
    }
}
