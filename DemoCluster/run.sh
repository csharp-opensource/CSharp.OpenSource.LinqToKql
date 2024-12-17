docker build -t kusto . && docker rm -f kusto && docker run -d -it --name kusto -p 8080:8080 kusto
