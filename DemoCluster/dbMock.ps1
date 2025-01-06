function req($body) {
    $bodyStr = $body | ConvertTo-Json
    Invoke-WebRequest -Method post -ContentType 'application/json' -Body "$bodyStr" http://localhost:8080/v1/rest/mgmt
}

function createDb($dbName) {
    req @{
        csl = ".create database $dbName persist (@`"/kustodata/dbs/$dbName/md@`",@`"/kustodata/dbs/$dbName/data@`")"
    }
}

req @{
    csl = ".show cluster"
}
createDb "TestDatabase1"
createDb "TestDatabase2"
req @{ 
    db="TestDatabase1"
    csl=".create table TestTable (TestColumn: string)"
}
req @{
    db="TestDatabase1"
    csl=".create function with (docstring = 'Function 1', folder = 'Functions') func1() { TestTable }"
}
req @{
    db="TestDatabase2"
    csl=".create table TestTable1 (userName: string,userLastName: string)"
}
req @{
    db="TestDatabase2"
    csl=".create function with (docstring = 'Function 2', folder = 'Functions') func2(name: string, lastName: string) { TestTable1 | where userName == name and userLastName == lastName }"
}
req @{
    db="TestDatabase1"
    csl=".create table SampleTable ( Id: int, Name: string, Date: datetime, DateOnly: date, TimeOnly: timespan, Time: timespan, IsActive: bool, Year: int, Description: string, Type: string, Value: long, Numbers: dynamic, Nested: dynamic, prop_newtonsoft: long, prop_text_json: long, prop_data_member: long)"
}
req @{
    db="TestDatabase1"
    csl=".ingest inline into table SampleTable <|
1,Active Item,2025-01-06T12:00:00Z,2025-01-06,12:00:00,12:00:00,true,2025,This is an active item,TypeA,100,`"[1,2,3]`",`"{\`"key1\`":\`"value1\`",\`"key2\`":\`"value2\`"}`",10,20,30
2,Inactive Item,2025-01-06T15:30:00Z,2025-01-06,15:30:00,15:30:00,false,2025,This is an inactive item,TypeB,200,`"[4,5,6]`",`"{\`"key1\`":\`"value3\`",\`"key2\`":\`"value4\`"}`",40,50,60"
}
