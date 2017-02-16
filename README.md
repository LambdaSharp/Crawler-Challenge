# Build a Serverless .NET Core Web Crawler using DynamoDB

In this challenge we're going to learn how to write a scalable web crawler using [AWS Lambda](https://aws.amazon.com/lambda/) and AWS [DynamoDB](https://aws.amazon.com/dynamodb/). The purpose of this simple crawler will be to simply count the words on every page that it encounters. 

We will kick off our crawler by inserting a link into DynamoDB, which will trigger a Lambda function that will go out and fetch the target page. Once the page is parsed, the lambda function will record the word count back into DynamoDB. In addition it will also insert any discovered links back into DynamoDB which will again trigger new lambda functions.


## Prerequisites
1. AWS Tools
    1. Sign-up for an [AWS account](https://aws.amazon.com)
    2. Install [AWS CLI](https://aws.amazon.com/cli/)
2. .NET Core
    1. Install [.NET Core 1.0](https://www.microsoft.com/net/core) **(NOTE: You MUST install 1.0. Do NOT install 1.1 or later!)**
    2. Install [Visual Studio Code](https://code.visualstudio.com/)
    3. Install [C# Extension for VS Code](https://code.visualstudio.com/Docs/languages/csharp)
3. AWS C# Lambda Tools
    1. Install [Nodejs](https://nodejs.org/en/)
    2. Install [Yeoman](http://yeoman.io/codelab/setup.html)
    3. Install AWS C# Lambda generator: `npm install -g yo generator-aws-lambda-dotnet`

## Level 1: Triggering the crawler to parse a single page

Let's begin by creating a DynamoDB table.

1. Create a table called `lambda_sharp_crawler`. Use `crawlerurl`(String) as the primary key for the table.

Let's deploy our crawler function. 

2. From the `src/CrawlerFunction` directory run the following command: `dotnet lambda deploy-function -fn lamda-sharp-crawler-function`.
*NOTE*: Ensure that the lambda function has full permissions to DynamoDB as it will need to both read and write to the table.

3. From the DynamoDB table set-up a trigger that will execute the deployed lambda function. I suggest using a `Batch Size` of `10`.

We are ready to trigger a simple crawl! I have provided a script for easily starting a crawl:

`./crawl.sh lambda_sharp_crawler https://en.wikipedia.org/wiki/Lambda 2`

*Note*: `USAGE: ./crawl.sh {table_name} {url} {depth}`

**ACCEPTANCE TEST**: You are able to verify that the URL and Depth is printed out by the lambda function by checking the CloudWatch Logs.

## Level 2: Count the words

Modify `Function.cs` to download the webpage and count the number of words on the downloaded web page and store the result back into DynamoDB as a separate column. Use `HelperFunctions.cs` to help you count the words.

## Level 3: Crawling recursively

Modify `Function.cs` to find up to 10 links on the downloaded page, and enqueue them for further processing (by inserting them back into DynamoDB). 

*WARNING*: The nature of this challenge lends itself to easily racking up a large AWS bill :) The sample code provided has a few safety guards:

1. Only grab 10 links from every page
2. You have to specify a maximum depth for the computation (number of link hops it will follow)
   *Note*: 10 links per page, and going 3 levels deep is already 1000 pages!
3. Only process URL's that match the domain of the original link, do not start crawling other sites!
4. Make sure to insert new links with a lower `Depth` value so that the computation stops eventually!

**ACCEPTANCE TEST**: You should see a lot more than 1 URL with its corresponding word count in DynamoDB
**ACCEPTANCE TEST**: The computation ends as expected and your credit score does not plummet.

## Boss Level: Determine the page with the most back links

If you have completed all other levels, then this challenge is for you. Figure out the most popular page, wich is defined as the page with the most links pointing to it.

Happy Hacking, grab a beer (you will need one). Trust me.
