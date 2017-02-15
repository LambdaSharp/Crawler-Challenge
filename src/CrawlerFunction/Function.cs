using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;

using Amazon.DynamoDBv2.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace CrawlerFunction {

    public class Function {

        private const int LINKS_PER_PAGE = 10;
        private const string TABLE_NAME = "lambda_sharp_crawler";

        //--- Types ---
        public struct UrlInfo {

            //--- Fields ---
            public readonly Uri Url;
            public readonly int Depth;

            //--- Constructors ---
            public UrlInfo(Uri url, int depth) {
                this.Url = url;
                this.Depth = depth;
            }
        }

        //--- Fields ---
        private readonly IAmazonDynamoDB _client;


        //--- Constructors ---
        public Function() {
            _client = new AmazonDynamoDBClient();
        }

        //--- Methods ---
        public string Handler(DynamoDbUpdate update, ILambdaContext context) {
            foreach(var record in update.Records.Where(r => "INSERT".Equals(r.EventName))) {
                var urlInfo = record.Details.NewValues;
                if(urlInfo.CrawlerDepth == null || urlInfo.CrawlerUrl == null) {
                    continue;
                }
                ProcessUrl(new UrlInfo(new Uri(urlInfo.CrawlerUrl.Value), urlInfo.CrawlerDepth.Value)).Wait();
                
            }
            return null;
        }

        public async Task ProcessUrl(UrlInfo urlInfo) {
            if(urlInfo.Depth <= 0) {

                // stop processing because we have reached the maximum depth
                Console.WriteLine($"Ignoring URL '{urlInfo.Url}' because we have reached the maximum depth");
                return;
            }

            // download the webpage
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(urlInfo.Url);
            var strContents = await response.Content.ReadAsStringAsync();

            // get the word count from the HTML
            var wordCount = HelperFunctions.CountWords(strContents);

            // update the record to reflect the word count
            await _client.UpdateItemAsync(TABLE_NAME, 
                new Dictionary<string, AttributeValue>() {
                    { "crawlerurl", new AttributeValue { S = urlInfo.Url.ToString() }}
                }, 
                new Dictionary<string, AttributeValueUpdate>() {
                    { "word_count", new AttributeValueUpdate { Action = "PUT", Value = new AttributeValue { N = wordCount.ToString() }} }
                }
            );

            // only enqueue child links if depth is greater than 1
            if(urlInfo.Depth > 1) {
                var foundLinks = new List<Uri>();
                foreach(var link in HelperFunctions.FindLinks(strContents)) {

                    // check that link is in the same domain
                    var llink = link;
                    if(llink.StartsWith("//")) {

                        // append the scheme
                        llink = $"{urlInfo.Url.Scheme}:{llink}";
                    }
                    if(llink.StartsWith("/")) {
                        llink = $"{urlInfo.Url.Scheme}://{urlInfo.Url.Host}{llink}";
                    }
                    Uri parsedLink = null;
                    try {
                        parsedLink = new Uri(llink); 
                    } catch { 
                        continue;
                    }

                    // ignore external urls
                    if(!parsedLink.Host.Equals(urlInfo.Url.Host)) {
                        continue;
                    }
                    foundLinks.Add(parsedLink);
                }

                Console.WriteLine($"Found {foundLinks.Count} links, only using {LINKS_PER_PAGE}");

                // perform a batch get so that we don't insert duplicate links
                var putRequests = new List<PutItemRequest>(foundLinks.Count);
                foreach(var link in foundLinks.Take(LINKS_PER_PAGE)) {
                    var putRequest = new PutItemRequest {
                        TableName = TABLE_NAME,
                        Item = new Dictionary<string, AttributeValue>() {
                            { "crawlerurl", new AttributeValue { S = link.ToString() }},
                            { "crawlerdepth", new AttributeValue { N = (urlInfo.Depth - 1).ToString() }},
                        },

                        // this is to prevent insertion of duplicate links
                        ConditionExpression = "attribute_not_exists(crawlerurl)"
                    };
                    putRequests.Add(putRequest);
                }
                foreach(var request in putRequests) {
                    try {
                        await _client.PutItemAsync(request);
                    } catch(ConditionalCheckFailedException) { /* */ }
                }
            }
        }
    } 
}
