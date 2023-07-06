FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

COPY . .

RUN dotnet restore
RUN dotnet publish --no-restore -c Release -o out

# Use the alpine nginx image as a base
FROM nginx:alpine
# Copy the local nginx configuration folder
COPY .nginx /etc/nginx
# Set the working directory to the default nginx html directory
WORKDIR /usr/share/nginx/html
# Remove the existing web files
RUN rm -rf ./*
# Copy the files from the static next export
COPY --from=build ./out/wwwroot /usr/share/nginx/html

EXPOSE 80