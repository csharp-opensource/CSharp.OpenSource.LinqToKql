FROM mcr.microsoft.com/azuredataexplorer/kustainer-linux:latest
ENV ACCEPT_EULA=Y
RUN mkdir /kusto/tmp
ENV TMP=/kusto/tmp/
ENV TEMP=$TMP
ENV TMPDIR=$TMP
RUN yum install powershell -y
RUN yum install dos2unix -y
COPY . .
RUN dos2unix /entrypoint.sh
RUN chmod +x /entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]