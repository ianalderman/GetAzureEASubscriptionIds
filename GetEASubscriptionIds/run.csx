#r "Newtonsoft.Json"
using System.Net;
using Newtonsoft.Json;
using System.Text;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    dynamic reqBody = await req.Content.ReadAsAsync<object>();
    string enrollmentNumber = reqBody?.enrollmentNumber;
    string accessKey = reqBody?.accessKey;
    
    int daysToCheck = -3;
    if (enrollmentNumber == null || accessKey == null) {
        req.CreateResponse(HttpStatusCode.BadRequest, "Please pass JSON in body with enrollmentNumber and accessKey");
    }

    string endDate = DateTime.Now.ToString("yyyy-MM-dd");
    string startDate = DateTime.Now.AddDays(daysToCheck).ToString("yyyy-MM-dd");

    //Use the URI below if you want to use the current billing period for your checks, current billing period is returned by default if
    //if you wish to use a different billing period you need to use billing period API to get it first
    //string usageURI = $"https://consumption.azure.com/v2/enrollments/{enrollmentNumber}/usagedetails";
    //Use the URI below if you want to use the billing data from the last x days as defined with daysToCheck
    string usageURI = $"https://consumption.azure.com/v2/enrollments/{enrollmentNumber}/usagedetailsbycustomdate?startTime={startDate}&endTime={endDate}";
        
    string nextLink = "";
    var subscriptionGUIDs = new List<string>();

    do {
        string usageDetail = "";
        if (nextLink == "") {
            WebRequest usageDetailRequest = WebRequest.Create(usageURI);
            usageDetailRequest.Headers.Add("Authorization", "bearer " + accessKey);

            HttpWebResponse usageDetailResponse = (HttpWebResponse)usageDetailRequest.GetResponse();
            StreamReader usageDetailreader = new StreamReader(usageDetailResponse.GetResponseStream());
            usageDetail = usageDetailreader.ReadToEnd();
        } else {
            WebRequest usageDetailRequest = WebRequest.Create(nextLink);
            usageDetailRequest.Headers.Add("Authorization", "bearer " + accessKey);

            HttpWebResponse usageDetailResponse = (HttpWebResponse)usageDetailRequest.GetResponse();
            StreamReader usageDetailreader = new StreamReader(usageDetailResponse.GetResponseStream());
            usageDetail = usageDetailreader.ReadToEnd();
        }
        
        dynamic usage = JsonConvert.DeserializeObject(usageDetail);
        nextLink = usage.nextLink;
        
        foreach(dynamic dataEntry in usage.data) { 
            if (!subscriptionGUIDs.Contains(dataEntry.subscriptionGuid.ToString())) {
                subscriptionGUIDs.Add(dataEntry.subscriptionGuid.ToString());
            }
        }
    } while (nextLink != null);

    var jsonGUIDs = JsonConvert.SerializeObject(subscriptionGUIDs);
    
    return req.CreateResponse(HttpStatusCode.OK, jsonGUIDs);
}