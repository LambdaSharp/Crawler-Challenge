
using Newtonsoft.Json;

namespace CrawlerFunction {

    //--- Types ---
    public class DynamoDbUpdate {

        public class DynamoDbRecord {

            public class DynamoDbDetails {

                public class NewImage {

                    public class KeyDepth {
                        
                        [JsonProperty("N")]
                        public int Value;

                    }

                    public class KeyUrl {

                        [JsonProperty("S")]
                        public string Value;
                    }

                    [JsonProperty("crawlerdepth")]
                    public KeyDepth CrawlerDepth;

                    [JsonProperty("crawlerurl")]
                    public KeyUrl CrawlerUrl;

                }

                [JsonProperty("NewImage")]
                public NewImage NewValues;

            }

            [JsonProperty("eventId")]
            public string EventId;

            [JsonProperty("eventName")]
            public string EventName;

            [JsonProperty("dynamodb")]
            public DynamoDbDetails Details;
        }

        //--- Fields ---
        [JsonProperty("Records")]
        public DynamoDbRecord[] Records;
    }
}