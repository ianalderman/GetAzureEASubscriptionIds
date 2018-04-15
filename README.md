# GetAzureEASubscriptionIds
Azure Function which uses [Azure Enterprise Billing APIs](https://docs.microsoft.com/en-us/azure/billing/billing-enterprise-api) to generate a JSON list of Subscription Ids in an Azure Enterprise Agreement.

By default this function looks back over 3 days of usage to capture any subs which may not have generated any usage over the weekend for example.

## Why?

A key building block in some Governance projects I have been looking at requires a list of all the Subscription Ids that an Enterprise owns and to date this is the only programatic way I have found to identify that.

## Usage
In order to call get the list of Subscriptiong GUIDs you will need to call the function with the following JSON body:

{
    "enrollmentNumber": "<YOUR ENROLLMENT NUMBER>",
    "accessKey": "<YOUR API ACCESS KEY>"
}

N.B. This only returns a list of current IDs you would need to build on this to track new subscriptions, deleted ones etc.

## Sample Output

{
    [
        "GUID1",
        "GUID2"
    ]
}


