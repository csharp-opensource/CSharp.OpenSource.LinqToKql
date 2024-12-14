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

