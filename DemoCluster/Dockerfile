FROM mcr.microsoft.com/azuredataexplorer/kustainer-linux:latest
ENV ACCEPT_EULA=Y
RUN mkdir /kusto/tmp
ENV TMP=/kusto/tmp/
ENV TEMP=$TMP
ENV TMPDIR=$TMP
RUN yum install powershell -y
COPY . .
RUN chmod +x /entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]