using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Configuration;
using Amazon.SimpleNotificationService;
using Amazon.Runtime;
using Amazon.SimpleNotificationService.Model;
using SNSEndPointCreation;

var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

IConfiguration configuration = builder.Build();

#region Extract list of device tokens from db
//Retrieve List of device tokens to create endpoints
var tokenList = new Dictionary<string, List<string>>();
Console.WriteLine("Please enter your db connection string: ");
var connectionString = Console.ReadLine();

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Empty connection string found");
    return;
}

using (var db = new AppDbContext(connectionString))
{
    foreach(Device device in db.Devices)
    {
        if (tokenList.ContainsKey(device.platform.ToString()))
        {
            if (!tokenList[device.platform.ToString()].Contains(device.token))
            {
                tokenList[device.platform.ToString()].Add(device.token);
            }
        }
        else
        {
            tokenList.Add(device.platform.ToString(), new List<string> { device.token });
        }
    }
}
Console.WriteLine("\nFinished extracting device tokens from db");
#endregion

#region Creating endpoints based on environment
Console.WriteLine("\n\nPlease select which environment to migrate (UAT or PROD):");

var platformToMigrate = Console.ReadLine();
if(platformToMigrate != "UAT" && platformToMigrate != "PROD")
{
    Console.WriteLine("Invalid environment entered");
    return;
}

string androidPlatformArn = "";
string iosPlatformArn = "";

if (platformToMigrate == "UAT")
{
    androidPlatformArn = configuration["Android_PlatformARN_UAT"];
    iosPlatformArn = configuration["IOS_PlatformARN_UAT"];
}
else
{
    androidPlatformArn = configuration["Android_PlatformARN_PROD"];
    iosPlatformArn = configuration["IOS_PlatformARN_PROD"];
}
// If using environment variable
//var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
//var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
//var sessionToken = Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN");

// If using AWS credentials basic/session
//Console.WriteLine("\n\n Enter your access key:");
//var accessKey = Console.ReadLine();
//Console.WriteLine("Enter your secret key:");
//var secretKey = Console.ReadLine();

//IAmazonSimpleNotificationService snsClient = new AmazonSimpleNotificationServiceClient(new BasicAWSCredentials("accessKey", "secretKey"));

//Console.WriteLine("Enter your session token:");
//var sessionToken = Console.ReadLine();
//IAmazonSimpleNotificationService snsClient = new AmazonSimpleNotificationServiceClient(new SessionAWSCredentials("accessKey", "secretKey", "sessionToken"));

Console.WriteLine("Creating endpoints");
Console.WriteLine(".\n.\n.\n.");
var newTokenList = new Dictionary<string, string>();
try
{
    foreach (KeyValuePair<string, List<string>> platform in tokenList)
    {

        foreach (string token in platform.Value)
        {
            //CreatePlatformEndpointRequest endPointRequest = new CreatePlatformEndpointRequest();
            //endPointRequest.Token = token;
            //if (platform.Key.Equals("A"))
            //{
            //    endPointRequest.PlatformApplicationArn = androidPlatformArn;
            //}
            //else
            //{
            //    endPointRequest.PlatformApplicationArn = iosPlatformArn;
            //}
            //CreatePlatformEndpointResponse endPointResponse = snsClient.CreatePlatformEndpointAsync(endPointRequest).Result;
            //if (!newTokenList.ContainsKey(token))
            //{
            //    newTokenList.Add(token, endPointResponse.EndpointArn);
            //}
            if (!newTokenList.ContainsKey(token))
            {
                newTokenList.Add(token, Guid.NewGuid().ToString());
            }
        }
    }
}
catch(AmazonSimpleNotificationServiceException ex)
{
    Console.WriteLine(ex.ToString());
    return;
}
#endregion

using (StreamWriter writer = new StreamWriter("update_db_script.sql"))
{
    string sqlQuery = "";
    string platformARN = "";
    foreach(KeyValuePair<string, List<string>> platform in tokenList)
    {
        if (platform.Key.Equals("A"))
        {
            platformARN = androidPlatformArn;
        }
        else
        {
            platformARN = iosPlatformArn;
        }
        foreach(string token in platform.Value)
        {
            sqlQuery = $"UPDATE db_owner.Device SET targetPlatformEndPoint = '{platformARN}/{newTokenList[token]}' WHERE token = '{token}';";
            writer.WriteLine(sqlQuery);
        }
    }
}
Console.WriteLine("Finish migration");
Console.ReadLine();