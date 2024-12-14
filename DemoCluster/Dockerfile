FROM mcr.microsoft.com/azuredataexplorer/kustainer-linux:latest
ENV ACCEPT_EULA=Y
RUN mkdir /kusto/tmp
ENV TMP=/kusto/tmp/
ENV TEMP=$TMP
ENV TMPDIR=$TMP
RUN yum install powershell dotnet-sdk-9.0 -y
COPY . .
ENTRYPOINT ["/entrypoint.sh"]