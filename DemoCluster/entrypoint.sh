#!/bin/bash
set -ex

nohup /kusto/Kusto.Personal/Kusto.Personal -createDefaultDatabase:false -gw -https:false -AutomaticallyDetachCorruptDatabases:true -enableRowStore:true < /dev/null > output.log 2>&1 &
echo "Kusto Personal started"
sleep 3
pwsh -File /dbMock.ps1

sleep infinity