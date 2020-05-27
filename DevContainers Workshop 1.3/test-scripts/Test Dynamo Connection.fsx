//  How to run this script from VS Code in the Dev Container
//  ================================================================================
//  
//  For the best user experience, it is best to run this script step-by-step. 
//  Steps are indicated with the code comments. You can also run the script in one go.
//  
//  To run:
//  1. select text in this script with your mouse, ie all lines related to "Step 1"
//  2. press Alt + Enter to run the text-selection
//  3. view the results in the output panel
//  
//  response in the output panel can be slow
//  all selected text will be repeated in the output panel
//  'let' statements will always display a result with "val" when finished



//  Step 1: retrieve required nuget packages and open namespaces
#r "nuget:include=FSharp.AWS.DynamoDB, version=0.8.0-beta"
#r "nuget:include=AWSSDK.DynamoDBv2, version=3.3.106.6"

open FSharp.AWS.DynamoDB
open Amazon.DynamoDBv2
open Amazon.Runtime
open System


//  Step 2: declare generic function which connects to DynamoDB and creates a table by name
let ``Connect to DynamoDB Table``<'a> tableName =
    let getDynamoDBAccount () =
        let credentials = BasicAWSCredentials("Fake", "Fake")
        let config = AmazonDynamoDBConfig()
        config.ServiceURL <- "http://dynamodb-local:8000"   // DynamoDB endpoint!
        new AmazonDynamoDBClient(credentials, config) :> IAmazonDynamoDB
    let client: IAmazonDynamoDB = getDynamoDBAccount ()

    TableContext.Create<'a>(client, tableName = tableName, createIfNotExists = true)


//  Step 3: declare serializabe type "WorkItemInfo"
type WorkItemInfo =
    { [<HashKey>]
      ProcessId: int64

      [<RangeKey>]
      WorkItemId: int64

      Name: string
      UUID: Guid
      Dependencies: Set<string>
      Started: DateTimeOffset option }


//  Step 4: create table "workItems" and connect to it. This table stores data of type "WorkItemInfo"
let table = ``Connect to DynamoDB Table``<WorkItemInfo> "workItems"


//  Step 5: create an instance of "WorkItemInfo"
let workItem =
    { ProcessId = 0L
      WorkItemId = 1L
      Name = "Test"
      UUID = Guid.NewGuid()
      Dependencies = set [ "mscorlib" ]
      Started = None }


//  Step 6: store the instance into the DynamoDB table
let key: TableKey = table.PutItem(workItem)


//  Step 7: retrieve the data stored in the previous step
let workItem' = table.GetItem(key)
