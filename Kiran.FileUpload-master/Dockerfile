FROM microsoft/aspnetcore-build AS build
WORKDIR /source
COPY . .


WORKDIR /source/Kiran.FileUpload
run dotnet restore
RUN dotnet build -c Release 

FROM build AS publish
RUN dotnet publish -c Release -o /app/
 
FROM microsoft/aspnetcore AS runtime
EXPOSE 80
WORKDIR /app
COPY --from=publish /app ./
ENTRYPOINT ["dotnet", "Kiran.FileUpload.dll"]


