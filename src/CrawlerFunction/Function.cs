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

        //--- Constants ---
        private const int LINKS_PER_PAGE = 10;
        private const string TABLE_NAME = "lambda_sharp_crawler";

        //--- Fields ---

        //--- Constructors ---
        public Function() {
        }

        //--- Methods ---
        public string Handler(DynamoDbUpdate update, ILambdaContext context) {

            // only process events that are insering records
            foreach(var record in update.Records.Where(r => "INSERT".Equals(r.EventName))) {
                var urlInfo = record.Details.NewValues;

                // a url and depth are required
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

            // LEVEL 1: get this to print :)
            Console.WriteLine($"Processing URL '{urlInfo.Url}' with depth {urlInfo.Depth}");

            // LEVEL 2:
            // 1. Download the webpage
            // 2. Count the words on the page (see HelpFunctions.CountWords)
            // 3. Store the word count back into DynamoDB under a separate column

            // only enqueue child links if depth is greater than 1
            if(urlInfo.Depth > 1) {
                
                // LEVEL 3:
                // 1. Parse all links out of the page content (see HelpFunctions.FindLinks) -> Plase use LINKS_PER_PAGE to limit the number of links you will be processing per page
                // 2. For each link, determine if it is pointing to the same domain as the original link, we do not want to be crawling other sites!
                // 3. If it is, insert it back into DynamoDB for further processing. (NOTE: ensure to store it back with a lower Depth! Otherwise the computation will continue indefinitely!!!)

            }

            // LEVEL _BOSS_:
            // Determine the most popular page by finding the page with the most number of backlinks
        }
    } 
}
