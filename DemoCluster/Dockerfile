FROM mcr.microsoft.com/azuredataexplorer/kustainer-linux:latest
ENV ACCEPT_EULA=Y
RUN mkdir /kusto/tmp
ENV TMP=/kusto/tmp/
ENV TEMP=$TMP
ENV TMPDIR=$TMP
RUN yum install tmux powershell -y
COPY . .
ENTRYPOINT ["/entrypoint.sh"]