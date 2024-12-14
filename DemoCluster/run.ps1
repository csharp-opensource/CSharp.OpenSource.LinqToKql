 docker build -t test . && docker rm -f kusto && docker run -it --name kusto -p 8080:8080  test
